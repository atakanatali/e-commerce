namespace Cloudify.Infrastructure;

/// <summary>
/// Allocates host ports for services using environment-scoped state.
/// </summary>
public sealed class PortAllocator : IPortAllocator
{
    private readonly IStateStore _stateStore;
    private readonly IPortAvailabilityChecker _portAvailabilityChecker;
    private readonly PortAllocatorOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="PortAllocator"/> class.
    /// </summary>
    /// <param name="stateStore">The state store for persistence.</param>
    /// <param name="portAvailabilityChecker">The port availability checker.</param>
    /// <param name="options">The allocator options.</param>
    public PortAllocator(
        IStateStore stateStore,
        IPortAvailabilityChecker portAvailabilityChecker,
        PortAllocatorOptions options)
    {
        _stateStore = stateStore;
        _portAvailabilityChecker = portAvailabilityChecker;
        _options = options;
    }

    /// <summary>
    /// Allocates ports for a resource based on allocation policies.
    /// </summary>
    /// <param name="request">The allocation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The allocated port result.</returns>
    public async Task<PortAllocationResult> AllocateAsync(
        PortAllocationRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        PortAllocationResult? result = null;

        await _stateStore.UpdateResourcePortsAsync(
            request.EnvironmentName,
            async (existing, token) =>
            {
                var state = existing ?? new ResourcePorts(request.EnvironmentName);
                if (state.Assignments.TryGetValue(request.ResourceId, out var existingAssignment))
                {
                    result = new PortAllocationResult(
                        request.EnvironmentName,
                        request.ResourceId,
                        existingAssignment.ServiceType,
                        existingAssignment.Ports);
                    return state;
                }

                var allocatedPorts = await AllocatePortsAsync(request, state, token).ConfigureAwait(false);
                state.Assignments[request.ResourceId] = new PortAssignment(request.ServiceType, allocatedPorts);
                result = new PortAllocationResult(
                    request.EnvironmentName,
                    request.ResourceId,
                    request.ServiceType,
                    allocatedPorts);
                return state;
            },
            cancellationToken).ConfigureAwait(false);

        if (result is null)
        {
            throw new InvalidOperationException("Port allocation failed to produce a result.");
        }

        return result;
    }

    /// <summary>
    /// Validates the allocation request arguments.
    /// </summary>
    /// <param name="request">The allocation request to validate.</param>
    private static void ValidateRequest(PortAllocationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EnvironmentName))
        {
            throw new ArgumentException("Environment name is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.ResourceId))
        {
            throw new ArgumentException("Resource id is required.", nameof(request));
        }
    }

    /// <summary>
    /// Allocates ports based on request and current environment state.
    /// </summary>
    /// <param name="request">The allocation request.</param>
    /// <param name="state">The current environment port state.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The allocated ports.</returns>
    private async Task<IReadOnlyList<int>> AllocatePortsAsync(
        PortAllocationRequest request,
        ResourcePorts state,
        CancellationToken cancellationToken)
    {
        var usedPorts = state.GetAllPorts();
        var basePortSets = GetBasePortSets(request.ServiceType);

        if (request.RequestedPort.HasValue)
        {
            var requestedPort = request.RequestedPort.Value;
            var baseSet = basePortSets[0];
            var offset = requestedPort - baseSet[0];
            var candidate = baseSet.Select(port => port + offset).ToList();
            await EnsurePortsAvailableAsync(candidate, usedPorts, cancellationToken).ConfigureAwait(false);
            return candidate;
        }

        foreach (var baseSet in basePortSets)
        {
            for (var offset = 0; offset <= 10000; offset++)
            {
                var candidate = baseSet.Select(port => port + offset).ToList();
                if (candidate.Any(port => port > 65535 || port < 1))
                {
                    break;
                }

                if (!candidate.All(port => !usedPorts.Contains(port)))
                {
                    continue;
                }

                if (!await ArePortsAvailableAsync(candidate, cancellationToken).ConfigureAwait(false))
                {
                    continue;
                }

                return candidate;
            }
        }

        throw new InvalidOperationException("Unable to allocate an available port set.");
    }

    /// <summary>
    /// Ensures requested ports are unique and available on the host.
    /// </summary>
    /// <param name="candidate">The candidate ports.</param>
    /// <param name="usedPorts">The ports already used in the environment.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task EnsurePortsAvailableAsync(
        IReadOnlyList<int> candidate,
        HashSet<int> usedPorts,
        CancellationToken cancellationToken)
    {
        if (candidate.Any(port => port is < 1 or > 65535))
        {
            throw new ArgumentOutOfRangeException(nameof(candidate), "Ports must be between 1 and 65535.");
        }

        if (candidate.Any(usedPorts.Contains))
        {
            throw new InvalidOperationException("Requested port is already allocated in the environment.");
        }

        if (!await ArePortsAvailableAsync(candidate, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException("Requested port is not available on the host.");
        }
    }

    /// <summary>
    /// Checks availability for all candidate ports on the host.
    /// </summary>
    /// <param name="ports">The ports to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> when all ports are available; otherwise, <c>false</c>.</returns>
    private async Task<bool> ArePortsAvailableAsync(
        IEnumerable<int> ports,
        CancellationToken cancellationToken)
    {
        foreach (var port in ports)
        {
            if (!await _portAvailabilityChecker.IsPortAvailableAsync(port, cancellationToken).ConfigureAwait(false))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets base port sets for a service type.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    /// <returns>The ordered base port sets.</returns>
    private IReadOnlyList<IReadOnlyList<int>> GetBasePortSets(ServiceType serviceType)
    {
        return serviceType switch
        {
            ServiceType.Redis => new List<IReadOnlyList<int>> { new List<int> { 6379 } },
            ServiceType.Postgres => new List<IReadOnlyList<int>> { new List<int> { 5432 } },
            ServiceType.Mongo => new List<IReadOnlyList<int>> { new List<int> { 27017 } },
            ServiceType.RabbitMq => new List<IReadOnlyList<int>> { new List<int> { 5672, 15672 } },
            ServiceType.AppService => _options.AppServiceBasePorts.Select(port => (IReadOnlyList<int>)new List<int> { port }).ToList(),
            _ => throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, "Unsupported service type.")
        };
    }
}

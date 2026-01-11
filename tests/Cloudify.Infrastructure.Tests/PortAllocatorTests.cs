using Cloudify.Infrastructure;
using Xunit;

namespace Cloudify.Infrastructure.Tests;

/// <summary>
/// Tests for <see cref="PortAllocator"/> behavior.
/// </summary>
public sealed class PortAllocatorTests
{
    /// <summary>
    /// Verifies that allocating many resources yields unique ports.
    /// </summary>
    /// <returns>The task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AllocateAsync_ManyResources_UsesUniquePorts()
    {
        var stateStore = new InMemoryStateStore();
        var availabilityChecker = new FakePortAvailabilityChecker();
        var options = new PortAllocatorOptions { AppServiceBasePorts = new List<int> { 5000 } };
        var allocator = new PortAllocator(stateStore, availabilityChecker, options);

        var tasks = Enumerable.Range(0, 60)
            .Select(index => allocator.AllocateAsync(
                new PortAllocationRequest("env-1", $"app-{index}", ServiceType.AppService)));

        var results = await Task.WhenAll(tasks);
        var allocatedPorts = results.SelectMany(result => result.Ports).ToList();

        Assert.Equal(allocatedPorts.Count, allocatedPorts.Distinct().Count());
    }

    /// <summary>
    /// Provides a deterministic port availability checker for tests.
    /// </summary>
    private sealed class FakePortAvailabilityChecker : IPortAvailabilityChecker
    {
        /// <summary>
        /// Returns <c>true</c> for any port to avoid host dependency in tests.
        /// </summary>
        /// <param name="port">The port to check.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>true</c> for all ports.</returns>
        public Task<bool> IsPortAvailableAsync(int port, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }
}

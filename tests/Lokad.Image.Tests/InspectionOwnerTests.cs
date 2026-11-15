using System.Diagnostics.CodeAnalysis;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class InspectionOwnerTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void UnbudgetedInterfaceDisposalFinishesLocalAccounting(bool probe)
    {
        using var input = new MemoryStream([7], writable: false);
        var reader = Create(input, new() { MaxWorkingBytes = 512 }, probe);
        Assert.False(reader.HasMemoryBudget);
        Assert.Equal(0, reader.RemainingWorkingBytes);
        ((IDisposable)reader).Dispose();
        Assert.Equal(512, reader.RemainingWorkingBytes);
        reader.Dispose();
        Assert.Equal(512, reader.RemainingWorkingBytes);
        Assert.True(input.CanRead);
        Assert.Equal(0, input.Position);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void InterfaceDisposalRefundsTheSingleOwnerAndKeepsBorrowedObjects(bool probe)
    {
        using var root = new TestMemoryBudget(512);
        var budget = new CountedBudget(root);
        using var input = new MemoryStream([7], writable: false);
        var reader = Create(input, new() { MaxWorkingBytes = 512, MemoryBudget = budget }, probe);
        Assert.Same(reader, reader.Context);
        Assert.Equal(512, root.Bytes);
        Assert.Equal(1, budget.LiveScopes);
        ((IDisposable)reader).Dispose();
        Assert.Equal(0, root.Bytes);
        Assert.Equal(0, budget.LiveScopes);
        Assert.Equal(512, reader.RemainingWorkingBytes);
        ((IDisposable)reader).Dispose();
        reader.Dispose();
        Assert.Equal(512, reader.RemainingWorkingBytes);
        Assert.False(root.IsDisposed);
        Assert.True(input.CanRead);
        Assert.Equal(0, input.Position);
        Assert.True(root.TryReserve(512, out var held));
        held.Dispose();
        Assert.Equal(0, root.Bytes);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ConstructorRefusalDisposesItsEmptyScopeAndSubsequentUseSucceeds(bool probe)
    {
        using var root = new TestMemoryBudget(512) { RefuseAtGrowth = 1 };
        var budget = new CountedBudget(root);
        using var input = new MemoryStream([7], writable: false);
        var limits = new ImageLimits { MemoryBudget = budget, MaxWorkingBytes = 512 };
        var error = Assert.Throws<ImageLimitException>(() => Create(input, limits, probe));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(1, root.Refusals);
        Assert.Equal(1, root.GrowthAttempts);
        Assert.Equal(0, root.Bytes);
        Assert.Equal(0, budget.LiveScopes);
        Assert.False(root.IsDisposed);
        Assert.True(input.CanRead);
        Assert.Equal(0, input.Position);
        root.RefuseAtGrowth = 0;
        using (var recovered = Create(input, limits, probe)) Assert.Equal(512, root.Bytes);
        Assert.Equal(0, root.Bytes);
        Assert.Equal(0, budget.LiveScopes);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ConstructorFailureAfterAdmissionRefundsBeforeReturning(bool probe)
    {
        using var root = new TestMemoryBudget(512);
        var budget = new CountedBudget(root);
        using var input = new MemoryStream([7], writable: false);
        var failure = new IOException("authored admission callback failure");
        root.Admitted = _ => throw failure;
        var limits = new ImageLimits { MemoryBudget = budget, MaxWorkingBytes = 512 };
        Assert.Same(failure, Assert.Throws<IOException>(() => Create(input, limits, probe)));
        Assert.Equal(0, root.Bytes);
        Assert.Equal(0, budget.LiveScopes);
        Assert.True(input.CanRead);
        Assert.False(root.IsDisposed);
        root.Admitted = null;
        using (var recovered = Create(input, limits, probe)) Assert.Equal(512, root.Bytes);
        Assert.Equal(0, root.Bytes);
        Assert.Equal(0, budget.LiveScopes);
    }

    private static InspectionReader Create(Stream input, ImageLimits limits, bool probe)
        => probe ? new ProbeReader(input, limits, default, false, 16, ImageProbeFacts.Dimensions)
            : new InspectionReader(input, limits, default, false, 16, dimensionsOnly: true);

    // Observe scope ownership while forwarding every charge to the same test root.
    private sealed class CountedBudget(TestMemoryBudget root) : IMemoryBudget
    {
        private int live;
        public int LiveScopes => Volatile.Read(ref live);
        public IMemoryScope CreateScope() => Wrap(root.CreateScope());
        public bool TryReserve(long bytes, [NotNullWhen(true)] out IMemoryReservation? reservation)
            => root.TryReserve(bytes, out reservation);
        private IMemoryScope Wrap(IMemoryScope scope)
        {
            Interlocked.Increment(ref live);
            return new CountedScope(this, scope);
        }
        private sealed class CountedScope(CountedBudget owner, IMemoryScope scope) : IMemoryScope
        {
            private int disposed;
            public IMemoryScope CreateScope() => owner.Wrap(scope.CreateScope());
            public bool TryReserve(long bytes, [NotNullWhen(true)] out IMemoryReservation? reservation)
                => scope.TryReserve(bytes, out reservation);
            public void Dispose()
            {
                if (Interlocked.Exchange(ref disposed, 1) != 0) return;
                scope.Dispose();
                Interlocked.Decrement(ref owner.live);
            }
        }
    }
}

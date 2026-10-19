using System.Diagnostics.CodeAnalysis;

namespace Lokad.Image.Tooling;

// Test-only adapter. Production has no governor implementation or dependency.
internal sealed class TestMemoryBudget : IMemoryScope
{
    private readonly TestMemoryBudget root;
    private readonly object gate;
    private readonly long limit;
    private readonly List<TestMemoryBudget> children = [];
    private readonly List<Reservation> reservations = [];
    private bool disposed;
    private long used, peak, attempts, refusals, growthAttempts;
    public Action<long>? Admitted { get; set; }
    public long RefuseAtGrowth { get; set; }
    public TestMemoryBudget(long limit) { this.limit = limit; root = this; gate = new(); }
    private TestMemoryBudget(TestMemoryBudget parent) { root = parent.root; gate = root.gate; }
    public long Bytes { get { lock (gate) return root.used; } }
    public long Peak { get { lock (gate) return root.peak; } }
    public long Attempts { get { lock (gate) return root.attempts; } }
    public long Refusals { get { lock (gate) return root.refusals; } }
    public long GrowthAttempts { get { lock (gate) return root.growthAttempts; } }
    public bool IsDisposed { get { lock (gate) return disposed; } }
    public IMemoryScope CreateScope()
    {
        lock (gate) { Check(); var child = new TestMemoryBudget(this); children.Add(child); return child; }
    }
    public bool TryReserve(long bytes, [NotNullWhen(true)] out IMemoryReservation? reservation)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bytes);
        lock (gate)
        {
            Check(); root.attempts++;
            if (Refused(bytes, 0)) { reservation = null; return false; }
            var result = new Reservation(this, bytes); reservations.Add(result);
            root.used += bytes; root.peak = Math.Max(root.peak, root.used); reservation = result;
            root.Admitted?.Invoke(root.used); return true;
        }
    }
    private void Check() => ObjectDisposedException.ThrowIf(disposed, this);
    private bool Refused(long bytes, long previous)
    {
        if (bytes > previous) root.growthAttempts++;
        var refuse = bytes - previous > root.limit - root.used
            || bytes > previous && root.RefuseAtGrowth != 0 && root.growthAttempts == root.RefuseAtGrowth;
        if (refuse) root.refusals++;
        return refuse;
    }
    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            foreach (var child in children) child.Dispose();
            foreach (var reservation in reservations) reservation.Dispose();
            disposed = true;
        }
    }
    private sealed class Reservation(TestMemoryBudget owner, long bytes) : IMemoryReservation
    {
        private long value = bytes;
        private bool disposed;
        public long Bytes { get { lock (owner.gate) return value; } }
        public bool TryResize(long bytes)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(bytes);
            lock (owner.gate)
            {
                owner.Check(); ObjectDisposedException.ThrowIf(disposed, this); owner.root.attempts++;
                if (owner.Refused(bytes, value)) return false;
                var previous = value; owner.root.used += bytes - value; value = bytes;
                owner.root.peak = Math.Max(owner.root.peak, owner.root.used);
                if (bytes > previous) owner.root.Admitted?.Invoke(owner.root.used);
                return true;
            }
        }
        public void Dispose()
        {
            lock (owner.gate)
            {
                if (disposed) return;
                disposed = true; owner.root.used -= value; value = 0;
            }
        }
    }
}

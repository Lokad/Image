namespace Lokad.Image.Internal;

// Admitted before allocation and owned until the final synchronous execution
// has finished. One-shot calls and retained plans use the same typed storage.
internal sealed class LanczosWorkspace : IDisposable
{
    internal long[]? ExactRows;
    internal Int128[]? ExactSums;
    internal double[]? FastRows, FastSums;
    // Exact word alpha products are prepared once per source row and borrowed
    // by overlapping horizontal windows. Byte execution needs no extra row.
    internal uint[] Products = [];
    internal int[] RowIds = [];
    internal readonly long Bytes;
    internal readonly bool Fast;
    private readonly OperationContext owner;
    private bool disposed;

    internal LanczosWorkspace(LanczosPlan horizontal, LanczosPlan vertical, int bitDepth,
        bool fast, OperationContext owner, long pendingOutputBytes = 0)
    {
        this.owner = owner; Fast = fast;
        var slots = Math.Min(vertical.MaximumCount, 64);
        var channels = horizontal.Windows.Length * 4L; var rows = channels * slots;
        var products = LanczosResize.PreparedHorizontalSupported(bitDepth)
            ? horizontal.SourceLength * 4 : 0;
        var bytes = 128 + rows * 8 + channels * (fast ? 8 : 16) + slots * 4L
            + (products == 0 ? 0 : 32 + products * 4L);
        // Two needed word rows can share coefficient preparation. Reuse this
        // array's owner; choose capacity from local limits before admission.
        // A shared refusal below still terminates without a smaller retry.
        if (fast && bitDepth == 16 && products != 0 && slots > 1
            && horizontal.Windows.Length < horizontal.SourceLength
            && vertical.Windows.Length < vertical.SourceLength
            && products <= Array.MaxLength / 2
            && bytes + products * 4L + pendingOutputBytes <= owner.RemainingWorkingBytes)
        {
            bytes += products * 4L;
            products *= 2;
        }
        Bytes = bytes;
        if (rows * 8 > Array.MaxLength || channels * (fast ? 8 : 16) > Array.MaxLength)
            throw new ImageLimitException(ImageLimitKind.BufferLength, "Lanczos row storage exceeds the managed limit.");
        owner.Cancellation.ThrowIfCancellationRequested(); owner.Reserve(Bytes);
        try
        {
            owner.Cancellation.ThrowIfCancellationRequested();
            RowIds = new int[slots];
            if (products != 0) Products = new uint[products];
            if (fast) { FastRows = new double[(int)rows]; FastSums = new double[(int)channels]; }
            else { ExactRows = new long[(int)rows]; ExactSums = new Int128[(int)channels]; }
        }
        catch
        {
            ExactRows = null; ExactSums = null; FastRows = null; FastSums = null; RowIds = []; Products = [];
            owner.Release(Bytes); throw;
        }
    }

    public void Dispose()
    {
        if (disposed) return; disposed = true;
        ExactRows = null; ExactSums = null; FastRows = null; FastSums = null; RowIds = []; Products = [];
        owner.Release(Bytes);
    }
}

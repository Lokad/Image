using System.Runtime.CompilerServices;

namespace Lokad.Image;

/// <summary>A thread-safe shared allowance for codec memory and cumulative work.</summary>
/// <remarks>Codec calls reserve their full per-operation MaxWorkingBytes and MaxWork
/// before executing. Completion returns unused work and temporary memory; decoded
/// pixels remain charged until their BudgetedImage is disposed. Charges are logical
/// admission, not GC heap or process RSS. Calls execute synchronously on one thread.</remarks>
public sealed class ImageBudget
{
    private readonly object gate = new();
    private long workingBytes, reservedWork, consumedWork;
    public long MaxWorkingBytes { get; }
    public long MaxWork { get; }
    public long RemainingWorkingBytes { get { lock (gate) return MaxWorkingBytes - workingBytes; } }
    public long RemainingWork { get { lock (gate) return MaxWork - consumedWork - reservedWork; } }
    public long ConsumedWork { get { lock (gate) return consumedWork; } }

    public ImageBudget(long maxWorkingBytes, long maxWork)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxWorkingBytes);
        ArgumentOutOfRangeException.ThrowIfNegative(maxWork);
        MaxWorkingBytes = maxWorkingBytes; MaxWork = maxWork;
    }

    /// <summary>Reserve caller-managed memory until disposal and charge a known amount of work.</summary>
    /// <remarks>This allows non-codec work and caller-owned storage to share the same
    /// budget. Work is cumulative and is not refunded by disposal. A reservation
    /// must outlive the storage it accounts for.</remarks>
    public IDisposable Reserve(long workingBytes, long work = 0, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(workingBytes);
        ArgumentOutOfRangeException.ThrowIfNegative(work);
        cancellationToken.ThrowIfCancellationRequested();
        var lease = new MemoryLease(workingBytes);
        lock (gate)
        {
            Admit(workingBytes, work);
            this.workingBytes += workingBytes; consumedWork += work;
            lease.Activate(this);
        }
        return lease;
    }

    public ImageInfo Identify(ReadOnlySpan<byte> input, ImageLimits? limits = null,
        CancellationToken cancellationToken = default) => ImageCodec.IdentifyWithBudget(input, this, limits, cancellationToken);
    public ImageInfo Identify(Stream input, ImageLimits? limits = null,
        CancellationToken cancellationToken = default) => ImageCodec.IdentifyWithBudget(input, this, limits, cancellationToken);
    public BudgetedImage Decode(ReadOnlySpan<byte> input, DecodeOptions? options = null,
        CancellationToken cancellationToken = default) => ImageCodec.DecodeWithBudget(input, this, options, cancellationToken);
    public BudgetedImage Decode(Stream input, DecodeOptions? options = null,
        CancellationToken cancellationToken = default) => ImageCodec.DecodeWithBudget(input, this, options, cancellationToken);
    public void EncodePng(ImageBuffer image, Stream output, PngEncodeOptions? options = null,
        CancellationToken cancellationToken = default) => ImageCodec.EncodePngWithBudget(image, output, this, options, cancellationToken);
    public void EncodeJpeg(ImageBuffer image, Stream output, JpegEncodeOptions? options = null,
        CancellationToken cancellationToken = default) => ImageCodec.EncodeJpegWithBudget(image, output, this, options, cancellationToken);
    public void EncodeWebp(ImageBuffer image, Stream output, WebpEncodeOptions? options = null,
        CancellationToken cancellationToken = default) => ImageCodec.EncodeWebpWithBudget(image, output, this, options, cancellationToken);

    internal OperationReservation Begin(ImageLimits limits)
    {
        lock (gate)
        {
            Admit(limits.MaxWorkingBytes, limits.MaxWork);
            workingBytes += limits.MaxWorkingBytes; reservedWork += limits.MaxWork;
        }
        return new OperationReservation(this, limits.MaxWorkingBytes, limits.MaxWork);
    }

    private void Admit(long bytes, long work)
    {
        if (bytes > MaxWorkingBytes - workingBytes)
            throw new ImageLimitException(ImageLimitKind.WorkingBytes, "Shared working memory budget exceeded.", ImageLimitScope.SharedBudget);
        if (work > MaxWork - consumedWork - reservedWork)
            throw new ImageLimitException(ImageLimitKind.Work, "Shared computational work budget exceeded.", ImageLimitScope.SharedBudget);
    }

    internal readonly struct OperationReservation(ImageBudget budget, long bytes, long work)
    {
        // The caller invokes this once, in finally. No work/format/cancellation
        // checks are added during cleanup or inside the original codec loops.
        internal void Complete(long usedWork, BudgetedImage? result = null)
        {
            var retained = result?.RetainedBytes ?? 0;
            lock (budget.gate)
            {
                budget.workingBytes -= bytes - retained;
                budget.reservedWork -= work; budget.consumedWork += usedWork;
                // Result construction and registration finish before publication.
                // An inactive lease cannot refund failed admission.
                result?.Admission.Activate(budget);
            }
        }
    }

    internal sealed class MemoryLease(long bytes) : IDisposable
    {
        private ImageBudget? budget;
        internal void Activate(ImageBudget owner) => budget = owner;
        ~MemoryLease() => Release();
        public void Dispose() { Release(); GC.SuppressFinalize(this); }
        private void Release()
        {
            var owner = Interlocked.Exchange(ref budget, null);
            if (owner is not null) lock (owner.gate) owner.workingBytes -= bytes;
        }
    }

    // Only budgeted images enter this weak table. The original ImageBuffer and
    // its disposal do not participate. An escaped ImageBuffer keeps its charge
    // even if the caller forgets the BudgetedImage owner without disposing it.
    private static class RetainedImages
    {
        internal static readonly ConditionalWeakTable<ImageBuffer, MemoryLease> Leases = new();
    }
    internal static void RetainImage(ImageBuffer image, MemoryLease lease) => RetainedImages.Leases.Add(image, lease);
    internal static void ReleaseImage(ImageBuffer image)
    {
        if (RetainedImages.Leases.TryGetValue(image, out var lease))
        {
            RetainedImages.Leases.Remove(image); lease.Dispose();
        }
    }
}

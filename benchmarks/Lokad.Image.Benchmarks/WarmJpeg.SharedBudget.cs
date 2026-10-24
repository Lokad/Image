using Lokad.Image;
using Lokad.Image.Tooling;

public partial class WarmJpeg
{
    private ImageBudget? sharedBudget;
    private IDisposable? sharedCallerMemory;
    private long sharedCallerBytes;
    private const long CallerMemoryLimit = 32 * 1024 * 1024;
    private TestMemoryBudget? callerMemoryBudget;
    private IMemoryReservation? callerSourceMemory;
    private string SharedBudgetBoundary => callerMemoryBudget is not null ?
        "Borrowed caller IMemoryBudget through ImageLimits.MemoryBudget; private coefficient storage, admission and refunds timed. " +
        "One finite root and resident-source reservation prepared outside timing. Source/root borrowing, terminal refusal/recovery, " +
        "cancellation, independent surviving packets and full temporary return checked. Native has no shared admission API; " +
        "managed accounting is included additional work. " : sharedBudget is null ? "" :
        "Shared ImageBudget EncodeJpeg, admission and actual-work/temporary-memory settlement timed. " +
        "One finite long-lived caller budget and resident-source reservation prepared outside timing; original per-operation limits unchanged. " +
        "Source stability, two independent packets, surviving output and full temporary-memory return checked. Native remains the same complete encode/owned-output reference. ";

    private void PrepareSharedBudget()
    {
        if (Environment.GetEnvironmentVariable("LOKAD_IMAGE_JPEG_MEMORY_BUDGET") is not null)
        {
            sharedCallerBytes = (long)source.Stride * source.Height;
            callerMemoryBudget = new TestMemoryBudget(CallerMemoryLimit);
            if (!callerMemoryBudget.TryReserve(sharedCallerBytes, out callerSourceMemory))
                throw new InvalidOperationException("Cannot reserve the caller's resident JPEG source.");
            options = options with { Limits = options.Limits with { MemoryBudget = callerMemoryBudget } };
            return;
        }
        var selection = Environment.GetEnvironmentVariable("LOKAD_IMAGE_SHARED_BUDGET");
        if (selection is null) return;
        if (selection != "1" || Case != WarmJpegCase.Encode420_8)
            throw new InvalidOperationException("Shared JPEG measurement requires the existing Encode420_8 case.");
        sharedCallerBytes = (long)source.Stride * source.Height;
        sharedBudget = new ImageBudget(2 * ImageLimits.Default.MaxWorkingBytes + sharedCallerBytes, 100_000_000_000_000);
        sharedCallerMemory = sharedBudget.Reserve(sharedCallerBytes);
    }

    private void ValidateSharedBudget()
    {
        if (sharedBudget is not null && (sharedBudget.RemainingWorkingBytes != sharedBudget.MaxWorkingBytes - sharedCallerBytes
            || sharedBudget.ConsumedWork <= 0))
            throw new InvalidOperationException("Shared JPEG completion did not settle actual work and temporary memory.");
        if (callerMemoryBudget is not null && (callerMemoryBudget.IsDisposed || callerMemoryBudget.Bytes != sharedCallerBytes
            || callerSourceMemory?.Bytes != sharedCallerBytes || callerMemoryBudget.Peak <= sharedCallerBytes))
            throw new InvalidOperationException("JPEG did not borrow the caller root or return temporary storage.");
    }

    private void ValidateCallerMemoryBudget()
    {
        if (callerMemoryBudget is not { } root) return;
        var refusals = root.Refusals; var growth = root.GrowthAttempts;
        root.RefuseAtGrowth = growth + 1;
        using var refused = new MemoryStream();
        try { EncodeManaged(refused); throw new InvalidOperationException("JPEG accepted refused shared admission."); }
        catch (ImageLimitException e) when (e.Limit == ImageLimitKind.WorkingBytes && e.Scope == ImageLimitScope.SharedBudget) { }
        finally { root.RefuseAtGrowth = 0; }
        if (root.Refusals != refusals + 1 || root.GrowthAttempts != growth + 1)
            throw new InvalidOperationException("JPEG retried refused shared admission.");
        ValidateSharedBudget();
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        using var canceled = new MemoryStream();
        try { ImageCodec.EncodeJpeg(source, canceled, options, cancellation.Token); throw new InvalidOperationException("JPEG ignored cancellation."); }
        catch (OperationCanceledException e) when (e.CancellationToken == cancellation.Token) { }
        if (canceled.Length != 0) throw new InvalidOperationException("Canceled JPEG wrote output.");
        ValidateSharedBudget();
        using var recovered = new MemoryStream(); EncodeManaged(recovered);
        if (!recovered.ToArray().AsSpan().SequenceEqual(encoded)) throw new InvalidOperationException("JPEG admission recovery changed its packet.");
        ValidateSharedBudget();
    }

    private void DisposeCallerMemoryBudget()
    {
        callerSourceMemory?.Dispose(); callerMemoryBudget?.Dispose();
    }
}

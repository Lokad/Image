using Lokad.Image;

public partial class WarmJpeg
{
    private ImageBudget? sharedBudget;
    private IDisposable? sharedCallerMemory;
    private long sharedCallerBytes;
    private string SharedBudgetBoundary => sharedBudget is null ? "" :
        "Shared ImageBudget EncodeJpeg, admission and actual-work/temporary-memory settlement timed. " +
        "One finite long-lived caller budget and resident-source reservation prepared outside timing; original per-operation limits unchanged. " +
        "Source stability, two independent packets, surviving output and full temporary-memory return checked. Native remains the same complete encode/owned-output reference. ";

    private void PrepareSharedBudget()
    {
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
        if (sharedBudget is null) return;
        if (sharedBudget.RemainingWorkingBytes != sharedBudget.MaxWorkingBytes - sharedCallerBytes
            || sharedBudget.ConsumedWork <= 0)
            throw new InvalidOperationException("Shared JPEG completion did not settle actual work and temporary memory.");
    }
}

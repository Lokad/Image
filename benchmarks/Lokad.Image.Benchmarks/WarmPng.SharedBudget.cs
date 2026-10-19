using Lokad.Image;

public partial class WarmPng
{
    private ImageBudget? sharedBudget;
    private IDisposable? sharedCallerMemory;
    private string SharedBudgetBoundary => sharedBudget is null ? "" :
        "Shared ImageBudget Decode, retained BudgetedImage ownership/disposal, admission and actual-work settlement timed. " +
        "One finite long-lived caller budget and resident-input reservation prepared outside timing; original per-operation limits unchanged. " +
        "Two live outputs, retained charges, surviving output and temporary-memory return checked. Native remains the same complete decode/owned-output reference. ";

    private void PrepareSharedBudget()
    {
        var selection = Environment.GetEnvironmentVariable("LOKAD_IMAGE_SHARED_BUDGET");
        if (selection is null) return;
        if (selection != "1" || Case != WarmPngCase.Decode8)
            throw new InvalidOperationException("Shared PNG measurement requires the existing Decode8 case.");
        sharedBudget = new ImageBudget(2 * ImageLimits.Default.MaxWorkingBytes + encoded.LongLength, 100_000_000_000_000);
        sharedCallerMemory = sharedBudget.Reserve(encoded.LongLength);
        ValidateSharedBudget();
    }

    private void ValidateSharedBudget()
    {
        if (sharedBudget is null) return;
        var available = sharedBudget.MaxWorkingBytes - encoded.LongLength;
        if (sharedBudget.RemainingWorkingBytes != available)
            throw new InvalidOperationException("Shared PNG temporary memory was not returned.");
        var work = sharedBudget.ConsumedWork;
        using var first = sharedBudget.Decode(encoded, decodeOptions, decodeCancellation);
        using var second = sharedBudget.Decode(encoded, decodeOptions, decodeCancellation);
        ValidatePixels(first.Image); ValidatePixels(second.Image);
        var retained = (long)first.Image.Stride * first.Image.Height;
        if (ReferenceEquals(first.Image, second.Image) || sharedBudget.RemainingWorkingBytes != available - 2L * retained)
            throw new InvalidOperationException("Shared PNG retained owners or admission differ.");
        first.Image.GetRow8(0)[0] ^= 255;
        ValidatePixels(second.Image);
        first.Dispose(); ValidatePixels(second.Image);
        if (sharedBudget.RemainingWorkingBytes != available - retained)
            throw new InvalidOperationException("Shared PNG surviving owner lost its admission.");
        second.Dispose();
        if (sharedBudget.RemainingWorkingBytes != available || sharedBudget.ConsumedWork <= work)
            throw new InvalidOperationException("Shared PNG completion did not settle actual work and temporary memory.");
    }
}

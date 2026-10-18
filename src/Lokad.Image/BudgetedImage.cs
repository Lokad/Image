namespace Lokad.Image;

/// <summary>Owns a decoded image and its retained shared-budget memory reservation.</summary>
/// <remarks>Dispose this owner after all uses of Image and its row spans. Disposing
/// Image directly does not release the reservation; disposing this owner does.
/// Other decoded outputs and copies have independent lifetimes.</remarks>
public sealed class BudgetedImage : IDisposable
{
    private ImageBuffer? image;
    public ImageBuffer Image => image ?? throw new ObjectDisposedException(nameof(BudgetedImage));
    internal long RetainedBytes { get; }
    internal ImageBudget.MemoryLease Admission { get; }

    private BudgetedImage(ImageBuffer image)
    {
        this.image = image; RetainedBytes = image.ByteLength;
        Admission = new ImageBudget.MemoryLease(RetainedBytes);
        ImageBudget.RetainImage(image, Admission);
    }

    internal static BudgetedImage Create(ImageBuffer image)
    {
        try { return new BudgetedImage(image); }
        catch { image.Dispose(); throw; }
    }

    public void Dispose()
    {
        var owned = Interlocked.Exchange(ref image, null);
        if (owned is not null)
        {
            try { owned.Dispose(); }
            finally { ImageBudget.ReleaseImage(owned); }
        }
    }
}

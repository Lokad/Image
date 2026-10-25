using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

internal static class WebpAdmissionControl
{
    // A successful public operation supplies its packet and actual admission
    // peak. Borrowed source bytes also count toward the independent local limit.
    internal static (byte[] Packet, long WorkingBytes) Reference(ImageBuffer source, int effort = 4, bool lossless = true, int quality = 85,
        CancellationToken cancellationToken = default)
    {
        using var root = new TestMemoryBudget(ImageLimits.Default.MaxWorkingBytes);
        using var output = new MemoryStream();
        ImageCodec.EncodeWebp(source, output, new() { Effort = effort, Lossless = lossless, Quality = quality, Limits = new() { MemoryBudget = root } }, cancellationToken);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        return (output.ToArray(), (long)source.Stride * source.Height + root.Peak);
    }
}

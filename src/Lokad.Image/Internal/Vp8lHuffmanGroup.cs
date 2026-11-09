namespace Lokad.Image.Internal;

// The five canonical alphabets shared by auxiliary and main-image emission.
// Populations are borrowed only during construction; codes own their storage.
internal sealed class Vp8lHuffmanGroup : IDisposable
{
    private readonly OperationContext context;
    private readonly Vp8lHuffmanCode?[] codes = new Vp8lHuffmanCode?[5];
    private bool disposed;
    private Vp8lHuffmanGroup(OperationContext context) => this.context = context;

    internal static Vp8lHuffmanGroup Build(ReadOnlySpan<int> counts, Vp8lHistogramLayout layout, OperationContext context,
        out long dataBits, Vp8lHuffmanWorkspace? workspace = null)
    {
        if (counts.Length != layout.Total) throw new ArgumentException("Expected one complete histogram group.", nameof(counts));
        var result = new Vp8lHuffmanGroup(context);
        try
        {
            using var owned = workspace is null ? new Vp8lHuffmanWorkspace(layout.GreenSize, context) : null;
            workspace ??= owned!;
            result.codes[0] = Vp8lHuffmanCode.Build(counts[..layout.GreenSize], context, out var green, workspace: workspace);
            result.codes[1] = Vp8lHuffmanCode.Build(counts.Slice(layout.Red, 256), context, out var red, workspace: workspace);
            result.codes[2] = Vp8lHuffmanCode.Build(counts.Slice(layout.Blue, 256), context, out var blue, workspace: workspace);
            result.codes[3] = Vp8lHuffmanCode.Build(counts.Slice(layout.Alpha, 256), context, out var alpha, workspace: workspace);
            result.codes[4] = Vp8lHuffmanCode.Build(counts.Slice(layout.Distance, 40), context, out var distance, workspace: workspace);
            // RFC 9649 sections 3.6.2.2 and 3.7.2: canonical symbol bits plus
            // the fixed extras of length/distance prefixes. Populations die
            // after preparation; no second length scan or retained code field.
            dataBits = green + red + blue + alpha + distance;
            for (var i = 4; i < 40; i++)
            {
                context.Cancellation.ThrowIfCancellationRequested(); var extra = Vp8lSymbols.Extra(i);
                if (i < 24) dataBits += (long)counts[256 + i] * extra;
                dataBits += (long)counts[layout.Distance + i] * extra;
            }
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    internal void WriteTrees(WebpBitWriter writer)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        foreach (var code in codes) Vp8lReferenceHuffman.WriteTree(writer, code!.Lengths, context);
    }

    internal void WriteToken(WebpBitWriter writer, Vp8lToken token, int width)
    {
        ObjectDisposedException.ThrowIf(disposed, this); context.Cancellation.ThrowIfCancellationRequested();
        if (token.Code > 0)
        {
            Vp8lReferenceHuffman.WritePrefix(writer, codes[0]!, token.Code, 256);
            Vp8lReferenceHuffman.WritePrefix(writer, codes[4]!, Vp8lSymbols.Plane((int)token.Value, width), 0);
        }
        else if (token.Code < 0) codes[0]!.Write(writer, 280 + ~token.Code);
        else
        {
            var pixel = token.Value;
            codes[0]!.Write(writer, (int)(pixel >> 8 & 255)); codes[1]!.Write(writer, (int)(pixel >> 16 & 255));
            codes[2]!.Write(writer, (int)(pixel & 255)); codes[3]!.Write(writer, (int)(pixel >> 24));
        }
    }

    public void Dispose()
    {
        if (disposed) return; disposed = true;
        for (var i = codes.Length - 1; i >= 0; i--) codes[i]?.Dispose();
    }
}

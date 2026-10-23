namespace Lokad.Image.Internal;

// The five canonical alphabets shared by auxiliary and main-image emission.
// Populations are borrowed only during construction; codes own their storage.
internal sealed class Vp8lHuffmanGroup : IDisposable
{
    private readonly OperationContext context;
    private readonly Vp8lHuffmanCode?[] codes = new Vp8lHuffmanCode?[5];
    private bool disposed;
    private Vp8lHuffmanGroup(OperationContext context) => this.context = context;

    internal static Vp8lHuffmanGroup Build(ReadOnlySpan<int> counts, Vp8lHistogramLayout layout, OperationContext context)
    {
        if (counts.Length != layout.Total) throw new ArgumentException("Expected one complete histogram group.", nameof(counts));
        var result = new Vp8lHuffmanGroup(context);
        try
        {
            result.codes[0] = Vp8lHuffmanCode.Build(counts[..layout.GreenSize], context);
            result.codes[1] = Vp8lHuffmanCode.Build(counts.Slice(layout.Red, 256), context);
            result.codes[2] = Vp8lHuffmanCode.Build(counts.Slice(layout.Blue, 256), context);
            result.codes[3] = Vp8lHuffmanCode.Build(counts.Slice(layout.Alpha, 256), context);
            result.codes[4] = Vp8lHuffmanCode.Build(counts.Slice(layout.Distance, 40), context);
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
        ObjectDisposedException.ThrowIf(disposed, this); context.Work(1);
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

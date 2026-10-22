namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    // A flat full-symbol model or operation-local sparse spectral-band models.
    // Sparse offsets carry the base in ten bits and the run capacity above it.
    internal readonly ref struct JointAcPrices
    {
        private readonly ReadOnlySpan<ushort> values, offsets;
        internal JointAcPrices(ReadOnlySpan<ushort> values, ReadOnlySpan<ushort> offsets = default)
        { this.values = values; this.offsets = offsets; }
        public static implicit operator JointAcPrices(ushort[] values) => new(values);
        public static implicit operator JointAcPrices(ReadOnlySpan<ushort> values) => new(values);
        internal bool IsEmpty => values.IsEmpty;
        internal int Families => offsets.IsEmpty
            ? values.Length is 256 or 512 ? values.Length / 256 : 0
            : offsets.Length is 64 or 128 ? offsets.Length / 64 : 0;
        internal JointAcPrices ForComponent(int component) => offsets.IsEmpty
            ? new(values.Slice(component == 0 ? 0 : 256, 256))
            : new(values, offsets.Slice(component == 0 ? 0 : 64, 64));
        internal int At(int symbol, int scan)
        {
            if (offsets.IsEmpty) return values[symbol];
            var descriptor = offsets[scan]; var index = SourcePriceSlot(symbol);
            if (index >= 2 + 10 * (descriptor >> 10))
                throw new InvalidOperationException("AC run exceeds its source-price band.");
            return values[(descriptor & 1023) + index];
        }
    }

    private static int SourcePriceSlot(int symbol)
        => symbol == 0 ? 0 : symbol == 0xF0 ? 1 : 1 + 10 * (symbol >> 4) + (symbol & 15);
}

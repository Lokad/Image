namespace Lokad.Image.Tooling;

// Original small RFC 1951 bitstream constructions. Explicit prefix paths provide
// an independent producer for the production framing walk and BCL inflater.
internal static class AuthoredDeflate
{
    internal sealed record Token(int Symbol, int LengthExtra = 0, int LengthBits = 0,
        int DistanceSymbol = -1, int DistanceExtra = 0, int DistanceBits = 0);

    public static byte[] Fixed(IEnumerable<Token> tokens, byte[] expanded, bool final = true, bool endCode = true, int window = 7)
    {
        var bits = new Writer(); bits.Number(final ? 1 : 0, 1); bits.Number(1, 2);
        foreach (var token in tokens)
        {
            Symbol(token.Symbol); bits.Number(token.LengthExtra, token.LengthBits);
            if (token.DistanceSymbol >= 0) { bits.Code(token.DistanceSymbol, 5); bits.Number(token.DistanceExtra, token.DistanceBits); }
        }
        if (endCode) Symbol(256);
        return Envelope(bits.Bytes(), expanded, window);

        void Symbol(int symbol)
        {
            if (symbol < 144) bits.Code(48 + symbol, 8);
            else if (symbol < 256) bits.Code(400 + symbol - 144, 9);
            else if (symbol < 280) bits.Code(symbol - 256, 7);
            else bits.Code(192 + symbol - 280, 8);
        }
    }

    public static byte[] DynamicLiteral(byte[] expanded, bool final = true, int repeat = 18)
    {
        if (expanded.Any(value => value is not (0 or 17))) throw new ArgumentException("This original case uses only literals 0 and 17.");
        if (repeat is not (16 or 17 or 18)) throw new ArgumentOutOfRangeException(nameof(repeat));
        var bits = new Writer(); Header(bits, final, 18);
        // Four equal two-bit code-length codes: 0=00, 1=01, 2=10, repeat=11.
        ReadOnlySpan<int> order = [16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1];
        foreach (var symbol in order) bits.Number(symbol is 0 or 1 or 2 || symbol == repeat ? 2 : 0, 3);
        bits.Code(2, 2); Zeroes(16); bits.Code(2, 2); Zeroes(238); bits.Code(1, 2); bits.Code(0, 2);
        // Literal prefixes: end=0, 0=10, 17=11; no distance codes are needed.
        foreach (var value in expanded) bits.Code(value == 0 ? 2 : 3, 2);
        bits.Code(0, 1); return Envelope(bits.Bytes(), expanded);

        void Zeroes(int count)
        {
            if (repeat == 16) { bits.Code(0, 2); count--; } // Repeat the previously emitted zero.
            var minimum = repeat == 18 ? 11 : 3; var maximum = repeat == 18 ? 138 : repeat == 17 ? 10 : 6;
            while (count >= minimum)
            {
                var length = Math.Min(count, maximum); bits.Code(3, 2); bits.Number(length - minimum, repeat == 18 ? 7 : repeat == 17 ? 3 : 2); count -= length;
            }
            while (count-- > 0) bits.Code(0, 2);
        }
    }

    public static byte[] DynamicCrossAlphabet(byte[] expanded)
    {
        var bits = new Writer(); bits.Number(1, 1); bits.Number(2, 2); bits.Number(0, 5); bits.Number(3, 5); bits.Number(14, 4);
        ReadOnlySpan<int> order = [16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1];
        foreach (var symbol in order) bits.Number(symbol is 0 or 2 or 16 or 18 ? 2 : 0, 3);
        // CL prefixes 0=00, 2=01, 16=10, 18=11. Four literal prefixes have length 2.
        bits.Code(1, 2); Zeroes(16); bits.Code(1, 2); Zeroes(237); bits.Code(1, 2);
        // Repeat length 2 five times: EOB plus all four distance codes.
        bits.Code(2, 2); bits.Number(2, 2);
        foreach (var value in expanded) bits.Code(value switch { 0 => 0, 17 => 1, 255 => 2, _ => throw new ArgumentException("Unsupported original literal.") }, 2);
        bits.Code(3, 2); return Envelope(bits.Bytes(), expanded);
        void Zeroes(int count) { while (count > 0) { var length = Math.Min(count, 138); bits.Code(3, 2); bits.Number(length - 11, 7); count -= length; } }
    }

    public static byte[] DynamicDeep(byte[] expanded)
    {
        var bits = new Writer(); Header(bits, true, 19);
        ReadOnlySpan<int> order = [16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15];
        foreach (var symbol in order) bits.Number(symbol < 16 ? 4 : 0, 3);
        // Complete comb: one leaf at depths 1..14, two leaves at depth 15.
        for (var symbol = 0; symbol < 257; symbol++)
            bits.Code(symbol is 0 or 17 ? 15 : symbol == 256 ? 1 : symbol is >= 1 and <= 13 ? symbol + 1 : 0, 4);
        bits.Code(0, 4); // No distances.
        foreach (var value in expanded) bits.Code(value == 0 ? 32766 : value == 17 ? 32767 : throw new ArgumentException("Unsupported original literal."), 15);
        bits.Code(0, 1); return Envelope(bits.Bytes(), expanded);
    }

    public static IEnumerable<(string Name, byte[] Zlib)> InvalidDynamic()
    {
        foreach (var name in new[] { "oversubscribed-code-lengths", "first-repeat-16", "repeat-17-overflow", "repeat-18-overflow", "missing-end-code", "literal-alphabet-overflow" })
        {
            var bits = new Writer();
            if (name == "literal-alphabet-overflow") { bits.Number(1, 1); bits.Number(2, 2); bits.Number(31, 5); bits.Number(0, 5); bits.Number(0, 4); }
            else
            {
                var count = name == "missing-end-code" ? 18 : 4; Header(bits, true, count);
                ReadOnlySpan<int> order = [16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1];
                foreach (var symbol in order[..count])
                {
                    var length = name switch
                    {
                        "oversubscribed-code-lengths" => symbol is 16 or 17 or 18 ? 1 : 0,
                        "first-repeat-16" => symbol is 0 or 16 ? 1 : 0,
                        "repeat-17-overflow" => symbol is 0 or 17 ? 1 : 0,
                        "repeat-18-overflow" => symbol is 0 or 18 ? 1 : 0,
                        _ => symbol is 0 or 1 ? 1 : 0
                    };
                    bits.Number(length, 3);
                }
                if (name == "first-repeat-16") { bits.Code(1, 1); bits.Number(0, 2); }
                else if (name == "repeat-17-overflow") for (var i = 0; i < 26; i++) { bits.Code(1, 1); bits.Number(7, 3); }
                else if (name == "repeat-18-overflow") for (var i = 0; i < 2; i++) { bits.Code(1, 1); bits.Number(127, 7); }
                else if (name == "missing-end-code") for (var i = 0; i < 258; i++) bits.Code(i is 0 or 17 ? 1 : 0, 1);
            }
            yield return (name, Envelope(bits.Bytes(), [0, 17]));
        }
    }

    private static void Header(Writer bits, bool final, int codeCount)
    {
        bits.Number(final ? 1 : 0, 1); bits.Number(2, 2); bits.Number(0, 5); bits.Number(0, 5); bits.Number(codeCount - 4, 4);
    }

    private static byte[] Envelope(byte[] deflate, byte[] expanded, int window = 7)
    {
        var cmf = (byte)(8 | window << 4); var check = (31 - (cmf << 8) % 31) % 31;
        return [cmf, (byte)check, .. deflate, .. AuthoredPng.StoredZlib(expanded)[^4..]];
    }

    private sealed class Writer
    {
        private readonly List<byte> bytes = [];
        private int used;
        public void Number(int value, int length) { for (var i = 0; i < length; i++) Bit((value >> i) & 1); }
        public void Code(int value, int length) { for (var i = length - 1; i >= 0; i--) Bit((value >> i) & 1); }
        private void Bit(int value)
        {
            if (used % 8 == 0) bytes.Add(0);
            bytes[^1] |= (byte)(value << (used++ % 8));
        }
        public byte[] Bytes() => bytes.ToArray();
    }
}

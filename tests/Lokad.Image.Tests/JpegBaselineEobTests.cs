using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegBaselineEobTests
{
    [Fact]
    public void BufferedEndKeepsFallbackPositionPendingBitsAndUntouchedStorage()
    {
        for (var codeLength = 1; codeLength <= 8; codeLength++)
        {
            var counts = new byte[16]; counts[codeLength - 1] = 1;
            using var tables = new OperationContext(ImageLimits.Default, default);
            var table = new JpegHuffman(counts, [0], tables);
            for (var phase = 0; phase < 8; phase++)
            for (var variant = 0; variant < 3; variant++)
            {
                var text = new string('1', phase) + new string('0', codeLength) + "10110101";
                var padding = (8 - text.Length % 8) % 8; text += new string('1', padding);
                var bytes = new List<byte>();
                for (var at = 0; at < text.Length; at += 8) Add(Convert.ToByte(text.Substring(at, 8), 2));
                Add(0x21); Add(variant == 0 ? (byte)0x6a : (byte)255); Add(0x38); Add(0x54);
                bytes.AddRange([255, 0xd0, 0xa0]); var full = bytes.ToArray();
                for (var length = 0; length <= full.Length; length++)
                foreach (var start in new[] { 1, 63 })
                {
                    var input = full.AsSpan(0, length).ToArray();
                    Assert.Equal(Run(false, input, start), Run(true, input, start));
                }
                if (variant != 2)
                {
                    var expected = Run(false, full, 1); var actual = Run(true, full, 1);
                    Assert.Equal(expected, actual); Assert.Null(actual.Error); Assert.Equal(64, actual.Coefficient);
                    Assert.Equal(5UL, actual.Tail & 7); Assert.True(actual.Untouched);
                }

                void Add(byte value) { bytes.Add(value); if (value == 255 && variant != 2) bytes.Add(0); }

                (Type? Error, string? Message, int Position, int Coefficient, ulong Tail, bool Untouched)
                    Run(bool buffered, byte[] input, int start)
                {
                    using var context = new OperationContext(ImageLimits.Default, default);
var reader = new JpegEntropyReader(input, 0, context);                     Span<int> storage = stackalloc int[66]; storage.Fill(12345); var coefficient = start;
                    Exception? error = null; ulong tail = 0;
                    try
                    {
                        if (phase != 0) reader.ReadBits(phase);
                        if (buffered) table.ReadPlainBaselineAc(ref reader, storage.Slice(1, 64), ref coefficient);
                        if (coefficient != 64) { Assert.Equal(0, table.Read(ref reader)); coefficient = 64; }
                        tail = (uint)reader.ReadBits(3); tail = (tail << 5) | (uint)reader.ReadBits(5);
                        if (padding != 0) reader.ReadBits(padding);
                        tail = (tail << 16) | (uint)reader.ReadBits(16);
                        tail = (tail << 16) | (uint)reader.ReadBits(16);
                        reader.Restart(0); tail = (tail << 3) | (uint)reader.ReadBits(3);
                    }
                    catch (Exception failure) when (failure is ImageDataException or ImageLimitException) { error = failure; }
                    return (error?.GetType(), error?.Message, reader.Position, coefficient,
                        tail, storage.IndexOfAnyExcept(12345) < 0);
                }
            }
        }
    }

    [Fact]
    public void ZeroRunAndInvalidTokensKeepTheirOriginalFallbackWithoutAdmission()
    {
        Span<int> storage = stackalloc int[64];
        foreach (var token in new byte[] { 0xf0, 0x10, 0x0b })
        {
            using var tables = new OperationContext(ImageLimits.Default, default);
            var table = new JpegHuffman([1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], [token], tables);
            for (var phase = 0; phase < 8; phase++)
            {
                using var context = new OperationContext(ImageLimits.Default, default);
                var reader = new JpegEntropyReader(new byte[] { 0, 0, 0, 0, 0, 255, 0xd0 }, 0, context);
                if (phase != 0) reader.ReadBits(phase);
var position = reader.Position;                 storage.Fill(12345); var coefficient = 1;
                table.ReadPlainBaselineAc(ref reader, storage, ref coefficient);                 Assert.Equal(1, coefficient); Assert.Equal(position, reader.Position);
                Assert.True(storage.IndexOfAnyExcept(12345) < 0); Assert.Equal(token, table.Read(ref reader));
                Assert.Equal(0, reader.ReadBits(3));
            }
        }
    }
}

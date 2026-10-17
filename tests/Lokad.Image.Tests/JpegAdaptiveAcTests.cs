using System.Numerics;
using System.Reflection;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegAdaptiveAcTests
{
    private delegate int Append(ReadOnlySpan<int> block, long[] frequencies, uint[] events, int count, int table);
    private static readonly Append Actual = typeof(JpegEncoder)
        .GetMethod("AppendAdaptiveAc", BindingFlags.Static | BindingFlags.NonPublic)!.CreateDelegate<Append>();

    [Fact]
    public void EveryScanPositionAndMagnitudeKeepsScalarEventsAndHistogram()
    {
        for (var scan = 0; scan < 64; scan++)
        foreach (var value in new[] { -1023, -512, -17, -1, 0, 1, 17, 512, 1023 })
        {
            var block = new int[64]; block[JpegTransform.Zigzag[scan]] = value;
            Compare(block, 2); Compare(block, 3);
        }
    }

    [Fact]
    public void DenseSparseAndInvalidBlocksKeepPriorEventsAndFailureOrder()
    {
        for (var pattern = 0; pattern < 7; pattern++)
        {
            var block = new int[64];
            for (var scan = 0; scan < 64; scan++)
                block[JpegTransform.Zigzag[scan]] = pattern switch
                {
                    0 => 0,
                    1 => scan % 17 == 0 ? -1023 : 0,
                    2 => scan % 2 == 0 ? 1023 : -1023,
                    3 => scan % 3 == 0 ? scan * 11 + 1 : 0,
                    4 => scan == 63 ? 1024 : 0,
                    5 => scan == 63 ? int.MinValue : 0,
                    _ => scan is 1 or 18 ? 7 : scan == 63 ? -2048 : 0
                };
            Compare(block, 2); Compare(block, 3);
        }
    }

    private static void Compare(int[] block, int table)
    {
        var original = block.ToArray();
        var expectedFrequencies = Enumerable.Range(0, 256).Select(n => (long)n * 3).ToArray();
        var actualFrequencies = expectedFrequencies.ToArray();
        var expectedEvents = Enumerable.Repeat(0xdeadbeefu, 80).ToArray(); var actualEvents = expectedEvents.ToArray();
        var expectedCount = -1; var actualCount = -1;
        var expectedError = Record.Exception(() => expectedCount = Scalar(block, expectedFrequencies, expectedEvents, 3, table));
        var actualError = Record.Exception(() => actualCount = Actual(block, actualFrequencies, actualEvents, 3, table));
        Assert.Equal(expectedError?.GetType(), actualError?.GetType()); Assert.Equal(expectedError?.Message, actualError?.Message);
        Assert.Equal(expectedCount, actualCount); Assert.Equal(expectedFrequencies, actualFrequencies);
        Assert.Equal(expectedEvents, actualEvents); Assert.Equal(original, block);
    }

    // Straightforward T.81 zero-run traversal, independent of occupancy masks.
    private static int Scalar(int[] block, long[] frequencies, uint[] events, int count, int table)
    {
        var zeros = 0;
        for (var scan = 1; scan < 64; scan++)
        {
            var value = block[JpegTransform.Zigzag[scan]];
            if (value == 0) { zeros++; continue; }
            while (zeros >= 16) { frequencies[240]++; events[count++] = Pack(240, 0, 0); zeros -= 16; }
            var category = 32 - BitOperations.LeadingZeroCount((uint)Math.Abs(value));
            if (category > 10) throw new ImageDataException("JPEG AC transform exceeds its coding range.");
            var symbol = zeros * 16 + category; frequencies[symbol]++;
            events[count++] = Pack(symbol, value, category); zeros = 0;
        }
        if (zeros != 0) { frequencies[0]++; events[count++] = Pack(0, 0, 0); }
        return count;

        uint Pack(int symbol, int value, int width)
            => (uint)table << 30 | (uint)width << 26 | (uint)symbol << 16
                | (uint)(value < 0 ? value + (1 << width) - 1 : value);
    }
}

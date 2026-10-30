using System.Buffers.Binary;
using System.Text.Json;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8ValidationTests
{
    [Theory]
    [InlineData("vp8-manifest.json", 110)]
    [InlineData("vp8-authored-manifest.json", 54)]
    public void CompleteCodingValidationAcceptsEveryExistingReconstructionFixture(string file, int count)
    {
        var directory = Fixtures();
        using var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(directory, file)));
        var fixtures = manifest.RootElement.GetProperty("fixtures"); Assert.Equal(count, fixtures.GetArrayLength());
        foreach (var fixture in fixtures.EnumerateArray())
        {
            var encoded = File.ReadAllBytes(Path.Combine(directory, fixture.GetProperty("name").GetString()! + ".webp"));
            var payload = Payload(encoded); var unchanged = payload.ToArray();
            using var context = new OperationContext(ImageLimits.Default, default);
            new Vp8Decoder(payload, context).ValidatePixels();
            Assert.Equal(unchanged, payload);
        }
    }

    [Fact]
    public void PayloadTruncationsAndBoundedMutationsAgreeWithActualReconstruction()
    {
        var directory = Fixtures();
        using var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(directory, "vp8-authored-manifest.json")));
        var fixtures = manifest.RootElement.GetProperty("fixtures").EnumerateArray().ToArray();
        var lateRejections = 0;
        foreach (var index in new[] { 0, 10, 20, 30, 40, 53 })
        {
            var data = Payload(File.ReadAllBytes(Path.Combine(directory, fixtures[index].GetProperty("name").GetString()! + ".webp")));
            Assert.Equal("ok", Outcome(data, false)); Assert.Equal("ok", Outcome(data, true));
            var headerEnd = 10 + ((data[0] | data[1] << 8 | data[2] << 16) >> 5);
            var sizes = new[] { 0, 1, 9, headerEnd - 1, headerEnd, headerEnd + 1, data.Length / 2, data.Length - 8, data.Length - 2, data.Length - 1 };
            foreach (var size in sizes.Where(n => n >= 0 && n < data.Length).Distinct())
            {
                var truncated = data[..size]; var decode = Outcome(truncated, false);
                Assert.Equal(decode, Outcome(truncated, true));
                if (size > headerEnd && decode == "data") lateRejections++;
            }
            foreach (var at in new[] { 0, 3, 6, 9, 10, headerEnd - 1, headerEnd, data.Length / 2, data.Length - 1 }.Distinct())
            {
                if ((uint)at >= data.Length) continue;
                var changed = data.ToArray(); changed[at] ^= 0x41;
                Assert.Equal(Outcome(changed, false), Outcome(changed, true));
            }
        }
        Assert.True(lateRejections > 0, "Validation must inspect coded tokens beyond the complete header partition.");
    }

    private static string Outcome(byte[] data, bool validate)
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        try
        {
            var decoder = new Vp8Decoder(data, context);
            if (validate) decoder.ValidatePixels(); else decoder.DecodePlanes();
            return "ok";
        }
        catch (ImageDataException) { return "data"; }
        catch (UnsupportedImageException error) { return "unsupported:" + error.Feature; }
        catch (ImageLimitException error) { return "limit:" + error.Limit; }
    }

    private static byte[] Payload(byte[] data)
    {
        for (var at = 12; at < data.Length;)
        {
            var count = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(at + 4));
            if (data.AsSpan(at, 4).SequenceEqual("VP8 "u8)) return data.AsSpan(at + 8, count).ToArray();
            at += 8 + count + (count & 1);
        }
        throw new InvalidOperationException("Authored fixture has no VP8 payload.");
    }

    private static string Fixtures()
    {
        for (var p = new DirectoryInfo(AppContext.BaseDirectory); p is not null; p = p.Parent)
        {
            var path = Path.Combine(p.FullName, "tests", "Fixtures");
            if (File.Exists(Path.Combine(path, "vp8-manifest.json"))) return path;
        }
        throw new InvalidOperationException("Original VP8 fixture directory is missing.");
    }
}

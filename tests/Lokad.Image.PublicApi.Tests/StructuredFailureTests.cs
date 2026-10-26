using System.Buffers.Binary;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class StructuredFailureTests
{
    [Theory]
    [InlineData(ImageLimitKind.Dimension)]
    [InlineData(ImageLimitKind.PixelCount)]
    [InlineData(ImageLimitKind.WorkingBytes)]
    [InlineData(ImageLimitKind.BufferLength)]
    public void PixelAdmissionIdentifiesTheLimitBeforeAllocating(ImageLimitKind expected)
    {
        var limits = expected switch
        {
            ImageLimitKind.Dimension => new ImageLimits { MaxDimension = 1, MaxPixels = 1 },
            ImageLimitKind.PixelCount => new ImageLimits { MaxPixels = 1 },
            ImageLimitKind.WorkingBytes => new ImageLimits { MaxWorkingBytes = 1 },
            _ => new ImageLimits { MaxDimension = 32768, MaxPixels = long.MaxValue, MaxWorkingBytes = long.MaxValue }
        };
        var size = expected == ImageLimitKind.BufferLength ? 32768 : 2;
        var error = Assert.Throws<ImageLimitException>(() => ImageBuffer.Create(size, size, 8, limits));
        Assert.Equal(expected, error.Limit);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InputAndLimitsPreserveSourceAndStreamOwnership(bool streamed)
    {
        var data = AuthoredJpeg.GrayscaleDcGrid(8, 8, false);
        var original = data.ToArray();
        foreach (var (limits, expected) in new[]
        {
            (new ImageLimits { MaxInputBytes = data.Length - 1 }, ImageLimitKind.InputBytes),
            (new ImageLimits { MaxContainerItems = 1 }, ImageLimitKind.ContainerItems),
            (new ImageLimits { MaxWorkingBytes = 8192 }, ImageLimitKind.WorkingBytes)
        })
        {
            // The last bound specifically exercises stream-buffer growth.
            if (!streamed && expected == ImageLimitKind.WorkingBytes) continue;
            using var input = new MemoryStream(data);
            var error = Assert.Throws<ImageLimitException>(() =>
            {
                if (streamed) ImageCodec.Identify(input, limits);
                else ImageCodec.Identify(data, limits);
            });
            Assert.Equal(expected, error.Limit);
            Assert.True(input.CanRead);
        }
        Assert.Equal(original, data);
    }

    [Theory]
    [InlineData(MetadataProfile.Png8)]
    [InlineData(MetadataProfile.JpegBaseline420)]
    [InlineData(MetadataProfile.WebpLosslessAlpha)]
    public void MetadataAdmissionReportsItsOwnCategory(MetadataProfile profile)
    {
        var fixture = OriginalExif.Create(profile, 9, 7);
        var data = OriginalExif.Attach(fixture, ImageOrientation.Rotate90, false, directoryEntries: 64);
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.Identify(data,
            new ImageLimits { MaxMetadataBytes = 1 }));
        Assert.Equal(ImageLimitKind.MetadataBytes, error.Limit);
        var entries = Assert.Throws<ImageLimitException>(() => ImageCodec.Identify(data,
            new ImageLimits { MaxContainerItems = 63 }));
        Assert.Equal(ImageLimitKind.ContainerItems, entries.Limit);
    }

    [Theory]
    [InlineData(ImageFormat.Png)]
    [InlineData(ImageFormat.Jpeg)]
    [InlineData(ImageFormat.WebP)]
    public void OutputLimitsLeaveTheCallerStreamAndPixelsAlive(ImageFormat format)
    {
        using var image = ImageBuffer.Create(2, 1, 8);
        image.GetRow8(0).Fill(255);
        using var output = new MemoryStream();
        var limits = new ImageLimits { MaxOutputBytes = 1 };
        var error = Assert.Throws<ImageLimitException>(() =>
        {
            switch (format)
            {
                case ImageFormat.Png: ImageCodec.EncodePng(image, output, new() { Limits = limits }); break;
                case ImageFormat.Jpeg: ImageCodec.EncodeJpeg(image, output, new() { Limits = limits }); break;
                case ImageFormat.WebP: ImageCodec.EncodeWebp(image, output, new() { Limits = limits }); break;
            }
        });
        Assert.Equal(ImageLimitKind.OutputBytes, error.Limit);
        Assert.True(output.CanWrite);
        Assert.All(image.GetRow8(0).ToArray(), sample => Assert.Equal(255, sample));
    }

    [Theory]
    [InlineData(0xC1, 8, 1, UnsupportedImageFeature.CodingProfile)]
    [InlineData(0xC0, 12, 1, UnsupportedImageFeature.BitDepth)]
    [InlineData(0xC0, 8, 2, UnsupportedImageFeature.ComponentLayout)]
    public void JpegRejectionsExposeFeatureAndFormat(byte marker, byte precision, byte components,
        UnsupportedImageFeature feature)
    {
        // Original T.81 frame header: a distinct unsupported feature is rejected
        // before the missing scan/EOI would be considered. No entropy is needed.
        var data = new byte[12 + 3 * components];
        data[0] = data[2] = 255; data[1] = 216; data[3] = marker;
        BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(4), (ushort)(8 + 3 * components));
        data[6] = precision; data[8] = data[10] = 1; data[11] = components;
        for (var i = 0; i < components; i++) { data[12 + i * 3] = (byte)(i + 1); data[13 + i * 3] = 0x11; }
        foreach (var decode in new[] { false, true })
        {
            var error = Assert.Throws<UnsupportedImageException>(() =>
            {
                if (decode) { using var result = ImageCodec.Decode(data); }
                else ImageCodec.Identify(data);
            });
            Assert.Equal(feature, error.Feature);
            Assert.Equal(ImageFormat.Jpeg, error.Format);
        }
    }

    [Fact]
    public void UnknownFormatAndMessageOnlyConstructorsHaveExplicitUnspecifiedValues()
    {
        var error = Assert.Throws<UnsupportedImageException>(() => ImageCodec.Identify("GIF89a"u8));
        Assert.Equal(UnsupportedImageFeature.Format, error.Feature); Assert.Null(error.Format);
        var messageLimit = new ImageLimitException("caller message");
        Assert.Equal(ImageLimitKind.Unspecified, messageLimit.Limit); Assert.Equal("caller message", messageLimit.Message);
        var messageFeature = new UnsupportedImageException("caller feature");
        Assert.Equal(UnsupportedImageFeature.Unspecified, messageFeature.Feature);
        Assert.Null(messageFeature.Format); Assert.Equal("caller feature", messageFeature.Message);
    }

    [Fact]
    public void CancellationStillPrecedesFormatAndInputLimitFailures()
    {
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() => ImageCodec.Identify("GIF89a"u8,
            new ImageLimits { MaxInputBytes = 1 }, cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
    }
}

using System.Buffers.Binary;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class ExifAdmissionTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void TruncatedDirectoryRemainsMalformedEvenWhenItsDeclaredCountExceedsTheLimit(bool bigEndian)
    {
        var tiff = new byte[26]; tiff[0] = tiff[1] = bigEndian ? (byte)'M' : (byte)'I';
        if (bigEndian)
        {
            BinaryPrimitives.WriteUInt16BigEndian(tiff.AsSpan(2), 42);
            BinaryPrimitives.WriteUInt32BigEndian(tiff.AsSpan(4), 8);
            BinaryPrimitives.WriteUInt16BigEndian(tiff.AsSpan(8), 64);
        }
        else
        {
            BinaryPrimitives.WriteUInt16LittleEndian(tiff.AsSpan(2), 42);
            BinaryPrimitives.WriteUInt32LittleEndian(tiff.AsSpan(4), 8);
            BinaryPrimitives.WriteUInt16LittleEndian(tiff.AsSpan(8), 64);
        }
        var context = new OperationContext(new ImageLimits { MaxContainerItems = 63 }, default);
        Assert.Throws<ImageDataException>(() => ExifOrientation.Read(tiff, context));
    }
}

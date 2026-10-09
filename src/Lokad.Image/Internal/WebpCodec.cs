using System.Buffers.Binary;

namespace Lokad.Image.Internal;

internal static class WebpCodec
{
    public static ImageBuffer Decode(ReadOnlySpan<byte> data, OperationContext context)
    {
        var info = ContainerHeaders.WebP(data, context);
        ReadOnlySpan<byte> alphaPayload = default;
        for (var position = 12; position < data.Length;)
        {
            var length = (int)BinaryPrimitives.ReadUInt32LittleEndian(data[(position + 4)..]);
            if (data.Slice(position, 4).SequenceEqual("VP8L"u8))
            {
                var result = new WebpLosslessDecoder(data.Slice(position + 8, length), info.Width, info.Height, context).Decode();
                result.Orientation = info.Orientation; return result;
            }
            if (data.Slice(position, 4).SequenceEqual("ALPH"u8)) alphaPayload = data.Slice(position + 8, length);
            if (data.Slice(position, 4).SequenceEqual("VP8 "u8))
            {
                var planes = new Vp8Decoder(data.Slice(position + 8, length), context).DecodePlanes();
                var alpha = alphaPayload.IsEmpty ? null : WebpAlpha.Decode(alphaPayload, info.Width, info.Height, context);
                var result = Vp8Color.Render(planes, alpha, context);
                result.Orientation = info.Orientation; return result;
            }
            position += 8 + length + (length & 1);
        }
        throw new ImageDataException("WebP pixel chunk is missing.");
    }
}

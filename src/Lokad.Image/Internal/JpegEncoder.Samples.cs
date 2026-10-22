namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    internal static void EncodeSamples(JpegSampleInput source, Stream output, JpegEncodeOptions options, OperationContext context)
        => EncodeReference(null, source, output, options, context);
}

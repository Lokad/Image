namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    internal static EncodedOutput AsyncOutput(ImageBuffer image, JpegEncodeOptions options, OperationContext context)
    {
        ValidateReferenceOptions(options, JpegSampleFormat.Rgb);
        return EncodedOutput.Segmented(context);
    }

    internal static EncodedOutput AsyncSampleOutput(JpegSampleInput source, JpegEncodeOptions options, OperationContext context)
    {
        ValidateReferenceOptions(options, source.Format);
        return EncodedOutput.Segmented(context);
    }
}

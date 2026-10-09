namespace Lokad.Image.Tooling;

// Development workload profiles; these are not additions to the production API.
public enum GeometryOperation
{
    Crop, CropCentered, CropClamped, FlipHorizontal, FlipVertical,
    Clockwise90, Half, CounterClockwise90, Transpose, Transverse
}

internal static class GeometryWorkload
{
    public static ImageBuffer Apply(ImageBuffer source, GeometryOperation operation)
        => operation switch
        {
            GeometryOperation.Crop => ImageTransforms.Crop(source, 3, 5, source.Width / 2, source.Height / 2),
            GeometryOperation.CropCentered => ImageTransforms.CropCentered(source, source.Width / 2, source.Height / 2),
            GeometryOperation.CropClamped => ImageTransforms.CropClamped(source, source.Width + 7, -7, source.Width / 2, source.Height / 2),
            GeometryOperation.FlipHorizontal => ImageTransforms.FlipHorizontal(source),
            GeometryOperation.FlipVertical => ImageTransforms.FlipVertical(source),
            GeometryOperation.Clockwise90 => ImageTransforms.Rotate(source, QuarterTurn.Clockwise90),
            GeometryOperation.Half => ImageTransforms.Rotate(source, QuarterTurn.Half),
            GeometryOperation.CounterClockwise90 => ImageTransforms.Rotate(source, QuarterTurn.CounterClockwise90),
            GeometryOperation.Transpose => ImageTransforms.Transpose(source),
            GeometryOperation.Transverse => ImageTransforms.Transpose(source, mirrored: true),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };

    public static void ValidateGeometry(int width, int height, GeometryOperation operation)
    {
        if (width < 1 || height < 1 || !Enum.IsDefined(operation))
            throw new ArgumentException("An edit profile requires positive geometry and a defined operation.");
        if (operation is GeometryOperation.Crop or GeometryOperation.CropCentered or GeometryOperation.CropClamped
            && (width < 9 || height < 9))
            throw new ArgumentException("The declared half-size crop profiles require a source of at least 9x9.");
    }
}

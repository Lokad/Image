using System.Text;

namespace Lokad.Image.Internal;

// Test-only partial extension, compiled with the owned decoder sources for the
// coefficient stage. Never included in the production project or package.
internal ref partial struct JpegDecoder
{
    internal void ObserveFirstBaselineScan(Stream output)
    {
        try
        {
            while (position < data.Length)
            {
                var marker = ReadMarker();
                var payload = ReadSegment();
                switch (marker)
                {
                    case 0xC0: ReadFrame(payload, false); break;
                    case 0xDB: ReadQuantization(payload); break;
                    case 0xC4: ReadHuffman(payload); break;
                    case 0xDA:
                        ReadScan(payload);
                        output.Write("LJCF0001"u8);
                        using (var writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen: true))
                        {
                            writer.Write(info.Width); writer.Write(info.Height); writer.Write(components.Length);
                            foreach (var component in components)
                            {
                                var columns = (component.SampleWidth + 7) / 8;
                                var rows = (component.SampleHeight + 7) / 8;
                                writer.Write(component.Id); writer.Write(component.H); writer.Write(component.V);
                                writer.Write(columns); writer.Write(rows);
                                foreach (var value in component.FrozenQuantization!) writer.Write(value);
                                for (var y = 0; y < rows; y++)
                                for (var x = 0; x < columns; x++)
                                foreach (var value in component.Coefficients.AsSpan((y * component.BlockColumns + x) * 64, 64))
                                    writer.Write(checked((short)value));
                            }
                        }
                        return;
                    case >= 0xE0 and <= 0xEF or 0xFE: break;
                    default: throw new InvalidOperationException("The bounded observer requires the original baseline control.");
                }
            }
            throw new InvalidOperationException("The bounded control has no scan.");
        }
        finally
        {
            foreach (var component in components)
            {
                var storage = component.Coefficients; component.Coefficients = [];
                context.ReturnCoefficients(storage); context.Release(storage.Length * 4L);
            }
        }
    }
}

namespace Lokad.Image.Internal;

internal sealed record JpegEncodingTable(byte[] Counts, byte[] Symbols, ushort[] Codes, byte[] Lengths);


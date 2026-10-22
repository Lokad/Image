// Original source-only encoder for the frozen420 row-restart control.
// All decisions are generated here or by independent specification-derived
// primitives. The encoder accepts no oracle packet, coefficient or coding table.
public static class JpegRestartParity
{
    public static JpegScanParity.Result Encode(byte[] rgba, bool nativeAttribution)
    {
        byte[] planes = JpegSubsampleParity.Convert(rgba, 8, 420);
        ushort[] quantization = JpegReferenceTables.Quantization(5);
        var tables = new JpegScanParity.CodeTable[4];
        for (int family = 0; family < 4; family++) tables[family] = JpegFixedReferenceTables.Create(family);
        return JpegScanParity.EncodeWithTables(planes, quantization, tables, null, nativeAttribution, 420, true);
    }
}

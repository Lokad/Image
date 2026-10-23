namespace Lokad.Image.Tooling;

// Original bounded spectra: DC ties/clipping, signed frequency impulses, three
// mixed spectra and two wide-quantizer boundaries. Native bytes are test data.
internal static class OriginalJpegInverseControls
{
    public static IEnumerable<object[]> Cases()
    {
        yield return [0, "f5a5fd42d16a20302798ef6ed309979b43003d2320d9f0e8ea9831a92759fb4b"];
        yield return [1, "e0205519f6bcde4208fd5e9aece72f518eb4ce5879ca1832a6cb221d534a2d7e"];
        yield return [2, "1df1b7ce1fd8fcbe20cde61646875e54fe38d8945ea7911afd59e025cc520a68"];
        yield return [3, "1df1b7ce1fd8fcbe20cde61646875e54fe38d8945ea7911afd59e025cc520a68"];
        yield return [4, "8667e718294e9e0df1d30600ba3eeb201f764aad2dad72748643e4a285e1d1f7"];
        yield return [5, "70fe63e96c27e8b5db0251f3707a3542755b373a4bb2684d3a786ed4c5f2ae6e"];
        yield return [6, "c739c1e56becd1eb8af75a29a6e74356bf026bd0d1e7a094f21d6ce2ed6ed635"];
        yield return [7, "bba08462257aa492462f36441dc4ce08ada1d31d527a0762b5fe9d94bed969a2"];
        yield return [8, "328aa548fe2f53f12da65a1f6ff44e8e21e835dcb26827eb5a6b8c2c94811c47"];
        yield return [9, "6f7210bb5af4e231d83e36d40dbb210bafcb50ac58313accba16404e2f86ccf1"];
        yield return [10, "04f19e938ae90c082e404c78dc0dd3e5366cde530f3fc9f1789e8895369d24bb"];
        yield return [11, "d323bc87f1e869f15292bef487f219ac2bcad8a82273e1159fa161dfd495da4b"];
        yield return [12, "94c35f57af1f4f9d86f561fa5f6f95761c10d4b49193f9f151730332ffa54b7b"];
        yield return [13, "39856e989bffd9645bc139f06542afe687fe0b8c2105522d9424d3a25215ee88"];
        yield return [14, "fd86ae4dc8d27c535380b6011ae2c7d20bca2ce8a903b5367b0967d402607e67"];
        yield return [15, "591f6986ec38c4a9d23392723733f8571b776ab4da15f50fdd5932e9d8e9f72e"];
        yield return [16, "f5a5fd42d16a20302798ef6ed309979b43003d2320d9f0e8ea9831a92759fb4b"];
        yield return [17, "1df1b7ce1fd8fcbe20cde61646875e54fe38d8945ea7911afd59e025cc520a68"];
    }

    internal static void Fill(int id, Span<int> coefficients, Span<ushort> quantization)
    {
        if (id is < 0 or >= 18 || coefficients.Length != 64 || quantization.Length != 64)
            throw new ArgumentOutOfRangeException(nameof(id));
        coefficients.Clear(); quantization.Fill(1);
        if (id < 5) { coefficients[0] = new[] { -1024, -4, 0, 4, 1016 }[id]; return; }
        if (id < 13)
        {
            var frequency = new[] { 1, 8, 4, 63 }[(id - 5) / 2];
            coefficients[0] = -2; coefficients[frequency] = ((id - 5) & 1) == 0 ? 17 : -17;
            quantization[frequency] = 3; return;
        }
        if (id < 16)
        {
            var phase = id - 13;
            for (var k = 0; k < 64; k++)
            {
                coefficients[k] = (k * 17 + phase * 11) % 7 == 0 ? (k * 13 + phase * 5) % 31 - 15 : 0;
                quantization[k] = (ushort)(1 + (k * 9 + phase) % 11);
            }
            return;
        }
        coefficients[1] = 1; quantization[1] = id == 16 ? (ushort)32767 : (ushort)65535;
    }
}

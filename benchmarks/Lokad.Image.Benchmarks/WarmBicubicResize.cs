using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmBicubicCase { Down8, Down16, Up8, Up16, Mixed8, Mixed16, Cross8, Cross16, Wide8, Wide16, Horizontal8, Horizontal16, Down64x32_8, Down64x32_16, Vertical8, Vertical16, HorizontalUp8, HorizontalUp16, VerticalUp8, VerticalUp16, Down32x16_8, Down32x16_16, Down16x8_8, Down16x8_16, TightDown8, TightDown16, TightWide8, TightWide16, Tight64x32_8, Tight64x32_16, Tight32x16_16, Tight32x16_8, Tight16x8_16, Tight16x8_8 }

/// <summary>Complete bicubic resize with an explicitly adjusted FFmpeg scaler backend.</summary>
[MemoryDiagnoser]
public class WarmBicubicResize
{
    public static IEnumerable<WarmBicubicCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmBicubicCase Case { get; set; }
    public static WarmBicubicCase[] SelectedCases() => Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES") switch
    {
        null or "Down8" => [WarmBicubicCase.Down8],
        "Down16" => [WarmBicubicCase.Down16],
        "Up8" => [WarmBicubicCase.Up8],
        "Up16" => [WarmBicubicCase.Up16],
        "Mixed8" => [WarmBicubicCase.Mixed8],
        "Mixed16" => [WarmBicubicCase.Mixed16],
        "Cross8" => [WarmBicubicCase.Cross8],
        "Cross16" => [WarmBicubicCase.Cross16],
        "Wide8" => [WarmBicubicCase.Wide8],
        "Wide16" => [WarmBicubicCase.Wide16],
        "Horizontal8" => [WarmBicubicCase.Horizontal8],
        "Horizontal16" => [WarmBicubicCase.Horizontal16],
        "Down64x32_8" => [WarmBicubicCase.Down64x32_8],
        "Down64x32_16" => [WarmBicubicCase.Down64x32_16],
        "Vertical8" => [WarmBicubicCase.Vertical8],
        "Vertical16" => [WarmBicubicCase.Vertical16],
        "HorizontalUp8" => [WarmBicubicCase.HorizontalUp8],
        "HorizontalUp16" => [WarmBicubicCase.HorizontalUp16],
        "VerticalUp8" => [WarmBicubicCase.VerticalUp8],
        "VerticalUp16" => [WarmBicubicCase.VerticalUp16],
        "Down32x16_8" => [WarmBicubicCase.Down32x16_8],
        "Down32x16_16" => [WarmBicubicCase.Down32x16_16],
        "Down16x8_8" => [WarmBicubicCase.Down16x8_8],
        "Down16x8_16" => [WarmBicubicCase.Down16x8_16],
        "TightDown8" => [WarmBicubicCase.TightDown8],
        "TightDown16" => [WarmBicubicCase.TightDown16],
        "TightWide8" => [WarmBicubicCase.TightWide8],
        "TightWide16" => [WarmBicubicCase.TightWide16],
        "Tight64x32_8" => [WarmBicubicCase.Tight64x32_8],
        "Tight64x32_16" => [WarmBicubicCase.Tight64x32_16],
        "Tight32x16_16" => [WarmBicubicCase.Tight32x16_16],
        "Tight32x16_8" => [WarmBicubicCase.Tight32x16_8],
        "Tight16x8_16" => [WarmBicubicCase.Tight16x8_16],
        "Tight16x8_8" => [WarmBicubicCase.Tight16x8_8],
        _ => throw new ArgumentException("Choose one bicubic case: Down8, Down16, Up8, Up16, Mixed8, Mixed16, Cross8, Cross16, Wide8, Wide16, Horizontal8, Horizontal16 , Down64x32_8 Down64x32_16 , Vertical8 , Vertical16, HorizontalUp8, HorizontalUp16, VerticalUp8, VerticalUp16, Down32x16_8, Down32x16_16, Down16x8_8, Down16x8_16, TightDown8, TightDown16, TightWide8, TightWide16, Tight64x32_8, Tight64x32_16, Tight32x16_16, Tight32x16_8, Tight16x8_16 or Tight16x8_8.")
    };
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private ImageLimits? operationLimits;
    private byte[] input = [], expected = [], nativePixels = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private bool smoke, ready;
    private int cpu, sampleCount;
    private double nativeRgbRmse, nativeAlphaRmse;
    private int nativeRgbMaximum, nativeAlphaMaximum;
    private bool Upscale => Case is WarmBicubicCase.Up8 or WarmBicubicCase.Up16;
    private bool ExplicitGeometry => Upscale || Case is WarmBicubicCase.Mixed8 or WarmBicubicCase.Mixed16 or WarmBicubicCase.Cross8 or WarmBicubicCase.Cross16 or WarmBicubicCase.Wide8 or WarmBicubicCase.Wide16 or WarmBicubicCase.Horizontal8 or WarmBicubicCase.Horizontal16 or WarmBicubicCase.Down64x32_8 or WarmBicubicCase.Down64x32_16 or WarmBicubicCase.Vertical8 or WarmBicubicCase.Vertical16 or WarmBicubicCase.HorizontalUp8 or WarmBicubicCase.HorizontalUp16 or WarmBicubicCase.VerticalUp8 or WarmBicubicCase.VerticalUp16 or WarmBicubicCase.Down32x16_8 or WarmBicubicCase.Down32x16_16 or WarmBicubicCase.Down16x8_8 or WarmBicubicCase.Down16x8_16 or WarmBicubicCase.TightWide8 or WarmBicubicCase.TightWide16 or WarmBicubicCase.Tight64x32_8 or WarmBicubicCase.Tight64x32_16 or WarmBicubicCase.Tight32x16_16 or WarmBicubicCase.Tight32x16_8 or WarmBicubicCase.Tight16x8_16 or WarmBicubicCase.Tight16x8_8;
    private int Width => Case is WarmBicubicCase.Down16x8_8 or WarmBicubicCase.Down16x8_16 or WarmBicubicCase.Tight16x8_16 or WarmBicubicCase.Tight16x8_8 ? 16 : Case is WarmBicubicCase.Down32x16_8 or WarmBicubicCase.Down32x16_16 or WarmBicubicCase.Tight32x16_16 or WarmBicubicCase.Tight32x16_8 ? 32 : Case is WarmBicubicCase.Vertical8 or WarmBicubicCase.Vertical16 or WarmBicubicCase.VerticalUp8 or WarmBicubicCase.VerticalUp16 ? 513 : Case is WarmBicubicCase.Down64x32_8 or WarmBicubicCase.Down64x32_16 or WarmBicubicCase.Tight64x32_8 or WarmBicubicCase.Tight64x32_16 ? 64 : Case is WarmBicubicCase.Wide8 or WarmBicubicCase.Wide16 or WarmBicubicCase.TightWide8 or WarmBicubicCase.TightWide16 ? 128 : Case is WarmBicubicCase.Cross8 or WarmBicubicCase.Cross16 or WarmBicubicCase.Horizontal8 or WarmBicubicCase.Horizontal16 ? 256 : ExplicitGeometry ? 1027 : 256;
    private int Height => Case is WarmBicubicCase.Down16x8_8 or WarmBicubicCase.Down16x8_16 or WarmBicubicCase.Tight16x8_16 or WarmBicubicCase.Tight16x8_8 ? 8 : Case is WarmBicubicCase.Down32x16_8 or WarmBicubicCase.Down32x16_16 or WarmBicubicCase.Tight32x16_16 or WarmBicubicCase.Tight32x16_8 ? 16 : Case is WarmBicubicCase.VerticalUp8 or WarmBicubicCase.VerticalUp16 ? 519 : Case is WarmBicubicCase.HorizontalUp8 or WarmBicubicCase.HorizontalUp16 ? 259 : Case is WarmBicubicCase.Down64x32_8 or WarmBicubicCase.Down64x32_16 or WarmBicubicCase.Tight64x32_8 or WarmBicubicCase.Tight64x32_16 ? 32 : Case is WarmBicubicCase.Horizontal8 or WarmBicubicCase.Horizontal16 ? 259 : Case is WarmBicubicCase.Wide8 or WarmBicubicCase.Wide16 or WarmBicubicCase.TightWide8 or WarmBicubicCase.TightWide16 ? 64 : Upscale || Case is WarmBicubicCase.Cross8 or WarmBicubicCase.Cross16 ? 519 : 129;
    private int Depth => Case is WarmBicubicCase.Down16 or WarmBicubicCase.Up16 or WarmBicubicCase.Mixed16 or WarmBicubicCase.Cross16 or WarmBicubicCase.Wide16 or WarmBicubicCase.Horizontal16 or WarmBicubicCase.Down64x32_16 or WarmBicubicCase.Vertical16 or WarmBicubicCase.HorizontalUp16 or WarmBicubicCase.VerticalUp16 or WarmBicubicCase.Down32x16_16 or WarmBicubicCase.Down16x8_16 or WarmBicubicCase.TightDown16 or WarmBicubicCase.TightWide16 or WarmBicubicCase.Tight64x32_16 or WarmBicubicCase.Tight32x16_16 or WarmBicubicCase.Tight16x8_16 ? 16 : 8;

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Use the dedicated Linux VM.");
        if (Case is not (WarmBicubicCase.Down8 or WarmBicubicCase.Down16 or WarmBicubicCase.Up8 or WarmBicubicCase.Up16 or WarmBicubicCase.Mixed8 or WarmBicubicCase.Mixed16 or WarmBicubicCase.Cross8 or WarmBicubicCase.Cross16 or WarmBicubicCase.Wide8 or WarmBicubicCase.Wide16 or WarmBicubicCase.Horizontal8 or WarmBicubicCase.Horizontal16 or WarmBicubicCase.Down64x32_8 or WarmBicubicCase.Down64x32_16 or WarmBicubicCase.Vertical8 or WarmBicubicCase.Vertical16 or WarmBicubicCase.HorizontalUp8 or WarmBicubicCase.HorizontalUp16 or WarmBicubicCase.VerticalUp8 or WarmBicubicCase.VerticalUp16 or WarmBicubicCase.Down32x16_8 or WarmBicubicCase.Down32x16_16 or WarmBicubicCase.Down16x8_8 or WarmBicubicCase.Down16x8_16 or WarmBicubicCase.TightDown8 or WarmBicubicCase.TightDown16 or WarmBicubicCase.TightWide8 or WarmBicubicCase.TightWide16 or WarmBicubicCase.Tight64x32_8 or WarmBicubicCase.Tight64x32_16 or WarmBicubicCase.Tight32x16_16 or WarmBicubicCase.Tight32x16_8 or WarmBicubicCase.Tight16x8_16 or WarmBicubicCase.Tight16x8_8)) throw new ArgumentOutOfRangeException(nameof(Case));
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        if (cpu is < 0 or > 62 || Process.GetCurrentProcess().ProcessorAffinity.ToInt64() != (1L << cpu)
            || Environment.ProcessorCount != 1 || GCSettings.IsServerGC
            || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
            || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("Bicubic requires one CPU, optimized JIT and non-concurrent workstation GC.");
        state = new JpegBenchmarkCase();
        try
        {
            using (var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            input = HorizontalFlipInput.Create(Depth);
            operationLimits = Case is WarmBicubicCase.TightDown8 or WarmBicubicCase.TightDown16 or WarmBicubicCase.TightWide8 or WarmBicubicCase.TightWide16 or WarmBicubicCase.Tight64x32_8 or WarmBicubicCase.Tight64x32_16 or WarmBicubicCase.Tight32x16_16 or WarmBicubicCase.Tight32x16_8 or WarmBicubicCase.Tight16x8_16 or WarmBicubicCase.Tight16x8_8
                ? new ImageLimits { MaxWorkingBytes = input.Length + (long)Width * Height * 4 * (Depth / 8) + 4096 } : null;
            if (Case == WarmBicubicCase.TightWide16 && operationLimits?.MaxWorkingBytes != 1132568)
                throw new InvalidOperationException("The word wider-support benchmark requires its declared tight working limit.");
            if (Case == WarmBicubicCase.Tight64x32_8 && (Width != 64 || Height != 32 || Depth != 8 || operationLimits?.MaxWorkingBytes != 543756))
                throw new InvalidOperationException("The byte 64x32 benchmark requires its declared geometry, depth and tight limit.");
            if (Case == WarmBicubicCase.Tight64x32_16 && (Width != 64 || Height != 32 || Depth != 16 || operationLimits?.MaxWorkingBytes != 1083416))
                throw new InvalidOperationException("The word 64x32 benchmark requires its declared geometry, depth and tight limit.");
            if (Case == WarmBicubicCase.Tight32x16_16 && (Width != 32 || Height != 16 || Depth != 16 || operationLimits?.MaxWorkingBytes != 1071128))
                throw new InvalidOperationException("The word 32x16 benchmark requires its declared geometry, depth and tight limit.");
            if (Case == WarmBicubicCase.Tight32x16_8 && (Width != 32 || Height != 16 || Depth != 8 || operationLimits?.MaxWorkingBytes != 537612))
                throw new InvalidOperationException("The byte 32x16 benchmark requires its declared geometry, depth and tight limit.");
            if (Case == WarmBicubicCase.Tight16x8_16 && (Width != 16 || Height != 8 || Depth != 16 || operationLimits?.MaxWorkingBytes != 1068056))
                throw new InvalidOperationException("The word 16x8 benchmark requires its declared geometry, depth and tight limit.");
            if (Case == WarmBicubicCase.Tight16x8_8 && (Width != 16 || Height != 8 || Depth != 8 || operationLimits?.MaxWorkingBytes != 536076))
                throw new InvalidOperationException("The byte 16x8 benchmark requires its declared geometry, depth and tight limit.");
            expected = ExactResizeReference.Resize(input, 513, 259, Depth, Width, Height, ResizeFilter.Bicubic);
            source = JpegBenchmarkCase.Buffer(input, 513, 259, Depth);
            if(Case is WarmBicubicCase.Horizontal8 or WarmBicubicCase.Vertical8)
            {
                // SPEC.md retains binary64 rounding at exact rational halves.
                // Pin the qualified direct output, certifying every exception
                // independently, so a candidate must preserve every byte.
                using var direct=ImageTransforms.Resize(source,Width,Height,ResizeFilter.Bicubic,
                    new ImageLimits { MaxWorkingBytes=input.Length+(long)Width*Height*4+4096 });
                var original=OriginalPixels.Bytes(direct);var halves=0;
                for(var i=0;i<original.Length;i++)if(original[i]!=expected[i])
                {
                    if(i%4!=3 || original[i]+1!=expected[i]
                        || !ExactResizeReference.AlphaIsHalf(input,513,259,Depth,Width,Height,i/4%Width,i/4/Width,ResizeFilter.Bicubic))
                        throw new InvalidOperationException("Horizontal reference differs outside an exact alpha half.");
                    halves++;
                }
                if(halves!=(Case==WarmBicubicCase.Vertical8?25:6) || JpegBenchmarkCase.Hash(original)!=(Case==WarmBicubicCase.Vertical8?"56ff0d8276c6427bb3ad49e8d3af6ad26cfb99b27db697580471ac273fcd7697":"a5d2e75dbc30006f845c8a69061e096ded94ed06248e50350ec2c18456606a20"))
                    throw new InvalidOperationException("Qualified horizontal pixels changed.");
                expected=original;
            }
            if(Case==WarmBicubicCase.VerticalUp8)
            {
                // Preserve the qualified direct output. Its one blue-channel
                // exception is an independently certified exact rational half.
                using var direct=ImageTransforms.Resize(source,Width,Height,ResizeFilter.Bicubic,
                    new ImageLimits { MaxWorkingBytes=input.Length+(long)Width*Height*4+4096 });
                var original=OriginalPixels.Bytes(direct);var halves=0;
                for(var i=0;i<original.Length;i++)if(original[i]!=expected[i])
                {
                    if(i%4!=2 || original[i]+1!=expected[i]
                        || !ExactResizeReference.SampleIsHalf(input,513,259,Depth,Width,Height,i/4%Width,i/4/Width,2,ResizeFilter.Bicubic))
                        throw new InvalidOperationException("Vertical enlargement differs outside an exact blue half.");
                    halves++;
                }
                if(halves!=1 || JpegBenchmarkCase.Hash(original)!="705c36dd72e49076298952566b7e68b7922cb649c45d4dbf4cf68d9c6cd2887d")
                    throw new InvalidOperationException("Qualified vertical enlargement pixels changed.");
                expected=original;
            }
            File.WriteAllBytes(state.Input, input);
            var rationalPath = Path.Combine(state.DirectoryPath, "rational.rgba");
            File.WriteAllBytes(rationalPath, expected);
            if (Depth == 16 || ExplicitGeometry)
            {
                var controls = Path.Combine(state.Root, "artifacts/warm-reference", Case is WarmBicubicCase.Down16x8_16 or WarmBicubicCase.Tight16x8_16 ? "bicubic-down16x8-16" : Case is WarmBicubicCase.Down16x8_8 or WarmBicubicCase.Tight16x8_8 ? "bicubic-down16x8-8" : Case is WarmBicubicCase.Down32x16_16 or WarmBicubicCase.Tight32x16_16 ? "bicubic-down32x16-16" : Case is WarmBicubicCase.Down32x16_8 or WarmBicubicCase.Tight32x16_8 ? "bicubic-down32x16-8" : Case == WarmBicubicCase.VerticalUp16 ? "bicubic-vertical-up16" : Case == WarmBicubicCase.VerticalUp8 ? "bicubic-vertical-up8" : Case == WarmBicubicCase.HorizontalUp16 ? "bicubic-horizontal-up16" : Case == WarmBicubicCase.HorizontalUp8 ? "bicubic-horizontal-up8" : Case == WarmBicubicCase.Vertical16 ? "bicubic-vertical16" : Case == WarmBicubicCase.Vertical8 ? "bicubic-vertical8" : Case is WarmBicubicCase.Down64x32_16 or WarmBicubicCase.Tight64x32_16 ? "bicubic-down64x32-16" : Case is WarmBicubicCase.Down64x32_8 or WarmBicubicCase.Tight64x32_8 or WarmBicubicCase.Tight64x32_16 ? "bicubic-down64x32-8" : Case == WarmBicubicCase.Horizontal16 ? "bicubic-horizontal16" : Case == WarmBicubicCase.Horizontal8 ? "bicubic-horizontal8" : Case is WarmBicubicCase.Wide16 or WarmBicubicCase.TightWide16 ? "bicubic-wide16" : Case is WarmBicubicCase.Wide8 or WarmBicubicCase.TightWide8 or WarmBicubicCase.TightWide16 ? "bicubic-wide8" : Case == WarmBicubicCase.Cross16 ? "bicubic-cross16" : Case == WarmBicubicCase.Cross8 ? "bicubic-cross8" : Case == WarmBicubicCase.Mixed16 ? "bicubic-mixed16" : Case == WarmBicubicCase.Mixed8 ? "bicubic-mixed8" : Upscale ? Depth == 16 ? "bicubic-up16" : "bicubic-up8" : "bicubic16");
                Directory.CreateDirectory(controls);
                File.WriteAllBytes(Path.Combine(controls, Depth == 16 ? "rational.rgba64le" : "rational.rgba"), expected);
            }
            ValidateManaged();
            native = new NativeWarmReference(state, Depth, cpu, "bicubic-resize", expectedRaw: rationalPath, resizeWidth: ExplicitGeometry ? Width : null, resizeHeight: ExplicitGeometry ? Height : null);
            nativePixels = File.ReadAllBytes(state.NativeOutput);
            RequireBounded(nativePixels, expected, out nativeRgbMaximum, out nativeRgbRmse, out nativeAlphaMaximum, out nativeAlphaRmse);
            using (var scalar = new NativeWarmReference(state, Depth, cpu, "bicubic-resize", expectedRaw: rationalPath, bicubicScalar: true, resizeWidth: ExplicitGeometry ? Width : null, resizeHeight: ExplicitGeometry ? Height : null))
            {
                var scalarPixels = File.ReadAllBytes(state.NativeOutput);
                RequireBounded(scalarPixels, expected, out _, out _, out _, out _);
                RequireBounded(nativePixels, scalarPixels, out _, out _, out _, out _);
                scalar.Check(); scalar.Stop();
            }
            native.Check(); RequireNativeOutput();
            before = native.Measure(smoke, sampleCount: sampleCount);
            ready = true;
        }
        catch { native?.Dispose(); source?.Dispose(); state.Dispose(); throw; }
    }
    private void ValidateManaged()
    {
        using var first = ImageTransforms.Resize(source, Width, Height, ResizeFilter.Bicubic, operationLimits);
        using var second = ImageTransforms.Resize(source, Width, Height, ResizeFilter.Bicubic, operationLimits);
        if ((first.Width, first.Height, first.BitDepth, first.Orientation) != (Width, Height, Depth, ImageOrientation.Normal)
            || !OriginalPixels.Bytes(first).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Bicubic output differs from the exact rational reference.");
        if (Depth == 8) first.GetRow8(0)[0] ^= 255;
        else first.GetRow16(0)[0] ^= 65535;
        if (!OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(input))
            throw new InvalidOperationException("Bicubic outputs alias or source changed.");
    }
    private void RequireBounded(byte[] actual, byte[] reference, out int rgbMaximum, out double rgbRmse, out int alphaMaximum, out double alphaRmse)
    {
        var sampleBytes = Depth / 8;
        if (actual.Length != reference.Length || actual.Length != Width * Height * 4 * sampleBytes) throw new InvalidOperationException("Bicubic output length differs.");
        rgbMaximum=alphaMaximum=0; long rgbSquare=0, alphaSquare=0;
        for (var i=0;i<actual.Length;i+=4*sampleBytes)
        {
            var alpha = sampleBytes == 1 ? actual[i+3] : BinaryPrimitives.ReadUInt16LittleEndian(actual.AsSpan(i+6,2));
            var referenceAlpha = sampleBytes == 1 ? reference[i+3] : BinaryPrimitives.ReadUInt16LittleEndian(reference.AsSpan(i+6,2));
            if (alpha==0 && actual.AsSpan(i,3*sampleBytes).ContainsAnyExcept((byte)0)) throw new InvalidOperationException("Native zero-alpha RGB differs.");
            for (var c=0;c<4;c++)
            {
                var av = sampleBytes == 1 ? actual[i+c] : BinaryPrimitives.ReadUInt16LittleEndian(actual.AsSpan(i+c*2,2));
                var rv = sampleBytes == 1 ? reference[i+c] : BinaryPrimitives.ReadUInt16LittleEndian(reference.AsSpan(i+c*2,2));
                var error=Math.Abs(av-rv);
                var rgbBound=Case==WarmBicubicCase.VerticalUp16 && referenceAlpha<=16?2:1;
                if(c<3 && error>rgbBound)throw new InvalidOperationException("Native bicubic RGB pixel bound exceeded.");
                if (c==3) { alphaMaximum=Math.Max(alphaMaximum,error);alphaSquare+=(long)error*error; }
                else { rgbMaximum=Math.Max(rgbMaximum,error);rgbSquare+=(long)error*error; }
            }
        }
        rgbRmse=Math.Sqrt((double)rgbSquare/(Width*Height*3));alphaRmse=Math.Sqrt((double)alphaSquare/(Width*Height));
        if (rgbMaximum>(Case==WarmBicubicCase.VerticalUp16?2:1) || alphaMaximum>1 || rgbRmse>0.25 || alphaRmse>0.25) throw new InvalidOperationException("Native bicubic pixel bounds exceeded.");
    }
    private void RequireNativeOutput()
    {
        if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(nativePixels)) throw new InvalidOperationException("Native bicubic pixels changed.");
    }
    [Benchmark] public int Managed()
    {
        using var result = ImageTransforms.Resize(source, Width, Height, ResizeFilter.Bicubic, operationLimits);
        return result.Stride * result.Height;
    }
    [GlobalCleanup] public void Cleanup()
    {
        try
        {
            if (!ready) return;
            ValidateManaged();
            var after=native.Measure(smoke,before[0].Operations,sampleCount);
            native.Check();RequireNativeOutput();native.Stop();
            var path=Path.Combine(state.Root,"artifacts/benchmark-validation",nameof(WarmBicubicResize)+"-"+Guid.NewGuid().ToString("N")+".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path,JsonSerializer.Serialize(new
            {
                schemaVersion=1,name=nameof(WarmBicubicResize),caseId=Case.ToString(),smoke,
                runId=Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"),width=513,height=259,depth=Depth,
                outputWidth=Width,outputHeight=Height,managedMaxWorkingBytes=operationLimits?.MaxWorkingBytes,cpu,managedAffinityCount=1,processorCount=Environment.ProcessorCount,
                serverGc=GCSettings.IsServerGC,gcConfiguration=GC.GetConfigurationVariables(),runtime=RuntimeInformation.FrameworkDescription,
                platform=RuntimeInformation.RuntimeIdentifier,sourceIdentity,
                inputSha256=JpegBenchmarkCase.Hash(input),outputSha256=JpegBenchmarkCase.Hash(expected),nativeOutputSha256=JpegBenchmarkCase.Hash(nativePixels),
                managedRationalReferenceExact=Case is not (WarmBicubicCase.Horizontal8 or WarmBicubicCase.Vertical8 or WarmBicubicCase.VerticalUp8),
                managedQualifiedOriginalPixelsExact=true,
                certifiedExactRationalAlphaHalves=Case==WarmBicubicCase.Vertical8?25:Case==WarmBicubicCase.Horizontal8?6:0,
                certifiedExactRationalRgbHalves=Case==WarmBicubicCase.VerticalUp8?1:0,
                nativeRgbMaximum,nativeRgbRmse,nativeAlphaMaximum,nativeAlphaRmse,
                nativeLowAlphaRgbBounded=Case==WarmBicubicCase.VerticalUp16,
                nativeLowAlphaRgbReferenceMaximum=Case==WarmBicubicCase.VerticalUp16?16:0,
                nativeLowAlphaRgbErrorMaximum=Case==WarmBicubicCase.VerticalUp16?2:1,
                nativeNormalScalarBounded=true, nativePremultipliedFloatOutput=ExplicitGeometry,nativePixelsDeterministic=true,nativeZeroAlphaRgbZero=true,
                sourceUnchanged=true,independentOwnedOutput=true,nativeExitCode=0,
                librarySha256=JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256=JpegBenchmarkCase.HashFile(typeof(WarmBicubicResize).Assembly.Location),
                nativeBuild=native.BuildIdentity,nativeReady=native.Ready,nativeBefore=before,nativeAfter=after,
                boundary=$"One resident original 513x259 RGBA{Depth} raster to independently owned {Width}x{Height} bicubic output at the same sample depth. Complete public Resize, admission, allocation/materialization/disposal. {(operationLimits is null ? "Default managed working limit" : $"Managed working limit {operationLimits.MaxWorkingBytes} bytes (input+output+4096); full output-row cache excluded")}. FFmpeg zscale's zimg backend with B=0/C=0.5, encoded-space premultiplied filtering. TIMED complete packed-source deinterleaving, {(Case is WarmBicubicCase.Down16x8_8 or WarmBicubicCase.Down16x8_16 or WarmBicubicCase.Tight16x8_16 or WarmBicubicCase.Tight16x8_8 ? "sixty-four-pixel clamped halo (86.703% input-area increase)" : Case is WarmBicubicCase.Down32x16_8 or WarmBicubicCase.Down32x16_16 or WarmBicubicCase.Tight32x16_16 or WarmBicubicCase.Tight32x16_8 ? "thirty-two-pixel clamped halo (40.269% input-area increase)" : Case is WarmBicubicCase.Down64x32_8 or WarmBicubicCase.Down64x32_16 or WarmBicubicCase.Tight64x32_8 or WarmBicubicCase.Tight64x32_16 ? "sixteen-pixel clamped halo (19.364% input-area increase)" : Case is WarmBicubicCase.Wide8 or WarmBicubicCase.Wide16 or WarmBicubicCase.TightWide8 or WarmBicubicCase.TightWide16 ? "eight-pixel clamped halo (9.489% input-area increase)" : "four-pixel clamped halo (4.697% input-area increase)")}, filtering, independent planar/packed output allocation, repacking and release. Explicit geometry uses premultiplied float output and TIMED straight RGBA quantization after unpremultiplication, clearing RGB when rounded alpha is zero. Graph/input-workspace/scratch contexts reused; every call processes every selected sample. {(Case is WarmBicubicCase.Horizontal8 or WarmBicubicCase.Vertical8 or WarmBicubicCase.VerticalUp8 ? "Managed exact qualified original pixels; independently certified exact rational sample halves retain SPEC binary64 rounding" : "Managed exact rational pixels")}; {(Case==WarmBicubicCase.VerticalUp16 ? "Explicitly approved native-only RGBmaximum2 ONLY where referencealpha<=16, RGBmaximum1 elsewhere, alpha maximum1, RGB/alpha RMSE0.25" : "Explicitly approved native-only RGB/alpha maximum1/RMSE0.25")}; normal/scalar bounded difference; zero-alpha RGBzero. This is the labeled scaler backend, not FFmpeg CLI. No output cache. Startup, IPC, I/O, generation and validation excluded; one CPU, normal SIMD, no concurrency. Only this geometry/depth is measured."
            },new JsonSerializerOptions {WriteIndented=true})+"\n");
        }
        finally {native?.Dispose();source?.Dispose();state?.Dispose();ready=false;}
    }
}

using BenchmarkDotNet.Running;
using Lokad.Image;
using Lokad.Image.Tooling;

if (args.SequenceEqual(["--prepare-warm-png-paeth-8"]))
{
    WarmPngPaethInput.Prepare(8);
    return;
}

if (args.SequenceEqual(["--prepare-warm-png-paeth-16"]))
{
    WarmPngPaethInput.Prepare();
    return;
}

if (args.SequenceEqual(["--prepare-warm-png-adam7-16"]))
{
    WarmPngAdam7Input.Prepare();
    return;
}

if (args.SequenceEqual(["--validate-png-crop-resize-cleanup"]))
{
    Environment.SetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID", "png-pipeline-cleanup-" + Guid.NewGuid().ToString("N"));
    PngCropResizePipeline.ValidateCleanupControls();
    return;
}

if (args.SequenceEqual(["--validate-png-crop-resize-full"]))
{
    Environment.SetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID", "png-pipeline-full-preflight-" + Guid.NewGuid().ToString("N"));
    var profiles = 0;
    foreach (var width in new[] { 129, 513, 1025, 2049 })
    foreach (var depth in new[] { 8, 16 })
    foreach (var frames in new[] { 1, 32 })
    foreach (var upscale in new[] { false, true })
    {
        var workload = new PngCropResizePipeline { Width = width, Depth = depth, Frames = frames, Upscale = upscale };
        try { workload.Setup(); profiles++; }
        finally { workload.Cleanup(); }
    }
    Console.WriteLine($"All {profiles} PNG crop/nearest pipeline profiles validated; no timing measurement.");
    return;
}

if (args.SequenceEqual(["--validate-raw-edit-cleanup"]))
{
    Environment.SetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID", "raw-edit-cleanup-" + Guid.NewGuid().ToString("N"));
    GeometryRawFile.ValidateCleanupControls(); NearestResizeRawFile.ValidateCleanupControls();
    return;
}

if (args.SequenceEqual(["--validate-geometry-full"]))
{
    Environment.SetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID", "geometry-full-preflight-" + Guid.NewGuid().ToString("N"));
    var profiles = 0;
    foreach (var width in new[] { 129, 513, 1025, 2049 })
    foreach (var depth in new[] { 8, 16 })
    foreach (var frames in new[] { 1, 32 })
    foreach (var operation in Enum.GetValues<GeometryOperation>())
    {
        var workload = new GeometryRawFile { Width = width, Depth = depth, Frames = frames, Operation = operation };
        try { workload.Setup(); profiles++; }
        finally { workload.Cleanup(); }
    }
    Console.WriteLine($"All {profiles} raw geometry profiles validated; no timing measurement.");
    return;
}

if (args.SequenceEqual(["--validate-nearest-resize-full"]))
{
    Environment.SetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID", "nearest-full-preflight-" + Guid.NewGuid().ToString("N"));
    var profiles = 0;
    foreach (var width in new[] { 129, 513, 1025, 2049 })
    foreach (var depth in new[] { 8, 16 })
    foreach (var frames in new[] { 1, 32 })
    foreach (var upscale in new[] { false, true })
    {
        var workload = new NearestResizeRawFile { Width = width, Depth = depth, Frames = frames, Upscale = upscale };
        try { workload.Setup(); profiles++; }
        finally { workload.Cleanup(); }
    }
    Console.WriteLine($"All {profiles} nearest resize profiles validated; no timing measurement.");
    return;
}

if (args.SequenceEqual(["--validate-matched-content-batch-cleanup"]))
{
    Environment.SetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID", "matched-content-cleanup-" + Guid.NewGuid().ToString("N"));
    MatchedQualityContentBatch.ValidateCleanupControls();
    return;
}

if (args.SequenceEqual(["--validate-matched-content-batches"]))
{
    Environment.SetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID", "matched-content-preflight-" + Guid.NewGuid().ToString("N"));
    using var identity = new JpegBenchmarkCase();
    var (_, catalog) = MatchedQualityEncodeInvocation.ReadCatalog(identity);
    var groups = MatchedQualityContentBatch.Groups(catalog);
    var cases = new MatchedQualityContentBatch().Cases.ToArray();
    if (cases.Length == 0) throw new InvalidOperationException("Calibration contains no fully matched content groups; excluded groups cannot be benchmarked.");
    foreach (var profile in cases)
    {
        var workload = new MatchedQualityContentBatch { Workload = profile };
        try { workload.Setup(); }
        finally { workload.Cleanup(); }
    }
    Console.WriteLine($"All {cases.Length} calibrated content batches validated; {groups.Count(group => !group.Accepted)} groups excluded; {MatchedQualityContentBatch.FrameCount} still files per subject, no timing measurement.");
    return;
}

if (args.SequenceEqual(["--validate-encode-batch-cleanup"]))
{
    Environment.SetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID", "encode-batch-cleanup-" + Guid.NewGuid().ToString("N"));
    EncodeBatch.ValidateCleanupControls();
    return;
}

if (args.SequenceEqual(["--validate-encode-batches"]))
{
    Environment.SetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID", "encode-batch-preflight-" + Guid.NewGuid().ToString("N"));
    var profiles = 0;
    foreach (var width in new[] { 129, 513, 1025, 2049 })
    foreach (var depth in new[] { 8, 16 })
    foreach (var encoder in Enum.GetValues<BatchEncoder>())
    {
        var workload = new EncodeBatch { Width = width, Depth = depth, Encoder = encoder };
        try { workload.Setup(); profiles++; }
        finally { workload.Cleanup(); }
    }
    Console.WriteLine($"All {profiles} encode batch profiles validated; {EncodeBatch.FrameCount} independent still files per subject, no timing measurement.");
    return;
}

if (args.SequenceEqual(["--validate-quality-calibration-controls"]))
{
    EncoderQualityCalibration.ValidateControls();
    Console.WriteLine("Quality calibration numeric, bounded-sampling and representative-coverage controls passed; no benchmark executed.");
    return;
}

if (args.Length is 1 or 2 && args[0] == "--calibrate-encoder-quality")
{
    EncoderQualityCalibration.Generate(args.Length == 2 ? args[1] : "representative");
    return;
}

if (args.SequenceEqual(["--validate-matched-encoder-quality"]))
{
    Environment.SetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID", "matched-quality-preflight-" + Guid.NewGuid().ToString("N"));
    var cases = new MatchedQualityEncodeInvocation().Cases.ToArray();
    if (cases.Length == 0) throw new InvalidOperationException("Calibration contains no matched encoder profiles.");
    foreach (var profile in cases)
    {
        var workload = new MatchedQualityEncodeInvocation { Workload = profile };
        try { workload.Setup(); }
        finally { workload.Cleanup(); }
    }
    Console.WriteLine($"All {cases.Length} calibrated matched-quality encoder pairs validated; no timing measurement.");
    return;
}

if (args.SequenceEqual(["--validate-matched-encoder-quality-batch"]))
{
    Environment.SetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID", "matched-quality-batch-preflight-" + Guid.NewGuid().ToString("N"));
    var cases = new MatchedQualityEncodeBatch().Cases.ToArray();
    if (cases.Length == 0) throw new InvalidOperationException("Calibration contains no matched encoder profiles.");
    foreach (var profile in cases)
    {
        var workload = new MatchedQualityEncodeBatch { Workload = profile };
        try { workload.Setup(); }
        finally { workload.Cleanup(); }
    }
    Console.WriteLine($"All {cases.Length} calibrated matched-quality encoder batches validated; {MatchedQualityEncodeBatch.FrameCount} still files per subject, no timing measurement.");
    return;
}

if (args.SequenceEqual(["--validate-invocations"]))
{
    Environment.SetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID", "invocation-preflight-" + Guid.NewGuid().ToString("N"));
    foreach (var depth in new[] { 8, 16 })
    {
        var workload = new PngFlipInvocation { Depth = depth };
        try { workload.Setup(); }
        finally { workload.Cleanup(); }
    }
    foreach (var progressive in new[] { false, true })
    {
        var workload = new JpegFlipInvocation { Progressive = progressive };
        try { workload.Setup(); }
        finally { workload.Cleanup(); }
    }
    foreach (var repeated in new[] { false, true })
    {
        var workload = new WebpLosslessFlipInvocation { RepeatedColor = repeated };
        try { workload.Setup(); }
        finally { workload.Cleanup(); }
    }
    foreach (var opaque in new[] { false, true })
    {
        var flip = new Vp8FlipInvocation { Opaque = opaque };
        try { flip.Setup(); }
        finally { flip.Cleanup(); }
        var encode = new Vp8EncodeInvocation { Opaque = opaque };
        try { encode.Setup(); }
        finally { encode.Cleanup(); }
    }
    Console.WriteLine("All ten codec/edit invocation profiles validated; no timing measurement.");
    return;
}

if (args.SequenceEqual(["--validate-identify"]))
{
    Environment.SetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID", "identify-preflight-" + Guid.NewGuid().ToString("N"));
    foreach (var width in new[] { 129, 513 })
    foreach (var profile in Enum.GetValues<MetadataProfile>())
    {
        var workload = new IdentifyInvocation { Width = width, Profile = profile };
        try { workload.Setup(); }
        finally { workload.Cleanup(); }
    }
    Console.WriteLine("All twelve coded-header inspection profiles validated; no timing measurement.");
    return;
}

if (args.SequenceEqual(["--validate-exif"]))
{
    Environment.SetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID", "exif-preflight-" + Guid.NewGuid().ToString("N"));
    foreach (var width in new[] { 129, 513 })
    foreach (var frames in new[] { 1, 32 })
    foreach (var profile in Enum.GetValues<MetadataProfile>())
    foreach (var orientation in Enum.GetValues<ImageOrientation>())
    {
        var workload = new ExifDecodeRawFile { Width = width, Frames = frames, Profile = profile, Orientation = orientation };
        try { workload.Setup(); }
        finally { workload.Cleanup(); }
    }
    Console.WriteLine("All 192 EXIF decode/orient profiles validated; no timing measurement.");
    return;
}

if (args.SequenceEqual(["--validate-nearest-resize"]))
{
    Environment.SetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID", "nearest-resize-preflight-" + Guid.NewGuid().ToString("N"));
    foreach (var upscale in new[] { false, true })
    {
        var workload = new NearestResizeRawFile { Width = 129, Depth = 16, Frames = 32, Upscale = upscale };
        try { workload.Setup(); }
        finally { workload.Cleanup(); }
    }
    Console.WriteLine("Both 16-bit nearest resize batch directions validated; no timing measurement.");
    return;
}

if (args.SequenceEqual(["--validate-geometry"]))
{
    Environment.SetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID", "geometry-preflight-" + Guid.NewGuid().ToString("N"));
    foreach (var operation in Enum.GetValues<GeometryOperation>())
    {
        var workload = new GeometryRawFile { Width = 129, Depth = 16, Frames = 32, Operation = operation };
        try { workload.Setup(); }
        finally { workload.Cleanup(); }
    }
    Console.WriteLine("All ten 16-bit geometry batch profiles validated; no timing measurement.");
    return;
}

if (args.SequenceEqual(["--validate-warm-webp-controls"]))
{
    WarmWebp.ValidateControls();
    return;
}

if (args.Length is 2 or 3 && args[0] == "--prepare-warm-webp")
{
    WarmWebp.Prepare(int.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture),
        args.Length == 3 ? int.Parse(args[2], System.Globalization.CultureInfo.InvariantCulture) : 75);
    return;
}

if (args.Length == 2 && args[0] == "--prepare-warm-webp-decode")
{
    WarmWebpDecode.Prepare(int.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture));
    return;
}

if (args.Length == 2 && args[0] == "--prepare-warm-jpeg-progressive-gray")
{
    WarmJpegDecode.PrepareProgressiveGray(int.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture));
    return;
}

if (args.Length == 2 && args[0] == "--prepare-warm-jpeg-progressive-420")
{
    WarmJpegDecode.PrepareProgressive420(int.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture));
    return;
}

if (args.Length == 2 && args[0] == "--prepare-warm-jpeg-progressive-444")
{
    WarmJpegDecode.PrepareProgressive444(int.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture));
    return;
}

if (args.Length == 2 && args[0] == "--prepare-warm-jpeg-progressive-422")
{
    WarmJpegDecode.PrepareProgressive422(int.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture));
    return;
}

if (args.Length == 2 && args[0] == "--prepare-warm-jpeg-gray-decode")
{
    WarmJpegDecode.PrepareGray(int.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture));
    return;
}

if (args.Length == 2 && args[0] == "--prepare-warm-jpeg422-decode")
{
    WarmJpegDecode.Prepare422(int.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture));
    return;
}

if (args.Length == 2 && args[0] == "--prepare-warm-jpeg420-decode")
{
    WarmJpegDecode.Prepare420(int.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture));
    return;
}

if (args.Length is 2 or 3 && args[0] == "--prepare-warm-jpeg")
{
    WarmJpeg.Prepare(int.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture),
        args.Length == 3 ? int.Parse(args[2], System.Globalization.CultureInfo.InvariantCulture) : 5);
    return;
}

var representativeSmoke = args.Contains("--representative-smoke");
var representativeShort = args.Contains("--representative-short");
var horizontalFlipSmoke = args.Contains("--horizontal-flip-smoke");
var horizontalFlipShort = args.Contains("--horizontal-flip-short");
var warmSmoke = args.Contains("--warm-smoke");
var warmShort = args.Contains("--warm-short");
var warmConfirm = args.Contains("--warm-confirm");
if (new[] { representativeSmoke, representativeShort, horizontalFlipSmoke, horizontalFlipShort, warmSmoke, warmShort, warmConfirm }.Count(value => value) > 1)
    throw new ArgumentException("Choose one benchmark selection/job.");
var benchmarkArguments = args.Where(argument => argument is not ("--representative-smoke" or "--representative-short"
    or "--horizontal-flip-smoke" or "--horizontal-flip-short" or "--warm-smoke" or "--warm-short" or "--warm-confirm")).ToArray();
var summaries = warmSmoke || warmShort || warmConfirm
    ? WarmBenchmarks.Run(benchmarkArguments, warmSmoke, warmConfirm)
    : horizontalFlipSmoke || horizontalFlipShort
    ? HorizontalFlipBenchmarks.Run(benchmarkArguments, horizontalFlipSmoke)
    : representativeSmoke || representativeShort
    ? RepresentativeBenchmarks.Run(benchmarkArguments, representativeSmoke)
    : BenchmarkSwitcher.FromAssembly(typeof(PngFlipInvocation).Assembly).Run(args).ToArray();
var inspection = args.Contains("--list") || args.Contains("--help") || args.Contains("-h");
if (!inspection && (summaries.Length == 0 || summaries.Any(summary => summary.HasCriticalValidationErrors
    || summary.Reports.Length == 0 || summary.Reports.Any(report => !report.Success))))
    Environment.ExitCode = 1;

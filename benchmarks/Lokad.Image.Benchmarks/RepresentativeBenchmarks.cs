using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;
using Lokad.Image.Tooling;

// Routine runs reuse existing validated bodies; extended matrices remain explicit.
internal sealed class RepresentativeBenchmarks : IFilter
{
    public bool Predicate(BenchmarkCase benchmark)
    {
        var type = benchmark.Descriptor.Type;
        if (type == typeof(MatchedQualityEncodeInvocation)) return true;
        var width = (int)Value("Width");
        var depth = (int)Value("Depth");
        var frames = (int)Value("Frames");
        if (width != 129 || frames != 1) return false;
        if (type == typeof(PngCropResizePipeline)) return depth is 8 or 16 && !(bool)Value("Upscale");
        if (depth != 16) return false;
        if (type == typeof(GeometryRawFile)) return (GeometryOperation)Value("Operation") is GeometryOperation.FlipHorizontal or GeometryOperation.Clockwise90;
        return type == typeof(NearestResizeRawFile) && (bool)Value("Upscale");

        object Value(string name) => benchmark.Parameters.Items.Single(parameter => parameter.Name == name).Value;
    }

    public static Summary[] Run(string[] args, bool smoke)
    {
        if (args.Any(argument => argument is "--job" or "-j" or "--filter" or "-f"))
            throw new ArgumentException("Representative runs use a fixed job and complete selection; use an explicit extended filter for custom runs.");
        var inspection = args.Any(argument => argument is "--list" or "--help" or "-h");
        var runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID");
        if (!inspection && (string.IsNullOrWhiteSpace(runId)
            || runId.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '-')))
            throw new InvalidOperationException("Set a simple unique LOKAD_IMAGE_BENCHMARK_RUN_ID before representative execution.");
        using var identity = new JpegBenchmarkCase();
        var (path, catalog) = MatchedQualityEncodeInvocation.ReadCatalog(identity);
        if (catalog.Scope != "representative")
            throw new InvalidOperationException("Routine benchmarks require the completed twelve-profile representative calibration.");
        var expectedCases = checked(2 * (catalog.Profiles.Sum(row => row.Matches.Count(match => match.Matched)) + 5));
        var job = (smoke ? Job.Dry : Job.ShortRun).WithId(smoke ? "SerialDry" : "SerialShort")
            .WithToolchain(new Toolchain("RepresentativeNet10", new BenchmarkProjectGenerator(identity.Root),
                new DotNetCliBuilder("net10.0"), new DotNetCliExecutor(null!)))
            .WithArguments([new MsBuildArgument("/m:1"), new MsBuildArgument("/p:BuildInParallel=false"),
                new MsBuildArgument("/p:NuGetAudit=false"), new MsBuildArgument("/p:RestoreIgnoreFailedSources=true")]);
        var config = ManualConfig.Create(DefaultConfig.Instance).AddJob(job).AddExporter(JsonExporter.Full).AddFilter(new RepresentativeBenchmarks())
            .WithOptions(ConfigOptions.KeepBenchmarkFiles);
        var summaries = BenchmarkSwitcher.FromTypes([typeof(MatchedQualityEncodeInvocation), typeof(PngCropResizePipeline),
            typeof(GeometryRawFile), typeof(NearestResizeRawFile)]).Run(["--filter", "*", .. args], config).ToArray();
        if (inspection) return summaries;
        var reports = summaries.SelectMany(summary => summary.Reports).ToArray();
        var complete = summaries.Length == 4 && summaries.All(summary => !summary.HasCriticalValidationErrors)
            && reports.Length == expectedCases && reports.All(report => report.Success);
        var directory = Path.Combine(identity.Root, "artifacts", "benchmark-runs", runId!);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "representative-run.json"), JsonSerializer.Serialize(new
        {
            project = "Lokad.Image", runId, complete, smoke, expectedCases, actualCases = reports.Length,
            runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
            calibrationSha256 = Hash(path), benchmarkAssemblySha256 = Hash(typeof(RepresentativeBenchmarks).Assembly.Location),
            librarySha256 = Hash(typeof(Lokad.Image.ImageCodec).Assembly.Location),
            probeSha256 = Hash(identity.Probe), ffmpegSha256 = Hash(identity.Ffmpeg),
            cases = reports.Select(report => new
            {
                type = report.BenchmarkCase.Descriptor.Type.Name,
                method = report.BenchmarkCase.Descriptor.WorkloadMethod.Name,
                parameters = report.BenchmarkCase.Parameters.Items.Select(parameter => new
                {
                    name = parameter.Name,
                    value = Convert.ToString(parameter.Value, CultureInfo.InvariantCulture)
                }).ToArray(), success = report.Success
            }),
            method = smoke ? "Complete representative Dry execution/output correctness only; no performance claim."
                : "Representative Short job; performance conclusions require the dedicated quiet VM and repeatable measurements."
        }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        if (!complete) Environment.ExitCode = 1;
        return summaries;
    }

    private static string Hash(string path)
    {
        using var input = File.OpenRead(path);
        return Convert.ToHexStringLower(SHA256.HashData(input));
    }

    // Avoid recursively discovering projects in unrelated ignored directories.
    private sealed class BenchmarkProjectGenerator(string root) : CsProjGenerator("net10.0", null!, null!, null!)
    {
        protected override FileInfo GetProjectFilePath(Type benchmarkTarget, ILogger logger)
        {
            if (benchmarkTarget.Assembly != typeof(RepresentativeBenchmarks).Assembly)
                throw new InvalidOperationException("Unexpected representative benchmark assembly.");
            return new FileInfo(Path.Combine(root, "benchmarks", "Lokad.Image.Benchmarks", "Lokad.Image.Benchmarks.csproj"));
        }
    }
}

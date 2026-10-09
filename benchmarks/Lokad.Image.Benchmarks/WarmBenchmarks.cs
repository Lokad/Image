using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;
using Perfolizer.Horology;

internal static class WarmBenchmarks
{
    public static Summary[] Run(string[] args, bool smoke, bool confirm)
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Warm native reference runs require the dedicated Linux environment.");
        var arguments = args.ToList();
        var cpuOption = arguments.IndexOf("--warm-cpu");
        if (cpuOption < 0 || cpuOption + 1 == arguments.Count || !int.TryParse(arguments[cpuOption + 1], out var cpu) || cpu is < 0 or > 62)
            throw new ArgumentException("Supply --warm-cpu with one allowed logical CPU (0..62).");
        arguments.RemoveRange(cpuOption, 2);
        var family = "Flip";
        var familyOption = arguments.IndexOf("--warm-family");
        if (familyOption >= 0)
        {
            if (familyOption + 1 == arguments.Count) throw new ArgumentException("Supply a warm family.");
            family = arguments[familyOption + 1]; arguments.RemoveRange(familyOption, 2);
        }
        if (family is not ("Flip" or "Png" or "Jpeg")) throw new ArgumentException("Choose Flip, Png or Jpeg.");
        string? selection = null;
        var selectionOption = arguments.IndexOf("--warm-cases");
        if (selectionOption >= 0)
        {
            if (selectionOption + 1 == arguments.Count || family == "Flip") throw new ArgumentException("Supply explicit codec case names.");
            selection = arguments[selectionOption + 1]; arguments.RemoveRange(selectionOption, 2);
        }
        if (arguments.Any(argument => argument is "--job" or "-j" or "--filter" or "-f"))
            throw new ArgumentException("Warm runs use a fixed narrow selection and job.");
        var runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID");
        if (string.IsNullOrWhiteSpace(runId) || runId.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '-'))
            throw new InvalidOperationException("Set a unique simple benchmark run ID.");
        using var process = Process.GetCurrentProcess();
        var oldAffinity = process.ProcessorAffinity;
        var oldCpu = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU");
        var oldMode = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE");
        var oldCases = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES");
        try
        {
            var affinity = new IntPtr(1L << cpu);
            process.ProcessorAffinity = affinity;
            if (process.ProcessorAffinity != affinity) throw new InvalidOperationException("Requested one-CPU affinity was not applied.");
            Environment.SetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU", cpu.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Environment.SetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE", smoke ? "smoke" : confirm ? "confirm" : "measure");
            Environment.SetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES", selection);
            var expectedPairs = family == "Png" ? WarmPng.SelectedCases().Length : family == "Jpeg" ? WarmJpeg.SelectedCases().Length : 2;
            var type = family == "Png" ? typeof(WarmPng) : family == "Jpeg" ? typeof(WarmJpeg) : typeof(WarmHorizontalFlip);
            using var identity = new JpegBenchmarkCase();
            var job = (smoke ? Job.Dry : Job.ShortRun).WithId(smoke ? "WarmDry" : confirm ? "WarmConfirm" : "WarmShort")
                .WithAffinity(affinity).WithGcServer(false).WithGcConcurrent(false)
                .WithEnvironmentVariable("DOTNET_PROCESSOR_COUNT", "1")
                .WithEnvironmentVariable("DOTNET_TieredCompilation", "0")
                .WithToolchain(new Toolchain("WarmReferenceNet10", new ProjectGenerator(identity.Root),
                    new DotNetCliBuilder("net10.0"), new DotNetCliExecutor(null!)))
                .WithArguments([new MsBuildArgument("/m:1"), new MsBuildArgument("/p:BuildInParallel=false"),
                    new MsBuildArgument("/p:NuGetAudit=false"), new MsBuildArgument("/p:RestoreIgnoreFailedSources=true")]);
            if (!smoke) job = job.WithIterationTime(TimeInterval.FromMilliseconds(200));
            if (confirm) job = job.WithIterationCount(12);
            var config = ManualConfig.Create(DefaultConfig.Instance).AddJob(job).AddExporter(JsonExporter.Full)
                .WithOptions(ConfigOptions.KeepBenchmarkFiles);
            var summaries = BenchmarkSwitcher.FromTypes([type])
                .Run(["--filter", "*", .. arguments], config).ToArray();
            if (arguments.Any(argument => argument is "--list" or "--help" or "-h")) return summaries;
            var reports = summaries.SelectMany(summary => summary.Reports).ToArray();
            var evidenceDirectory = Path.Combine(identity.Root, "artifacts", "benchmark-validation");
            var evidence = Directory.Exists(evidenceDirectory)
                ? Directory.GetFiles(evidenceDirectory, type.Name + "-*.json")
                    .Select(path => (Path: path, Value: Read(path)))
                    .Where(record => record.Value.GetProperty("runId").GetString() == runId).ToArray()
                : [];
            var complete = summaries.Length == 1 && reports.Length == expectedPairs && evidence.Length == expectedPairs
                && reports.All(report => report.Success) && summaries.All(summary => !summary.HasCriticalValidationErrors);
            var pairs = new List<object>();
            if (complete)
                foreach (var report in reports)
                {
                    var caseId = family != "Flip" ? report.BenchmarkCase.Parameters.Items.Single(p => p.Name == "Case").Value.ToString()!
                        : "RGBA" + report.BenchmarkCase.Parameters.Items.Single(p => p.Name == "Depth").Value;
                    var receipt = evidence.Single(e => family != "Flip" ? e.Value.GetProperty("caseId").GetString() == caseId
                        : "RGBA" + e.Value.GetProperty("depth").GetInt32() == caseId);
                    var value = receipt.Value;
                    var depth = value.GetProperty("depth").GetInt32();
                    var outputVerified = family == "Jpeg"
                        ? value.GetProperty("qualityMatched").GetBoolean() && value.GetProperty("alphaExact").GetBoolean()
                            && value.GetProperty("decoderAgreementWithinBounds").GetBoolean()
                            && value.GetProperty("psnrGapDb").GetDouble() <= EncoderQualityCalibration.MaximumPsnrGapDb
                        : value.GetProperty("allRgbaSamplesExact").GetBoolean();
                    if (!outputVerified || !value.GetProperty("sourceUnchanged").GetBoolean()
                        || !value.GetProperty("independentOwnedOutput").GetBoolean() || value.GetProperty("cpu").GetInt32() != cpu
                        || value.GetProperty("nativeExitCode").GetInt32() != 0 || value.GetProperty("smoke").GetBoolean() != smoke)
                        throw new InvalidOperationException("Warm receipt contract differs.");
                    var before = Samples(value, "nativeBefore"); var after = Samples(value, "nativeAfter");
                    if (before.Length != (smoke ? 1 : confirm ? 12 : 3) || after.Length != before.Length)
                        throw new InvalidOperationException("Warm native measurement count differs.");
                    var nativeBefore = before.Average(); var nativeAfter = after.Average();
                    var managed = report.ResultStatistics?.Mean ?? throw new InvalidOperationException("Missing managed statistics.");
                    var ratio = managed / Math.Min(nativeBefore, nativeAfter);
                    pairs.Add(new
                    {
                        caseName = report.BenchmarkCase.DisplayInfo, caseId, depth, cpu,
                        sourceSha256 = value.GetProperty("sourceIdentity").GetProperty("sourceSha256").GetString(),
                        evidenceFile = Path.GetFileName(receipt.Path), evidenceSha256 = JpegBenchmarkCase.HashFile(receipt.Path),
                        managedStatistics = report.ResultStatistics, nativeBeforeNanoseconds = before, nativeAfterNanoseconds = after,
                        ratio = smoke ? (double?)null : ratio,
                        status = smoke ? "execution/output check only" : ratio > 2 ? ">2x-screened" : "<=2x-screened; independent repeat needed"
                    });
                    if (!smoke) Console.WriteLine($"Warm {family}/{caseId}: managed {managed / 1000:F2} us; native before/after {nativeBefore / 1000:F2}/{nativeAfter / 1000:F2} us; worst bracket ratio {ratio:F2}x (screening).");
                }
            var output = Path.Combine(identity.Root, "artifacts", "benchmark-runs", runId, "warm-parity.json");
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            File.WriteAllText(output, JsonSerializer.Serialize(new { schemaVersion = 1, project = "Lokad.Image", runId, complete,
                smoke, confirm, family, cpu, expectedPairs, actualPairs = pairs.Count,
                method = "Managed BenchmarkDotNet elapsed samples and native worker internal monotonic samples, one pinned CPU, complete owned memory outputs. Native pre/post samples bracket managed measurements. IPC/startup/filesystem excluded. Screened rows require independent repeats before certification.", pairs },
                new JsonSerializerOptions { WriteIndented = true, NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals }) + "\n");
            if (!complete) Environment.ExitCode = 1;
            return summaries;
        }
        finally
        {
            Environment.SetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU", oldCpu);
            Environment.SetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE", oldMode);
            Environment.SetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES", oldCases);
            process.ProcessorAffinity = oldAffinity;
        }
    }

    private static double[] Samples(JsonElement receipt, string name)
        => receipt.GetProperty(name).EnumerateArray().Select(sample => sample.GetProperty("NanosecondsPerOperation").GetDouble()).ToArray();
    private static JsonElement Read(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.Clone();
    }
    private sealed class ProjectGenerator(string root) : CsProjGenerator("net10.0", null!, null!, null!)
    {
        protected override FileInfo GetProjectFilePath(Type benchmarkTarget, ILogger logger)
            => new(Path.Combine(root, "benchmarks", "Lokad.Image.Benchmarks", "Lokad.Image.Benchmarks.csproj"));
    }
}

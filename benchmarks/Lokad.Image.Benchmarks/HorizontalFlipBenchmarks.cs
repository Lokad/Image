using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;

internal static class HorizontalFlipBenchmarks
{
    public static Summary[] Run(string[] args, bool smoke)
    {
        if (args.Any(argument => argument is "--job" or "-j" or "--filter" or "-f"))
            throw new ArgumentException("Horizontal flip runs use a fixed six-case selection and job.");
        using var identity = new JpegBenchmarkCase();
        var job = (smoke ? Job.Dry : Job.ShortRun).WithId(smoke ? "FlipDry" : "FlipShort")
            .WithToolchain(new Toolchain("HorizontalFlipNet10", new ProjectGenerator(identity.Root),
                new DotNetCliBuilder("net10.0"), new DotNetCliExecutor(null!)))
            .WithArguments([new MsBuildArgument("/m:1"), new MsBuildArgument("/p:BuildInParallel=false"),
                new MsBuildArgument("/p:NuGetAudit=false"), new MsBuildArgument("/p:RestoreIgnoreFailedSources=true")]);
        var config = ManualConfig.Create(DefaultConfig.Instance).AddJob(job).AddExporter(JsonExporter.Full)
            .WithOptions(ConfigOptions.KeepBenchmarkFiles);
        var summaries = BenchmarkSwitcher.FromTypes([typeof(HorizontalFlipPrimitive), typeof(HorizontalFlipBatch)])
            .Run(["--filter", "*", .. args], config).ToArray();
        if (!args.Any(argument => argument is "--list" or "--help" or "-h")
            && (summaries.Length != 2 || summaries.Sum(summary => summary.Reports.Length) != 6))
            Environment.ExitCode = 1;
        return summaries;
    }

    private sealed class ProjectGenerator(string root) : CsProjGenerator("net10.0", null!, null!, null!)
    {
        protected override FileInfo GetProjectFilePath(Type benchmarkTarget, ILogger logger)
            => new(Path.Combine(root, "benchmarks", "Lokad.Image.Benchmarks", "Lokad.Image.Benchmarks.csproj"));
    }
}

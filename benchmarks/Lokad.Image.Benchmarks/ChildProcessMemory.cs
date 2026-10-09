using System.Globalization;
using System.Text;
using Lokad.Image.Tooling;

// Development-only kernel counters, collected in separate untimed executions.
internal static class ChildProcessMemory
{
    public sealed record Sample(long? PeakResidentBytes, long? MajorPageFaults, long? MinorPageFaults,
        string Scope, string? CounterExecutableSha256, string? CounterVersion);

    public static Sample Measure(string executable, string[] arguments, string directory, byte[]? expectedStdout = null, TimeSpan? timeout = null)
    {
        if (!OperatingSystem.IsLinux())
            return new(null, null, null, "Not collected: kernel child-process counters require the Linux benchmark VM.", null, null);
        const string counter = "/usr/bin/time";
        if (!File.Exists(counter)) throw new InvalidOperationException("Linux child-memory validation requires GNU time at /usr/bin/time.");
        var output = Path.Combine(directory, "child-memory-" + Guid.NewGuid().ToString("N") + ".txt");
        var stdout = OracleProcess.Run(counter, ["--output", output, "--format", "peakResidentKiB=%M\nmajorPageFaults=%F\nminorPageFaults=%R\nexitCode=%x", "--", executable, .. arguments], timeout: timeout);
        if (expectedStdout is not null && !stdout.AsSpan().SequenceEqual(expectedStdout))
            throw new InvalidOperationException("Child-memory counter execution changed the validated standard output.");
        var values = File.ReadAllLines(output).Select(line => line.Split('=', 2)).ToDictionary(pair => pair[0], pair => pair[1]);
        if (Number("exitCode") != 0 || Number("peakResidentKiB") <= 0) throw new InvalidOperationException("Child-memory counters are invalid.");
        var version = Encoding.UTF8.GetString(OracleProcess.Run(counter, ["--version"])).Replace("\r\n", "\n").Trim();
        return new(checked(Number("peakResidentKiB") * 1024), Number("majorPageFaults"), Number("minorPageFaults"),
            "GNU time wait4/rusage maximum resident set for the complete codec child process and its threads; includes runtime, JIT, native libraries, buffers and file mappings. Separate from the BenchmarkDotNet caller's allocations. One untimed sample, not an allocation count or statistical memory budget.",
            JpegBenchmarkCase.Hash(File.ReadAllBytes(counter)), version);

        long Number(string key) => long.Parse(values[key], NumberStyles.None, CultureInfo.InvariantCulture);
    }
}

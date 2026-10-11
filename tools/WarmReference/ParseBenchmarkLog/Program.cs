using System.Text.Json;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Reports;

if (args.Length != 2)
    throw new ArgumentException("Usage: ParseBenchmarkLog LOG OUTPUT.json (one twelve-iteration confirmation).");
if (File.Exists(args[1]))
    throw new IOException("The output already exists; preserve previous evidence.");

var lines = File.ReadAllLines(args[0]);
// Parse elapsed nanoseconds and operation counts with the same pinned BDN API
// as the generated child. Printed per-operation values are rounded.
var measurements = lines
    .Where(line => line.StartsWith("WorkloadActual", StringComparison.Ordinal)
        || line.StartsWith("WorkloadResult", StringComparison.Ordinal))
    .Select(line => Measurement.Parse(line, 0)).ToArray();
var actual = measurements.Where(m => m.IterationStage == IterationStage.Actual).ToArray();
var results = measurements.Where(m => m.IterationStage == IterationStage.Result).ToArray();
if (actual.Length != 12 || results.Length is < 8 or > 12
    || measurements.Any(m => m.Operations <= 0 || m.Nanoseconds <= 0 || !double.IsFinite(m.Nanoseconds)))
    throw new InvalidDataException("Expected twelve actual iterations and eight to twelve filtered results.");

// WorkloadResult already contains the engine's overhead adjustment and outlier
// selection. Apply Statistics to those values, as BDN's full report does.
var statistics = new Statistics(results.Select(m => m.Nanoseconds / m.Operations));
var evidence = new
{
    statistics, actual, results,
    rawGc = lines.Single(line => line.StartsWith("// GC:", StringComparison.Ordinal)),
    rawThreading = lines.Single(line => line.StartsWith("// Threading:", StringComparison.Ordinal))
};
File.WriteAllText(args[1], JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine($"Parsed {actual.Length} actual iterations and {results.Length} filtered results.");

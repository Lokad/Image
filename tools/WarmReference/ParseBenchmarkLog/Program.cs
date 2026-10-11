using System.Text.Json;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Reports;

if (args.Length is not (2 or 3))
    throw new ArgumentException("Usage: ParseBenchmarkLog LOG OUTPUT.json [3|12] (default twelve iterations).");
var expectedActual = args.Length == 3 ? int.Parse(args[2]) : 12;
if (expectedActual is not (3 or 12))
    throw new ArgumentException("Only fixed three- or twelve-iteration warm jobs are supported.");
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
if (actual.Length != expectedActual || results.Length < (expectedActual == 12 ? 8 : 2) || results.Length > expectedActual
    || measurements.Any(m => m.Operations <= 0 || m.Nanoseconds <= 0 || !double.IsFinite(m.Nanoseconds)))
    throw new InvalidDataException("Actual/result counts differ from the declared warm job.");

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

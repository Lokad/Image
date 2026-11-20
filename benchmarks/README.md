# Development benchmarks

The [0.2.1 benchmark snapshot](../docs/benchmark.md) reports performance and
allocations for the immutable package. The [evidence index](final-package-benchmarks.json)
records the package identity and archive digests. The
[finite coverage inventory](WARM-COVERAGE.md) records passing evidence for all
289 named paths. The [historical inventory](https://github.com/lokad/image/blob/27fc0425be9e21d913e7bd049fe3ce629bf28955/docs/benchmark-pre-cleanup.md)
preserves earlier controls and reports with their recorded producers, settings
and targets.

Run performance only on the dedicated quiet VM, sequential Release and one CPU
per operation. Use tools/WarmReference/capture-vm-environment.py's versioned
idle/process guard; do not wait for our own historical load average to decay.
No builds/sync/profilers during scoring. Short BenchmarkDotNet smoke runs verify
execution/output; local timings are not published performance evidence.

Measure equivalent complete operations, including normalization, input delivery,
EOF and independent output ownership. Preserve frozen inputs/reference settings.
Check exact packets and pixels before timing. Reference exceptions and bounded
native-only editing tolerances remain explicitly documented. Record managed
allocation, encoded bytes and quiet guards alongside elapsed time; allocation
is separate from cooperative admission capacity.

Resolve uncertain measurements before tuning. Investigate substantial repeated
work or storage with coherent architectural changes and narrow controls. Reuse an immutable verified build
through a bounded experiment. Independently confirm promising changes; qualify
accepted production changes on Windows/Linux, scalar paths and the actual Release
package. Stop passing cases; add a workload only for a concrete missing required
algorithm or public boundary. Compression correctness precedes CPU gains.
Temporary regressions may be resolved across a coherent experiment. Judge completed
stages on elapsed time, allocations, retention and simplification together; preserve
adverse evidence and distinguish failed designs from inconclusive measurements.

Generated cached selectors can bind fixed parameter values. Verify their prepared
case and ordinal before reuse; changing the case environment filter does not
rebind them. Check the actual case in each receipt and exclude mismatched runs.

Regenerate docs/benchmark.md with `python benchmarks/render-benchmark.py` and
verify with `--check` after accepted rounds. Archived reports/raw JSON are immutable
history. The ledger's `rawArchiveRoot` points to the ignored evidence store.
Each `archive` is relative to that root. Keys under `members` are entry names
inside the archive, including some original paths from the VM. Verify the
archive and extracted bytes against their respective SHA-256 digests.
The former long narrative and retired experiment recipes remain in
[39402e7](https://github.com/lokad/image/blob/39402e74e0f95c82ab209e5a950066a206c2af65/benchmarks/README.md).
Tool sources and fixture provenance remain beside their respective projects.
The single [predictive PNG control](PREDICTIVE-PNG-CURRENT.md) has its own compact
preparation command and frozen mixed-prediction reference; original None workers
remain unchanged.
Published packages and cited raw masters are retained; no arbitrary checkout-size
ceiling blocks routine verification.

See the [image-read report](WARM-IMAGE-READ.md) for its preparation recipe and
the twelve declared control boundaries.

`./bench.ps1 -Smoke` validates five original process-level PNG and editing
workloads. Use an explicit warm family or filter for other workloads. The
[historical matched-quality report](RESULTS.md) retains its inputs and method.
Its calibration and matched-quality commands are available in the
[last checkout with those tools](https://github.com/lokad/image/tree/0dfbdabde827219970880342c5a3148fbfd7dc4d).

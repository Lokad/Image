# Development benchmarks

[Current state](../docs/benchmark.md) gives the release snapshot, allocations and
exact packet scope. [Finite coverage](WARM-COVERAGE.md) defines outstanding
required paths; [historical inventory](https://github.com/lokad/image/blob/27fc0425be9e21d913e7bd049fe3ce629bf28955/docs/benchmark-pre-cleanup.md) preserves
individual controls/reports. Earlier reports describe their recorded producers,
settings and targets, not current 0.2.0 proof. Raw evidence and rejected trials stay.

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

Resolve uncertain measurements before tuning. Optimize a demonstrated substantial
gap using one primitive and narrow controls. Reuse an immutable verified build
through a bounded experiment. Independently confirm promising changes; qualify
accepted production changes on Windows/Linux, scalar paths and the actual Release
package. Stop passing cases; add a workload only for a concrete missing required
algorithm or public boundary. Compression correctness precedes CPU gains.

Regenerate docs/benchmark.md with `python benchmarks/render-benchmark.py` and
verify with `--check` after accepted rounds. Archived reports/raw JSON are immutable
history. The former long narrative and retired experiment recipes remain in
[39402e7](https://github.com/lokad/image/blob/39402e74e0f95c82ab209e5a950066a206c2af65/benchmarks/README.md).
Tool sources and fixture provenance remain beside their respective projects.
Published packages and cited raw masters are retained; no arbitrary checkout-size
ceiling blocks routine verification.

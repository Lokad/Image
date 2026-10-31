# Current byte bicubic one-axis costs

These timings precede the [exact-alpha producer](WARM-EXACT-ALPHA-INT128.md), which supplies the latest costs for these paths. Original evidence is retained.

Both existing one-axis strip controls now have independent current costs on the
accepted fixed-certificate build. Both remain **confirmed gaps** against the ≤1×
target. This resolves the timing uncertainty created by the changed guard; it
establishes no causal speedup or regression against older producers.

| Complete warm single-CPU resize | Lokad µs | FFmpeg backend µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| TightHorizontalUp8 | 1,800.860 | 1,515.490 | 1.188 / 1.235 | 1,178,374 |
| TightVerticalUp8 | 2,374.698 | 1,732.263 | 1.371 / 1.379 | 1,280,363 |

The original513×259 RGBA8 becomes1027×259 or513×519. Local working limits remain
**1,599,536/1,600,552 bytes**, source+output+**4 KiB**. Public Resize, admission,
geometry, filtering, independent output and disposal are timed. The existing
zimg backend used by FFmpeg zscale includes halo/packing/premultiplication,
filtering, independent planar/packed outputs, unrounded-alpha normalization,
quantization and release. Native allocation is unmeasured; preparation/I/O/startup
and validation are excluded. No workload or quality settings change.

Horizontal pixels match the independent rational frame exactly. Vertical pixels
match the qualified original frame exactly, including the **one already-certified
exact rational RGB half** retaining declared binary64 rounding. Source/output/
native hashes and native max1/RMSE≤0.25 are unchanged, including normal/scalar
bounds. Source immutability and independently owned output checks pass.

One twelve-actual confirmation per case retains all actuals, filtered/all
statistics, native brackets and passing idle-v2 guards. **Zero builds**: reuse
the accepted immutable library, harness, cached selectors, workers and fixtures.
Unchanged full5,245/live checks per OS and the same actual Release-package
consumer/three-example evidence are reused from the
[accepted guard change](WARM-BICUBIC-BYTE-FIXED-CERTIFICATE.md).

Reference reconciliation is explicit: the shared worker source subsequently
added an inactive1×1 macro branch, an equivalent HALO_Y=HALO alias for these axial
controls and readiness-only haloX/Y metadata. Compiler, flags, build scripts,
zimg archive/version and complete timed processing/settings stay unchanged.
Archived workers fail the current source-identity gate before actuals; current
source/binary-verified installed workers are used. Preparation failures remain
in raw evidence; no successful score is repeated and no gate is weakened.

These are two existing distinct required boundaries, bringing the summary to
**66 unique comparisons** while the full268-path objective stays unchanged.
The two-axis tight-byte gap1.565× remains larger than either axial gap.

[Numeric evidence](current-byte-bicubic-axial-costs-evidence.json) records exact
producers, precision, reference reconciliation, allocations and qualification reuse.
Raw **current-byte-bicubic-axial-costs.tar.xz**, SHA256**d103ea87d229fbb05b77674342bd34a6722feb54d00374d2052a6b80fc2752a5
**, is mirrored and member-verified with sources, workers, selectors, fixtures,
scores, guards and preparation failures. Reproduce existing
WarmBicubicResize/TightHorizontalUp8 and TightVerticalUp8 using the
[reference tooling](../tools/WarmReference/README.md).

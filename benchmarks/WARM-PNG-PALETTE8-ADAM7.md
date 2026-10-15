# Palette8 Adam7 decoding

The short screen measures **1.303× FFmpeg**, at **4,960 managed B/op**.
This pair remains **unconfirmed**: the independent run has an unexplained native
timing anomaly. Production is unchanged; no speedup is claimed.

| Observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Three actuals | 1,838.89 | 1,410.80 | 1.303 / 2.651 | 4,960 | Screen only |
| Independent twelve actuals | 1,750.60 | 8,900.36 | — | 4,960 | Unscored: native timing anomaly |
| Twelve actuals with process monitoring | 1,708.25 | 1,462.48 | — | 4,960 | Diagnostic only |

The existing original 513×259 byte raster and palette projection are reused:
index `i` is original red, palette RGB is `(i,255-i,(73*i+19)%256)`, and the full
tRNS table sets alpha to `i`. Nonzero hidden RGB is preserved. Original authoring
follows [PNG Adam7](https://www.w3.org/TR/png-3/): seven specified passes, filter
None and BCL Optimal level6. Every pass sample, filter, palette/alpha entry count,
geometry and CRC is checked. There is one new workload and no new original raster,
depth, shape, filter or effort grid.

Both subjects time complete identical-packet decoding, inflation, reconstruction,
palette expansion, scatter and independent output acquisition/release. Native
codec/input packet/frame wrappers are reused; actual PAL8 output is checked and
full PAL8-to-owned-RGBA conversion runs inside every operation. No decoded output
is cached. Startup, I/O, IPC, fixture authoring and validation are excluded.
Native allocations are **unmeasured**; managed figures include harness overhead.

Exact analytical RGBA, normal/scalar native CLI pixels, alpha/hidden RGB, source
preservation and two simultaneous independent owners pass. Sequential Release
runs use one pinned CPU, one codec/conversion thread, normal SIMD, disabled tiering
and workstation nonconcurrent GC. Both versioned quiet guards pass for each run.
All actuals, including both sets of twelve, are retained. The short upper uses
selected and all-actual intervals against the faster native bracket's lower bound.

The first independent native bracket was over six times slower despite identical
input, production DLL, harness and native binary hashes. Its favorable ratio is
not accepted. Two bounded native-only processes return to approximately 1.34 ms
with matching wall and CPU time; a parent/child CPU diagnostic also returns to
normal timing. That monitored run is retained as a diagnostic, not substituted
for a clean confirmation. The cause remains unresolved; no repeated confirmation
or production tuning follows this investigation.

Initial preparation correctly refused to overwrite the frozen noninterlaced
palette packet. A separate Adam7 directory fixes the collision; the refusal is
retained. Only the benchmark selector/authoring and native adapter admission guard
change. One native adapter build and two necessary harness builds suffice; the
current verified DLLs are preloaded into each measured child. Existing **3,432
Release tests per OS**, **898 live comparisons**, and the same actual package's
consumer plus all three examples on Windows/Linux are reused from the
[JPEG qualification](WARM-JPEG-BASELINE-AC-RUN.md). No production rebuild, full
suite, package proof or method audit is repeated.

[Numeric evidence](warm-png-palette8-adam7-evidence.json) retains every actual,
allocation, guard, identity, refusal and diagnostic. Leave this pair open;
prioritize a materially different workload before further tuning here.

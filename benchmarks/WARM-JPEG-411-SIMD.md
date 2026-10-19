# Exact SIMD JPEG 4:1:1 rows

Complete JPEG 4:1:1 decode takes **82.6% less time**, 3,527.46→613.09µs.
The gap falls from7.771x to **1.352x / upper1.587x**. It remains above parity;
no long confirmation was run. Two controls refresh the common EOI selector.

| Complete resident JPEG operation | Lokad / FFmpeg µs | Ratio / upper | Lokad allocated B/op | JPEG / RGBA bytes |
| --- | ---: | ---: | ---: | ---: |
| H4V1 / 4:1:1 → owned RGBA8 | 613.09 / 453.46 | 1.352 / 1.587 | 751,279 | 3,904 / 531,468 |
| Baseline 4:2:0, selector rejection | 1,210.66 / 1,011.13 | 1.197 / 1.308 | 757,345 | 52,093 / 531,468 |
| Progressive 4:4:4 / live token, selector rejection | 5,069.78 / 3,349.49 | 1.514 / 1.611 | 984,298 | 132,961 / 531,468 |

The retained profile located90.0% in scalar interpolation, rounding and rendering.
One primitive now reconstructs eight pixels from four quarter-width chroma
samples using exact integer-eighth weights, then preserves every separate binary64
matrix operation and final-only rounding. It packs directly to RGBA. Four-byte
chroma reads stay inside valid samples; scalar borders/tails and hardware-off
fallback preserve the same contract. No FMA, reduced precision, heap workspace,
pool policy or resource-charge change.

A narrow EOI selector requires full-resolution Y, quarter-width/full-height Cb/Cr,
YCbCr color and supported SIMD. The existing general Render loop, entropy decoding,
inverse transforms and materializer are unchanged. The new renderer uses the same
three-component materialization walk and preserves allocation/admission/release
order, orientation metadata and work charges before each output row.

Same original packet, exact analytical pixels, frozen harness, native worker,
normal/scalar CLI checks and compiled selectors as the
[baseline](WARM-JPEG-411.md). FFmpeg still times software MJPEG decoding plus
explicit centered516-column conversion and in-place compaction to513. Both sides
include complete decode, normalization, fresh independent owned output and release;
one CPU/thread, with startup, I/O, IPC and checks excluded. All three411 pixel
channels and alpha match exactly. No pixel gate or baseline adjustment changed.

Three short screens retain all nine actuals/outliers, native pre/post brackets and
idle-v2 guards. The control reader was corrected for existing PascalCase native
statistics after its first measurement completed; that measurement was recovered,
not repeated. One immutable producer per OS; no new profile, harness, worker,
selector or fixture build during this round.

Accepted qualification: **4,210 tests/OS** (1,437 internal,1,845 public,928 live
FFmpeg), **97 hardware-off checks/OS**, and the SAME actual private Windows package
consumer/all three examples on both OS. Tests cover randomized exact binary64
rows, clipping, unaligned spans, guards, all width residues, baseline/progressive
partial blocks, surviving owners, shared budgets, work exhaustion and cancellation.
The released0.1.0 package/tag remain unchanged.

Invalidation is limited to the owned H4V1 color path; baseline420 and progressive
444/live-token selector controls refresh. The other223 numeric rows and all70
passing proofs retain evidence: other layouts reject the one-time selector and
keep their existing hot loops, four-component rendering returns before it, and
caller samples use a separate entry. Scope247=226numeric+21unmeasured remains
70passes/155mean gaps/one confidence-open. Next: the named H4V2 JPEG410 boundary.

[Numeric evidence](warm-jpeg-411-simd-evidence.json). Raw retained on both hosts:

- `h10x-measurements.tar.gz`: SHA `83dd30fa8f8e3845f717ec99cd98f39da2f476b21015f28dcd31a61bdd0f8171`.
- `h10x-qualified-windows.tar.gz`: SHA `6991e4e69197a9bb79d38d35674484a9ac8e770dbc6331c20bb3a2f99a198abd`.
- `h10x-qualified-linux.tar.gz`: SHA `96af0dbec0293069b6f4675bdd5c35231c53fbc5c0130e909c2af2e91371220f`.

The unchanged native binary/fixtures remain in the baseline archive cited above.

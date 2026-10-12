# Rejected packed-palette table trial

This independently implemented trial built packed RGBA words once per wide
palette8 pass, replacing per-pixel RGB/tRNS assembly. At most 1 KiB of optional
scratch was admitted after the existing row buffers. Tight memory and cancelable
operations retained the original path; construction followed first-row work
admission. Existing invalid-index prefix writes and logical work stayed unchanged.
FFmpeg's packed palette representation informed the investigation; no upstream
implementation or tables were copied.

One three-actual screen reused the existing 513x259 palette8 PNG, normal SIMD,
one CPU and the same full resident-input/owned-output decode boundary. All pixels,
alpha/hidden RGB, source and held owners are exact. The pre-guard passes, but the
post-guard **fails**: aggregate I/O wait is 1.45%, above the unchanged 1% limit.
No overlapping build/codec/profiler is observed. The raw result is **unscored**.

| Retained unscored trial | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Packed-palette candidate, post-guard failed | 788.31 | 360.13 | 2.189 / 2.408 | 89,562 |

The observed mean also remains above 2x. No long confirmation or repeated screen
was launched. The prototype was **reverted**, and the summary retains the
accepted 2.433x palette8 result. This trial establishes no durable performance
gain or parity. All actuals and the original failed guard remain unchanged;
native allocation totals are unmeasured.

Candidate row controls and 34 focused public PNG tests pass on Windows and Linux,
with SIMD enabled and disabled. After exact source restoration and forced
rebuilds, all 582 production method bodies/flags match the accepted actual
package on both systems. Its full-suite/package validation is reused.

Two useful wide-palette public controls remain, covering noninterlaced/Adam7
pixels, the exact original memory peak and one-byte-below failure, plus the
minimum successful work budget against the cancelable path. No optional table
or additional workspace remains in production.

The [numeric evidence](warm-png-palette-word-rejected-evidence.json) preserves
source/input/derived-raster hashes, raw archive, four timed child identities,
failed guard, candidate package and restoration proofs. No original performance
image was added. Next investigate remaining compressed-input allocation or
substantial shared work; the full required uniform warm scope remains open.

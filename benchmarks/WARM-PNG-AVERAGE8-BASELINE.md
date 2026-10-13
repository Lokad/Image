# PNG Average RGBA8 with the current reconstruction path

One new material case uses the existing original 513x259 RGBA8 raster unchanged.
Original authoring follows [PNG sections 9.2/9.3, type 3 Average](https://www.w3.org/TR/png-3/):
subtract floor((left+up)/2) modulo 256, with integer sums retaining at least nine bits
and zero left/top boundaries. The input is noninterlaced RGBA8, every row Average,
zlib level 6. Frozen Sub/Up/Paeth8/16 encoded/raster bytes remain exact. No original
performance image is added; the total stays two original rasters. Production,
package and native worker are unchanged.

Complete resident public Decode/owned-output disposal is timed. FFmpeg decodes
directly to RGBA8 and includes independent output acquisition/release; contexts,
input packet and frame wrappers are reused. Full pixels/alpha/hidden RGB, native
CLI normal/scalar pixels, source and held owners are exact. Startup/I/O/IPC/
preparation/validation stay outside timing.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 3228.76 | 1835.00 | 1.760 / 2.308 | 12,976 |
| Independent twelve-actual confirmation | 3137.74 | 1841.23 | 1.704 / 1.743 | 12,976 |

This fixed pair is **confirmed: 1.704x / upper 1.743x**, with **12,976 B/op**.
Both guards pass; all twelve actuals and eleven filtered results remain retained.
Upper takes the larger filtered/all-actual bound. Allocation includes harness
overhead; native totals are unmeasured. Stop timing/tuning this pair. First-use,
held-output, tight-budget and other coded layouts remain distinct.

The existing generic reconstruction already meets this fixed target; no production
primitive was changed. Forced fresh and both timed children match all 595 methods
and flags of the same actual [palette1 Release package](WARM-PNG-PALETTE1-ROW.md).
Its 2,682 tests per OS, focused normal/scalar controls, original authored profiles
and all three package usage examples remain validation. No full suite or pack was
repeated for this harness-only round.

The [numeric evidence](warm-pngaverage8-baseline-evidence.json) retains every actual/
bracket, source/input/raster/child/package hash and immutable raw archives. Earlier
failures/rejections and descriptive recipe errata remain unchanged. Each of five
filter types now has a confirmed RGBA8 case; other material depths/layouts, PNG
encoding projections, JPEG420's upper-bound gap and the full required scope remain
open. This fixed confirmation is not uniform warm parity; RGB8 PNG encoding is
the next distinct material screen.

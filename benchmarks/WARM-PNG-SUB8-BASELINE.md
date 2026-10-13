# PNG Sub RGBA8 with the current reconstruction path

One new material case uses the existing original 513x259 RGBA8 raster unchanged.
Original authoring follows [PNG section 9.2, type 1 Sub](https://www.w3.org/TR/png-3/):
subtract each byte four positions earlier in the raw row modulo 256, with zero
left bytes for the first pixel. The input is noninterlaced RGBA8, every row Sub,
zlib level 6. Frozen Up8 and Paeth8/16 encoded/raster bytes remain exact. No original
performance image is added; the total stays two original rasters. Production,
package and native worker are unchanged.

Complete resident public Decode/owned-output disposal is timed. FFmpeg decodes
directly to RGBA8 and includes independent output acquisition/release; contexts,
input packet and frame wrappers are reused. Full pixels/alpha/hidden RGB, native
CLI normal/scalar pixels, source and held owners are exact. Startup/I/O/IPC/
preparation/validation stay outside timing.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 1223.19 | 681.57 | 1.795 / 2.061 | 12,976 |
| Independent twelve-actual confirmation | 1209.60 | 682.52 | 1.772 / 1.780 | 12,976 |

This fixed pair is **confirmed: 1.772x / upper 1.780x**, with **12,976 B/op**.
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

The [numeric evidence](warm-pngsub8-baseline-evidence.json) retains every actual/
bracket, source/input/raster/child/package hash and immutable raw archives. Earlier
failures/rejections and descriptive recipe errata remain unchanged. JPEG420's
upper-bound gap and the complete required material scope remain open. This fixed
confirmation is not uniform warm parity; PNG Average RGBA8 is the next distinct
reconstruction screen.

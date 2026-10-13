# Packed palette2 row expansion

One new material case uses the existing original 513x259 RGBA8 raster: red's top
two bits select v=0/85/170/255, RGB=(v,255-v,73*v+19 mod256), alpha=v. Original
authoring produces noninterlaced two-bit palette PNG, full four-entry PLTE/tRNS,
filter None and zlib level6. Existing palette4/8 encoded/raster bytes remain exact.
No original performance image is added; the total stays two original rasters.

Complete resident public Decode/owned-output disposal is timed. FFmpeg includes
PAL8 unpacking, one-thread conversion and independent output acquisition/release;
contexts and frame wrappers are reused. Full pixels/alpha/hidden RGB, native CLI
normal/scalar pixels, source and held owners are exact. Startup/I/O/IPC/preparation/
validation stay outside timing. The development worker's legal-depth whitelist
was extended; its timed native work is unchanged and is never a production input.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Scalar baseline | 330.40 | 97.51 | 3.388 / 3.844 | 9,208 |
| Sixteen-pixel SIMD screen | 77.02 | 98.58 | 0.781 / 1.017 | 9,208 |
| Independent twelve-actual confirmation | 76.33 | 98.66 | 0.774 / 0.789 | 9,208 |

The screen reduces decode mean **76.7%**. The independent confirmation is
**0.774x / upper 0.789x**, with **9,208 B/op** unchanged: this fixed pair is
**confirmed**. Both guards pass; all twelve actuals and nine filtered results
remain retained. Upper takes the larger filtered/all-actual bound. Allocation
includes harness overhead; native totals are unmeasured. Stop tuning this pair.
Held-output, first-use, tight-budget and partial-palette/alpha paths remain distinct.

One primitive expands four packed input bytes into sixteen MSB-first two-bit
indices through safe SSE2 shifts/unpacks, then selects exact RGBA channels through
SSSE3 register lookups. Full four-entry palettes with absent/full tRNS qualify;
partial palettes/alpha, other CPUs and tails retain scalar validation. No new
workspace, admission, work charge, ownership or error-order changes occur.
Cancellation still checks every 4,096 pixels. Baseline JIT retained per-pixel bit
offset/shift, palette assembly and bounds checks; candidate JIT and exact row
controls prove the vector path. These diagnostics are not timing claims.

Each OS passes **2,677 Release tests** (910 internal, 871 public, 896 live FFmpeg),
48 focused public PNG controls and 62 packed-row controls normally and with SIMD
disabled. Original authored profiles, bit/lane order, padding, unaligned guards,
vector/row tails, partial alpha, invalid-index prefix and canceled vector paths
pass. The same actual dependency-free Release package and all three usage examples
pass on both OSes. All 595 final fresh/timed method bodies and flags match it.

An initial development-worker whitelist rejection is frozen with its four prepared
child identities and native binary; it launched no scored timing and is retained
as a failure. The [numeric evidence](warm-pngpalette2-row-evidence.json) retains it,
every actual/bracket, source/input/raster/child/package hash and frozen raw/JIT/QA
archives. JPEG420's upper-bound gap and the complete required material scope remain
open. This fixed-case confirmation does not establish uniform warm parity.

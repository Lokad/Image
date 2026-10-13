# Packed palette1 row expansion

One new material case projects the existing original 513x259 RGBA8 raster:
red's top bit selects v=0/255, RGB=(v,255-v,73*v+19 mod256), alpha=v. Original
authoring produces noninterlaced one-bit palette PNG, full two-entry PLTE/tRNS,
filter None and zlib level6. Frozen palette2/4/8 encoded/raster bytes remain exact.
No original performance image is added; the total stays two original rasters.

Complete resident public Decode/owned-output disposal is timed. FFmpeg includes
PAL8 unpacking, one-thread conversion and independent output acquisition/release;
contexts and frame wrappers are reused. Full pixels/alpha/hidden RGB, native CLI
normal/scalar pixels, source and held owners are exact. Startup/I/O/IPC/preparation/
validation stay outside timing. The existing development native worker is unchanged
and is never a production input.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Scalar baseline | 325.61 | 87.09 | 3.739 / 4.800 | 9,072 |
| Sixteen-pixel SIMD screen | 62.16 | 87.14 | 0.713 / 0.975 | 9,072 |
| Independent twelve-actual confirmation | 61.91 | 86.80 | 0.713 / 0.723 | 9,072 |

The screen reduces decode mean **80.9%**. Independent confirmation is
**0.713x / upper 0.723x**, with **9,072 B/op** unchanged: this fixed pair is
**confirmed**. Both guards pass; all twelve actuals and ten filtered results
remain retained. Upper takes the larger filtered/all-actual bound. Allocation
includes harness overhead; native totals are unmeasured. Stop tuning this pair.
Held-output, first-use, tight-budget and partial-palette/alpha paths remain distinct.

One primitive repeats each of two packed bytes over eight SIMD lanes, tests its
MSB-first bits and selects exact RGBA channels through safe SSE2/SSSE3 register
lookups. Full two-entry palettes with absent/full tRNS qualify; partial palettes/
alpha, other CPUs and tails retain scalar validation. No new workspace, admission,
work charge, ownership or error-order changes occur. Cancellation still checks
every 4,096 pixels. Baseline JIT retained per-pixel bit extraction/palette assembly/
bounds checks; candidate JIT and exact row controls prove the vector path. These
diagnostics are not timing claims.

Each OS passes **2,682 Release tests** (915 internal, 871 public, 896 live FFmpeg),
48 focused public PNG controls and 67 packed-row controls normally and with SIMD
disabled. Original authored profiles, bit/lane order, padding, unaligned guards,
vector/row tails, arbitrary alpha and unrelated channels, partial alpha, invalid-
index prefix and canceled vector paths pass. The same actual dependency-free
Release package and all three usage examples pass on both OSes. All 595 final fresh/
timed method bodies and flags match it.

The [numeric evidence](warm-pngpalette1-row-evidence.json) retains every actual/
bracket, source/input/raster/child/package hash and immutable raw/JIT/QA archives.
Earlier failures/rejections stay unchanged. JPEG420's upper-bound gap and the
complete required material scope remain open. This fixed-case confirmation does
not establish uniform warm parity; packed gray2 is the next distinct screen.

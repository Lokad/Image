# Warm RGBA8 JPEG422 adaptive Huffman revalidation

The shared adaptive encoder independently confirms **1.821× FFmpeg**, conservative
upper **1.862×**, averaging **4,277.49 µs** with **294,110 managed bytes allocated
per operation**. Both quietness guards pass; all twelve actual iterations are
retained, including three excluded by the standard statistical filter. The upper
uses the larger filtered/all-actual bound. Stop tuning this fixed pair; uniform
warm parity remains open.

| Stage | Lokad µs | Ratio / upper | Managed allocated B/op |
| --- | ---: | ---: | ---: |
| Earlier fixed-table confirmation | 4,632.95 | 1.963 / 1.990 | 527,307 |
| Adaptive three-actual screen | 4,298.31 | 1.831 / 2.986 | 294,110 |
| Adaptive independent confirmation | 4,277.49 | 1.821 / 1.862 | 294,110 |

The original 513×259 RGBA8 raster, managed quality **65**, native qscale 5 and
background RGB 0x204060 are unchanged. Preparation evaluates this single existing
anchor, with no quality sweep or extra image. The RGB source-error gap remains
**0.245991 dB**, within the unchanged 0.25 dB gate. The original rejected
quality-69 comparison remains recorded in the
[previous round](WARM-JPEG422-ENCODE.md).

Blending, color conversion, sampling, transform, histogram/table construction,
entropy coding and independent output acquisition/release are timed. Independent
entropy recoding reproduces the adaptive packet byte for byte. Every coefficient
and amplitude is unchanged; normal and scalar FFmpeg decoding preserve the
previous decoded pixels exactly. Alpha, geometry, decoder agreement, unchanged
source and simultaneously held independent output checks pass. Private event
storage is reused, with contents recomputed for every operation.

Managed packets shrink from **181,958 to 90,214 bytes**; native packets remain
71,168 bytes. This is a source-quality timing comparison; compression efficiency
parity is not established. Allocations decrease **44.2%** from the earlier
confirmation. Production is unchanged from the
[accepted adaptive round](WARM-JPEG-ADAPTIVE-HUFFMAN.md). Fresh build and both
actual children match all 629 bodies and implementation flags in the tested
Release package. Its 2,767 tests and three actual-package usage examples per OS
are reused.

Sequential Release runs use one pinned CPU, one native codec/conversion thread,
normal SIMD, tiering off and workstation nonconcurrent GC. Startup, I/O, IPC,
fixture preparation and checks are excluded; native allocations are unmeasured.
Raw evidence, source, actual package and all four child files were frozen before
subsequent preparation. The [numeric evidence](warm-jpeg4228-adaptive-evidence.json)
retains both runs, including the screen's wide native uncertainty, and their
identities. Next, revalidate JPEG444 RGBA8 with its existing quality-69 anchor.
JPEG420 RGBA8's upper-bound gap and the remaining material contract remain open.

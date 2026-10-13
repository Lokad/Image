# Warm RGBA16 JPEG444 adaptive Huffman revalidation

The shared adaptive encoder independently confirms **1.757× FFmpeg**, conservative
upper **1.790×**, averaging **6,767.17 µs** with **556,280 managed bytes allocated
per operation**. Both quietness guards pass; all twelve actual iterations are
retained. Stop tuning this fixed pair. Uniform warm parity remains open.

| Stage | Lokad µs | Ratio / upper | Managed allocated B/op |
| --- | ---: | ---: | ---: |
| Earlier fixed-table confirmation | 7,372.57 | 1.913 / 1.922 | 1,051,739 |
| Adaptive three-actual screen | 6,833.94 | 1.779 / 1.992 | 556,280 |
| Adaptive independent confirmation | 6,767.17 | 1.757 / 1.790 | 556,280 |

The original 513×259 RGBA16 raster, managed quality 69, native qscale 5 and
background RGB 0x204060 are unchanged. One quality is evaluated, with no sweep
or extra source image. The RGB source-error gap stays exactly **0.090625 dB**
within the existing 0.25 dB gate. Complete 16-bit alpha blending and normalization
to 8-bit use one rounding inside each timed operation. The native adapter checks
every normalized sample against the integer reference before and after timing;
a corrupted reference fails explicitly.

Color conversion, sampling, transform, histogram/table construction, entropy
coding and independent output acquisition/release are also included. Independent
entropy recoding reproduces the adaptive packet byte for byte. Every coefficient
and amplitude is unchanged; normal and scalar FFmpeg decoding preserve the
previous decoded pixels exactly. Alpha, geometry, decoder agreement, unchanged
source and simultaneously held independent output checks pass. Private event
storage is reused, with contents recomputed for every operation.

Managed packets shrink from **323,990 to 158,827 bytes**; native packets remain
133,043 bytes. This is a source-quality timing comparison; compression efficiency
parity is not established. Allocations decrease **47.1%** from the earlier
confirmation. Production is unchanged from the
[accepted adaptive round](WARM-JPEG-ADAPTIVE-HUFFMAN.md). Fresh build and both
actual children match all 629 bodies and implementation flags in the tested
Release package. Its **2,767 tests** and **three actual-package usage examples**
per OS are reused.

Sequential Release runs use one pinned CPU, one native codec/conversion thread,
normal SIMD, tiering off and workstation nonconcurrent GC. Startup, I/O, IPC,
fixture preparation and checks are excluded; native allocations are unmeasured.
Raw evidence, source, actual package and all four child files were frozen before
subsequent preparation. The [numeric evidence](warm-jpeg44416-adaptive-evidence.json)
retains both runs and their identities. The earlier
[component-row result](WARM-JPEG16-COMPONENT-ROW.md) remains intact.

Next, revalidate JPEG422 RGBA8 after the shared entropy change. JPEG420 RGBA8's
upper-bound gap, other material boundaries and final-source uniform parity remain
open.

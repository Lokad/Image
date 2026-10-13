# Warm RGBA8 JPEG420 adaptive Huffman revalidation

The shared adaptive encoder averages **1.977× FFmpeg**, conservative upper
**2.014×**, with **297,772 managed bytes allocated per operation**. The mean
is below two, but this fixed pair remains open because the independent upper
bound exceeds the target. Both quietness guards pass; all twelve actual
iterations are retained. Do not repeat unchanged timing to seek a passing run.

| Stage | Lokad µs | Ratio / upper | Managed allocated B/op |
| --- | ---: | ---: | ---: |
| Earlier fixed-table confirmation | 3,605.37 | 1.978 / 2.022 | 527,335 |
| Adaptive three-actual screen | 3,486.58 | 1.976 / 2.139 | 297,772 |
| Adaptive independent quiet repeat | 3,501.53 | 1.977 / 2.014 | 297,772 |

The original 513×259 RGBA8 raster, managed quality 69, native qscale 5 and
background RGB 0x204060 are unchanged. Preparation evaluates one quality,
with no sweep or extra source image. The RGB source-error gap stays exactly
0.106110 dB within the existing 0.25 dB gate. Blending, color conversion,
sampling, transform, histogram/table construction, entropy coding and independent
owned output release are included in both complete-operation boundaries.

Independent entropy recoding reproduces the adaptive packet byte for byte.
Every quantized coefficient and amplitude is unchanged; normal and scalar FFmpeg
decoding preserve the previous decoded pixels exactly. Managed packets shrink
from 147,635 to 74,040 bytes. This is a source-quality timing comparison;
compression efficiency parity is not established. Source, alpha, geometry,
decoder agreement and simultaneously held independent output checks pass.
Private event storage is reused, with contents recomputed on every operation.

Allocations decrease **43.5%** from the earlier confirmation. Production is
unchanged from the [accepted adaptive round](WARM-JPEG-ADAPTIVE-HUFFMAN.md).
All 629 bodies and implementation flags in the fresh build and both actual
benchmark children match the tested Release package. Its 2,767 tests and three
actual-package usage examples per OS are reused. No redundant full validation
or package rebuild is needed for this revalidation.

Sequential Release runs use one pinned CPU, one native codec/conversion thread,
normal SIMD, tiering off and workstation nonconcurrent GC. Startup, I/O, IPC,
fixture preparation and checks are excluded; native allocations are unmeasured.
Raw evidence, source, actual package and all four child files were frozen before
subsequent preparation. The [numeric evidence](warm-jpeg4208-adaptive-evidence.json)
retains both runs and their identities. The older
[fixed-table result](WARM-JPEG420-CHROMA-ROW.md) remains intact.

Revalidate the other shared JPEG encoder cases before choosing the next
substantial primitive. Uniform warm parity and the remaining material contract
are open.

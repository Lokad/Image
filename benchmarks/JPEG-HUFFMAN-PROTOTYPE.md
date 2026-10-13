# Adaptive JPEG Huffman correctness prototype

An original development-only prototype reduces one existing JPEG420 packet from
**147,730 to 74,056 bytes (49.9%)**, preserving every quantized coefficient and
decoded pixel. **Production is unchanged and speed is unmeasured.** The current
warm JPEG420 RGBA16 result remains 2.046× FFmpeg, upper 3.240×; the prototype
does not establish performance parity.

| Fixed input | Encoded bytes | Coefficients and pixels |
| --- | ---: | --- |
| Current Lokad.Image encoder | 147,730 | Original reference |
| Adaptive entropy prototype | 74,056 | Exact original samples |
| Existing quality-matched FFmpeg baseline | 52,117 | Existing distinct quantization/quality anchor |

The input is the already frozen 513×259 JPEG produced from the existing original
RGBA16 raster, quality 69 and background RGB 0x204060. Its 3,366 blocks yield
122,378 entropy events. Frame geometry, quantization, coefficient amplitudes and
ordering remain exact; Huffman tables and scan table selectors change. It performs no
resampling, new transform, source normalization or quality search. No additional
original performance raster is introduced.

Original Python and safe C# implementations independently produce the same
adaptive tables and packet. The builder derives frequencies from the image,
uses canonical codes limited to 16 bits, and reserves the all-ones code as
required by [T.81 Annex C](https://www.w3.org/Graphics/JPEG/itu-t81.pdf).
Its length-count adjustment follows the Annex K construction; this is adaptive
coding, with no claim of globally optimal length-limited weighted cost. No
upstream implementation or tables are copied.

Python checks 259 deterministic histograms, including single-symbol, full
alphabet and forced length limiting. C# agrees on all four image histograms and
three structural controls, checks symbol coverage/prefixes/reserved codes, and
reproduces the exact packet. The actual validated Lokad.Image package decodes
both packets to identical pixels on Windows/Linux. Normal and scalar FFmpeg
also preserve the exact original pixels, alpha and geometry.

Avoiding a repeated transform could use 430,848 bytes of quantized coefficient
payload or 489,512 bytes of packed observed entropy events for this case. These
are **payload estimates, not measured per-operation allocations**; admission,
array overhead, table construction and full encode timing remain to be evaluated.
The larger compression opportunity justifies testing a complete C# encoder path,
without retaining the earlier rejected stream-dispatch experiments.

The [correctness evidence](jpeg-huffman-prototype-evidence.json) preserves exact
identities, both-OS managed checks, normal/scalar FFmpeg checks and frozen original
prototype sources. Production integration, resource/cancellation/stream controls,
actual Release package validation and narrow complete-operation VM measurements
are the next gates. The [benchmark summary](../docs/benchmark.md) keeps the
existing performance result open.

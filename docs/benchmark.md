# Benchmark state

Updated2026-10-26. Development0.2.0 remains under qualification; warm single-CPU
parity and replacement readiness are open. Compression correctness precedes CPU
gains. Published0.1.0 remains unchanged.

The cleanup removes fuel, ceiling reservations and unpublished wrappers. Earlier timings describe their frozen producers, **not the cleaned build**.
Two current WebP screens reuse the qualified cleaned library; parity remains open.
The [complete historical inventory](benchmark-pre-cleanup.md) retains all257cases,
raw links and baseline exceptions; [coverage](../benchmarks/WARM-COVERAGE.md)
defines the remaining finite scope and evidence invalidation.

| Current qualification | Evidence | Status |
| --- | --- | --- |
| Packet/pixel/memory/cancellation regression | [Cleanup qualification](cleanup-qualification.md) | See current Windows/Linux/scalar/package results |
| CPU and allocation measurements after cleanup | VP8/VP8L current shorts; other required paths pending | No independent parity or speedup claim |
| Full warm target | Independent quiet upper≤1× for each required path, one CPU/operation | Open |

Current screens are labeled; other cost anchors remain **pre-cleanup**. B/op is managed allocation, distinct
from peak cooperative admission and native allocation. Ratios use the faster native
bracket; upper is the recorded conservative confidence bound. Historical shorts
are leads for measurement, not current demonstrated gaps.

| Operation / fixed profile | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [WebP VP8 + alpha, fresh seekable Stream](../benchmarks/WARM-VP8-DECODER-RECONSTRUCTION.md) | 10,046.91 | 5,991.89 | 1.677 / 1.684 | 1,966,591 | Historical independent; remeasure |
| [JPEG 4:4:4, forward-only / exact reference](../benchmarks/WARM-JPEG444-ENTROPY-REPLAY.md) | 5,049.03 | 3,776.93 | 1.337 / 1.343 | 820,131 | Historical independent; remeasure |
| [JPEG 4:2:0 from caller RGB samples](../benchmarks/WARM-JPEG-VECTOR-RECORDS.md) | 2,617.06 | 1,578.04 | 1.658 / 1.669 | 373,421 | Historical independent; remeasure |
| [VP8 RGBA8](../benchmarks/WARM-POST-CLEANUP-WEBP.md) | 94,853.24 | 53,467.74 | 1.774 / 1.911 | 24,424,944 | Current gap (short; qualified) |
| [VP8L RGBA8 / libwebp exact=1](../benchmarks/WARM-POST-CLEANUP-WEBP.md) | 223,136.69 | 121,152.46 | 1.842 / 2.624 | 48,232,248 | Current gap (short; qualified) |
| [PNG RGBA8, SmallestSize9 / native9](../benchmarks/WARM-CURRENT-PNG-SMALLEST8.md) | 25,360.59 | 17,299.67 | 1.466 / 1.601 | 1,325,678 | Historical short; remeasure |
| [Bilinear resize RGBA16](../benchmarks/WARM-BILINEAR-WORD-CACHE.md) | 534.35 | 418.51 | 1.277 / 1.336 | 290,127 | Historical short; remeasure |
| [Bicubic resize RGBA8](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 635.08 | 416.65 | 1.524 / 1.610 | 332,178 | Historical short; remeasure |
| [Bicubic upscale RGBA16](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 4,258.22 | 2,871.12 | 1.483 / 1.690 | 5,016,578 | Historical short; remeasure |
| [Lanczos3 reduction RGBA8](../benchmarks/WARM-LANCZOS3-CERTIFIED-FMA.md) | 758.75 | 490.07 | 1.548 / 1.646 | 138,466 | Historical short; remeasure |
| [Lanczos3 enlargement RGBA16](../benchmarks/WARM-LANCZOS3-CERTIFIED-FMA.md) | 5,248.30 | 3,935.96 | 1.333 / 1.459 | 4,584,270 | Historical short; remeasure |

Complete encoder packets remain the acceptance boundary; metadata differences
are limited to the approved JPEG attribution COM. These are the named controls,
not broad arbitrary-content/settings compression claims.

| Frozen packet scope | Lokad.Image bytes | Reference bytes | Verification |
| --- | ---: | ---: | --- |
| PNG,14depth/color/policy controls | Identical | Identical | Complete raw packets |
| JPEG RGBA8,444 /422 /420 | 133039 /71166 /52091 | 133041 /71168 /52093 | Exact except approved COM |
| JPEG gray8, Q69 | 58910 | 58910 | Complete raw packet; SIMD libjpeg-turbo |
| VP8L photograph / diagram | 160032 /7234 | 160032 /7234 | Exact=1; complete raw packets |
| VP8 photograph / diagram, Q75/method4 | 17688 /7100 | 17688 /7100 | Complete raw packets |

[Codec parity](codec-parity.md) records the full packet identities and producer
scope: JPEG10controls, PNG14, VP8L19selected+2forced, VP8 twenty513×259identities
plus the large retry control. Decoder exactness and editing oracle requirements
remain separate. A timing pass,5%IDAT tolerance or equal numeric qualities cannot
replace packet equality.

Reference operations include normalization, input delivery and independent output
ownership. FFmpeg is pinned; grayscale encoding uses the approved explicitly
labeled SIMD libjpeg-turbo3.1.4.1 reference; VP8L uses libwebp exact=1; bicubic uses
zimg, FFmpeg's zscale backend, with the approved full halo/alpha/packing boundary.
Native-only resize tolerances remain in the retained reports; managed pixels
stay exact. GIF/animation, startup, concurrency and arbitrary profiles are excluded.

Use the exclusive quiet VM, versioned idle/process guard and sequential Release
jobs. Screen narrowly; independently confirm promising cases and stop passing
ones. Reuse immutable builds and unchanged qualification. Historical experiments,
failed guards and rejected candidates stay retained; none silently becomes current
evidence. Regenerate with `python benchmarks/render-benchmark.py`, then `--check`.

# Exact batched VP8 intra4 preparation

The existing 513×259 RGBA8 VP8 control retains frozen FFmpeg Q75/method4
settings, compressed alpha and its exact 98,590 B packet. Complete normalization,
encoding and fresh output acquisition/release use one CPU.

| Quiet VM run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| Short | 98,770.84 | 53,420.76 | 1.849/1.973 | 24,424,952 |
| Independent | 99,721.94 | 53,340.56 | 1.870/1.911 | 24,424,952 |

Independent confirmation saves **2.3% elapsed** against
[the preceding confirmation](WARM-VP8-VECTOR-RECONSTRUCTION.md), with unchanged
allocation. All 12 actual/11 selected iterations and both idle-v2 guards pass;
the upper bound includes selected and unfiltered confidence intervals.

Four independent predictions share exact integer AVX512F/BW transforms.
Scalar arithmetic, candidate evaluation and tie order remain unchanged.
The non-trellis batch requires no cancellation token and full ten-mode work
coverage; all original work charges remain. Additional 576 B stack scratch fits
the existing 16 KiB workspace. Ownership/admission are unchanged; no root peak
is measured separately by this worker.

Both platforms pass 5,130 checks each, including 947 live comparisons, plus
60 focused checks in normal/scalar modes and 56 scalar public WebP checks.
The arithmetic proof covers 302,576 original blocks, 36 residual aliases,
96 byte/stride/overlap layouts and 512 refusals, with zero heap allocation.
Linux exercises SIMD; Windows uses the scalar fallback. The same actual private
0.2.0 Release package and all three usage examples pass both platforms.

[Evidence](warm-vp8-four-candidate-evidence.json) records identities and reuse.
Other affected VP8 CPU observations remain historical. JPEG, full-color VP8L,
PNG, decoding, editing and text retain valid evidence. No cases were added;
full 257-case/warm 1× and replacement readiness remain open.

# Exact SIMD VP8 reconstruction

The existing 513×259 RGBA8 VP8 control retains frozen FFmpeg Q75/method4
settings, compressed alpha and the raw-exact 98,590 B packet. Complete
normalization, encoding and fresh output acquisition/release use one CPU.

| Quiet VM run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|
| Short | 101,945.77 | 54,154.11 | 1.883/1.999 | 24,424,952 |
| Independent | 102,032.50 | 53,405.80 | 1.911/1.923 | 24,424,952 |

Independent confirmation saves **4.4% elapsed** against
[the preceding confirmation](WARM-WEBP-ALL-WIDTH-HISTOGRAMS.md), with unchanged
allocation. All 12 actual/selected iterations and both idle-v2 guards pass.

The fused SSE4.1 inverse transform and pixel addition retain column signed16
wrapping, row rounding and final byte clipping. Full input snapshots preserve
paired/overlapping buffers. Original scalar arithmetic remains available;
decoder transforms, work, cancellation, ownership and admission are unchanged.
No storage or allowance is added; this worker does not measure a root peak.

Both platforms pass 5,129 checks each, including 947 live comparisons, 28 scalar
reconstruction/macroblock/admission checks and 56 scalar public WebP checks.
The 229,504 signed16 arithmetic controls and 90 aliases retain their frozen hash
and zero allocation on both hardware paths. The same actual private 0.2.0 Release
package and all three examples pass both platforms.

[Evidence](warm-vp8-vector-reconstruction-evidence.json) records identities and
reuse. Other affected VP8 timings remain historical; full-color VP8L, JPEG, PNG,
decoding, editing and text reuse unchanged evidence. Caller RGB420 is now the
largest confirmed encoder anchor. Full 257-case/1× and readiness remain open.

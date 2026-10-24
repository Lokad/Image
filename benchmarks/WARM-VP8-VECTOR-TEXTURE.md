# Exact vector VP8 texture scoring

Existing 513×259 RGBA8 VP8 retains frozen FFmpeg Q75/method4 settings, compressed
alpha and the raw-exact 98,590 B packet. Complete normalization, encoding and
fresh independently owned output acquisition/release are measured on one CPU.

| Quiet VM run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|
| Short | 112,908.09 | 53,499.39 | 2.110/2.236 | 24,329,736 |
| Independent | 113,006.89 | 53,643.55 | 2.107/2.118 | 24,329,736 |

Independent confirmation uses 4.7% less elapsed than the
[preceding confirmation](WARM-VP8-FUSED-GREEN-HISTOGRAM.md), with unchanged
allocation. All 12 actual/selected iterations and both idle-v2 guards pass.

The exact 4×4 sequency Hadamard uses sixteen AVX2 signed16 lanes. Equality on
all sixteen linear bases and integer range bounds prove identical coefficients;
weighted energy fits signed32. Exact row gathers/weight reads preserve tight,
unaligned spans. Existing scalar arithmetic remains available. No storage, owner,
copy, scope, allowance, metric, work or cancellation boundary changes.

Both platforms pass 5,120 checks each, including 947 live comparisons, 21 scalar
distortion/macroblock checks and 56 scalar public WebP checks. Existing 102,656
blocks and 1,642,496 isolated magnitudes remain exact, with unchanged frozen
energy, native decisions and allocation controls. The same actual private 0.2.0
Release package and all three examples pass both platforms. No new test matrix
or benchmark/native/host/fixture build.

[Evidence](warm-vp8-vector-texture-evidence.json) records identities and reuse.
Other affected VP8 timings stay historical; JPEG, full-color VP8L, PNG, decoding,
editing and text reuse unchanged evidence. JPEG now has the slightly larger
confirmed encoder gap. Full 257-case/1× and replacement readiness remain open.

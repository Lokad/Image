# Exact fused alpha predictor histograms

Existing 513×259 RGBA8 VP8 controls retain frozen FFmpeg Q75/method4 settings,
compressed alpha and the raw-exact 98,590 B packet. Complete operations include
normalization, encoding and fresh independently owned output acquisition/release,
on one CPU. Inputs, quality, settings and operation boundaries are unchanged.

| Quiet VM run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|
| Short | 118,282.32 | 53,653.31 | 2.205/2.534 | 24,329,736 |
| Independent | 118,536.73 | 53,629.80 | 2.210/2.223 | 24,329,736 |

Independent confirmation uses 5.5% less elapsed than the
[preceding confirmation](WARM-VP8-CURRENT.md), with unchanged allocation.
All 12 actual/selected iterations and both idle-v2 guards pass.

All fourteen green-only predictor histograms accumulate together. Neighbours
load once and identical pair averages are shared. Every rounding stage, clamp,
tie, boundary rule and histogram update remains. The superseded mode-dispatch
helper is removed. No storage, owner, scope, copy or allowance is added. Work,
cancellation, shared/local admission, refusal and output lifetimes remain.

Both platforms pass 5,120 checks each, including 947 live comparisons, 11 scalar
green-stage checks and 56 scalar public VP8/VP8L checks. Existing 8,613 frozen
histograms and 65,536 neighbour controls remain exact, alongside plan, work,
refusal, cancellation and ownership checks. The same actual private 0.2.0 Release
package and all three examples pass both platforms. Benchmark/native/prepared
hosts and fixtures were reused.

[Evidence](warm-vp8-fused-green-histogram-evidence.json) records samples,
identities and qualification. Only compressed-alpha prediction changes; other
affected VP8 timings stay historical. Opaque VP8, full-color VP8L, JPEG, PNG,
decoding, editing and text reuse unchanged evidence. Full 257-case/1× and readiness
remain open; VP8 and JPEG still form the largest confirmed encoder gap group.

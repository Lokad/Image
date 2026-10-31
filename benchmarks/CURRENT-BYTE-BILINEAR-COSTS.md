# Current byte-bilinear costs

Two existing 513×259 RGBA8 controls resolve complete-operation uncertainty after
the word-bilinear dispatch change. Each receives one independent 12-iteration
quiet-VM confirmation on the current immutable producer, with zero builds.

| Existing control | Lokad µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: |
| Down8 | 504.78 | 1.303 / 1.315 | 157,971 |
| Up8 | 3,768.78 | 1.740 / 1.751 | 2,235,501 |

Down8 reduces to 256×129; Up8 enlarges to 1027×519. Both remain demonstrated
gaps against warm single-CPU upper ≤1×. These are current costs, without a paired
speedup claim. The four named byte/word bilinear controls now have current costs;
no shape, filter, depth or quality grid is added.

The complete public operation includes admission, geometry, materialization,
independent output ownership and disposal. The fixed FFmpeg/zscale triangle graph
includes packing, encoded-space alpha normalization and output acquisition/release;
Up8 includes timed zero-alpha RGB normalization. Managed rational-reference pixels
are exact. Native RGB max1/RMSE≤0.25, Down8 exact alpha and approved Up8 native-only
alpha max1/RMSE≤0.25 remain unchanged. Normal/scalar CLI, source and owner gates pass.

Production/package inputs are unchanged. Full 5,240 checks/OS, affected scalar
checks, same actual Release package on both OSes and three examples reuse
[current qualification](WARM-VP8L-COLOR-BIN-REUSE.md). Cached selector identities
are verified against their successful archived executions. Metadata-preparation
failures and the Down8 postcheck recovery remain; successful timings are not rerun.

[Evidence](current-byte-bilinear-costs-evidence.json) retains every actual/native
bracket, allocation, producer and idle-v2 guard in mirrored/member-verified
`current-byte-bilinear-costs.tar.xz`, SHA256 c37903f2e31a37100aecf3d2fbc466e0bc8e7b22c09a1b552ea129fff6fe2766. The full 268-path objective remains open.

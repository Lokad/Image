# Separable strips for JPEG 4:1:0

The existing owned 4:1:0 control falls from **2,388.42 to 382.11 µs**: **84.0%
less elapsed time**, with every pixel unchanged. Its independently confirmed
**0.752× / upper 0.765** cost passes the one-CPU warm target. Stop this control.

| Complete owned RGBA8 decode, 513×259 | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Encoded input B |
| --- | ---: | ---: | ---: | ---: | ---: |
| Original 4:1:0, independent | 2,388.42 | 507.81 | 4.703 / 4.718 | 720,792 | 3,399 |
| 4:1:0 strips, independent | 382.11 | 508.00 | 0.752 / 0.765 | 720,835 | 3,399 |
| Historical 4:1:1 companion | 1,257.52 | 449.06 | 2.800 / 2.823 | 751,280 | 3,904 |

64-column strips prepare horizontal positions once and reuse two filtered source
rows per chroma plane. Safe AVX2 processes vertical filtering and color over rows;
the portable/tail path preserves the actual scalar precision. Native byte
half-grid expansion still precedes Q14 horizontal filtering, Q7 storage, Q12
vertical filtering and Q9/Q13 color. Edge clamping and the 1/500 leading-tap rule
remain unchanged. No approximation or quality/settings change is introduced.

The fixed **1,792-byte stack workspace** includes 1,280 bytes of descriptors and
512 bytes of filtered rows. Existing `OperationContext` admits this capacity
**before stack allocation**, alongside the live planes and output, then refunds
it after actual users finish. Refusal is terminal and cleans the already-owned
result; no lower-memory retry or new budget/owner exists. No new owned heap
buffer, frame copy or cache across operations. Full-operation allocation measures
+43 B/op; this is not an allocation improvement.

Original DC-only packets, authored data, reference settings, output normalization
and complete-operation boundaries remain frozen. Independent and normal/scalar
native RGBA are exact. The 4:1:1 companion is a current cost for the shared
dispatch, without a paired causal-gain claim. Caller sampling and its real pixel
implementation remain; other owned 4:1:0 boundaries need applicable costs before
CPU-pass claims. No broad JPEG speed claim follows.

A fresh three-actual pair gives 84.23% less elapsed. One candidate twelve reuses
the valid original twelve: **84.0017% filtered / 84.0016% all elapsed**,
**84.0076% native-normalized** reduction. Raw 99.9% intervals and conservative
normalized envelopes separately separate. All actuals, native brackets and idle-v2
guards are retained; no scored job, variant or profile is repeated.

Both OS pass **5,185 checks each** (2,092 internal, 2,145 public, 948 live), plus
338 internal/342 public focused checks normal/scalar. Five new cases cover exact
row arithmetic, extrema/tails/guards, zero heap, baseline/progressive strip edges,
retained ownership and targeted stack-admission refusal/cancellation/recovery.
The same actual Release package, governed async consumer and three examples pass
on both OSes. Its existing array/sync/async 4:1:0 and near-integral-tap controls
exercise this implementation. One successful core build per OS; a Windows span-
lifetime annotation error was fixed before scoring. Zero harness/native/selector
builds. Linux's initial document check was corrected by syncing the admission
guide; the package was neither regenerated nor successfully reverified twice.

Passing YCCK is unchanged and was not retimed. The historical 4:1:1 companion is superseded by
[its separable strips](WARM-JPEG-QUARTER-SINGLE-STRIPS.md). The full **268-path ≤1×** objective remains open.

[Evidence](warm-jpeg-quarter-strips-evidence.json),
[independent references](WARM-EXTENDED-JPEG-CURRENT-COSTS.md).
Mirrored/member-verified jpeg-quarter-strip-cache-qualified.tar.xz, 162 members,
SHA256 b4f746112aa154e269762c0302267069f4fecbb8874532c9ffbe9ada5ad26e25; prior build/reference dependencies are pinned.

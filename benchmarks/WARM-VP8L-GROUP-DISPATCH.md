# VP8L group dispatch

Complete resident VP8L decoding improves **5.93% filtered / 5.94% all elapsed**
and **5.79% native-normalized**, with separated raw/normalized confidence:
**3,427.41 → 3,224.25 µs**, **1.068× / upper 1.079**. Warm allocation remains
**152,728 B/op**. The CPU gap stays open.

| Existing complete operation, 513×259 | Lokad µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Resident native VP8L | 3,224.25 | 1.068 / 1.079 | 152,728 | gap |
| Literal/run VP8L | 1,536.36 | 0.714 / 0.726 | 10,840 | pass |
| Indexed VP8L | 553.18 | 1.008 / 1.039 | 820,194 | confidence-open |
| Live-token VP8L | 3,332.00 | 1.094 / 1.132 | 152,728 | gap |
| Seekable-stream VP8L | 3,271.53 | 1.073 / 1.102 | 152,824 | gap |
| VP8 + compressed alpha | 7,151.61 | 1.195 / 1.219 | 1,434,731 | gap |
| Seekable VP8 + alpha | 7,037.26 | 1.165 / 1.211 | 1,434,827 | gap |
| Validate VP8L + alpha | 3,397.14 | 1.114 / 1.134 | 685,421 | gap |
| Validate VP8 + alpha | 6,696.68 | 1.224 / 1.250 | 554,478 | gap |

The decoder borrows each active group's immutable Huffman trees and existing
green lookup until its tile/row edge. Copy jumps select the group at their actual
endpoint; reads stop before output overwrites tail lookup storage. This is the
archived dispatch strategy adapted to the existing owned Span, with no new array,
table, owner, cache, precision, admission or cancellation policy.

The earlier 4.71% short missed its then-5% gate. Current owned-frame/reader changes
invalidate that timing as current proof. This round reuses the same strategy:
one current original/candidate short, then one candidate twelve against a valid
independent original twelve. Thirteen relevant source hashes match; fresh original
raw/all/native means stay within 5%. The new confirmation clears the default 5%
gate; no confidence exception or variant. All actuals and both bracket views remain.

Original 183,812-byte primary packet and all companion packets/settings remain
fixed. Complete operations include input delivery as labeled, normalization,
independent output and release. RGBA, alpha and hidden RGB are exact; coding
validation returns the same exact descriptor. All native/scalar, ownership and
quiet-v2 gates pass. Eight companions are current costs, without paired gain
claims; normalized point changes range −4.29%..+3.08%. Literal/run passes and
stops. Indexed confidence remains open; it is neither a pass nor a confirmed gap.

Both OSes pass **5,305 Release checks each** (2,164 internal, 2,193 public, 948 live),
plus 58 internal/306 public focused controls normal/scalar and Linux AVX512-off.
The same actual Windows package/DLL passes both OSes, governed/retained/async/
one-frame/alpha consumers and all three usage examples. One core/internal build
per OS; zero public/differential/harness/native/selector builds, new inputs or
profiles. Development package 0.2.1 is 387,794 bytes, SHA256
19b99d035e0a6db4288264c8433665edfc19367f70022622e7d45b1a98a8209d.
Published releases stay unchanged.

[Evidence](warm-vp8l-group-dispatch-evidence.json) identifies all nine affected
named inventory paths; the three opaque VP8 controls skip this decoder and retain
unchanged proof. Encoders/metadata/other codecs/editing/text remain unchanged.
Mirrored/member-verified raw vp8l-group-dispatch-qualified.tar.xz SHA256
9e8562f3efa8b5d74e28f85471fc7ef6e471b71d384129930f94f16b0c0fcc23
(7,070,940 bytes, 315 members) retains source, binaries, package, tests, actuals,
guards and exact proof restoration. All 268 required paths remain in scope.

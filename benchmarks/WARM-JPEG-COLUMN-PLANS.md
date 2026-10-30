# JPEG column preparation

The existing caller-RGB420 control uses **7.55% less elapsed time** with exact
packets. Its **1.190× / upper1.197** warm one-CPU cost remains a gap. Measured
allocation rises by **14,806 B/op** for one temporary column-descriptor array.

| Complete 513×259 encode | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Packet bytes, Lokad/native |
| --- | ---: | ---: | ---: | ---: | ---: |
| Prior caller RGB420, independent | 2,030.36 | 1,582.17 | 1.283 / 1.294 | 373,416 | 52,091 / 52,093 |
| Prepared caller RGB420, independent | 1,877.03 | 1,577.94 | 1.190 / 1.197 | 388,222 | 52,091 / 52,093 |
| Governed RGBA16, current short | 2,092.66 | 1,833.49 | 1.141 / 1.292 | 819,452 | 52,115 / 52,117 |

Q85/native qscale5, sampling, original inputs and reference settings stay frozen.
Both complete operations include normalization/conversion, encoding, fresh owned
output acquisition and release on one CPU. Only the approved COM attribution
differs from native packets; coded content and reconstructed pixels stay exact.
Equal numerical quality values are not used to establish equivalence. The RGBA16
row is a current screen, without a causal speedup or independent parity claim.
Native allocations are unmeasured; no broader content/settings claim follows.

The 33 column positions determine horizontal area phases, tile bounds and SIMD
selectors. Preparing them once per pass replaces 561 repetitions across MCU rows.
MCU traversal, vertical plans, scalar/tiny tails, tap weights, signed guards,
rounding and cancellation remain. The existing admission context reserves actual
array element capacity plus a 32-byte header allowance before allocation, and
releases it after the last user. Declared local nonfit selects stack preparation
before admission; shared refusal remains terminal. No global cache, new budget,
owner abstraction or full-frame sample copy is added. The governed root's peak
stays **1,704,856 B**, including **1,062,936 B** of retained source; temporary
charges return, the root stays borrowed and refusal/cancellation recovery passes.

A fresh three-actual pair clears the predeclared 5% gate. One candidate twelve
reuses the valid original twelve: 7.552% filtered/7.599% unfiltered elapsed and
7.304% normalized point improvement. Both raw 99.9% CIs and conservative
normalized envelopes separately separate. All actuals, brackets and idle-v2
guards are retained. A governed-worker postcheck was corrected to its existing
schema after completion; exact original readiness and packet/reconstruction
hashes pass, with **zero measurement reruns** and no relaxed gate.

All **5,163 checks/OS** pass (2,080 internal, 2,135 public, 948 live), plus 50
internal and 66 public focused checks normal/scalar per OS. Five new cases cover
retained capacity, exact cached/tight packets at both depths, refusal, cancellation
and successful recovery; existing independent phase/edge/guard tests exercise the
actual descriptor. The same actual Release package, governed consumer and three
examples pass Windows/Linux. One core build/OS; no harness/native/selector rebuild.
Other affected 420/422 boundaries need applicable costs before CPU-pass claims;
the full 268-path parity objective remains open. This primitive is closed.

[Evidence](warm-jpeg-column-plans-evidence.json) and
[prior horizontal filtering](WARM-JPEG-HORIZONTAL-AREA-FILTER.md).
Mirrored/member-verified jpeg-column-plans-qualified.tar.xz, 173 members,
SHA256 632a38b92fc8a5cdb38fdc97d9527928b5ae4eab62331ae946397213cf0a4025; dependencies are pinned by hash.

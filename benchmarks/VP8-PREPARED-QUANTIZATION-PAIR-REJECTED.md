# Prepared VP8 quantization pairs: rejected

The existing photograph/effort4 short observes **2.39% less elapsed time**
(2.42% all actuals, **2.92% native-normalized**), below the 4.1% continuation gate.
Reject without confirmation or variants. Short point means do not establish a
causal gain or change current production costs; confidence intervals overlap.

| Complete original 513×259 encode | Lokad µs | Ratio / upper | Managed B/op | Packet B |
|---|---:|---:|---:|---:|
| Candidate, short3 | 13,566.826 | 1.391 / 2.728 | 809,072 | 17,688 |

The trial quantizes two already prepared Luma4 modes together using AVX512F+BW.
It preserves independent coefficients/levels/nonzero flags, decision order/ties,
rates and reconstruction. All ten modes already quantize before the block can
abort. Extra 64 B stack scratch fits the existing 16 KiB admitted envelope;
no heap/cache/owner/admission/cancellation change. Scalar/AVX2 fallbacks remain.
Native point drift is +0.54%; allocation and the complete packet are unchanged.
The applicable independent12 original is reused from
[quantization caller invariants](WARM-VP8-QUANTIZATION-CALLER-INVARIANTS.md).

Candidate Linux **36 internal/392 public** checks pass normal/scalar, plus
36 internal with AVX512 disabled. Three focused controls independently exercise
Int64 arithmetic boundaries, generated matrices, two-block lane order and guards,
nonzero bits and zero allocation. Frozen packet/reconstructed pixels, source,
independent output ownership and both idle-v2 guards pass. One core/internal build;
zero public/harness/native/selector builds, new images or settings.

Both source files, core/PDB/XML and every saved linked test binary are restored
**byte-exactly**, without rebuilding. The three implementation-only tests leave
with the candidate. Restored **33 internal/392 public** checks pass normal/scalar,
plus 33 internal with AVX512 disabled. Windows and the actual Release package are
unchanged; the applicable **5,369 checks/OS**, 948 live comparisons, same actual
package/three examples and green CI remain valid. No full/package repeat.

Close paired prepared quantization, including width/layout/inlining variants.
The [current attribution](CURRENT-VP8-PHOTO-ATTRIBUTION.md) remains diagnostic;
all 268 required paths remain in scope and full warm parity is incomplete.

[Evidence](vp8-prepared-quantization-pair-rejected-evidence.json). Mirrored/member-verified
vp8-prepared-quantization-pair-rejected.tar.xz, 2,642,204 B/85 members,
SHA256 a18d8c6fef1a8f44bbdc9fb067796295410964ce719e90528c975bfa921c1c01.
Raw retains source, binaries, tests, selectors, guards, actuals, restoration and
pinned qualified/current-profile/original-confirmation dependencies.

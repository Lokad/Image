# ALPH match-index reuse

An ALPH encode with multiple filters now owns one match index until all sequential
preparation users finish. Fill borrows it; the ordinary factory still owns its
temporary index. Unused storage is discarded before a differently sized request;
admission precedes allocation and refusal is terminal. Same-sized storage is fully
overwritten, including singleton/two-pixel results. Selected tokens/trees and
payloads remain independently owned. No pool, wrapper owner, prepared-object field,
quota or budget snapshot is introduced. Method6/all-family policy is unchanged.

Complete RGBA8 encoding allocates **7,615,104 B/op**, saving **531,528 B/op (6.5%)**
at the same four-operation batch. The reused array payload is531,468 B; measured
allocation also includes array/owner overhead. Actual-DLL admission diagnostics
retain **3,656,309 B peak / zero retained scratch** for alpha8 and3,470,059 B peak
for the unchanged photograph. Caller input/output and diagnostic storage are
excluded; physical/native peaks are unmeasured. Do not infer CPU gains from this.

| Existing complete513×259 control | Managed µs | Reference µs | Ratio / upper | Allocated B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| VP8 + ALPH RGBA8 | 65,641.70 | 53,332.85 | 1.231 / 1.246 | 7,615,104 | 98,590 |
| VP8 + ALPH RGBA16 / normalization | 65,789.20 | 53,515.74 | 1.229 / 1.238 | 7,614,288 | 98,590 |
| VP8L original RGBA8 | 115,815.96 | 130,140.89 | 0.890 / 0.906 | 7,476,640 | 183,812 |
| VP8L photograph | 86,323.75 | 83,862.10 | 1.029 / 1.090 | 6,517,254 | 160,032 |
| VP8L text/diagram | 8,159.66 | 9,566.19 | 0.853 / 0.871 | 4,363,930 | 7,234 |
| VP8L opaque RGBA8 | 77,597.76 | 75,729.69 | 1.025 / 1.036 | 5,983,424 | 121,896 |
| VP8L RGBA16 / normalization | 108,771.66 | 121,544.11 | 0.895 / 0.924 | 7,476,640 | 183,812 |
| VP8L async / fresh nonseekable output | 109,097.10 | 121,293.70 | 0.899 / 0.950 | 7,477,024 | 183,812 |

References retain the existing FFmpeg/libwebp boundaries; lossless WebP uses
explicitly labeled libwebp exact=1. Normalization, full preparation and independent
output/disposal stay timed; native wrappers/workspaces retain their declared
lifetimes. Async includes fresh nonseekable acquisition and awaited delivery.
Source/settings/quality, exact pixels/alpha/hiddenRGB and complete packets pass.

All eight affected costs receive twelve-actual confirmation on one immutable
producer. Four lossless boundaries pass and stop; opaque/ALPH retain gaps and
photograph is confidence-open. No CPU gain, equivalence or causal regression is
claimed. Alpha8 raw elapsed is lower with a substantially faster native clock.
Photograph raw points are higher, but managed/reference confidence overlaps;
its native brackets move83.86→89.70ms within the run. Preserve that uncertainty,
including the noisy three-actual screen. Previous stricter paired photo/opaque
bounds remain linked history and do not qualify this changed matcher.

Both OSes pass **5,575 full/live checks** (2,352 internal,2,275 public,948 live),
plus824 internal/357 public affected checks per normal/scalar mode. Normal reuses
822 prior passes plus two corrected fault-phase checks; scalar runs all824.
Six new cases cover dirty reuse/shapes/tiny inputs, context/disposal guards,
refusal/cancellation replacement peaks, borrowed ownership and surviving prepared
candidates. Two old injection hooks expected the removed allocation; they now
target actual second-filter preparation. Initial failures/source are preserved.

One async launch stops before actuals because its proof names the prior DLL.
The existing probe requalifies exact packets, normal/scalar native/CLI pixels,
resource/cancellation/sink failures and independent lifetimes against this producer.
Only async/word remaining runs resume; six completed confirmations are reused.
No production/harness/native/probe rebuild follows either test/proof correction.

The SAME actual Release package and all three examples pass Windows/Linux,
including cooperative admission and async surviving outputs. One core build per
OS, one test-only correction, reused audit binaries and one package. Published
releases stay immutable; publication is postponed. Other families reuse valid
unchanged-path evidence. Histograms already release populations at last consumer;
no additional lifetime change follows that source check.

[Evidence](webp-alpha-match-index-reuse-evidence.json) retains eight current costs,
all actuals/brackets, source/producer identities, admission observations and gates.
Mirrored/member-verified raw `webp-alpha-match-index-reuse-qualified.tar.xz`: 6,045,696 B / 456 members,
SHA256 `cf2d7eb6670e793581a69e74a4a7dbe4252ac90483588cfa64de2782fc11d214`. Sources, immutable producers/package, failures,
proof refresh, guards and reproduction remain; baseline/reference dependencies
are pinned. [Earlier token-tail reuse](WEBP-TOKEN-TAIL-HISTORY.md) and
[photo/opaque reconciliation](VP8L-INVARIANT-ALPHA-PREPARATION-INCONCLUSIVE.md)
retain history. The full282-path objective stays open.

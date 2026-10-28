# Exact SIMD JPEG color conversion

Selected JPEG420 costs are superseded by [paired inverse columns](WARM-JPEG-PAIRED-COLUMNS.md).
The color arithmetic and its qualification remain applicable.


The existing JPEG420 decode independently falls from1.746ms to1.424ms,
**18.4% less elapsed**, retaining exact pixels. Its **1.396×/upper1.518** gap
remains to the warm single-CPU≤1× objective. Allocation is757319B/op (+21B).

| Quiet run | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: | ---: |
| Qualified baseline, confirm | 1,746.224 | 1,008.441 | 1.732 / 1.862 | 757,298 |
| SIMD color, short | 1,523.770 | 1,056.975 | 1.442 / 2.216 | 757,314 |
| SIMD color, confirm | 1,424.367 | 1,020.523 | 1.396 / 1.518 | 757,319 |

Same513×2598bit source,52,093B packet and complete Decode/independentRGBA8/release.
Native softwareMJPEG includes centered chroma expansion/crop,BT.601 conversion,
state reset and output ownership. CPU0,one codec/conversion thread,optimizedJIT
and nonconcurrentGC stay fixed. Managed and normal/scalar native pixels are exact.

AVX2 processes eight byte-plane samples with the same derivedQ13 coefficients,
rounding bias,signed shift and clipping. RGB lane packing writes exactly24bytes;
RGBA writes32. Existing padding,overlap and length guards precede stores. Scalar
tails,single-pixel calls and unsupported CPUs remain. No owner,buffer,cache,table,
frame copy or ISA requirement is introduced. FilteredQ9 conversion is unchanged.

The12.1% short reduction clears the5% gate. One independent12actual/12filtered
confirmation reuses JCH2baseline12/12; filtered/all-actual intervals separate and
all brackets/idle-v2 pass. No baseline retiming or scoring repeats. These are
sequential means,not paired/general-content gains. Confirmed allocation rises21B/op
(0.0028%,including harness overhead); short757314B/op retained. Native allocation
and live admission peaks are unmeasured.

BothOS pass5054full checks,8existing color controls normal/scalar,293public JPEG
normal/scalar and15AVX2-disabled pixel/caller controls. The SAME actual Release
package and three examples pass bothOS. Focused scalar checks are reused after
selection; no duplicate test algorithm or image grid. One corebuild perOS reuses
the immutable harness,workers,selectors and fixture. Published releases stay fixed.

Bulk byte-plane color costs change,including444/420/422 render/callerRGB. Only this
named cost is refreshed; other affected cases need current evidence before pass
claims. Filtered/single-pixel arithmetic,inverse/chroma,validation/Identify,encoding
and unrelated paths retain applicable proof. [Evidence](warm-jpeg-simd-color-evidence.json)
retains all actuals/brackets,bounds,guards,identities,allocations and the mirrored/
member-verified158-member raw master.

Next priorities are VP8effort6/trellis1.519×,photograph1.479× and PNGFastest1.471×.
One six-second current trellis profile (5912samples,exact6818B packet,quiet guards)
places40.7% in Apply,7.7% in Luma4self,3.2% in Band,2.6% in ZeroCandidates and2.2%
in rate-view access. Diagnostic shares make no timing/gain claim. One profile-host
build reuses production/harness/native workers. Inspect Apply instruction offsets
before choosing a substantial primitive; state-collapse/uniform/scan/batch trials
stay closed. No profile repeat. Full267-path parity/general readiness remain open.

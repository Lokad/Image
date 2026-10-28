# Exact SIMD JPEG chroma tiles

The existing JPEG420 decode independently falls from2.095ms to1.746ms,
**16.7% less elapsed**, retaining exact pixels and757298B/op. Its
**1.732×/upper1.862** gap remains to the warm single-CPU≤1× objective.

| Quiet run | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: | ---: |
| Qualified baseline, confirm | 2,095.452 | 1,007.974 | 2.079 / 2.241 | 757,298 |
| SIMD chroma, short | 1,734.148 | 1,038.875 | 1.669 / 30.849 | 757,298 |
| SIMD chroma, confirm | 1,746.224 | 1,008.441 | 1.732 / 1.862 | 757,298 |

Same513×2598bit source,52,093B reference packet and complete public Decode/
independentRGBA8/release. Native softwareMJPEG includes centered chroma expansion,
crop,BT.601 conversion,state reset and output ownership. CPU0,one codec/conversion
thread,optimizedJIT and nonconcurrentGC remain fixed. Normal/scalar native samples
and every managed pixel agree exactly.

SSE2 computes16interior outputs from three eight-sample verticalQ2 windows:
even=left+3center,odd=3center+right,bias8/shift4,then interleave/pack. Bounded word
sums retain the exact quarter-phase integer round. Full source spans precede
vector loads; clamped edges,tails,odd starts and single-pixel callers keep the
qualified integer path. Platforms withoutSSE2 keep that path too. Same64B stack
tiles,cancellation and owned capacity; no buffer,table,cache or API is introduced.

The23.6% short reduction clears the5% gate. Its wide three-sample upper30.849 is
retained. One independent12actual/12filtered confirmation reuses the validJCH1
baseline12/12; filtered/all-actual intervals separate and all brackets/idle-v2
pass. No baseline retiming or scoring repeats. These are sequential means,not
paired/general-content gains. Allocation is unchanged including harness overhead;
native allocation and live admitted peaks are unmeasured.

BothOS pass5054full checks (947live),293public JPEG normal/scalar controls and
15existing exact-pixel/callerRGB controls with AVX2 disabled. The SAME actual
Release package and three examples pass bothOS. Existing scalar proof is reused,
with no duplicate test algorithm or image grid. One corebuild perOS reuses the
immutable harness,workers,selectors and fixture. Published packages stay immutable.

Only vector-eligible420/422YCbCr→RGB/RGBA costs change; other affected controls
need current evidence before CPU-pass claims. Unrelated codecs/encoding/editing/
text and unchanged validation/Identify/inverse/color proofs remain applicable.
[Evidence](warm-jpeg-simd-chroma-evidence.json) retains all actuals/brackets,
filtered/all-actual bounds,guards,identities,allocations and the mirrored/member-
verified109-member raw master.

Next is one eight-pixel integer color-conversion primitive. The retained JCP2
profile identified19.3% in that unchanged method; no postchange component-cost
claim or fresh profile is made. Keep chroma/inverse variants closed. Full267-path
warm single-CPU parity and general readiness remain open.

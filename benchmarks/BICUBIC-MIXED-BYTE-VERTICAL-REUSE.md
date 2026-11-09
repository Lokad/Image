# Mixed-byte vertical source reuse

Complete bounded RGBA8 bicubic resize,513×259→1027×129, saves **145.36 µs /
14.06% raw /13.54% reference-normalized**:1,033.73→888.37 µs. Filtered and
all-actual raw/normalized confidence separate; native movement is−0.606%.
The original immutable DLL confirmation is reused through the bounded revision.
Inputs, settings, harness, owned output and complete operation boundaries agree.

| Existing complete control | Lokad µs | zimg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Byte mixed, default | 936.03 | 872.48 | 1.073 /1.084 | 114 | gap |
| Byte mixed, input+output+4 KiB | 888.37 | 866.88 | 1.025 /1.030 | 114 | gap |
| Byte height reduction, input+output+4 KiB | 494.90 | 635.25 | 0.779 /0.786 | 113 | pass |

All three affected costs refresh on the final actual-package DLL, with twelve
actuals and twelve native samples per bracket. Vertical remains passing and stops;
the mixed cases remain gaps. Other fresh points establish no causal companion
gain. No benchmark image or settings grid is added.

The source kernel reads ten decoded vectors instead of sixteen across two
independently accumulated rows when preparation proves contiguous8tap/+2
geometry. Each vector retains the existing source-column pair. Two delayed values
preserve each output's tap0..7 Multiply/Add order. The proof is prepared once per
band in reset dead padding; full, clamped, unequal and odd cases retain indexed
filtering. Certificates, normalization, original-order and tiny-alpha repair stay
unchanged. Source reads are not measured cache traffic. No floating-point policy
or encoder/metadata exception changes.

The existing64-row result loan, small-band suffix, column cache, conversion,
publication and ownership schedule remain. No array, reservation, owner, pool,
session, API or retained scratch is added. Working admission remains4,096 B and
the large band borrows28,136 B of already-admitted output before suffix writes.
Mixed/vertical results retain529,932/264,708 B until disposal. Total allocation,
admitted capacity and retained storage are separate; physical/native/total JIT
stack peaks are unmeasured. All original cancellation checkpoints remain; paired
rows skip only odd successors, whereas the original band poll occurs every eighth
row. The preparation adds a direct checkpoint. Refusal remains terminal.

The first coherent stage also applies the kernel to word resizing. A contemporary
bounded-word comparison gives893.16→947.47 µs: **54.32 µs /6.08% raw loss**, with
separated filtered/all-actual raw confidence. The5.21% normalized loss point has
overlapping intervals. That caller is unselected: the entire word source is
restored byte-for-byte, without a compatibility wrapper or dispatch flag. Every
sample and both stages' sources/modules remain. Final byte/kernel/test sources
are unchanged from that stage; final producer identity changes, so its three byte
costs refresh. No candidate timing is repeated to seek a favorable result.

The original word comparison also supplies a newer unchanged-path cost:
893.16/zimg851.29 µs, upper1.056 remains a gap. It supersedes the older bounded
862.21 µs observation, without a causal historical regression claim. Default and
vertical word paths reuse their unchanged evidence. The retained
[word bands](BICUBIC-RESULT-BACKED-BANDS.md) and
[byte bands](BICUBIC-BYTE-RESULT-BANDS.md) explain the preceding storage design.

One current two-second diagnostic capture motivates the stage:1,995 sampled
locations,1,871 in mixed-byte drivers including inlined certification. Address/
code-size mapping distinguishes the64-row and small-band generic instantiations.
The geometry model finds59/64 eligible pairs on the existing bands. These are
sampled locations and source applicability, not phase times, dynamic branch counts
or a predicted saving. Profile elapsed is discarded; its old fixed console byte
count has no evidence role. The frozen receipt identifies the529,932-byte result.

Windows/Linux each pass **6,124 full/live checks**,289internal/729public per
normal/scalar/AVX2-disabled/AVX512-disabled mode, and the SAME actual Release
package/all three examples. Four new direct controls independently check every
ordered double in both column pairs, byte/word ranges, tiny alpha and canaries;
existing complete pixel/admission/cancellation/survivor checks remain. Two coherent
main/embedded candidates per OS are retained; the final builds are reused through
qualification. Harnesses, selectors, native workers and fixtures require zero
builds. The initial local-name compile failure produced no candidate module.

Managed timing includes preparation, working storage, filtering, independently
owned output and disposal. The labeled FFmpeg zscale **zimg backend** retains its
graph/scratch; conversion, halo, filtering, packing and fresh output remain timed.
Exact managed pixels and scoped native float bounds stay distinct. This is not an
equivalent fresh-graph comparison. Runs are sequential Release/one CPU on the
exclusive quiet VM with idle-v2 guards.

[Evidence](bicubic-mixed-byte-vertical-reuse-evidence.json) owns current costs,
adverse stages, source/lifetime proof, qualification and reproducibility. Mirrored,
member-verified raw `bicubic-mixed-byte-vertical-reuse.tar.xz`:8,701,928 B /296members,
SHA256 `8ce15cc6a105a6ca5d77eb996856ac98c6d40472d43f60bb718c3781126933ab`.
Private package440,432 B, SHA256
`f3e113812c758f6f2100f0a47dc60f97e4f8900aa2fd571cc9c34fb195eb90ef`.
Published packages remain immutable. Publication is postponed and all283paths
remain required. Larger cached-wide and bounded narrow-byte gaps take priority.

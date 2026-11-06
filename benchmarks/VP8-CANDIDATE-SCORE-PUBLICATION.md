# VP8 candidate score publication

Retain scalar candidate pricing followed by publication of a full score only for
a winner. Complete photograph/diagram encoding independently saves **229.78µs/
2.02%** and **134.58µs/2.01%**, with unchanged paired allocation and exact packets.
This is a scoped improvement; the family still has native performance gaps.

## Design and correctness

The luma16/luma4/chroma searches previously constructed and repriced a40B record
before rejecting candidates. Compute the identical long integer expression first,
apply the same strict winner test, then construct the complete record. A repeated
uniform prediction similarly materializes only a winner. Uniform mode0 necessarily
publishes first; its reuse snapshot is unchanged. Keep final mode-lambda repricing
and score aggregation. No prediction, quantization, rate, reconstruction, pruning,
tie order, surcharge, cancellation, admission or ownership changes.

A narrow source projection reverses the four changed publication blocks and
shared pure price helper to the qualified source. Existing independent native
mode/level/pixel/packet oracles cover production paths, including uniform/strict
ties, dirty DP scratch, disposed borrowed state, aliasing, failure and async
ownership. No implementation-mirroring test or fixture is added.

The preceding photograph profile identifies constructor/copy work as a hypothesis
lead, not phase timing or predicted savings. An erroneous family-name lookup was
recovered from the completed trace/maps/actuals/guards without another capture.
Instrumented elapsed times are discarded. This stage does not repeat the earlier
compact-vector scoring implementation or introduce another rate table.

## Complete-operation effects

Original513×259 photograph/diagram inputs, Q75 and efforts4/6 remain frozen.
Adjacent immutable managed builds alternate order on quiet pinned CPU0. Three
short pairs use8complete encodes per subject; independent confirmations use12pairs
×16. Source setup/JIT/validation stay outside timing on both sides; normalization,
preparation, admission, fresh output growth, independent ownership and disposal
remain timed. The reused generic worker's case name is disclosed in evidence;
declared effort/settings and actual packet checks identify each control.

| Complete control | Original µs | Candidate µs | Change µs | Candidate/original interval | Allocated B/op | Result |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| photograph, effort4 | 11,353.82 | 11,124.04 | -229.78 | 0.97643–0.98311 | 809,072, both | Gain, independent |
| diagram, effort4 | 6,682.87 | 6,548.29 | -134.58 | 0.97576–0.98398 | 740,984, both | Gain, independent |
| trellis, effort6 | 11,116.83 | 11,005.99 | -110.84 | 0.93811–1.04483 | 754,344, both | Uncertain, short |

The photograph screen is promising but uncertain. The very wide ordinary-diagram
screen justifies its independent caller/generalization confirmation. Trellis
remains inconclusive: no gain, loss or equivalence claim, and no repeat seeking a
favorable number. Retain all points, including adverse short intervals. These
comparisons establish incremental effects, independently of native parity.

## Current native boundaries

Seven affected comparisons refresh on the final immutable Linux producer, with
12actuals each, native before/after brackets, conservative filtered/all-actual
confidence and existing source/packet/quality/alpha/owner/quiet checks. Native
wrappers retain their stated prepared context/scratch; managed preparation stays
timed. No settings grid or preparation exclusion. Distortion-only uses unchanged
code and reuses its qualified cost.

| Existing control | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| opaque8 | 16,092.23 | 15,493.47 | 1.039 / 1.088 | 1,201,552 | Uncertain |
| opaque16 | 16,613.63 | 16,959.27 | 0.980 / 0.987 | 1,201,552 | Pass/stop |
| photograph | 11,617.33 | 9,791.15 | 1.187 / 1.271 | 809,225 | Gap |
| diagram | 6,535.06 | 5,833.46 | 1.120 / 1.128 | 741,137 | Gap |
| distortion | 2,987.26 | 2,503.72 | 1.193 / 1.197 | 741,386 | Gap; unchanged source reused |
| trellis | 11,046.61 | 9,909.97 | 1.115 / 1.122 | 754,497 | Gap |
| alpha | 54,336.93 | 53,344.78 | 1.019 / 1.027 | 7,034,392 | Gap |
| alpha16 | 55,062.16 | 53,494.15 | 1.029 / 1.041 | 7,033,984 | Gap |

One passes/stops, six remain gaps, and opaque8 is uncertain. Its widened interval
does not preserve the preceding passing claim; alpha16 now has a demonstrated
gap. Keep both results without unchanged favorable resampling. Old/new standalone
clock movement is not an incremental gain or loss attributable to this stage.

Paired allocations stay unchanged; standalone GC/batch totals are reported
separately. No new heap capacity, owner, pool, session or cache is introduced.
Existing16KiB macroblock/2KiB trellis coarse admission remains; operation scratch
retained after return is zero. Whole-operation physical/admitted peak and native
allocation are not newly measured. Do not infer CPU gains from allocation.

## Qualification and evidence

Windows/Linux each pass **5,724 full/live checks** (2485internal/2291public/
948differential),110internal/137public affected checks per normal/scalar/
AVX2-disabled/AVX512-disabled mode. One immutable core/embedded build perOS is
reused throughout. The SAME actual unpublished0.2.1 Release package and all3usage
examples pass bothOS. Its existing22large OriginalVp8Controls tuples include
methods5/6, both depths, async, shared refusal and surviving outputs. Published
packages remain unchanged. No new harness/native/selector/fixture tool builds.

Only seven displayed costs invalidate;118others, including distortion-only,
reuse relevant unchanged production paths. Decoder/inverse, VP8L, PNG, JPEG,
editing and text are untouched. This is not a125-row actual-package release
refresh,282-path pass or publication.

[Evidence](vp8-candidate-score-publication-evidence.json) pins source, producers,
inputs, packets, every comparison and qualification. Windows `24e8191f10aa6b2929cc40c7dd4514af8915247659c8d2aa75917065d4171212`;
Linux `3d0a4a490ff1e9b1f54969553319a9b5593a1328a1022091e19f7a7dff0b3ae1`.
Actual package `c34f3992c7a0ce7d7b9c4c4e573667fc61641be868a6ca0836c6517c662ef386` (417,384B).
Raw `vp8-candidate-score-publication-qualified.tar.xz`,
`824cbbede338912f716c42ddcea64b7eebc9e092628b0b81a50cff98e008e5e8` (2,602,044B/230members),
mirrored/member-verified with pinned dependencies and payload-by-hash reuse.

[Trellis execution](VP8-TRELLIS-EXECUTION.md) retains its independently measured
gain and photograph tradeoff as history. Current costs above own present status.
Stop this bounded round. Continue larger remaining codec/resize work from actual
attribution, resolving uncertain paths before tuning. Publication is postponed.

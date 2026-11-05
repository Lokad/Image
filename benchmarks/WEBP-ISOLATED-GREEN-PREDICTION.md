# Isolated WebP green prediction

The retained SIMD/compact-green stage saves **2.73ms /4.39%** on the frozen
513×259 RGBA8 transparent WebP encode, Q75/effort4: **62,322.32→59,589.03µs**,
independent counterbalanced ratio .956148, interval [.952009,.960295]. All12pairs
of16complete public operations remain. Packets stay98,590B, exact against FFmpeg;
allocation stays7,033,576B/op in this adjacent-build driver. This is a scoped
incremental gain, not native parity or a general compression claim.

Four-pixel SSSE3/SSE4.1 neighbour arithmetic preserves averages, signed truncation,
selector ties, clamp rules and scalar row/edge/right-wrap tails. Compact green
counts, history and temporary products borrow the existing Analysis owner's
admitted storage/maps/geometry. Full-color fields/recursive visitor remain;
the actual general green implementation is the portable fallback. Cold map
coarsening/copy are shared; typed selectors keep small local exception cleanup.
Candidate order, selected maps, exact packets/pixels and independent output
ownership are unchanged. No public API, owner, pool, cache or dependency is added.

Compact execution uses4,368int words/level versus the existing owner's15,374,
with256long prepared products borrowing the unused padded histogram tail. This
removes repeated clearing/merging/pricing of singleton planes. **Actual admitted
capacity and total allocations are unchanged**: the borrowed token capacity is
still needed later, and initial owner preparation remains timed. Scratch retained
after disposal is zero. Complete physical/admitted peaks and native allocations
are unmeasured; logical representation savings are not heap savings.

The first stage confirms a2.49ms alpha gain but a1.01ms/1.12% photograph slowdown.
Keep both results. Targeted JIT shows identical normalized2,514B full-color
visitor instructions, while extracting finalization changes its caller. An
inlining annotation has no effect on the exception-bearing helper; that second
implementation receives no performance sampling. The final stage shares helpers
without exception handlers, which actually inline; full-color Select is1,331B
versus original1,214B. This supports investigation, not a proved causal account.

Final independent photograph ratio .999646, interval [.991485,1.007836], shows
no demonstrated gain/loss and does not establish equivalence. Both final short
screens are noisy and remain evidence; independent confirmations are separate.
Stop revising cold map finalization in this round, without favorable resampling.

| Current complete warm operation | Managed µs | Reference µs | Ratio / upper | B/op | Exact packet B, both | Status |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| VP8L RGBA8 | 109,807.33 | 127,005.55 | 0.865 / 0.940 | 7,476,544 | 183,812 | pass |
| VP8L opaque | 75,840.35 | 76,643.85 | 0.990 / 1.008 | 5,983,328 | 121,896 | uncertain |
| VP8L photograph | 81,361.78 | 83,323.50 | 0.976 / 1.022 | 6,517,158 | 160,032 | uncertain |
| VP8L text/diagram | 7,550.15 | 9,576.69 | 0.788 / 0.823 | 4,363,930 | 7,234 | pass |
| VP8L RGBA16, normalization | 116,271.20 | 123,756.44 | 0.940 / 0.977 | 7,476,544 | 183,812 | pass |
| VP8L async, fresh nonseekable output | 108,317.01 | 123,053.03 | 0.880 / 0.953 | 7,476,928 | 183,812 | pass |
| VP8 RGBA8 + lossless alpha | 60,891.24 | 57,859.54 | 1.052 / 1.063 | 7,034,392 | 98,590 | gap |
| VP8 RGBA16 + alpha, normalization | 61,467.08 | 57,931.10 | 1.061 / 1.073 | 7,033,984 | 98,590 | gap |

Lossless uses **libwebp exact=1, FFmpeg's encoder backend**; transparent VP8 uses
FFmpeg. All eight existing controls retain frozen inputs/settings, full packet,
quality/pixel/alpha and ownership checks,12actuals and both native brackets.
Worst filtered/all-actual confidence remains; the photograph's10filtered results
do not discard its12actuals. Four lossless controls pass/stop; opaque/photo remain
uncertain; transparent8/16remain gaps. No broader parity or word incremental-gain
claim. Other117displayed comparisons reuse unchanged production paths.

Current public-benchmark allocations differ from adjacent-driver allocation
because their measurement/batching differs; no library saving is inferred.
Necessary source/depth preparation, fresh complete output, ownership and disposal
stay timed on one quiet pinned CPU. Native contexts retain their declared
boundaries. Setup/JIT/checks and all instrumented profiling elapsed are excluded.

The final immutable build passes **5,711full/live checks per OS** (2,474internal,
2,289public,948differential),42internal/137public targeted normal/scalar checks
eachOS, and the SAME actual private0.2.1Release package/all3examples bothOS.
Earlier904internal/357public normal/scalar proofs remain for unchanged kernels.
The independent oracle checks8,613tiles and32,768prepared entropy scores,
including tails/ties, alternate alpha masses, dirty borrowed guards, cancellation,
terminal refusal/root recovery and independently owned surviving selected maps.
A C public-test filter initially matches no cases; correcting it needs no rebuild.
Unchanged54AVX512-off and wide-column proof is reused. Published packages are
unchanged and publication is postponed. This is not a full release refresh.

[Evidence](webp-isolated-green-prediction-evidence.json) pins every producer,
source, packet, comparison, adverse stage, JIT view and package/qualification.
Raw `webp-isolated-green-prediction-qualified.tar.xz`: 6,562,252B/489members, SHA256
`6c616579c4898110911cea8f84c4400111f065e0eee57f6c02827e6126c09fdf`, mirrored/member-verified. Dependencies retain the
[previous cost owner](WEBP-FRESH-HISTOGRAM.md),
[earlier compact tradeoff](WEBP-COMPACT-PREDICTOR-TRADEOFF.md), original profile
and unchanged fixtures/workers. All builds/commands/logs/guards/points remain.
Next investigate the remaining matcher/search cost using current attribution,
without blindly replaying the earlier chunked-prefix/preparation trials. Full
282-path CPU parity remains open; text/drawing work stays independent.

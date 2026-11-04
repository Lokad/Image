# PNG Fastest execution state

The greedy parser holds its logical cursor, fixed input extent, history origin
and token count locally. Literal/copy tokens share one emission point. Persistent
state is published before history sliding, full-block output/callbacks and every
return or failure. Original search/probe/link/insertion order, ties, blocks and
cancellation checkpoints remain. No alternate parser, buffer, pool or session.

| Existing 513×259 Fastest1 / native1, filter None | Before µs | Selected µs | FFmpeg µs | Ratio / upper | Allocated B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| RGBA16 | 22,266.14 | 20,918.85 | 18,611.95 | 1.124 / 1.144 | 3,420,516 | 968,216 |
| RGBA8 | 5,821.43 | 5,757.79 | 4,343.39 | 1.326 / 1.350 | 951,212 | 236,828 |

Independent word confirmation improves **6.05% raw / 3.14% reference-normalized**,
saving **1,347.29 µs raw**. Filtered and all-actual raw confidence and conservative
normalized intervals separate. The native clock runs **3.095% faster** between
captures; retain both raw and normalized figures rather than attributing the whole
raw point to this change. Byte's **1.09% / 1.04%** points have overlapping all-actual
and normalized intervals: no byte incremental gain or regression claim. Both
current controls retain demonstrated CPU gaps.

Four complete short runs precede four independent twelve-actual runs, each actual
containing four managed operations. All actuals, including estimator exclusions,
native brackets and versioned quiet guards remain. Both sides include fresh codec/
zlib/packet setup, complete encoding, independent output and disposal on one CPU.
The word reference also allocates/converts/releases its endian frame inside timing.
Original source/settings, full samples, hidden RGB, exact complete packets and
native CLI normal/scalar checks pass. No grid expansion or favorable resampling.

The preceding one-second current-word capture places 615/955 samples in the fast
parser, with token-emission/state and pruning/insertion hot sites. These are
attribution, not component times or available savings. The retained coordinate-only
trial left token count in the object; this stage also localizes emission state.
It does not replay the unchanged word-comparer call-site trial.

Allocation is unchanged: **3,420,516 / 951,212 B/op** for word/byte. Deflate workspace
admission stays **384 KiB**, retained operation scratch zero; no new arrays or
reservations. Complete admitted/physical peaks and native allocation are not
newly measured. Local limits, the borrowed shared root, refusal and cleanup remain.

Two new level1 cases extend existing cancellation/writer failures beyond real
dictionary slides, then prove successful reuse of the released caller budget and
the frozen complete packet. Existing level1/6/9 packet controls retain delivery
chunks, origins0/2^40, ring overlap and full BCL decompression. Both OSes pass
**5,547 full/live checks** (2,324 internal + 2,275 public + 948 differential), plus
563 internal/211 public PNG checks per normal/scalar mode. The same actual Release
package and all three examples pass Windows/Linux, including retained/async outputs.

One immutable core per OS serves all timings/checks. Adding the two tests requires
one additional Linux internal-test build, with no core rebuild. Windows builds its
core/internal tests once. Existing public tests, workers, selectors and harnesses
are reused; one package build. Published packages stay unchanged.

Only level1 parsing/emission changes. Other affected color/depth/async/cancelable/
forward-only costs remain historical; lazy levels6/9, stored0, decoding/inspection
and all other families reuse relevant unchanged-path evidence. This development
update is not a complete release refresh; the full 282-path goal stays open and
publication remains postponed.

[Evidence](png-fast-local-execution-state-evidence.json) pins producers, sources,
full actuals, comparisons, exact gates and actual-package proof. Raw
`png-fast-local-execution-state-qualified.tar.xz`: 3,360,440 B / 184 members, SHA256
`2e43ed736a2e97be6bae9bde4159a5067b61f9955910169845c3920cdc5f5d7f`,
mirrored/member-verified. Source/core/test snapshots, all runs/guards, package proof
and reproduction are retained. Qualified baseline, one-shot reference and current
fast-word attribution archives are pinned dependencies.

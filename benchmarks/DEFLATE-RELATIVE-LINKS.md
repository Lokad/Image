# Compact Deflate dictionary links

PNG encoding now keeps relative UInt16 dictionary links, reducing their capacity
from **512 KiB to 128 KiB**. The byte-history ring, keys, chain order, match limits,
ties and complete packets remain unchanged. A guarded AVX2 slide and scalar
fallback move link coordinates in place; no byte history, extra owner or pool.

| Complete one-shot encode | Before → after µs | Current FFmpeg µs | Ratio / upper | Allocated B/op, before → after |
| --- | ---: | ---: | ---: | ---: |
| RGBA8 Optimal6 | 24,462.89 → 20,504.02 | 17,281.42 | 1.186 / 1.194 | 1,325,732 → 932,516 |
| RGBA8 Fastest1 | 6,060.82 → 6,093.28 | 4,433.27 | 1.374 / 1.398 | 1,344,428 → 951,212 |

Optimal saves **3.96 ms / 16.18% raw elapsed**, or **9.61% reference-normalized**,
with separated filtered/all-sample and normalized confidence. Native elapsed
shifts −7.27%, disclosed rather than attributed to the code change. Fastest's
−0.54% raw / +0.11% normalized point changes have overlapping confidence.
Both CPU parity gaps remain open. Claims cover these two existing controls.

Each encode allocates **393,216 fewer bytes**. Codec workspace admission falls
from **1 MiB to 384 KiB**: fixed buffers use 266,236 B, with the unchanged
<64 KiB tree/code allowance and minor owners. This is a conservative codec
capacity charge, not a measured physical heap or whole-operation peak. No scratch
is retained afterward. Refusal precedes allocation and remains terminal.

At the existing origin boundary, relative position is below 65,275 before
insertion; even deferred-match insertion stays below 65,536. Saturating subtraction
by 32,768 preserves every still-valid absolute position. Zero maps to the already
excluded origin. Search limits/distances remain Int64, including translated
logical origins. Cancellation checkpoints bound sliding work.

One three-actual screen and one independent twelve-actual measurement per control
reuse immutable builds, the PNG84 one-shot FFmpeg worker, selectors and fixtures.
Valid unchanged before-runs are reused from the
[preceding bounded round](PNG-ROLLING-HASH-INCONCLUSIVE.md). All original packet,
pixel, source, ownership, settings and quiet single-CPU gates remain. Preparation,
encoding, independent output ownership and disposal are timed on both sides.

Windows/Linux each pass **5,418 full/live checks** and 577 affected checks in
normal/scalar modes. Fourteen complete native packets remain exact. New tests
cancel/fail after actual dictionary slides and verify cleanup plus exact recovery
on the borrowed root. The same actual Release package passes both platforms,
including large-input exact sync/async packets, the 384 KiB boundary, independent
outputs and all three usage examples. Published packages remain immutable.

Old stored-PNG test/consumer literals still assumed 1 MiB. Correcting those
expectations required public-test and consumer recompilation only; production
DLLs, timings and passing internal checks were reused. Failed logs remain.
Other affected PNG encoding costs are historical; decode/inspection and unrelated
paths retain applicable evidence. Publication is postponed.

[Evidence](deflate-relative-links-evidence.json) pins source/producers, all results,
proof and packages. Raw deflate-relative-links.tar.xz: 9,105,724 B / 295 members,
SHA256 78f41a7b55cf0b1594cf7d9e0c61c20a9e290e4cc3ef02258b242eb34e714890,
mirrored/member-verified. Reused before-runs/runtime dependency and every correction
are explicit; no unchanged candidate was resampled for favorable results.

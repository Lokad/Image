# VP8 shared-event stream investigation

Recording events with statistics preserves exact packets and removes the second
coefficient classification walk, but neither completed stage establishes a useful
primary opaque-encode CPU gain. Its allocation grows by **1,126,872 B/op**; photo
allocation falls by **120,744 B/op**. Photo CPU remains inconclusive, particularly
the noisy second-stage screen. Production is restored; no confirmation follows.

| Quiet one-CPU screen, three actuals | Managed µs | Managed/FFmpeg ratio / upper | Allocated B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: |
| 224 baseline, opaque16 | 20,356.30 | 1.302 / 1.402 | 1,201,552 | 72,380 |
| 224 candidate, opaque16 | 20,947.61 | 1.339 / 1.539 | 2,328,424 | 72,380 |
| 224 baseline, photograph | 13,671.34 | 1.409 / 1.455 | 809,072 | 17,688 |
| 224 candidate, photograph | 13,480.23 | 1.387 / 1.475 | 688,328 | 17,688 |
| 225 baseline, opaque16 | 20,278.95 | 1.299 / 1.349 | 1,201,552 | 72,380 |
| 225 candidate, opaque16 | 20,429.67 | 1.320 / 1.353 | 2,328,424 | 72,380 |
| 225 baseline, photograph | 13,830.38 | 1.433 / 1.538 | 809,072 | 17,688 |
| 225 candidate, photograph | 14,493.88 | 1.390 / 2.356 | 688,328 | 17,688 |

Stage 224 replaces ordinary effort 3–6 full-frame levels with 400-short rolling
scratch and admitted token chunks. Adaptive probability indices are resolved
after final probability selection; fixed-category/sign events and the counter9/
emission10 distinction remain exact. Chunks reuse capacity on format retries and
trim unused tails. The direct effort 0–2 skip/finalization policy is preserved.
There is no simultaneous full-frame coefficient/event copy or cross-operation pool.

One two-second stage-224 capture records 1,987 target-process samples: replay 371,
statistics 335. Their JIT bodies are 629/4,569 B. Per-event owner/state traffic
motivates stage 225: bulk block append and arithmetic state local to indexed replay.
Per-symbol owner APIs are removed. Both capture guards and the frozen packet pass;
samples/skid are not phase times or available savings. Instrumented elapsed is
discarded. This revision is a different implementation, not favorable resampling.

Complete existing 513×259 operations retain all normalization, preparation, output
growth/acquisition and disposal. FFmpeg retains codec/frame/packet wrappers;
repacking/complete encoding still execute each operation. Frozen inputs/settings,
raw packets, reconstructed samples, ownership and both quiet guards pass throughout.
All actuals and native brackets remain in the evidence. These short points prove
neither CPU gains nor causal regressions; no parity claim follows.

Each stage passes 820 internal/357 public checks per OS in normal/scalar modes.
New owner checks cover the signed coefficient domain/category boundaries against
the actual direct encoder, retry reuse/tail trimming, terminal refusal, cancellation
and output independence. Each real chunk is admitted before allocation, including
capacity/metadata. Stage 224 adds 832 B admitted stack; stage 225 adds another 640 B.
Complete admitted/physical peaks are unmeasured, not inferred from B/op.

Both qualified production cores/PDB/XML/sources and the actual package are restored
byte-for-byte, with no restoration/core/package rebuild. Two useful real-frame
fault-injection controls remain against production: every observed frame-admission
boundary refuses or cancels, releases partial storage, and permits a later encode.
49 writer/frame checks pass per OS/mode. Reuse PFE218's 5,547 full/live checks per
OS and same actual Release-package/all-three-example qualification, supplemented
by the two new controls. Candidate-only owner/code/tests are removed.

[Evidence](vp8-shared-event-stream-inconclusive-evidence.json) retains both stages,
the capture, all samples, failures and restoration. Mirrored/member-verified raw
`vp8-shared-event-stream-inconclusive.tar.xz`: 3,807,668 B / 320 members,
SHA256 `985d872580b5f20efa6c299036556afab1077057536e40d3161bc66d116dfa16`.
Its manifest pins qualified-package/reference and the earlier shared-grammar proof.
Earlier [state](VP8-COEFFICIENT-STATE-INCONCLUSIVE.md) and
[quantizer handoff](VP8-QUANTIZED-SCORING-INCONCLUSIVE.md) trials remain history.
No family is closed; publication is postponed.

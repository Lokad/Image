# Current opaque VP8 photograph and diagram controls

The two existing 513×259 opaque RGBA8 controls now have current cost evidence on
the qualified production build. Both complete packets match the frozen FFmpeg
libwebp quality75/method4 reference, including reconstructed pixels. Equal quality
numbers are not the acceptance criterion. These 3.56×/3.78× gaps take priority over
small gains elsewhere. No repeated confirmation is warranted for clearly failing
screens; full warm parity remains open.

| Quiet short | Lokad µs | FFmpeg/libwebp µs | Ratio / upper | Allocated B/op | Both packet bytes |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| PhotographLossy8 | 34,540.06 | 9,711.78 | 3.557 / 3.845 | 909,696 | 17,688 |
| DiagramLossy8 | 22,048.89 | 5,827.02 | 3.784 / 3.951 | 841,608 | 7,100 |

Both idle-v2 guards and all three actual iterations pass per case. Source bytes,
normal/scalar reconstructed reference pixels, exact alpha, syntax and independent
output ownership pass before/after. The approved normalized VP8 decoder graph
retains centered chroma and RGB16/Q15/Q13 precision. Timed complete operations
include source processing, normalization/import where needed, encoding, fresh
output acquisition/growth and release, without a redundant ToArray. Preparation,
validation, I/O, IPC and startup are excluded. One CPU, sequential warm Release.

The same immutable production and benchmark assemblies, native workers, fixtures
and archived selectors are reused. Actual Runnable_2/3 case receipts are checked;
no production/tool/worker build or expanded matrix. Existing 4,924 Release checks,
603 affected scalar WebP checks and the same actual private 0.2.0 package plus
three examples per OS remain valid. Earlier lossy figures describe superseded
encoders; these screens are not a comparison of gains against those producers.

One subsequent bounded diagram profile contains 715 managed workload samples.
Exclusive samples include quantization 79, small-block prediction 76, Luma4 71,
Luma16 64 and scalar forward transform 60. Inclusive counts overlap and inlining
limits precise method-cost attribution; instrumented timings are discarded.
The next bounded lead is repeated identical intra-mode predictions on this fixed
diagram. Count their incidence before selecting one substantial reuse candidate;
preserve distinct mode headers, rate/flat-state/tie behavior and exact packets.
No rate-bound/emitter retuning or settings sweep is implied.

[Evidence](warm-current-opaque-vp8-controls-evidence.json) retains samples, allocation
figures, pinned core/harness/selector/sampler identities, qualification reuse and
the mirrored member-verified raw trace/source archive. Other named required paths
remain open under the finite 253-case checklist.

+A count-only follow-up preserves exact input and the 7,100-byte packet. Of 51,260
+Luma4 candidate predictions across 5,126 block searches, 23,385 duplicate an earlier
+prediction; 1,665 blocks have all ten predictions identical. Luma16 has 803 repeats
+among 2,244 predictions (127 of 561 calls have all four equal). This is an ignored
+linked-source diagnostic, without a CPU-gain claim or production change. The
+mirrored member-verified count archive is recorded in the evidence. These counts
+support one substantial candidate to reuse identical pure Luma4 candidate work,
+retaining each mode's header, mode-zero rate distinction and exact tie behavior.
+
# Exact single-block VP8 reconstruction

The existing photograph Q75/effort4 independently measures 16.56 ms: 1.705×
reference time (upper 1.721×), exact 17,688 B and unchanged 809,056 B/op. Elapsed
is 3.06% below the preceding independent mean; separate runs are not a paired
baseline gain. Effort2 diagram independently measures 4.309 ms, 1.716×/upper1.727,
741,342 B/op and exact7,330 B. Each confirmation retains12actual/11filtered
samples; conservative bounds include all actuals and both native brackets.

| Existing quiet one-CPU control | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Output bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| DiagramLossyDistortion8 short | 4,302.11 | 2,513.47 | 1.712 / 1.929 | 741,342 | 7,330 |
| DiagramLossyDistortion8 confirm | 4,309.24 | 2,511.31 | 1.716 / 1.727 | 741,342 | 7,330 |
| PhotographLossy8 short | 16,713.06 | 9,722.40 | 1.719 / 1.768 | 809,056 | 17,688 |
| PhotographLossy8 confirm | 16,555.10 | 9,709.67 | 1.705 / 1.721 | 809,056 | 17,688 |
| DiagramLossy8 short | 8,865.19 | 5,800.56 | 1.528 / 1.574 | 741,121 | 7,100 |
| DiagramLossyTrellis8 short | 16,232.10 | 9,824.87 | 1.652 / 1.786 | 754,328 | 6,818 |
| vp8 short | 77,049.63 | 53,442.96 | 1.442 / 1.603 | 23,258,168 | 98,590 |
| EncodeOpaque16 short | 26,789.77 | 15,590.02 | 1.718 / 1.781 | 1,201,536 | 72,380 |

Single-block SSE4.1 reconstruction reads all sixteen coefficients before any
pixel publication, so a redundant stack snapshot is unnecessary even for aliased
views. Pairs still snapshot both blocks before reconstructing either. Compact
4×4 predictions round/clamp and pack through one vector pixel load/store; other
strides retain their stores. Scalar math remains unchanged. RFC6386 signed16
first-pass wrapping, inverse constants, rounding and final clipping are exact.

No buffer, cache, owner, admission or cancellation change. Borrowed inputs and
surviving outputs retain their lifetimes. Existing coarse scratch suffices; prior
opaque8 preparation still admits4KiB before its local tables. Measured B/op is
unchanged on all six anchors. Complete normalization, fresh output acquisition
and release remain inside equivalent single-CPU operation boundaries.

Both systems pass4,949 Release checks (1,967 internal,2,035 public,947 live),628
scalar WebP and52 focused checks. Frozen229,504 signed-domain controls and90 alias
cases remain;2,176 new compact overlapping controls compare the whole backing
storage against a snapshotted scalar reference. Native decision/complete-packet,
resource, cancellation, decoder and editing gates pass. The SAME actual private
0.2.0 package and all three usage examples pass both systems. Published0.1.0 is
unchanged. One production and linked-test build per OS serve the bounded round.

A current photo-only730-sample attribution justified this primitive after a
marginal source-energy trial was rejected/restored. All instrumented timings are
discarded. [Evidence](warm-vp8-single-block-reconstruction-evidence.json) retains
samples, guards, actual producers/package, source/native/fixture identities and
the mirrored/member-verified raw master. No worker, harness or selector rebuild.
Other affected VP8 costs are historical; unrelated paths retain valid evidence.
Stop reconstruction variants. Full255-path warm upper≤1× and readiness remain open.

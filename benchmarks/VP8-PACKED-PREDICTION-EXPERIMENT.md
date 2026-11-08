# Packed VP8 prediction boundary

VPK596/VPI597, unselected. A shared computation produces the original ten exact
4×4 predictors. The effort-2 search scores packed values, stores only winning
predictions and reuses widened source samples for residuals. Strided reconstruction
and other encoder consumers share the arithmetic. A second version requests
inlining to reduce the added call boundary.

| Complete operation | Qualified original µs | VPK596 µs | VPI597 µs |
| --- | ---: | ---: | ---: |
| Diagram encode, effort 2 | 2,638.29 | 2,548.79 | 2,532.60 |
| Resident alpha decode | 5,844.19 | 6,004.63 | 5,867.53 |
| Selected-trellis diagram encode | 6,396.63 | Not measured | 6,521.89 |

Effort-2 encoding saves **89.50µs/3.39%** and **105.69µs/4.01%** respectively;
both save about 3.62% after reference normalization. Filtered/all-actual raw and
normalized intervals separate. Its exact complete packet remains **7,330B** and
allocation remains **406,705B/op**. This useful lead is preserved.

The shared-consumer costs prevent accepting either implementation. VPK596 loses
160.44µs on alpha decode. Inlining recovers most of that cost, but VPI597 loses
125.27µs/2.51% normalized on selected-trellis encoding, turning a passing boundary
into a demonstrated gap. Opaque8 and effort-6 encoding also have separated raw
and normalized regressions. Other point changes and uncertainties remain in the
[evidence](vp8-packed-prediction-experiment-evidence.json); they are not pooled into
a general gain claim. A revised design must preserve strided/RDO consumer costs,
rather than replaying either implementation. This is not a permanent family ban.

All measured jobs preserve frozen inputs/settings, exact reference packets/pixels,
quality/alpha gates and independent outputs. Complete normalization, preparation,
admission, encoding/decoding and disposal remain timed. The labeled FFmpeg/libwebp
reference retains its declared contexts/scratch. Quiet single-CPU jobs use idle-v2
guards; no new content/settings grid or statistical measurement repeats occur.

Each epoch and OS passes **110 internal/239 public checks per normal/scalar mode**,
including the independent RFC pixel oracle, ten mode hashes over fifty contexts,
overlap/guard failures, cancellation and shared ownership. Both Windows full/live
runs pass 5,909 checks. Unselected Linux broad/package runs and three additional
effort-6 launches are avoided. A Windows case-folding collision between encode
`opaque8` and decode `Opaque8` downloads is recovered from byte-verified VM originals;
the completed measurements are not rerun. Future output names need a family prefix.

No owned capacity, cache or retained state is added; GC allocation is unchanged at
every measured boundary. Physical/JIT-stack and native peaks are unmeasured.
Qualified sources, main/embedded modules and public bindings are restored byte for
byte. Existing Windows/Linux 5,909-check and SAME actual package/three-example
qualification remains valid without rebuilding or repeating it. Current numeric
summaries and published packages are unchanged; publication is postponed.

Raw `vp8-packed-prediction-experiment.tar.xz`, SHA256
`e2e7482ef16b2844038ab517a074c1a85e04c3eb6b0c99b3b798d97350fb4108`,
15,212,304B/571 members, is mirrored/member-verified. It retains both source epochs,
modules, workers, all actuals/guards, source proofs, recovery and restoration.
The evidence links current attribution and qualified original providers.

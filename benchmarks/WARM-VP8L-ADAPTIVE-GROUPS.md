# Lossless WebP adaptive prediction with coding groups

The fixed photograph falls from **174,748 to 172,418 bytes**, saving **2,330 bytes
(1.33%)** with exact RGBA. It remains **12,386 bytes above** the frozen 160,032-byte
reference. Diagram remains 7,636/7,234 bytes; procedural opaque remains
123,602/121,896 bytes. All three complete-packet gates remain open. The other
lossless packet gates retain passing sizes. No broader compression claim is made.

One independently implemented 8×8 predictor map ranks all fourteen RFC 9649 modes
by channel entropy. Existing nine spatial coding groups are learned from its
residuals before complete-packet comparison with the grouped LEFT baseline.
Map/header/body costs and RIFF padding are included. The
[earlier predictor screen](WARM-VP8L-ADAPTIVE-PREDICTOR-AUDIT.md) used a different
basis; its old 2,652-byte gain is not counted here. Compression now takes priority
over CPU cost, so this verified incremental improvement is accepted.

The six existing operations below use unchanged 513×259 inputs, effort4 and
**libwebp exact=1, FFmpeg’s encoder backend**, quality75/method4. Managed RGBA16
normalization, hidden RGB, exact alpha, native settings and complete output
ownership are unchanged. Normal/scalar managed and FFmpeg pixels are exact.
These are **three-actual quiet-VM cost screens**, not independent CPU-parity
confirmations. Both selected/all-actual conservative bounds are retained. Native
allocations are unmeasured.

| Existing operation | Packet B managed/reference | Managed/reference µs | Ratio/upper | Managed allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| EncodeOpaque8 | 123,602/121,896 | 134,581.89/71,028.27 | 1.895/2.390 | 19,004,240 |
| DiagramLossless8 | 7,636/7,234 | 6,766.65/9,675.96 | 0.699/0.917 | 107,568 |
| PhotographLossless8 | 172,418/160,032 | 173,551.49/83,433.25 | 2.080/2.366 | 19,231,864 |
| Encode8 | 145,648/183,812 | 153,325.09/122,727.00 | 1.249/1.401 | 19,973,528 |
| Encode16 | 145,648/183,812 | 165,089.28/121,436.61 | 1.359/1.699 | 19,971,896 |
| WebpLossless8AsyncNonSeekable | 145,648/183,812 | 146,589.83/121,202.16 | 1.209/1.520 | 19,973,912 |

The added analysis costs CPU and allocation even when the older packet wins.
Photograph allocation is19,231,864 B/op (historical C49:9,912,952); opaque is
19,004,240 B/op (historical C49:9,669,728). Historical observations are not paired
estimates of the CPU change. All six old C49 independent confirmations remain
historical; current rows use the short costs above. No timing repeats or speed-only
tuning were performed. The compression gates and deferred full CPU objective stay open.

Both OSes pass **4,772 tests each**, including936 live FFmpeg comparisons, plus
37internal/117public affected scalar checks. Thirteen new tests cover every
predictor/border, partial tiles,8/16-bit normalization, selected map/group owners,
terminal refusal during replacement, cancellation, exact output limits, cleanup,
recovery and independent survivors. Capacity is admitted before allocation;
predictor-map charges survive coding-table replacement. No shared-refusal fallback.

One production and one internal-test build per OS are reused throughout. Unchanged
public/differential tests, harness, native workers and selectors are reused without
builds. The same actual private0.2.0 package and all three examples pass on both
platforms; published0.1.0 is unchanged. All seven lossy gates stay closed, with
unchanged primary anchors and separate ALPH code. Other paths retain valid evidence.

[Numeric evidence](vp8l-adaptive-groups-evidence.json) preserves every actual,
allocation, guard, producer, quality record and qualification. Mirrored raw:

- `c91-qualified-windows.tar.gz`: `1d5644526f48798b5d07aa9d5a3fb0faa204d8fc4b189203e39a4d6a1be3f3e3`

- `c91-qualified-linux.tar.gz`: `ef88cdb5176029364b7f49e77fca222e451ae2e58aee0568a6689439946db828`

Actual private package SHA256: `f3a75af310a5014cda2ab4e2181c0b35aa52f1c4b0039f8e252e29027e2c89d1`.

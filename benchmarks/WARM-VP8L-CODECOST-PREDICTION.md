# Lossless WebP predictor refinement by coding cost

The fixed photograph falls **164,008 → 161,410 bytes**, saving **2,598 bytes
(1.58%)** with exact RGBA. It remains **1,378 bytes above** its 160,032-byte
reference. Actual managed output exactly matches the fixed C103 prototype;
no recipe retuning was needed. These controls establish no broader size claim.

Diagram **7,160/7,234**, procedural opaque **123,602/121,896** and alpha8/16/async
**145,648/183,812** packets are unchanged. The diagram and alpha gates pass;
photograph and opaque remain open. Opaque now has the larger remaining gap
(1,706 bytes). All seven lossy gates remain stopped with identical primary VP8
packets/quality anchors and unchanged separate ALPH helpers.

One refinement of an uncached predictor map scores all fourteen format modes
against actual selected channel-code lengths plus predictor-map code cost.
Missing codes cost fifteen bits; stable ties retain the old mode. Color
coefficients remain fixed. One existing spatial-group refit and complete-packet
comparison decide adoption. Integer scoring uses a bounded tile cache rather
than another frame copy; 621 photograph tiles change mode.

New predictor/code storage and overlapping old/new tables use the same borrowed
admission root before allocation. Both alternatives borrow the same color owner;
only the losing predictor/coding owners release at comparison. Shared refusal,
cancellation and work exhaustion are terminal, with no fallback. The new map
remains charged until its final user finishes.

The six existing complete operations retain original 513×259 inputs/settings:
effort4, **libwebp exact=1, FFmpeg's encoder backend**, quality75/method4, frozen
reference hashes, 16-to-8 normalization, hidden RGB and independent output
ownership. Normal/scalar managed and FFmpeg pixels are exact. These are
**three-actual quiet-VM cost screens**, retaining selected/all-actual bounds,
not independent CPU passes. Native allocations are unmeasured.

| Existing operation | Packet B managed/reference | Managed/reference µs | Ratio/upper | Managed allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| EncodeOpaque8 | 123,602/121,896 | 221,143.26/70,308.02 | 3.145/3.968 | 36,142,008 |
| DiagramLossless8 | 7,160/7,234 | 21,922.45/9,575.16 | 2.290/3.462 | 1,721,182 |
| PhotographLossless8 | 161,410/160,032 | 397,627.31/82,836.29 | 4.800/5.633 | 44,090,320 |
| Encode8 | 145,648/183,812 | 240,372.88/121,953.91 | 1.971/2.316 | 37,808,768 |
| Encode16 | 145,648/183,812 | 267,128.46/122,039.66 | 2.189/3.035 | 37,807,136 |
| WebpLossless8AsyncNonSeekable | 145,648/183,812 | 238,002.05/121,435.65 | 1.960/2.364 | 37,809,152 |

Photograph allocation rises from 36,327,472 to 44,090,320 B/op, with higher CPU
cost. The verified compression increment is accepted under compression-first
ordering; these observations are not a controlled estimate of the speed change.
All six costs refresh because the shared encoder entry changed. Other five
packet/allocation values are unchanged. No independent repeat or speed-only work.

Both OSes pass **4,815 tests each**, including 936 live FFmpeg comparisons, plus
80 internal/117 public affected scalar checks. Eleven new tests cover 8/16-bit
complete packets, exact output limits and independent lifetimes, actual retained
charges, replacing predictor ownership while transferring one color owner,
local guards, early/late refusal/cancellation/work cleanup/recovery, and variable
alpha/hidden RGB with frozen color coefficients.

One immutable production/internal-test build per OS is reused. Unchanged public/
differential tests, harness, native workers and selectors require no builds.
The same actual private 0.2.0 package/all three examples pass on both OSes;
published 0.1.0 remains immutable. No workload is added; other relevant proofs
are reused and CPU parity stays deferred. The
[backward-reference round](WARM-VP8L-REFERENCES.md) is historical.

[Numeric evidence](vp8l-codecost-prediction-evidence.json) retains every actual,
allocation, guard, producer identity, quality result and qualification. Mirrored raw:

- `c104-qualified-windows.tar.gz`: `8f03b4852a4d018db051617c594489217d4991c4841aaf7eb3ab6a1782803192`
- `c104-qualified-linux.tar.gz`: `430b3c4410389a4bf560c00882f1fd74dcaefb06f2f9853c1974dae806e17423`

Actual private package SHA256: `f7d7f5c27c4f72443a0da31d6c521ecad7879d75cd6697b3fcb218039d2ea130`.

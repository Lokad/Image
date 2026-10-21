# Lossless WebP bounded backward references

The fixed text/diagram packet falls **7,636 → 7,160 bytes**, saving **476 bytes
(6.23%)** and passing its **7,234-byte reference** with 74 bytes of headroom.
The actual managed packet exactly matches the earlier fixed byte-only candidate;
no recipe retuning was needed. This closes this declared complete-packet gate
with exact RGBA; it establishes no broader compression claim.

Photograph **164,008/160,032** and procedural opaque **123,602/121,896** remain
unchanged and open. Alpha8/16 and async **145,648/183,812** retain passing packets.
All seven lossy gates stay closed with identical primary VP8 packets/quality
anchors and unchanged separate ALPH code.

One raw cached alternative uses four-pixel keys, at most sixteen matching keys
and 256 hash-chain nodes per token, a 262,144-pixel history and 1,024-pixel maximum
copy. Strict current-code-cost gains replace spans ending at original token
boundaries; missing codes cost fifteen bits during this proposal step. Literal/
distance trees are refit once before comparing complete packets. Original cache
prefix semantics remain exact; retained tokens drive both counting and writing.
Predicted models and spatial grouping are unchanged.

Original/result token capacities, prefix costs, bounded index arrays, records and
overlapping tables use the same borrowed admission root before allocation. Samples
are normalized on demand without another frame copy. Search scratch and losing
owners release promptly; selected token capacity remains charged through final use.
Shared refusal, cancellation and work exhaustion are terminal, with no fallback.

The six existing complete operations preserve original 513×259 inputs/settings:
effort4, **libwebp exact=1, FFmpeg's encoder backend**, quality75/method4, frozen
reference hashes, normalization, hidden RGB and independent output ownership.
Normal/scalar managed and FFmpeg pixels are exact. These are **three-actual quiet-VM
cost screens**, retaining selected/all-actual bounds, not independent CPU passes.
Native allocations are unmeasured.

| Existing operation | Packet B managed/reference | Managed/reference µs | Ratio/upper | Managed allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| EncodeOpaque8 | 123,602/121,896 | 223,323.17/71,325.46 | 3.131/4.333 | 36,142,008 |
| DiagramLossless8 | 7,160/7,234 | 21,919.21/9,457.14 | 2.318/3.789 | 1,721,182 |
| PhotographLossless8 | 164,008/160,032 | 327,683.41/83,295.82 | 3.934/4.246 | 36,327,472 |
| Encode8 | 145,648/183,812 | 239,021.47/121,346.40 | 1.970/2.256 | 37,808,768 |
| Encode16 | 145,648/183,812 | 268,183.76/121,761.68 | 2.203/2.712 | 37,807,136 |
| WebpLossless8AsyncNonSeekable | 145,648/183,812 | 238,520.42/121,133.47 | 1.969/2.477 | 37,809,152 |

Diagram allocation rises from 107,568 to 1,721,182 B/op, with higher CPU cost.
The verified compression gain is accepted under compression-first ordering;
these observations are not a controlled estimate of the speed change. All six
costs refresh because the common baseline scanner changed. Other five packet and
allocation values are unchanged. No independent timing repeats or speed-only work.

Both OSes pass **4,804 tests each**, including 936 live FFmpeg comparisons, plus
69 internal/117 public affected scalar checks. Twelve new tests cover 8/16-bit
complete-packet bounds, hidden pixels, output/source independence, actual retained
capacity and replacement peaks, local guards, early/late refusal/cancellation,
work cleanup/recovery, plane/direct distances with narrow-width aliases, and
overlapping copies across history-ring wrap.

One immutable production/internal-test build per OS is reused. Unchanged public/
differential tests, harness, native workers and selectors require no builds. The
same actual private 0.2.0 package/all three examples pass on both OSes; published
0.1.0 remains immutable. The passing diagram control stops here. Other valid
proofs remain scoped to their producer; no workload is added and CPU parity stays
deferred. The [uncached-model round](WARM-VP8L-NO-CACHE.md) is historical.

[Numeric evidence](vp8l-reference-evidence.json) retains all actuals, allocations,
guards, producer identities, quality results and qualification. Mirrored raw:

- `c99-qualified-windows.tar.gz`: `ab57dcc1ef880ee283dbeb16a34329befd78c02bb17fa1408592b0b1fd734d32`
- `c99-qualified-linux.tar.gz`: `5c18284b24b89daeb08f90716eb22a5ddc81f7efd73f622caba81792f4937518`

Actual private package SHA256: `37f1cb1aaec691c739c29a3d8d7c135be54c8364c9c8cc26ca2610ccddbc24da`.

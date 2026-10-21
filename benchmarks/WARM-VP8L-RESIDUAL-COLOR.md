# Lossless WebP residual color decorrelation

Historical producer and costs; current results are in the
[uncached-model round](WARM-VP8L-NO-CACHE.md).

The fixed photograph falls from **172,418 to169,808 bytes**, saving **2,610 bytes
(1.51%)** with exact RGBA. It remains **9,776 bytes above** its160,032-byte reference.
Diagram7,636/7,234 and procedural opaque123,602/121,896 bytes are unchanged; all
three complete-packet gates remain open. Alpha8/16 and async packets retain passing
145,648/183,812-byte sizes. These controls establish no broader compression claim.

One source-derived signed cross-color transform per8×8 predictor-residual tile
uses least-squares seeds and a fixed entropy-guided coordinate pass. Current
spatial groups are refit before comparing full coefficient-map, tree, payload and
RIFF costs with the retained baseline. The original red drives blue decorrelation;
signed shifts and modulo256 inversion preserve exact pixels. Neighbors/residuals
are read on demand, without a full-frame residual copy or imported tables.

The bounded C92 diagnostic reproduced both predecessor packets exactly, then
suggested169,788 bytes. Actual managed output is169,808; that20-byte difference
was not retuned. Acceptance uses the verified managed packet. The useful increment
is accepted under compression-first ordering despite higher CPU/allocation costs.

The six existing complete operations retain original513×259 inputs and settings:
effort4, **libwebp exact=1, FFmpeg’s encoder backend**, quality75/method4. Reference
packet hashes,16-to-8 normalization, hidden RGB/alpha and equivalent independent
output ownership are unchanged. Normal/scalar managed and FFmpeg pixels are exact.
These are **three-actual quiet-VM cost screens**, not independent CPU-parity passes.
Both selected/all-actual bounds are retained; native allocations are unmeasured.

| Existing operation | Packet B managed/reference | Managed/reference µs | Ratio/upper | Managed allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| EncodeOpaque8 | 123,602/121,896 | 197,183.16/71,368.36 | 2.763/3.331 | 28,375,728 |
| DiagramLossless8 | 7,636/7,234 | 8,007.95/9,538.74 | 0.840/1.047 | 107,568 |
| PhotographLossless8 | 169,808/160,032 | 270,154.34/83,524.33 | 3.234/4.214 | 28,606,536 |
| Encode8 | 145,648/183,812 | 216,209.87/121,811.73 | 1.775/1.988 | 29,691,384 |
| Encode16 | 145,648/183,812 | 236,999.99/121,419.93 | 1.952/2.461 | 29,689,752 |
| WebpLossless8AsyncNonSeekable | 145,648/183,812 | 215,150.08/120,136.58 | 1.791/2.104 | 29,691,768 |

Added analysis costs CPU/allocation even when the previous packet wins. Photograph
allocation is28,606,536 B/op, versus historical C91’s19,231,864; these elapsed
observations are not a controlled estimate of the performance change. No independent
timing repeats or speed-only work were performed. The deferred CPU objective remains open.

Both OSes pass **4,783 tests each**, including936 live FFmpeg comparisons, plus
48internal/117public affected scalar checks. Eleven new cases cover signed
coefficients across65,536 channel/coefficient pairs, borders,8/16-bit normalization,
exact output limits, terminal refusal/cancellation/work exhaustion, recovery and
independent outputs. A selected-owner check independently totals retained tables
and both maps, detecting early release of the inherited predictor owner. Color and
coding-plan replacement keep those charges live; shared refusal never selects a fallback.

One immutable production/internal-test build per OS is reused. Unchanged public/
differential tests, harness, native workers and selectors are reused without builds.
The same actual private0.2.0 package/all three examples pass on both OSes;0.1.0 is
unchanged. Exactly six cost rows refresh. All seven lossy gates remain closed,
with unchanged primary packets/quality anchors and separate ALPH code. Other paths
retain their valid evidence. No workload is added; full compression/CPU/readiness
objectives remain open. The [earlier adaptive-map round](WARM-VP8L-ADAPTIVE-GROUPS.md)
retains its identified historical producer and observations.

[Numeric evidence](vp8l-residual-color-evidence.json) preserves every actual,
allocation, guard, producer, quality result and qualification. Mirrored raw:

- `c93-qualified-windows.tar.gz`: `d348dc598a2beba0d58ec3f8483dd3c561b029215e2f4e0ae5fd166b5c7c3cae`

- `c93-qualified-linux.tar.gz`: `dd88ba1914655e879f3fa37dc9989a1ba56a1bab35059400db2dcbc57367d2cf`

Actual private package SHA256: `63783853f1ab5c2fb5545aa835eca4544e946381e873ec8c2fd3b36b220b6c6c`.

# VP8L complete-packet reproduction

The production managed encoder reproduces **19 selected complete packet
identities across25 existing source/settings checks**, plus two additional forced
candidate identities. Every packet byte, including framing and padding, matches;
there is no metadata exception. The reference is libwebp1.6.0, quality75,
`exact=1`, explicitly labeled as FFmpeg's encoder backend. Claims are limited to
these original controls and declared settings.

| Original control | Method | Complete bytes |
|---|---|---:|
| Asymmetric RGBA8, 513×259 | 4 | 183,812 |
| Opaque projection, 513×259 | 4 | 121,896 |
| Photograph, 513×259 | 4 | 160,032 |
| Text/diagram, 513×259 | 4 | 7,234 |
| Singleton RGBA `(255,0,1,0)` | 4 | 34 |
| Two-color, 4103×2 | 0 / 1..6 | 48 / 46 |
| Xorshift direct coding, 129×129 | 4 | 50,010 |
| Hidden RGB, 19×7 | 4 | 82 |
| Single axis, 16383×1 / 1×16383 | 4 | 74 / 74 |
| Four-color partial packing, 7×3 | 4 | 66 |
| Sixteen-color partial packing, 17×3 | 4 | 130 |
| Eighteen-color implicit zero, 23×13 | 4 / 0 | 236 / 268 |
| Asymmetric RGBA8, 513×259 | 0 / 1 / 5 | 316,566 / 176,216 / 155,162 |
| Photograph, 513×259 | 5 | 160,600 |


Source pixels and options determine transform choice, predictor/color maps,
residuals, palette ordering/packing, cache/reference parsing, spatial groups,
Huffman trees and framing. Reference packets, tokens and maps are verification
inputs only. Production has no native or package dependency.

Method0 uses forced prediction or numeric palette ordering and the low-effort
parser. Other methods use greedy palette ordering; small palettes compare ordinary
and geometry-derived Box matches. Method5 compares cached/uncached coding and,
for palettes, plain/spatial candidates. The eighteen-color control has an implicit
final zero. Partial packed rows and maximum native single-axis dimensions are
included. Method6/quality100 brute-force search is outside the pinned quality75 scope.

Two independently forced method5 candidates add distinct packet identities:
**256B palette-plus-spatial** for the eighteen-color input and **50B spatial Box**
for the two-color input. Their selected plain packets are236B/46B, already counted
above. All candidates, fine/coarse adaptive predictor decisions and selection ties
reuse the frozen observations; no new content/settings grid was added.

Production borrows the caller's admission root through every stage. One normalized
working image is transformed in place, then released before emission. Owned
transform maps, tokens and canonical codes survive independently; counting and
writing reuse them without an encoded-frame copy or repeated pixel analysis.
Unused populations and losing candidates release promptly. Refusal is terminal;
cancellation, work limits, output limits and writer failure retain cleanup guarantees.
Async output uses one admitted staging owner until actual delivery finishes.

The public integration passes1,855 internal,2,070 public and940 live FFmpeg checks
per platform using the same immutable Windows-built workers. A final test-only
addition covers the two existing16383 axes through internal/public packet checks;
it leaves the qualified production DLL unchanged. Scalar, cooperative admission,
normalization and sync/async/ImageBudget checks preserve exact pixels, independent
outputs and borrowed inputs/root. The actual private Release package and all three
usage examples are checked on both platforms. [The evidence ledger](vp8l-byte-parity-evidence.json)
records identities, separate worker scopes and qualification receipts.

The16384×1/1×16384 controls remain functional format extensions: their pixels
round-trip exactly, but the native encoder stops at16383. They establish no native
encoder-byte or performance claim.

The earlier source-model and component proofs remain reusable development evidence;
production now uses their admitted typed implementation. The bounded production
checklist is [qualified separately](VP8L-PRODUCTION-COVERAGE.md). Lossy WebP encoder
replication, exact decoder coverage and subsequent one-CPU/allocation qualification
remain open. Historical timing rows describe their frozen producers.

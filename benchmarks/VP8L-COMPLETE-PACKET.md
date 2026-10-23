# VP8L complete-packet reproduction

One dependency-free source model reproduces **19 complete packet identities** on
Windows and Linux with the same frozen assembly. Every packet byte, including
framing and padding, matches; there is no metadata exception. The pinned reference
is libwebp 1.6.0, quality 75, `exact=1`, explicitly labeled as FFmpeg's encoder
backend. This is development tooling; production adoption remains open.

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

Transform choice, predictor/color maps, residuals, palette ordering and packing,
cache-aware/free parsing, clustering, Huffman trees and framing come from source
pixels. Reference tokens, maps, trees and packets are verification inputs only.
Palette auxiliary encoding uses internal quality 20; its parsing skips optimal
tracing below 25. The eighteen-color control encodes 17 entries and uses the
format's implicit final zero.

Method 0 uses forced prediction, four literal-cost bins with unconditional bin
merging, and lossless histogram-map coarsening from 128×128 to 512×512 tiles.
Method 1 uses 64×64 predictor/color tiles. At method 5, independent hierarchical
selection reproduces **every 4×4 and 8×8 candidate mode and entropy cost** on the
asymmetric input and photograph. Separate accumulated histories and quadrant
traversal select 8×8 and 4×4 respectively, followed by complete cached/uncached
writer-payload comparison. Both complete packets match. The photograph's method-5
packet is larger than its method-4 packet; this is byte reproduction of distinct
settings, without a compression-gain claim.

The existing eighteen-color input also qualifies both method-5 transform
candidates: **236 B plain palette / 256 B palette-plus-spatial**. These are
complete forced-candidate observations; the pinned public reference selects the
236-byte packet already counted above. The independent model predicts packed
opaque index carriers, applies palette-bounded cache analysis and compares complete
writer payloads. Every byte of both candidates and the selected result matches.
No new fixture or spatial-winning control was added.

Effort 0 now reproduces the existing eighteen-color input's **268-byte** packet:
numeric ordering moves zero last and omits it from storage. For small palettes,
the independent box matcher derives its window from format distance geometry,
reuses prior matches and compares complete writer payloads with the ordinary
parser. The existing two-color control qualifies both 1,026-entry indices
(1,020 nonzero copies) and complete **46/50-byte** plain/spatial box candidates.
Their ordinary candidates tie and remain selected. No new image or settings grid
was added. Pinned FFmpeg decodes both newly checked packets to exact source RGBA,
including hidden RGB.

One frozen assembly passes all 19 selected identities and the forced candidate/index
checks, source preservation, independent outputs and mutation rejection on both
platforms. This round used two Windows model builds: the first isolated the numeric
palette mismatch, the second fixes its implicit-zero order. Two native adapter builds
were needed to correct its input-width guard; four forced candidates and three
public-worker preparation encodes reuse existing inputs. Production builds,
timing/allocation runs and repeated package qualification were unnecessary.
[The evidence](vp8l-byte-parity-evidence.json) records exact scope and mirrored raw
receipts. Production integration is in progress. Matching, prediction and color
transforms pass 8/9/10 focused checks in one assembly on Windows/Linux. Prediction
uses two admitted original-row buffers; color search borrows unchanged tiles and
then transforms them in place, without tile or full-frame copies. Compacted color
maps retain their actual capacity charge until disposal. Native maps/residuals,
admission/refusal/recovery, cancellation and independent work limits are checked.
Transform-family analysis adds 18 checks: thirteen frozen-control decisions across
all five families and the required effort paths, exact 256/257-color cardinality,
shared refusal/recovery, cancellation and independent work cleanup. It borrows
untransformed normalized ARGB and returns only scalar decisions; its fixed histogram
and color-counting storage is released before subsequent transforms. Palette ordering/packing adds
12 checks: six frozen native palette/delta/index controls, independent owners,
overlapping capacity, refusal/recovery, cancellation and early/late work failure.
Palette data has its own retained owner; index packing reuses the working buffer
without refunding its still-allocated capacity. All 57 focused checks pass with one
assembly on Windows/Linux. Uncached parsing adds 11 checks: exact predictor/color
and palette auxiliary token streams, frozen greedy/repeat/trace decision costs,
candidate/trace refusal cleanup, cancellation, late work failure and typed token
ownership. Tokens use owned eight-byte entries; positions are derived cumulatively.
Trace scratch is admitted before allocation and losing candidates are released.
Cached main parsing adds 11 checks. The asymmetric control matches all 110,306
native semantic tokens, both cache choices and all candidate costs; the hidden-RGB
repeat winner, ordinary/Box/cache-disabled palette streams and 18-color uncached
decision also match. Cache conversion reuses owned token storage and trace shares
the uncached traversal. All 79 focused checks pass with one assembly on both
platforms, including refusal/cancellation/late-work cleanup. No additional native
encoding or timing was needed.
The last full shared-build qualification (E53) passed 3,884 offline tests per
platform, the same private Release package and all three usage examples on both.
These public paths remain unchanged. The encoder is not switched: complete-operation
qualification remains open for the integrated codec.

Build once with `dotnet build tools/WarmReference/Vp8lParityModel -c Release --tl:off`
and reuse the assembly with the packet verifiers. The four-core verifier uses
Lokad.Image only to prepare frozen PNG inputs, outside the encoding model.
`verify-vp8l-small-palette-efforts.ps1` checks numeric order and box candidates/indices.
`verify-vp8l-palette-candidates.ps1` checks the two transform candidates and selection.
`verify-vp8l-adaptive-predictor.ps1` checks both adaptive selectors and packets;
`verify-vp8l-nonpalette-efforts.ps1` checks the fixed effort/cache primitives.
The other verifiers cover their named families.

The six original workload rows share four core packet identities. Their complete
RGBA16 normalization and asynchronous public operations still need integration
qualification. The native public encoder refuses sides above 16383: the axis
companions do not establish packet parity for existing 16384-pixel functional tests.
The frozen source model's two 16384-axis packets pass exact pinned-FFmpeg decoding;
these remain separate functional format-extension checks, without an encoder-byte
or timing claim.
Complete effort-family routing, admitted production integration, work and
cancellation limits, output ownership and actual-package qualification remain
open in the [finite coverage checklist](VP8L-PRODUCTION-COVERAGE.md).
CPU parity and allocation tuning resume after required exact codec coverage.

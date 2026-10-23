# VP8L complete-packet reproduction

One dependency-free source model reproduces **18 complete packet identities** on
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
| Eighteen-color implicit zero, 23×13 | 4 | 236 |
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

The same consolidated assembly passes all 18 identities, repeated packets,
source preservation, independent outputs and mutation rejection on both platforms.
This round used two Windows model builds (adaptive selector, then complete packet
composition), one native observation adapter and three preparation encodes for
one new photograph snapshot. It reused the asymmetric packet and all existing
inputs. There were no Linux/production builds, timing/allocation runs or repeated
package proofs. Unchanged production qualification is reused. Numerical identities,
source hashes and mirrored raw receipts are in
[the evidence](vp8l-byte-parity-evidence.json).

Build once with `dotnet build tools/WarmReference/Vp8lParityModel -c Release --tl:off`
and reuse the assembly with the packet verifiers. The four-core verifier uses
Lokad.Image only to prepare frozen PNG inputs, outside the encoding model.
`verify-vp8l-adaptive-predictor.ps1` checks both adaptive selectors and packets;
`verify-vp8l-nonpalette-efforts.ps1` checks the fixed effort/cache primitives.
The other verifiers cover their named families.

The six original workload rows share four core packet identities. Their complete
RGBA16 normalization and asynchronous public operations still need integration
qualification. The native public encoder refuses sides above 16383: the axis
companions do not establish packet parity for existing 16384-pixel functional tests.
Complete effort-family routing, admitted production integration, work and
cancellation limits, output ownership and actual-package qualification remain
open in the [finite coverage checklist](VP8L-PRODUCTION-COVERAGE.md).
CPU parity and allocation tuning resume after required exact codec coverage.

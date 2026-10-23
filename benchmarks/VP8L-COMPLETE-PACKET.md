# VP8L complete-packet reproduction

One dependency-free source model reproduces **17 complete packet identities** on
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

Transform choice, predictor/color maps, residuals, palette ordering and packing,
cache-aware/free parsing, clustering, Huffman trees and framing come from source
pixels. Reference tokens, maps, trees and packets are verification inputs only.
Palette auxiliary encoding uses internal quality 20; its parsing skips optimal
tracing below 25. The eighteen-color control encodes 17 entries and uses the
format's implicit final zero.

The latest round adds the asymmetric input's non-palette effort primitives.
Method 0 uses forced prediction, four literal-cost bins with unconditional bin
merging, and lossless histogram-map coarsening from 128×128 to 512×512 tiles.
Method 1 uses 64×64 predictor/color tiles. The method-5 snapshot chooses the same
8×8 transform prefix as method 4, then selects the smaller uncached writer
payload (155,141 bytes versus the cached candidate), excluding RIFF padding.
This is a cache-comparison proof, **not general adaptive-sampling qualification**.

The same consolidated assembly passes all 17 identities, repeated packets,
source preservation, independent outputs and mutation rejection on both platforms.
Two Windows model builds were needed in this round: the method-0 prototype and
the typed consolidation. There were no Linux/production/native builds, native
encodes, timing/allocation runs or repeated package proofs. Unchanged production
qualification is reused. Numerical identities, source hashes and mirrored raw
receipts are in [the evidence](vp8l-byte-parity-evidence.json).

Build once with `dotnet build tools/WarmReference/Vp8lParityModel -c Release --tl:off`
and reuse the assembly with the packet verifiers. The four-core verifier uses
Lokad.Image only to prepare frozen PNG inputs, outside the encoding model.
`verify-vp8l-nonpalette-efforts.ps1` checks the three additional effort packets
against retained snapshots; the other verifiers cover their named families.

The six original workload rows share four core packet identities. Their complete
RGBA16 normalization and asynchronous public operations still need integration
qualification. The native public encoder refuses sides above 16383: the axis
companions do not establish packet parity for existing 16384-pixel functional tests.
General adaptive predictor sampling, admitted production integration, work and
cancellation limits, output ownership and actual-package qualification remain
open in the [finite coverage checklist](VP8L-PRODUCTION-COVERAGE.md).
CPU parity and allocation tuning resume after required exact codec coverage.

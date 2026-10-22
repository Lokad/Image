# VP8L complete-packet reproduction

The source-only model reproduces **all four unique frozen VP8L packets**:
183,812 B asymmetric RGBA8, 121,896 B opaque projection, 160,032 B photograph
and 7,234 B text/diagram. Windows/Linux use the same frozen model and compare
every byte, including framing and padding, without metadata exceptions.
The reference is libwebp 1.6.0, method 4, quality 75, `exact=1`, explicitly
labeled as FFmpeg's encoder backend.

Initial transform choice comes from source histograms and entropy estimates.
The first three inputs select subtract-green, prediction and cross-color;
the diagram selects subtract-green alone. Selected cache sizes are 10, 2, 0
and 10 bits. The first three packets use nine spatial coding groups; the
single-group diagram omits the group-map syntax. Candidate matching, cache-aware
and cache-free parsing, clustering, all Huffman trees and packet serialization
come from source pixels. No reference tokens, maps, trees or packets enter the model.

The six existing workload rows contain these four packet identities. RGBA16
normalization and asynchronous delivery reuse the first packet's core proof;
their complete public-operation qualification remains open for integration.
Other shapes, palette/direct transform families and settings are unqualified
in this development model. Production continues to support its existing scope.

Both platforms pass source preservation, repeated complete-packet comparison,
independent outputs and mutation rejection. The standalone source-host entrypoint
passes on the asymmetric control. Production and the frozen predictor/parser/math
models are unchanged. This round adds no native build/query/encode, timing,
allocation measurement, library rebuild, full suite or package proof.

`verify-vp8l-controls.ps1` accepts the original raster, frozen compression fixture
directory, two reference archives, model assembly and existing dependency directory.
Its library argument supplies managed PNG decoding during fixture preparation only;
it is outside the encoder model and production dependencies. Numerical identities,
exact scope and raw evidence are in [the evidence](vp8l-byte-parity-evidence.json).

Next: named production-coverage gaps, bounded shared admission, cancellation/work
limits, and complete public-operation/package qualification before adoption.
CPU parity and allocation tuning resume after the required exact codec coverage.

The separate palette model also reproduces the existing two-color 4103×2
functional input's complete 46-byte packet on Windows/Linux. Palette ordering,
channel deltas, 1,026 packed samples and all five main Huffman trees agree;
data ends at bit 206. The same frozen binary and direct source-host verifier pass
source preservation, independent outputs and mutation rejection. This qualifies
two-color packing with a partial byte, not arbitrary palettes or production
admission. It reuses the existing reference snapshot and frozen helpers, with
no native encode or timing run. `verify-vp8l-palette-packet.ps1` records its scope.

The existing 129×129 xorshift input's direct-coding packet is also raw-exact:
50,010 bytes on both platforms with the same frozen model. Dimension-aware repeat
seeding, trace parsing and framing produce no transform/cache, one group, five
Huffman trees and the reference end bit 399,906. An independent packet reader
recovers every source sample. `verify-vp8l-direct-packet.ps1` checks complete
packets, source preservation, independent outputs and mutation rejection using
the retained reference archive. General shape qualification and production
adoption remain open; the earlier four-core/palette binaries are unchanged.

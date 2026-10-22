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

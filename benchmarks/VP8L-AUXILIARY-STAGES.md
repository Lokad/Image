# VP8L transform and auxiliary-image stages

The source-only model reproduces the first **6,006 payload bits** of the existing
513×259 RGBA8 reference packet, through both transform images. It also reproduces
all cross-color coefficients and coded residual samples. The reference remains
libwebp 1.6.0, method 4, quality 75, `exact=1`, explicitly labeled as FFmpeg's encoder
backend. This is one frozen control; complete encoder parity remains open.

The predictor image contains 337 tokens and occupies 2,492 bits. The cross-color
image contains 713 tokens and occupies 3,459 bits. Source-derived match candidates,
parser selection, all ten Huffman trees, tree headers, canonical codes and token
bits agree with the packet. Predictor coding selects the refined parse; cross-color
coding selects the repeat-oriented parse. The maps and residuals come from source
pixels, without reference tokens, trees or decoded maps entering the model.

Cross-color coefficient search uses generated geometry and mathematical entropy
costs. Match indexing and parsing preserve deterministic candidate/tie decisions.
Code-length run selection and serialization follow RFC 9649. Upstream sources
and implementation tables are absent from the model and production build.

The same Windows-built model passes Windows/Linux checks for exact bits, source
preservation, independent outputs and mutation rejection. A separate source-host
entrypoint also passes on Windows. An adapter queried the already pinned native
backend once to observe predictor match candidates/references; it performed no
complete image encode. Its tokens agree with the independently read frozen packet.

Production remains unchanged. These development models have no production memory
admission or performance qualification. No library rebuild, package proof, timing
or allocation run was repeated. Main-image cache/group selection and coding come
next, followed by complete packets, admitted integration and the remaining controls.
The subsequent [main-image parser](VP8L-MAIN-PARSER.md) now matches every token on
this control; source-derived spatial clustering remains open.

Run `tools/WarmReference/verify-vp8l-auxiliary-stage.ps1 -SourcePath SOURCE.rgba
-NativePacketPath REFERENCE.webp`. Supply `-AssemblyPath` to reuse a frozen model
assembly with its own predictor-model dependency alongside it.
[Numeric evidence](vp8l-byte-parity-evidence.json) records identities, boundaries,
build counts and retained raw evidence.

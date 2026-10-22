# VP8L complete-packet reproduction

The source-only encoder model reproduces the **entire 183,812-byte reference
packet** for the existing 513×259 RGBA8 control. Windows and Linux use the same
frozen model and produce raw-identical bytes, without metadata exceptions.
The reference remains libwebp 1.6.0, method 4, quality 75, `exact=1`, explicitly
labeled as FFmpeg's encoder backend. This qualifies one control, not production
encoding or the remaining lossless/lossy controls.

Source pixels determine the transforms, residuals, 110,306 tokens, 2,145 spatial
histograms, nine coding groups and all 45 main Huffman trees. Clustering performs
2,049 entropy-bin merges, 54 fixed-seed sampling merges and 33 greedy merges,
then remaps the original histograms. Sampling reproduces a fixed algorithmic
sequence; no image/settings search or output-driven tuning is involved.

The group map occupies 4,466 bits. Main-image data begins at payload bit 36,698
and ends at bit 1,470,323. Generated canonical codes, tree headers, cache/copy
references, RIFF/VP8L framing and padding all agree. Public syntax follows
RFC 9649. No upstream source or implementation tables enter the models or
production build; existing independent stage models supply their mathematics.

Both platforms pass complete byte comparison, borrowed-source preservation,
independent output ownership and mutation rejection. The standalone source-host
entrypoint passes on Windows. Existing production decoding evidence is reused
because the packet is byte-identical to its frozen input. Production remains
unchanged; admission, work limits, cancellation and performance require separate
qualification during integration. No new native build/query/encode, library build,
full suite, package proof, timing or allocation measurement accompanies this round.

Run `tools/WarmReference/verify-vp8l-packet-stage.ps1 -SourcePath SOURCE.rgba
-NativePacketPath REFERENCE.webp`. Optional `-AssemblyPath` reuses the frozen
packet model, with its predictor, auxiliary and main-parser models alongside it.
[Numeric evidence](vp8l-byte-parity-evidence.json) records hashes, exact scope and
retained raw evidence. Next: the five remaining frozen lossless controls and
bounded production integration, followed by the other required codecs/objectives.

# VP8L main-image parser

The source-only model now reproduces **all 110,306 main-image tokens** on the
existing 513×259 RGBA8 control: 52,617 literals, 6,039 copies and 51,650 color-cache
references. Every position, type, value and length agrees with the frozen packet.
The same Windows-built model passes Windows/Linux checks, with repeatable results
and unchanged source pixels. The separate source-host entrypoint passes on Windows.

The pinned reference remains libwebp 1.6.0, method 4, quality 75, `exact=1`, explicitly
labeled as FFmpeg's encoder backend. Source-derived coded residuals feed the already
qualified candidate index. All 132,867 packed candidates agree with a backend stage
query. Cache selection follows the pinned encoder's range of zero through ten bits;
the format itself permits eleven. The chosen cache uses ten bits.

The standard seed has 116,487 tokens and the repeat-oriented seed has 132,843.
The refined standard parse wins after source-derived histogram pricing, cache-hit
pricing and deterministic path selection. Generated geometry supplies plane-distance
codes. Independent observation of the frozen packet and the native stage query give
the same complete semantic-token digest. Neither enters the source model.

Production remains unchanged. No new native build/query, full encode, timing,
allocation measurement, library build or package proof accompanies parser refinement;
the preceding index/cache observation used one native adapter build/query against
the already pinned archive. An initial source-host import omission was corrected;
the final immutable model passes both platforms and the source entrypoint.

This proves one control's parse, not complete packet equality. The nine spatial
coding groups, their map and their Huffman serialization must still be generated
from source. The subsequent [complete-packet model](VP8L-COMPLETE-PACKET.md)
now reproduces those stages and the entire packet on this control. Other controls
and production admission/performance remain open.

Run `tools/WarmReference/verify-vp8l-main-parser.ps1 -SourcePath SOURCE.rgba
-NativePacketPath REFERENCE.webp`; optional `-AssemblyPath` reuses the frozen model,
with the independent predictor and auxiliary models alongside it.
[Numeric evidence](vp8l-byte-parity-evidence.json) records exact identities and scope.

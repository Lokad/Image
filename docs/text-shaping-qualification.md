# OpenType text layout qualification

This records the public shaping checkpoint. [Subsequent fitting qualification](text-fitting-qualification.md)
completes the twelve fixed controls within the declared font/table scope.

`TextOptions.Shaping = TextShaping.OpenType` connects supplied-font substitution,
positioning and Unicode direction rules to the public layout API. Measurement
and rendering share retained placements/outlines. Latin, Hebrew and Arabic use
the supplied face's default language system; styles come from that face. Existing
default legacy pixels are preserved.

Joining controls remain effective through direction processing; source spans
survive ligatures. Wrapping preserves grapheme/ligature clusters, reapplies
positioning at line boundaries and checks the resulting width. Start/center/end
alignment and each line's paragraph direction are explicit. [SPEC.md](../SPEC.md#supplied-truetype-fonts)
lists supported tables and typed unsupported cases, including other scripts,
missing joining features, selected cursive/contextual positioning and mirrored
glyphs without an encoded partner or supported font substitution. This checkpoint
does not include fitting or prove general font compatibility/replacement readiness.

Font tables borrow retained input. Temporary glyph/scalar/line workspace and
retained output geometry use the same cooperative admission context. Preparation
owners release before publication; a layout remains renderable after font
disposal. Every growing admission in the authored layout can refuse terminally,
with cleanup and subsequent successful admission. Cancellation/work failures also
refund owned storage. No global font/glyph cache or RGBA intermediate was added.

Qualification passed **4,383 tests per OS**: 1,574 internal, 1,881 public API and
928 live FFmpeg comparisons. All **135 shaping/direction/GSUB/GPOS checks per OS**
also passed with intrinsics disabled. Seventeen new checks exercise public
composition/anchors, required feature order, joining controls, source spans,
direction, mirrors, cluster wrapping, line-edge positioning, alignment and memory
ownership/refusal/cancellation. The same actual private 0.2.0 Release package
passed both platform consumers, including authored Arabic layout/rendering,
font disposal, shared admission and all three usage examples.

The [fixed font comparisons](text-compatibility.md) pass eleven controls on both
platforms at this checkpoint; fitting was qualified subsequently. No performance
timing was measured. Existing historical timing evidence
retains its original producers; new text paths still require qualification.

Retained SHA-256 identities:

- Qualified source: `04e844eb61eeb1acb647a0d457d85804f9a94e53ae0383e6e72015fe7520eef6`.
- Windows producer: `d3f26f22ad12d445fa24d7e746089bc3875b37a6e95de74595e53e1523a1383f`.
- Linux producer: `e89c6cbf8cb3f3e4d763d7367a533f675bf9336ddb786c68b1c3ea036e95ca51`.
- Actual package: `5264d30d4377ce85449f6d52305bc55dbd2fb8fec49c0d9be73b5d8727f70ca0`.
- `p2e-qualified-windows.tar.gz`: `e89ef4ae14a995bcb83ea51ae8bc0171cb4615594fc86ebe815465e5b9a01323`.
- `p2e-qualified-linux.tar.gz`: `4ad9acaa0c0052dfb3c183459c50c2cb7814aed98678070c994d7f1d2a5d44b7`.

Published 0.1.0 remains unchanged.

# Staged OpenType positioning

This records the internal GPOS checkpoint before public integration. The current
[OpenType layout mode](text-shaping-qualification.md) uses this interpreter;
replacement qualification remains open.

The implementation independently follows the public
[GPOS specification](https://learn.microsoft.com/en-us/typography/opentype/spec/gpos).
It supports single and pair positioning, mark-to-base, mark-to-ligature,
mark-to-mark and extensions. Standard features share a selector and execute in
lookup-list order. Deferred attachments propagate later parent movement and
advance changes, including right-to-left placement. Design-coordinate anchors
support unhinted outlines; selected cursive/contextual lookups and nonzero
device/variation adjustments currently fail with `UnsupportedImageFeature.FontTable`.

Tables borrow retained font storage. Glyph storage, selectors and attachment
workspace use the same admitted operation context. Replacement capacity includes
old/new overlap. Refusal is terminal and owned reservations are released on
failure, cancellation and disposal.

Qualification passed **4,316 tests per OS** on Windows/Linux: 1,507 internal,
1,881 public API and 928 live FFmpeg comparisons. All **68 substitution and
positioning checks per OS** also passed with intrinsics disabled. The 32 new
positioning checks cover lookup/anchor semantics, filtering, ligature components,
run boundaries, deferred parent movement, malformed tables, typed unsupported
features, cancellation after admission, work limits and refusal cleanup/reuse.
The same actual private 0.2.0 Release package passed isolated consumers and all
three usage examples on both systems.

The fixed real-font combining-mark and Latin-kerning probes match reference glyph
sequences and total advances. Maximum nonempty-glyph origin error is
**0.000000938 pixel**, below the existing 1/64-pixel gate. This internal probe
does not qualify Arabic shaping or rasterization. The twelve public text controls
remain six passing and six open. No performance timing was measured.

Retained SHA-256 identities:

- Qualified source: `bfe916a648f94eb8d36acdd33697812a3a95826824f597454e1d722be7f9763f`.
- Windows producer: `a3f069a89d8a3889d6715b7b8c2bbc0771c1d27578685c0bd223135784454f97`.
- Linux producer: `f476661f849e8a107732d868e8ab5bb351f00731fe130a452fdce6e521f1d416`.
- Actual package: `2a53c3cc4aa4468d2cec1f08da22cb0799ceb56a2322c76c00fa3d697bfd9a18`.
- `p2c-qualified-windows.tar.gz`: `0526757af80dea9f69745ba7e86d11fdf689c3b4d097af017c80033340b0a157`.
- `p2c-qualified-linux.tar.gz`: `4664ed35fe8b62eaa7cf61a4b1f210e9d9678d57fb458e256b6e49fcd13a6e2c`.

Published 0.1.0 is preserved. Existing performance results remain scoped to their
qualified historical producers; these checks make no new timing claim.

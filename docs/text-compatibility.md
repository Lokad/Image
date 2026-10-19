# Supplied-font compatibility qualification

Explicit `TextShaping.OpenType` now passes **all twelve** fixed controls on
Windows/Linux: Latin kerning, multiline/empty CRLF, narrow long-word wrapping,
line spacing, center/end alignment, supplied bold/italic faces, combining marks,
mixed Hebrew/Latin/digits, joined Arabic with marks/digits and constrained-area
fitting. The [fitting qualification](text-fitting-qualification.md) records the
finite size grid, ownership/failure tests and matching reference decision.

The bounded comparison uses DejaVu Sans 2.37 regular/bold/oblique, Fonts 2.1.3,
ImageSharp.Drawing 2.1.7 and ImageSharp 3.1.12. Fonts/license stay in ignored
`external/`; [provenance](../tools/TextReference/font-provenance.json) pins their
release URL and bytes. The [comparison tool](../tools/TextReference/README.md)
is separate from the solution and production package and makes no timings.

Glyph/grapheme-cluster sequences and line breaks agree exactly. Maximum glyph
position error is 0.0000011 pixel, union-ink alpha RMSE is at most 9.701/255,
coverage error is below 1.550%, and raster bounds agree. The unchanged gates
require positions within 1/64 pixel, raster edges within one pixel, mean absolute
alpha error ≤8/255, RMSE ≤16/255 and coverage error ≤2%. Reference DPI, hinting,
line advance and leading origin are explicit; different default line boxes are
not compatibility work.

[Public shaping qualification](text-shaping-qualification.md) records producers,
package checks, ownership/failure tests and raw evidence. [SPEC.md](../SPEC.md#supplied-truetype-fonts)
defines supported scripts/font tables and explicit unsupported features. These
named fonts/text/settings do not prove general font/script compatibility,
performance parity or replacement readiness. Legacy mode preserves its previous
logical layout.

The initial six-passing/six-open audit is retained for comparison:
`p2-text-compatibility.tar.gz` SHA-256
`8827f8e2116d252d1f1091db41a0a644278199411e4538b74cfe6fb288500a30`;
raw `3cd5092c253d78faae307390371d16fb3d7f1a2f624ee729f9127056c97ab87e`.

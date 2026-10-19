# Supplied-font compatibility qualification

Qualification is in progress. The initial bounded comparison uses twelve original
text cases, DejaVu Sans 2.37 regular/bold/oblique faces, Fonts 2.1.3,
ImageSharp.Drawing 2.1.7 and ImageSharp 3.1.12. Fonts and their license remain in
ignored `external/`; [provenance](../tools/TextReference/font-provenance.json)
pins the release URL and bytes. The [comparison tool](../tools/TextReference/README.md)
is separate from the solution and production package, and reports no timings.

A [staged GSUB primitive](text-substitution-qualification.md) is now qualified
internally. Public layout integration and the gaps below remain outstanding.

Six controls pass: Latin kerning, multiline/empty CRLF, narrow long-word wrapping,
line spacing, supplied bold and supplied italic. Glyph and grapheme-cluster
sequences and line breaks agree exactly. Maximum glyph-position difference is
0.0000011 pixel; union-ink alpha RMSE is at most 9.274/255 and coverage error is
below 1%. Reference DPI, hinting, line advance and leading origin are explicit.
Different default line boxes and trailing-leading policies are not compatibility
work. The gates retain raw measurements and require positions within 1/64 pixel,
raster edges within one pixel, mean absolute alpha error ≤8/255, RMSE ≤16/255 and
coverage error ≤2%. These tolerances were established from the control cases.

Remaining demonstrated gaps:

- Center/end alignment: absent public options; current glyph origins differ by
  up to 38.526/77.051 pixels in the fixed 96-pixel wrap box.
- Combining marks: glyph substitution/positioning differs; alpha RMSE 54.897/255.
- Mixed Hebrew/Latin/digits: visual glyph/cluster order differs; RMSE 129.859/255.
- Arabic marks/digits: joining, ligatures, mark positioning and visual order
  differ; 20 managed versus 18 reference glyphs and RMSE 157.688/255.
- Constrained-area fitting: current prepared layout matches the measurement/
  raster control, but a qualified public fitting operation is still required.

This is initial evidence for the named fonts/text/settings, not general text
compatibility or replacement readiness. The production producer is the qualified
0.2.0 memory checkpoint (`4c79daf9ab28932dc0874e64685550be1345da522d91d2ee184bfc10868796d0`).
Retained `p2-text-compatibility.tar.gz` SHA-256:
`8827f8e2116d252d1f1091db41a0a644278199411e4538b74cfe6fb288500a30`.
The raw comparison SHA-256 is
`3cd5092c253d78faae307390371d16fb3d7f1a2f624ee729f9127056c97ab87e`.

# Supplied-font compatibility

Supply standalone TrueType faces and select their actual style. Lokad.Image does
not synthesize bold/italic, search system fonts, execute hints or select variation
axes. Default logical layout uses horizontal `kern`; choose `TextShaping.OpenType`
for supported Latin, Hebrew and Arabic shaping and bidirectional paragraphs.

OpenType supports the default language system, composition/localized/joining
forms, required/contextual/standard ligatures and mark positioning. Implemented
GSUB lookups are single, ligature, chained-context, extensions and type 5 format 3.
GPOS supports single, pair, mark-to-base/ligature/mark and extensions. Selected
cursive/contextual positioning, other scripts, device corrections and nonzero
default-instance variation corrections fail with typed `FontTable` details.
Validated GDEF corrections summing to zero at the default instance are accepted;
this does not enable variable-font axes. Missing required Arabic joining features
or unsupported mirroring also fail explicitly. There is no font fallback.

Measurement and rendering share retained placements and outlines. Wrapping keeps
OpenType clusters intact; alignment follows paragraph direction. Layout, ink and
union bounds distinguish advance boxes from visible overhangs. Fitting uses union
bounds on an explicit finite size grid. See [the contract](spec.md#supplied-fonts-and-text)
and [examples](usage.md) for positioning and lifetime rules.

The public comparison set is DejaVu Sans 2.37 regular/bold/oblique, against
SixLabors.Fonts 2.1.3, ImageSharp.Drawing 2.1.7 and ImageSharp 3.1.12. Twelve fixed
controls cover kerning, multiline/empty CRLF, narrow wrapping, line spacing,
center/end alignment, supplied styles, combining marks, Hebrew/Latin/digits,
joined Arabic with marks/digits and constrained-area fitting. These named faces
and settings do not establish arbitrary font/script compatibility.

Glyph/cluster selection and line breaks must agree exactly; positions within
1/64 pixel, raster edges within one pixel, mean absolute alpha error ≤8/255,
RMSE ≤16/255 and coverage error ≤2%. Recorded controls pass: maximum position
error 0.0000011 pixel, RMSE ≤9.701/255 and coverage error <1.550%. Reference DPI,
hinting and line origins are explicit; different defaults are not compared.

[Font provenance](../tools/TextReference/font-provenance.json) pins licensed
reference bytes; fonts/licenses stay in ignored `external/`. The independent
[comparison tool](../tools/TextReference/README.md) reproduces these controls
without production dependencies. Authored table/outline fixtures cover unsupported
features and malformed input. Broader font-engine and performance claims remain open.

# Supplied-font comparison

This standalone correctness tool is excluded from the solution and production
package. It uses the pinned ImageSharp.Drawing 2.1.7, Fonts 2.1.3 and ImageSharp
3.1.12 references; it makes no performance measurements. Upstream fonts/license
stay in ignored `external/`, with their release URL and hashes in
[font-provenance.json](font-provenance.json). No upstream assets are tracked.

Prepare once, build against a verified immutable producer, then reuse that build:

```powershell
python tools/TextReference/prepare-fonts.py
dotnet build tools/TextReference -c Release --tl:off -p:NuGetAudit=false -p:LokadImageAssembly=D:/path/to/frozen/Lokad.Image.dll
dotnet tools/TextReference/bin/Release/net10.0/TextReference.dll D:/code/image > tmp/text-reference.json
```

The twelve fixed cases cover kerning, multiline/empty CRLF, narrow wrapping,
line spacing, center/end alignment, supplied regular/bold/italic faces, combining
marks, mixed Hebrew/Latin, Arabic marks/digits and constrained-area fitting.
Each measures and renders with the same layout options. DPI72 and disabled
hinting match pixel units; reference line advance/leading are explicitly matched
to the supplied face metrics. Different default line boxes are not a production
compatibility task. Raw outer advance rectangles are retained, including their
different trailing-leading policy.

The public comparison explicitly selects `TextShaping.OpenType` and maps line
alignment by name. Eleven controls currently pass on Windows/Linux; constrained
fitting stays a failure until the public fitting boundary is implemented.

Require exact glyph/source-cluster sequences and line breaks, glyph positions
within 1/64 pixel, and raster edges within one pixel. On the union of nonzero-
alpha pixels (0..255), require mean absolute alpha error ≤8, RMSE ≤16 and total
coverage error ≤2%. These raster tolerances were set from the matched Latin,
wrapping and supplied-style controls before accepting any complex-script case.
Missing public boundaries remain failures. Source clusters are checked through
the reference's public character-advance API, mapping managed scalar offsets to
Unicode grapheme starts and excluding its nonrendered CR/LF placeholders. The
renderer/measurement code-point association is checked before scoring. Raw records
support investigating shaped-text differences. This is a bounded initial
qualification set, not a claim of general script/font compatibility.

For a producer containing the staged GSUB primitive, the correctness-only
`--probe-substitution <repo-root>` mode applies composition to the existing
combining-mark and Arabic controls. It exercises the actual internal parser
through reflection in this tool, verifies borrowed/retained memory cleanup, and
explicitly reports that joining, positioning and public layout integration remain
incomplete. Production has no reflection-based shaping or reference dependency.

`--probe-positioning <repo-root>` additionally exercises staged GPOS on the same
combining-mark and Latin-kerning controls, recording glyph origins and advances.
It verifies admission cleanup and reports that public layout integration and
complete shaping remain unavailable. It does not time operations or qualify
Arabic layout or rasterization.

`python tools/TextReference/generate-unicode-properties.py --check` verifies the
staged direction/joining property constants against the pinned Unicode 17
specification files in ignored `external/specifications/unicode-17.0.0/`. The
generator records exact input hashes, independently combines normative facts,
and is excluded from production builds. It does not import an upstream algorithm
or test corpus. Public OpenType mode consumes these property constants.

# Authored-font cmap4 reference correction

AFC1140 resolves the demonstrated **reference forward-mapping defect**, without
changing Lokad.Image, fixtures or gates. The five original comparisons remain
performance-unmeasured: tab positioning/representation and compound/raster issues
still require equivalent output before timing. No text slowdown or speedup claim.

The stock Fonts 2.1.3 reader returns a glyph-array entry without its segment delta.
The [OpenType cmap4 rule](https://learn.microsoft.com/en-us/typography/opentype/spec/cmap#format-4-segment-mapping-to-delta-values)
adds that delta modulo 65,536 for nonzero entries; zero remains `.notdef`. The
frozen packed face's U+0041 has array value 1 and delta 1: correct glyph 2, stock
reference glyph 1. The correction affects only this forward-array branch.

Reproducible reference source: Fonts commit
`eadc8dbc041d1b335ecf790ce400b5e069f075de`, infrastructure submodule
`353b9afe32a8000410312d17263407cd7bb82d19`. Build the unmodified source control and
the four-line corrected reader with the archived Release/net6.0 command, preserving
assembly version/token for API binding. Stock NuGet assemblies and the original
checkout remain unchanged. Distinct SHA-256 identities are explicit:

- Stock NuGet: `e661c6ac204861eab86e45eb8736988491170da9a8f5d040d380cf6b9cd395ff`.
- Source-built control: `ff38f12034d6012dd62ec81c28ab5000c00cd638959940a72fe205875c78cb5d`.
- Source-built **Fonts 2.1.3 + cmap4 delta**:
  `f9d47adcb475285b03bf920fd377fd85d4e5838bfa830d02dcdd916cde6ed7be`.

The source-built control reproduces all six stock observations exactly. With the
correction, every packed non-tab glyph matches the managed sequence; the other
five reference observations remain unchanged. All managed results, four font
byte sequences and full-precision frames remain unchanged. All 18 observations
agree between Windows/Linux for each of the three reference contexts, including
retained layouts and shared-root cleanup. This isolates the mapping correction;
it does not establish complete parser/layout equivalence or a passing packed case.

The existing compiled probe and actual package DLL are reused; no production,
harness, VM or package build. Two Windows reference-source builds only. Reuse
6,749 full/live checks per OS and SAME actual package/all-three-example proof for
unchanged production. No broad qualification or performance captures.

Next: tab semantics/control representation using equivalent complete public
operations, then independent compound-placement attribution and clipped RGBA8/16
visual gates. Include every necessary layout/preparation call and owned result
inside its proper eventual timing boundary. Keep exact managed glyph/cluster/
line/fitting/pixel checks, position/bounds tolerances and full16-bit visual gates.
Do not repair a reference result using managed positions, reproduce its bugs or
alter the frozen inputs. Existing qualified DejaVu/default-variation passes stop.

[Evidence](authored-font-cmap4-correction-evidence.json) records the patch's source
hashes, exact build commands, artifact identities, unchanged fixtures and package.
Its mirrored/member-verified raw archive retains the original/corrected source
and patch, both compiled references, existing probe, all frames/observations/logs
and source-build/probe/Linux controllers. Upstream material remains ignored and
is absent from tracked sources and the production package. Publication remains
postponed; all 289 material paths stay required.

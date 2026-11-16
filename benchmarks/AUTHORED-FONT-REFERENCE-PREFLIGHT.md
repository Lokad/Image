# Authored-font reference preflight

AFP1134 checks the six remaining original font boundaries using the unchanged
actual 0.2.1 package DLL on Windows and Linux. **This is correctness evidence,
not timing evidence or an overall text-processing slowdown claim.** The existing
complete DejaVu one-shot and prepare/eight-draw integrations remain passing and
stopped. No production implementation, reference assembly or acceptance gate changes.

Subsequent [ADV1135 measurement](DEFAULT-VARIATION.md) confirms default variation
as a pass-stop. This preflight retains its earlier correctness-only observations;
the other five controls still need reference-output alignment before timing.

[AFC1140](AUTHORED-FONT-CMAP4-CORRECTION.md) subsequently isolates and corrects the
stock cmap4 forward-array mapping in a labeled source-built reference. Non-tab
packed glyphs now match; the complete comparison still needs tab alignment.

| Original control | Current comparison readiness |
| --- | --- |
| `TextLayout/OriginalUnicode` | Open: reference emits a nonrendered tab placeholder; tab increments and managed tab stops produce different later positions. |
| `TextLayout/PackedBmp` | Open: pinned Fonts 2.1.3 omits the required nonzero cmap4 glyph-array delta. Preserve the fixture and correct managed mapping. |
| `TextLayout/CompositeTransforms` | Open: tab differences plus an additional 0.56 px ink/bearing/point-placement difference, not yet attributed. |
| `TextRender/Rgba8Clipped` | Open: current reference placement fails the unchanged visual gate; mean 75.62 / RMSE 128.92 normalized alpha units. |
| `TextRender/Rgba16Clipped` | Open: same issue; mean 74.93 / RMSE 128.34, with full 16-bit frames retained. |
| `TextLayout/OpenTypeDefaultVariation` | Ready: exact glyphs/clusters/lines; position and ink error below 0.000002 px. Still performance-unmeasured. |

All six managed retained layouts survive font disposal and release shared-budget
charges on their own disposal; the borrowed root remains usable. Both operating
systems reproduce the observations and complete byte/word frames. Four missing
`using` declarations in the fixture generator now release its fonts/layouts;
generated font bytes and geometry remain unchanged.

The public reference adapter aligns positive line-gap placement using the face's
own public metrics and options. Explicit `TabWidth=4` still yields an 8 px advance
on this fixture, while managed layout uses four-space tab stops. These are reference
boundary issues to resolve before timing, without fixture-specific output repair.
No conclusion about the additional compound placement difference is established.

For packed U+0041, an independent table read gives glyph-array value 1 and delta 1:
the specified and managed glyph is 2, but the reference returns 1. The
[OpenType cmap4 specification](https://learn.microsoft.com/en-us/typography/opentype/spec/cmap#format-4-segment-mapping-to-delta-values)
requires that addition. The clean reference checkout matches the package's
[exact source revision](https://github.com/SixLabors/Fonts/blob/eadc8dbc041d1b335ecf790ce400b5e069f075de/src/SixLabors.Fonts/Tables/General/CMap/Format4SubTable.cs).
No patched/updated reference or weakened gate is substituted.

The original string, size 16, wrap 64, spacing 1.25 and three authored fonts remain
fixed. The required two-glyph default-variation program is frozen separately:
contextual substitution plus active GDEF regions with signed corrections summing
to zero at the default coordinate. This adds no path or workload grid. Complete
layout/outline decoding, measurement, positioned output and disposal belong inside
both eventual timers; only fonts are prepared outside. Rendering additionally
includes coverage and drawing into the original 513×259 caller canvas at (-.25,1.5).

Next: short quiet-VM screen of the ready default-variation case, followed by an
independent confirmation if justified. The other five remain unscored until
comparison gates pass. Eager/lazy font validation and different retained reference
state remain disclosed. No stage sums infer end-to-end cost. Independent codec,
copy and resizing work continues; font reference issues do not block the project.

Zero production/VM builds or broad/package reruns. Reuse the unchanged production
qualification: 6,747 full/live checks per OS and the SAME actual package consumer
plus all three examples. [Evidence](authored-font-reference-preflight-evidence.json)
pins package, reference, source, fixture and ownership observations. Mirrored raw
`authored-font-reference-preflight.tar.xz`: SHA-256
`29ff07e91a36b24f02b6e574c068b7335766034a21bd7e16c993d9adabde076b`,
2,609,948 B / 69 verified members; includes reproducible own probe sources,
initial and corrected-option results, and full-precision frames. Cited package
qualification remains in the dependency archive; no upstream sources are vendored.

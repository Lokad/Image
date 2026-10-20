# Default-instance positioning qualification

GPOS value records and format-three anchors can now reference GDEF variation data
whose total correction is proven zero at normalized default coordinates. Valid
regions contribute zero or one at that instance; signed word/byte deltas are
summed exactly. At most65,535 signed16-bit contributions fit a signed32-bit sum.
Indices, row bounds and referenced region coordinates are checked, including
inactive regions. The no-variation sentinel and null outer entries follow their
specified semantics. Offsets use each record's immediate parent, including PairSet.

This follows public [variation-store formats](https://learn.microsoft.com/en-us/typography/opentype/spec/otvarcommonformats)
and [GPOS value records](https://learn.microsoft.com/en-us/typography/opentype/spec/gpos#value-record).
All tracked fixtures are independently authored. There is no added heap storage,
cache, input copy or owner. Existing reader work/cancellation and shared shaping/
retained-layout admission remain in force. Axis selection, synthetic styles,
nonzero default corrections, device corrections and long-word positioning deltas
remain explicitly unsupported; malformed references remain typed data failures.

Windows/Linux each pass4,596 tests, including932 live FFmpeg comparisons, plus105
affected scalar checks. Fifteen new controls cover relative origins, exact sums,
sentinels, malformed/truncated/mutated programs, work/item limits, cancellation,
refusal cleanup and subsequent success. All twelve existing licensed public font
comparisons retain their glyph/cluster/line/fitting and raster gates. The same
actual private0.2.0 package and all three usage examples pass on both platforms,
including contextual substitution and a default correction rendered after font disposal.
One core/internal-test build per OS is reused; public/differential/reference and
benchmark binaries are reused. No timing or general font-compatibility claim follows.

Exactly one required warm representative is added to the finite checklist:
`TextLayout/OpenTypeDefaultVariation`, an authored two-glyph contextual program
with two variation regions and signed word/byte deltas. It exercises new region/
row validation absent from legacy and Arabic layouts. Font preparation is excluded;
complete owned layout, bounds measurement and disposal are included. Equivalent
reference ownership/quality must be established before scoring. It is unmeasured,
with allocations unmeasured. Other timing evidence is unchanged; full parity and
replacement readiness remain open.

[Numeric qualification](../benchmarks/text-default-instance-qualification-evidence.json)
records producers and the same actual package. Mirrored raw archives:

- `c50b-qualified-windows.tar.gz`: `7c44ed9c932c90d24555e28b18850a020a72bfd0336fafd449de35ae4e4d61d2`

- `c50b-qualified-linux.tar.gz`: `0bc6f20aa1ecf2407473c651cba1d5000ea360de8a0df679bbeb2e27c6833228`

Actual package SHA256: `519328b7e41d3671a7570be59780d1e89981e904a26eab94fb5e5319b5912f8b`.

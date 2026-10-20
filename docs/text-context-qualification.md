# OpenType contextual substitution qualification

GSUB type5 format3 now matches coverage sets and applies ordered nested lookups.
It shares the existing chained-context matcher and actions, including mark filtering,
run boundaries, changed sequence indices after ligature deletion, complete input
consumption and empty-action rules. Extension lookup handling preserves the same
behavior. Type5 formats1/2 remain explicitly unsupported.

Implementation follows the public [OpenType sequence-context definition](https://learn.microsoft.com/en-us/typography/opentype/spec/chapter2#sequence-context-format-3-coverage-based-glyph-contexts).
All new tracked fixtures are independently authored programs. No font data or
upstream implementation is added. There is no new heap storage, retained cache or
input copy. Counts/ranges and nested work retain existing limits and cancellation;
font storage and the shaping buffer retain their existing shared-root owners.

Windows/Linux each pass4,581 tests, including932 live FFmpeg comparisons, plus58
affected scalar checks. Truncation/mutation and typed-failure cleanup controls pass.
The existing twelve licensed public font comparisons retain their exact glyph/
cluster/line/fitting decisions and documented position/raster tolerances. The same
actual private0.2.0 package and all three usage examples pass on both platforms,
including an authored contextual layout rendered after its font is disposed.

One core and one internal-test build per OS are reused throughout. Public-test,
differential, benchmark and reference-tool binaries are reused with the verified
new producer; no performance measurement or broad font-compatibility claim is made.
Codec/edit/inspection benchmark paths remain unchanged. This extends supported
lookup syntax; full font, performance and release readiness remain open.

[Numeric qualification](../benchmarks/text-context-qualification-evidence.json)
records producers, builds, counts and the same actual package. Mirrored raw archives:

- `c50a-qualified-windows.tar.gz`: `057f9f7fb25db579933301b642726446547ec3bc9822a7e15e3563f86b4ec2f9`

- `c50a-qualified-linux.tar.gz`: `4e95050e60ef534e5e0c61ddbf1e33abadf1957a84ffcb15dab866c82175ef61`

Actual package SHA256: `ae5647bc47c5bc831f34a92d927ee75472b736617dae521eab0840d0336478a2`.

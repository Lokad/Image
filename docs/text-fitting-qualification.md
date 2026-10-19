# Constrained text fitting qualification

`TextLayout.TryFit` returns the largest fitting candidate on an explicit finite
descending size grid. It preserves ink overhangs: translate the returned bounds
origin into the target area before drawing. The exact minimum and maximum remain
candidates despite floating-point subtraction or division rounding. False/null
means no completed candidate fits; resource failures and cancellation propagate.

One admission/work context owns preparation before allocation and survives a
successful layout. Rejected capacities release before the next trial; no best
candidate overlaps another and work never resets. The caller's root/font/input
remain borrowed. Nested caller results can survive outer no-fit cleanup. No
raster canvas, global cache or intermediate retained layout was introduced.

All **twelve fixed font controls pass on Windows/Linux** with the existing
[comparison gates](text-compatibility.md). The constrained control uses the same
regular face/text and 96-pixel wrap box, a 30-pixel height and a 24–8 px size range
with 0.25 px steps. Both implementations select **16.75 px after 30 candidates**.
The reference uses public Fonts measurement/renderer observations with the same
explicit face-metrics line-box policy. Glyphs, clusters, line breaks and size
decisions agree; position/raster tolerances remain unchanged.

Qualification passed **4,397 tests per OS**: 1,588 internal, 1,881 public API and
928 live FFmpeg comparisons. All **149 text/GSUB/GPOS/direction checks per OS** also
passed with intrinsics disabled. Fourteen new fitting checks cover size endpoints,
floating-point boundaries, wrapping discontinuities, overhang placement, identical
rendering, no fit, aggregate/retained/nested ownership, peak capacity, every growth
refusal, cumulative work, cancellation cleanup and successful subsequent calls.
The same actual private 0.2.0 Release package passed both consumers, including
fitting/shared admission, plus all three usage examples.

This qualifies the named font/text/settings and declared table/script support,
not general font compatibility, replacement readiness or performance parity.
No performance timings were taken. Published 0.1.0 remains unchanged.

Retained SHA-256 identities:

- Qualified source: `fa5fc95ae23209ecd4988de557ac70c4daac73eed0eb9e326b280048e3b1e58d`.
- Windows producer: `b4122bee450486ebeedfbed4c6ab2dd2fc1633fc6b2876b3adc20d674d0ea3eb`.
- Linux producer: `48c51566605302e267e4e185137c1c1c0ed0344253e84861d30d5a9f776234ad`.
- Actual package: `525d0ccb3d5c84acb27f1cb7c588c07cd69b7a7c6b5c063e3ceff9ae2d24c1ae`.
- `p2f-complete-qualified-windows.tar.gz`: `9ec27cf6954b30f0e6101bcd84c98b42bc0f2de93d7394f090d80b69ad20d78a`.
- `p2f-complete-qualified-linux.tar.gz`: `25f822f25c86b64dede30bce89be6931f1cd9e8a5c66d43322a74dfa8bd30df6`.

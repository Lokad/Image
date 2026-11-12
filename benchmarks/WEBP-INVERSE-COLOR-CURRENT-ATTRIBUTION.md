# Current inverse-color attribution

One two-second capture on the accepted [Huffman build](WEBP-HUFFMAN-READ-FRONTEND.md)
finds1,998sample locations: main decoder1,247(includes inlined readers), scalar
transform210, cold canonical reader183, predictor145 and PredictAndAdd96.
Inlining/skid counts are not phase times, removable percentages or gain forecasts.
Instrumented elapsed is discarded. All original pixels/native/settings/source/
owner/cancellation and idle-v2 gates pass. No build, fixture/settings expansion or
qualification repeat; current6,449checks/SAMEpackage evidence is reused.

The scalar inverse color loop recomputes tile indexing and loads/sign-extends the
same coefficient triplet per pixel, with three signed products. Current annotated
instructions retain this work. The next bounded experiment prepares coefficients
once per tile and batches independent pixels in AVX2, preserving ordered signed
arithmetic, modulo256, updated-red-to-blue dependency, scalar tails/fallbacks,
per-row cancellation and existing ownership/admission. No new workspace or API.
Small tiles and code growth may defeat amortization; exact tests and complete
short/independent measurements decide retention. No predicted speedup or new parity.

HFR847 remains the timing owner:3,124.89/FFmpeg3,025.07µs,range1.018–1.043,
152,728B/op. Attribute the demonstrated gap; uncertainties are not presumed gaps.
All289paths remain; publication is postponed.

[Evidence](webp-inverse-color-current-attribution-evidence.json) retains the
hypothesis and source/JIT/sample identities. Raw`webp-inverse-color-current-attribution.tar.xz`, SHA256`df28181378a81d28901a88eab1ca8b4b7b6ba6a61802bdfb1d192fe9ae3ef44e`
(135112B/27members), is mirrored/member-verified; dependencies retain
the actual package, immutable harnesses and frozen reference inputs/settings.

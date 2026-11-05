# Operation-owned WebP predictor workspace

Green-only predictive encoding now creates its already-required matching/token
seed before prediction. Adaptive counts/history and residual rows borrow that
capacity in separate synchronous phases, then matching and parsing reuse it.
Retired tokens carry the same storage through subsequent alpha filters.
Selected maps, coding candidates and encoded payloads keep independent ownership.
No pool, cross-operation cache, production dependency or unchecked access.

Complete transparent encoding allocates **131,488 B less (1.84%)**, from 7,165,064
to 7,033,576 B at equal direct/paired batching. First-stage reuse of only the
retained next-filter seed saved 65,840 B; the completed design covers the first
filter too. Its earlier allocation is selected by transform/physical capacity,
never available budget. Dirty counts/history reset before reuse; every borrower
finishes before the next phase. Admission refusal remains terminal.

Four existing direct controls keep exact packets and admitted peaks: photograph
3,470,059 B, diagram 3,251,328 B, alpha 3,656,309 B and authored bars 312,716 B.
Scratch retention after disposal is zero. Photograph allocation falls 96 B;
diagram/bars are unchanged. These peaks exclude borrowed input/caller output;
physical heaps and native allocations are unmeasured.

Twelve independent counterbalanced pairs ×16 complete byte-alpha operations give
64,277.55→64,324.72 µs, ratio 1.000829,
interval 0.990704–1.011000.
**No CPU gain, regression or equivalence is established.** All pairs, adverse
intermediate points and both stage screens remain. Fresh output construction,
growth, complete encode and disposal stay timed; equal resident sources, JIT
and validation are excluded. Do not infer elapsed savings from fewer allocations.

| Existing 513×259 complete control | Lokad µs | Reference µs | Ratio / upper | Managed B/op | Exact packet B, both | Status |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| ordinary | 106,564.54 | 120,990.53 | 0.881 / 0.891 | 7,476,544 | 183,812 | pass |
| opaque | 70,583.24 | 70,048.90 | 1.008 / 1.028 | 5,983,328 | 121,896 | uncertain |
| photograph | 81,444.99 | 82,603.80 | 0.986 / 0.996 | 6,517,158 | 160,032 | pass |
| diagram | 7,840.69 | 9,655.09 | 0.812 / 0.861 | 4,363,930 | 7,234 | pass |
| rgba16 | 108,219.74 | 120,638.13 | 0.897 / 0.902 | 7,476,544 | 183,812 | pass |
| async | 106,645.98 | 120,749.59 | 0.883 / 0.906 | 7,476,928 | 183,812 | pass |
| alpha | 61,021.42 | 53,495.95 | 1.141 / 1.150 | 7,034,392 | 98,590 | gap |
| alpha16 | 61,227.04 | 53,549.88 | 1.143 / 1.151 | 7,033,984 | 98,590 | gap |

Eight affected current costs refresh once on the immutable selected producer,
twelve actuals and native brackets each. Passing cases stop; uncertain cases
resolve before assuming optimization. 5 of these eight currently pass;
uncertain: opaque. Other 117 displayed comparisons reuse
unchanged paths, including six opaque VP8 rows. Other affected named H costs
remain historical. No full-release refresh or broader content/settings claim.
Lossless references are explicitly labeled libwebp exact=1; transparent uses
FFmpeg/libwebp. Original inputs/settings, complete packets, exact alpha and all
quality/ownership gates remain. Normalization, output and actual awaited async
delivery stay inside their declared complete boundaries.

Both OSes pass **5,668 full/live checks**, plus 876 internal /357 public checks
per normal/scalar mode and the SAME actual Release package/all three examples.
Nine new tests cover dirty scratch, partial physical fit, every residual/map,
surviving map ownership, source aliasing, refusal/cancellation and root recovery.
Existing exact predictor/packet oracles and shared/async consumers remain.
Analysis is a synchronous ref struct; only the independent selected map escapes.
Two immutable main/internal versions per OS; one corrected Windows test-only
compile failure reuses the successful main build. Zero harness/native/selector/
pair-tool builds, one broad/package milestone; unchanged AVX512 proof is reused.

[Earlier chain links](WEBP-CHAIN-LINK-PREPARATION.md) and
[alpha token reuse](WEBP-ALPHA-TOKEN-REUSE.md) retain their scoped gains/history.
[Evidence](webp-prediction-workspace-evidence.json) pins both stages, all actuals,
sources/producers, ownership and qualification. Raw `webp-prediction-workspace-qualified.tar.xz`:
5,003,308 B / 327 members, SHA256 `dc7bb7a294f99787b930c0326d7d02706b7c528f9e278d143e57498f89c7dc54`;
mirrored/member-verified with reproduction and unchanged dependencies retained.
Full 282-path CPU parity stays open; publication is postponed.

# Green predictor runs and shared row storage

The subsequent [alpha cache-history stage](WEBP-ALPHA-CACHE-HISTORY.md) owns the
current eight WebP comparisons. This report retains the predictor milestone,
its scoped incremental gain and superseded standalone costs.

Normalized ALPH prediction now prepares tile-row runs, with three shared exact
scalar/AVX2 kernels for copies, averages and clamped addition. Other predictors
keep the exact scalar fallback. One shared storage-preparation factory reserves
or borrows the same working rows.
The qualified full-color pixel body retains its original execution method;
green has a separate row executor. Short copy/swap schedules remain local as
measured specializations. No generic row interface, array, owner, pool or cache
is added. Selected maps/payloads remain independent.

Twelve independent counterbalanced pairs ×16 complete operations confirm
**1,341.17 µs / 2.11%** on the existing
513×259 transparent RGBA8 Q75/effort4 encode: 63,629.96→
62,288.79 µs, ratio 0.978926, interval
0.974367–0.983495. All pairs remain.
Fresh output construction/growth, complete encoding and disposal stay timed;
equal resident sources, JIT and validation are excluded. No broader incremental
CPU gain is claimed, including the word-input or photographic controls.

Actual immutable-production counting finds two alpha-filter trials, both
prediction/bits5: 264,192 interior mode lookups become 8,772 run selections.
One favors clamp12; the other average8/9 and copies. These counts and old source
samples motivate the design; they are not phase times or savings forecasts.
Only local strategy was inspected in libwebp; no implementation was copied.

The combined first stage has an adverse matched photograph short point. A
generic shared scheduler then has a near-flat private ALC photograph short, but
actual standalone BenchmarkDotNet costs are 87.49 ms versus a fresh original
81.87 ms. That functionally qualified stage is rejected; its scoped alpha gain
cannot substitute for the failed standalone boundary. The selected revision
restores the full-color pixel execution method and shares storage preparation.
Its standard photograph short is normalized-favorable while the native clock
moves. The final photograph job steps between 81.45 and 88.89 ms with unchanged
source; its confidence spans parity and remains uncertain. All twelve actuals
remain. Resolve this uncertainty before assuming another optimization is needed.
All versions, adverse results and corrections remain; no candidate replay.

Complete byte-alpha allocation stays **7,033,576 B/op** at equal direct/paired
batching. Four admitted peaks stay unchanged: photo 3,470,059 B, diagram 3,251,328 B,
alpha 3,656,309 B and authored bars 312,716 B. Scratch retention after disposal is zero.
These exclude borrowed input/caller output; physical peaks/native allocation
are unmeasured. [Earlier workspace reuse](WEBP-PREDICTION-WORKSPACE.md) retains its
131,488 B/op saving and superseded current costs.

| Existing 513×259 complete control | Lokad µs | Reference µs | Ratio / upper | Managed B/op | Exact packet B, both | Status |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| ordinary | 114,479.23 | 130,367.90 | 0.878 / 0.891 | 7,476,544 | 183,812 | pass |
| opaque | 70,720.26 | 70,022.85 | 1.010 / 1.023 | 5,983,328 | 121,896 | uncertain |
| photograph | 84,049.81 | 81,783.68 | 1.028 / 1.081 | 6,517,158 | 160,032 | uncertain |
| diagram | 7,808.12 | 9,791.77 | 0.797 / 0.808 | 4,363,930 | 7,234 | pass |
| rgba16 | 113,812.00 | 126,761.49 | 0.898 / 0.905 | 7,476,544 | 183,812 | pass |
| async | 113,549.72 | 127,120.66 | 0.893 / 0.909 | 7,476,928 | 183,812 | pass |
| alpha | 59,835.53 | 53,430.27 | 1.120 / 1.129 | 7,034,392 | 98,590 | gap |
| alpha16 | 59,910.73 | 53,505.69 | 1.120 / 1.127 | 7,033,984 | 98,590 | gap |

Eight affected costs refresh once on the immutable selected producer, twelve
actuals/native brackets each. 4/6 lossless controls currently pass and stop.
Other 117 displayed costs reuse unchanged paths, including six opaque VP8 rows;
other affected named H boundaries remain historical. Refreshed costs establish
current state, not additional gains. Lossless references are explicitly labeled
libwebp exact=1; transparent uses FFmpeg/libwebp. All original inputs/settings,
complete packets, exact alpha and quality/ownership gates remain. Normalization
and actual awaited async output stay inside their declared complete boundaries.

Both OSes pass **5,685 full/live checks**, 893 internal / 357 public per normal/scalar
mode, and the SAME actual Release package/all three examples. Seventeen new
controls cover every predictor, 65,536 neighbor controls, mixed tiles, borders,
odd-average rounding, vector tails and live-token fallback. Mode 0/origin alpha
residual 1, signed mode 13 division and last-column top-right remain exact.
Cancelable tokens retain per-pixel scalar checkpoints; vector blocks require an
uncancelable token. Shared admission and asynchronous scratch lifetimes persist.

Three immutable main/internal versions per OS, one private actual-module count tool,
zero pair/harness/native/selector builds. Two broad/package milestones are retained:
the generic-row stage passes correctness/package proof but fails photograph
performance; the selected changed source is qualified again. Unchanged paths reuse their evidence.
Unchanged AVX512 input/quantizer proof is reused. [Evidence](webp-predictor-runs-evidence.json)
pins source/producer/output identities, all actuals, all three stages and qualification.
Mirrored/member-verified raw webp-predictor-runs-qualified.tar.xz: 7,535,216 B / 651 members,
SHA256 `6bcc0827a453e4e1024307c37d9391f102e57f8aa0cbe9ae40f6b91cb444cdb0`. Reproduction and unchanged dependencies remain.
Full 282-path parity stays open; publication is postponed.

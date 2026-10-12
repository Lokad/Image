# Warm progressive grayscale JPEG: accounting trial rejected

The newly covered six-scan grayscale input screens at 2.39x
FFmpeg, conservative upper 2.62157x. Both baseline quiet
guards pass. Progressive decode is the largest measured remaining gap.
One local-accounting trial changes screened managed time by only
2.65%, with wide uncertainty and a failed
post-run load guard. It remains above 2x and is discarded. Production and its
actual package are restored exactly; the benchmark and fixture tooling remain.
Uniform warm parity and independent certification remain open.

| Implementation/run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Accepted progressive decode baseline | 4157.28 | 2.39298x | 2.62157x | Pass |
| Rejected local-accounting screen | 4047.09 | 2.33306x | 3.18792x | FAIL |

The input is a lossless progressive rewrite of the existing original-raster
owned grayscale JPEG, fixed quality 85/RGB 0x204060 alpha background, 513x259.
The default progressive rewrite has six scans and 75,112 bytes. No image,
shape, quality or scan-strategy enumeration is added. The original public-API
coefficient visitor proves all 137,280 signed DCT values, quantization and
component geometry byte-identical in 274,728-byte canonical dumps. Owned decoded
pixels retain the exact baseline hash, including on Windows in normal/scalar
modes. SOF2 and one component are verified. Both subjects decode identical
resident bytes; native output matches the pinned normal CLI exactly. Normal/
scalar agreement remains maximum 1/RMSE 0.119141 under the fixed 4/1.5 bounds,
with exactly opaque alpha, unchanged source and independent held outputs.
Fifteen negative native input/protocol/affinity controls pass.

Pinned libjpeg-turbo 3.1.4.1 at revision
9217719d3a58633923b096af4c1d50d304768a64 supplies jpegtran solely for untimed
fixture preparation. Its checkout, build, binaries and static library remain
ignored under external/ and never enter production or tracked files. The
tracked build script and coefficient visitor are independently authored adapters
for public APIs; no upstream implementation, tables, docs or corpus is copied.
The static Release fixture producer disables SIMD and builds one job at a time;
its speed is outside the workload. Producer/source/binary hashes are retained.

The rejected primitive wraps ReadBlock in the already-existing exact local
entropy work scope, publishing on every exit. Cancellable readers retain the
original per-call checks. Only that body changes (707 to 738 IL bytes), with
all 558 methods and flags retained and other bodies equivalent after metadata
resolution. Ten frozen public controls retain exact prior work minima and pixel
hashes for positive/negative refinement, sparse bands, EOB runs and restart
streams, including cancellable tokens, one-less-budget refusal and cancellation.
All 96 focused public JPEG checks pass per intrinsic mode on both OSes. The
candidate passes 2,316 Windows Release tests and the actual package examples.
After rejection, both OSes pass all 2,306 restored Release tests and restored
production methods/flags match the previously accepted actual package.

Source inspection finds FFmpeg bounding refinement tails with the last nonzero
coefficient. This fixture is dense (mean 52.12 nonzero AC coefficients/block),
limiting that sparse-tail opportunity. Inspection supplies insight only. The next
substantial hypothesis batches correction bits while preserving exact logical
work, scalar cancellation checks and marker/limit error precedence. The small
uncertain accounting result receives no confirmation run or further tweaks.

Sequential Release screens use CPU0, one codec/conversion thread, thread type
zero, normal SIMD, tiering off and non-concurrent workstation GC. Native pre/post
batches target 50 ms; three actual managed iterations target 200 ms. Generated
children validate outside scored windows with one native operation per bracket
and null ratios. Unchanged five-second guards precede/follow each statistical
run; the failed load guard remains failed. Startup, IPC, I/O, generation,
coefficient dumps, rewriting, hashing and validation are excluded.

The [numeric evidence](warm-jpegprogressive-accounting-rejection-evidence.json)
retains all managed, GC/threading and native measurements, guards, provenance,
package/source/actual-child identities and immutable archives. Conservative
bounds divide managed confidence upper by the smaller native confidence lower
using the same critical value. Regenerate with
`python benchmarks/render-warm-jpegprogressive-accounting-rejection.py`; add
`--check` to verify. See the [reference recipe](../tools/WarmReference/README.md).

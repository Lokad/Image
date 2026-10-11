# Warm RGBA16 nearest resize baseline

The first complete `Nearest/Down16` screen reaches 2.11693x FFmpeg. Its
conservative upper is 4.05823x, so the short run does not establish parity.
Independent quiet confirmation remains open: the pre-run guard passes and the
post-run guard fails under unchanged thresholds. No production code changes.

| Managed mean, microseconds | Native before / after, microseconds | Worst bracket ratio | Conservative upper |
| ---: | ---: | ---: | ---: |
| 360.82 | 170.44 / 172.90 | 2.11693x | 4.05823x |

The [numeric evidence](warm-nearest16-baseline-evidence.json) retains the three
actual managed samples, native samples before and after, statistical intervals,
quiet guards, source and binary identities, and immutable raw-archive digest.
The ratio upper divides the managed confidence upper by the smaller native
confidence lower, using the managed interval's critical value conservatively
for both three-sample native brackets. This screen ranks further work only.

One existing original 513x259 RGBA16 raster becomes independently owned 256x129
RGBA16 output. Every full-word sample, including alpha and hidden RGB, matches
the independent integer center reference and pinned FFmpeg CLI. Native formats
remain RGBA64LE, GBRP16LE and GBRAP16LE. There is no conversion to eight bits,
extra performance raster, quality search or matrix sweep. Down8 remains the
default; Down16 must be selected explicitly and runs alone.

Managed timing includes the complete public Resize call, admission, allocation,
materialization and disposal. The original development-only native worker uses
public FFmpeg filter APIs, including packing and plane extraction, separate RGB
and replicated-alpha zscale point kernels, merging, and independent output
acquisition/release. Graph, source, frame wrappers and geometry contexts are
reused; every call processes every selected sample. Padded rows are exposed
directly. Startup, IPC, I/O, generation, hashing and validation are excluded.
Both subjects use CPU0, one filter thread, thread type zero and normal SIMD;
managed tiering is disabled with non-concurrent workstation GC.

Both depths pass fourteen native negative controls, exact selected samples,
source integrity and simultaneously live independent owners. The Down16 BDN
smoke passes, and 116 focused transform/reference tests pass on each OS. All 543
production method bodies and implementation flags in the saved statistical
child match the retained actual Release package. The unchanged production
already passed all 2,129 tests on each OS; a benchmark-only change does not
require repacking or repeating package examples.

The next substantial candidate is a specialized nearest row kernel with exact
integer coordinate recurrence. The current generic mapping callback recomputes
horizontal and vertical coordinate divisions and acquires pixel views for each
output pixel. Reusing row state can remove this repeated work without changing
sample selection, precision, hidden RGB, ownership, admission or cancellation.
This is a hypothesis, with no claimed gain before measurement. RGBA16 bilinear
alpha matching and the other uncovered operations remain separate open cases.

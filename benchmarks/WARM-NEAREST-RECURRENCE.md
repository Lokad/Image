# Warm nearest resize: exact row recurrence

Complete RGBA16 nearest resize improves 70.78% by screened mean
on one original 513x259-to-256x129 pair. The longer repeat reaches
0.62x the FFmpeg reference, with conservative upper 0.64x.
Independent quiet confirmation and uniform warm parity remain open.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: |
| Generic mapping baseline | 360.82 | 2.11693x | 4.05823x |
| Row recurrence short screen | 105.07 | 0.61433x | 0.85325x |
| Row recurrence longer repeat | 105.43 | 0.61952x | 0.63772x |

The specialized nearest kernel replaces the generic per-pixel mapping callback.
Each row acquires its input and destination pixel views once and computes the
vertical integer center once. Horizontal selection maintains quotient/remainder
of `(2*x+1)*sourceWidth/(2*targetWidth)`: each step adds `2*sourceWidth`, split into
a whole-cell increment and remainder increment. Both remainder terms are below
the denominator, so each step needs at most one carry and no division. Long
intermediates retain exact selection for admitted dimensions. Whole uint/ulong
pixels preserve the original RGBA bytes, full UInt16 precision and hidden RGB.

The operation keeps ordinary zeroed output allocation, independent ownership,
source orientation, same-size cloning and the exact two-buffer memory budget.
It adds no heap scratch or output cache. Logical work and cancellation checks
remain bounded to 1,024 pixels with partial final chunks. Failure disposes the
private result. The same kernel serves both depths; only Down16 is timed here.
No performance image, pattern, size or quality sweep is added.

Eight new correctness cases use independent closed-form integer references
across row/chunk boundaries, up/down and single-axis scaling at both depths.
They check source integrity, two simultaneously live owners, mutation isolation
and exact memory/work limits. 143 baseline focused tests per OS and 151 candidate
tests per OS with intrinsics enabled/disabled pass. All 2,137 Release tests per
OS and all three usage examples through the same dependency-free Release package
pass. The saved statistical children and both fresh builds match its production
methods and flags. Among 543 production methods, only the Resize dispatch body
changes; its two compiler-generated nearest closure methods are replaced by the
two row-kernel methods. Other existing bodies and implementation flags are
equivalent with metadata references resolved. Untimed fixed-count allocation
counters are retained as diagnostics; they vary by batch and do not claim a
stable per-call byte count. Exact resource admission has separate tests.

Both subjects reproduce every full-word sample and the pinned CLI output on
the existing original raster. Native formats remain RGBA64LE/GBRP16LE/GBRAP16LE;
fourteen native negative controls per depth and the Down16 BDN smoke pass.
Managed timing includes complete public Resize, admission, allocation,
materialization and disposal. The original public-FFmpeg-API worker includes
packing/plane extraction, separate RGB and replicated-alpha zscale point
kernels, merging, and independent output acquisition/release. Its graph, source,
frame wrappers and geometry contexts are reused; every call processes every
selected sample. Padded rows are exposed directly. Startup, IPC, I/O, generation,
hashing and validation are excluded. Both use CPU0, one filter thread, thread
type zero and normal SIMD; managed tiering is off with non-concurrent workstation
GC. Native samples before and after bracket managed samples.

One short screen and one twelve-sample longer repeat run sequentially in Release
on the dedicated VM. The unchanged pre-run quiet guards pass; post-run guards
fail and remain preserved. These observations clear the numerical 2x target for
this pair, without quiet certification or claims about other directions and
filtered kernels. No further micro-optimization of this passing pair is planned.

The [numeric evidence](warm-nearest-recurrence-evidence.json) retains actual/raw
samples, statistical intervals, native brackets, correctness and source/binary
identities, along with immutable archive digests. The conservative ratio divides
the managed confidence upper by the smaller native confidence lower, using the
managed critical value for each native bracket. Regenerate with
`python benchmarks/render-warm-nearest-recurrence.py`; use `--check` to verify.

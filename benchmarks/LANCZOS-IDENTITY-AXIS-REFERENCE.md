# One-axis Lanczos complete-operation comparisons

The canonical identity-axis preparation now has two explicitly bounded external
comparisons on the existing 513×259 RGBA16 input. These are additional views of the
existing public one-shot boundary, not new APIs or a shape/depth/content grid.
The eight [existing Lanczos comparisons](LANCZOS-IDENTITY-AXIS.md) remain passing.

| Complete one-shot operation / zimg3.0.6 FFmpeg zscale backend | Managed / reference µs | Ratio / upper | Managed GC B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Horizontal →256×259 | 608.600 / 2,105.193 | 0.289 / 0.294 | 44,712.00–44,719.75 | pass/stop |
| Vertical →513×129 | 735.958 / 2,061.402 | 0.357 / 0.362 | 254,671.50–254,679.25 | pass/stop |

Both clocks include necessary fresh geometry/work preparation, normalization,
independently owned output acquisition and disposal. Inputs are resident borrowed
packed full-word samples; file I/O, verification and IPC are outside clocks.
The reference rebuilds its graph, planar input and scratch for every operation;
it retains none of that prepared state between operations. Managed calls use the
unchanged default idle storage policy, up to two arrays/4 MiB. Native allocator
caching and physical/JIT peaks are unmeasured. This is not a prepared-use comparison.

Three-pair screens are followed by independent 12-pair confirmations, 32 operations
per managed actual with equal 32-operation native brackets before and after.
All actuals, raw wire lines, 64 warmups, idle-v2 guards and natural nonconcurrent GC
are retained. One CPU0 operation runs at a time. The upper bound uses the existing
99.9% paired-log convention. This measurement-only round claims no production
speedup from subtracting historical clocks. Both controls pass and stop.

Full frames preserve the prior qualified managed hashes. The existing independent
decimal/BigInteger one-axis tests, coefficient proof and exact-pixel contract remain.
Normal/scalar native outputs have maximum RGB/alpha error 1 and RMSE below 0.053
RGB/0.044 alpha; zero-alpha RGB stays exact. Native float tolerances do not relax
managed Q30/rational results. Both sides verify independent surviving outputs and
unchanged inputs before timing. Native output/source checks and managed source
checks repeat after timing.

Separate caller-root probes retain 46,656/257,100 B of explicit horizontal/vertical
plans and report one-shot admitted peaks 577,088/786,516 B. Their outputs own
530,432/529,416 B. These probes bypass idle storage; retained plans are not retained
by timed one-shot operations. Native scratch 223,808/149,824 B is only one component,
not total allocation or peak memory. GC allocation ranges in the table are kept
without asserting their cause or treating lower allocation as a CPU gain.

No production, managed harness, fixture or package is rebuilt. One original
benchmark adapter build changes only the accepted-size argument guard of the
prepared one-shot adapter; filtering, packing, normalization and timing reconstruct
exactly to its retained source. zimg links statically from the same pinned archive.
The existing ImageProbe and immutable managed helper generate/check frames.
Each OS's 6,742 full/live checks, 163 internal/15 public normal/scalar/AVX512-disabled,
three named resize-storage suites and same actual package consumer/all three
examples remain valid and are reused. Package/source/core identities are checked
on both OSs; no broad repeat.

[Evidence](lanczos-identity-axis-reference-evidence.json) pins the scope, boundaries,
all actuals, quality, ownership, memory, adapter build and reused qualification.
Raw `lanczos-identity-axis-reference.tar.xz` (1,626,904 B/60 members), SHA256
`ad1563d71a068c70a33b9ae02de23571a6b6d083b58ea4233b75e84af4a3a6a6`,
is mirrored/member-verified. It retains original adapter/base source, build command,
worker, reused ImageProbe, scalar/output checks and all captures. Its
`lanczos-identity-axis-qualified.tar.xz` dependency retains the immutable package,
managed helper/source, input and qualification. Reproduce with the archived
`tmp/lax1116-{prepare,measure,record}.py` recipes in a fresh ignored experiment
workspace with the pinned producer/reference dependencies; do not overwrite a
completed capture. PLAN remains unchanged outside its authorized font/text section.
The full 289-path objective remains open and publication postponed.

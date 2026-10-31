# Exact separable alpha convolution

The bounded candidate is **rejected** after one quiet-VM short pair. Complete
TightUp8 bicubic resizing measures **4,483.082 → 4,573.316 µs**, with unchanged
**2,132,668 B/op**. Filtered/all means are about 2.01% slower; native-normalized
point improvement is only 0.85%. The short confidence intervals are wide: this
misses the useful-gain screen, rather than proving an independent 2% regression.
There is no independent confirmation, retry or tuning variant.

The candidate factors exact integer alpha convolution into separable axis sums,
reusing coefficients in a 256-byte stack plan within the same 4 KiB admission.
Signed terms, normalization, half-away rounding, clamps and larger-support
fallbacks remain exact. A bounded proof covers arithmetic range, 952 independent
integer comparisons and 832 comparisons against the actual C# alpha routine on
each OS. These prototype comparisons concern alpha, not complete rendered frames.

The production trial preserves frozen complete-frame pixels, native comparisons,
source and output ownership. Twenty-eight internal and 658 public checks pass
per normal/scalar mode on both OSes, also Linux without AVX512. They cover exact
halves, negative lobes, support/width guards, terminal refusal, cancellation
cleanup, root reuse and zero arithmetic heap allocation. One core/internal build
per OS; existing workers, harness, selectors and image inputs are reused.

The qualified source, both core/internal assemblies, package and source manifests
are restored exactly without rebuilding. Candidate-only code/tests are archived
and removed. Existing 5,289 checks per OS and the same actual Release-package
consumer evidence apply. The current independent TightUp8 cost remains
**4,472.999 µs, 1.473× / upper 1.481**, **2,132,668 B/op**. The accepted
approximately 4.1% color-histogram improvement remains in production.

[Numeric evidence](exact-alpha-separable-rejected-evidence.json) preserves all
actuals, brackets, allocations, proofs, producer IDs and restoration records.
Raw exact-alpha-separable-rejected.tar.xz, SHA256
c1bd7850bc86ebefeb39a19a94556e74fbcd2f19f1169ce486e4561b2badbd9e,
is mirrored with all 101 members verified. It includes candidate/original code,
tests, builds, probes, reused workers/selectors, scoring receipts and guards;
prior qualified archives are pinned dependencies. No input, reference, pixel or
resource tolerance changes. All 268 material paths remain required.

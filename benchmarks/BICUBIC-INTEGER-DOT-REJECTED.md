# Bicubic integer horizontal convolution

One bounded Q30 integer-dot candidate is **rejected**. The existing Wide8
513×259 RGBA8 → 128×64 control slows from **531.867 to 858.410 µs** in the
quiet-VM short screen: **61.40% slower**, with unchanged exact pixels and
owned workspace. All-actual and native-normalized views agree. Allocation is
231,231 versus 231,239 B/op; the 8-byte difference is measurement noise.
There is no independent confirmation or tuning variant after this failure.

The candidate splits Q30 coefficients into signed 16-bit high/low halves,
uses integer pair dots and widens before accumulation, correcting the unique
positive 32-bit overflow. Affine sample bias restores exact premultiplied
values. The coefficient plan fits the existing decoded-row region; original
vertical arithmetic, publication margins and original-order repair remain.
This is a distinct arithmetic/data-format experiment; it is now closed.

The actual binary64-coefficient prototype matches the frozen pixels on both
OSes, including Linux SIMD. Production checks pass: six internal and 217 public
controls per normal/scalar mode, also Linux without AVX512. They cover coefficient
and output canaries, signed endpoint overflow, odd rows/outputs, zero arithmetic
allocation, unchanged 198,432-byte workspace, terminal admission refusal,
cancellation cleanup and root reuse. One core/internal-test build per OS;
existing harness, selectors, reference worker and inputs are reused.

The exact qualified source, both OS builds, internal assemblies, package and
source manifests are restored without rebuilding. Candidate-only tests are
removed with their implementation. Existing full 5,285 checks per OS and the
same actual Release-package consumer evidence remain applicable. The current
independent Wide8 cost remains **529.180 µs**, **1.518× / upper 1.534**,
**231,239 B/op**; the fresh original is stable within 1% of that evidence.
The accepted 4.1% color-histogram gain and 5.61% local bicubic repair remain.

[Numeric evidence](bicubic-integer-dot-rejected-evidence.json) retains all short
actuals, brackets, confidence views, allocations, producer identities, checks
and exact-restoration records. Raw bicubic-integer-dot-rejected.tar.xz,
SHA256 d4c348c6d7882f8da70545635d1325c53d2ed43064bd23a8aa4c965a34ce023a,
is mirrored with all 115 members verified. It includes the candidate, original,
proofs, builds, tests, cached drivers/workers, profile, scores and guards; prior
qualified archives remain dependencies. No new tolerance, input or reference
setting. All 268 material required paths remain in scope.

# Byte bicubic reciprocal screen: rejected

One bounded normalization candidate preserves exact pixels but regresses the
complete tight-memory byte reduction from **653.668 µs to 8,586.159 µs** on the
quiet VM short screen. It is rejected immediately, with no independent candidate
timing or further variant. The verified implementation and package are restored.

The existing control remains 513×259 RGBA8 → 256×129, with input, owned output
and 4 KiB scratch included in its 667,660-byte local limit. Native boundaries,
frozen inputs/settings and approved pixel bounds are unchanged. Both subjects
include complete operations; startup and validation are excluded.

The candidate replaces two wide divisions with a hardware reciprocal and one
fused Newton correction, then uses a fixed certified byte rounding margin.
An exact-rational error proof passes; uncertain half decisions retain original
replay. This differs from the earlier rejected word candidate's two nonfused
corrections and dynamic word margins. The fixed certificate can reject a pair
and send the complete operation through direct replay. The measured complete
cost makes this approach unsuitable; no component timing claim is made.

A single cached-driver two-second diagnostic on the accepted build identifies
the actual column kernel and its two divisions per pair. Sample fractions are
diagnostic attribution. A descriptive cache-label mismatch is recovered from
the original completed sample without resampling.

Fifteen focused arithmetic/half/clipping/canary controls and 369 existing public
checks pass in each available normal/scalar mode on both OSes, plus Linux with
AVX512 disabled. Frozen exact managed pixels, native frame hashes, source and
independent output ownership and quiet-v2 gates pass in both short jobs.
One core/internal-test build per OS; no public, harness, selector or native build.
Candidate-specific tests leave with the rejected implementation. After exact
source/core/test-assembly restoration, 369 existing public checks pass on each
OS. The unchanged actual package and full 5,267-check/OS qualification remain valid.

The accepted independent TightDown8 cost remains **1.584× / upper 1.591**, with
**132,306 B/op**. This family is closed. The separately accepted approximately
4.1% gain remains retained with its recorded confidence caveat.

[Numeric evidence](byte-bicubic-one-newton-rejected-evidence.json) records all
short actuals, bounds, allocations, proof, restoration hashes and qualification
reuse. Candidate source/builds, original library, focused checks, scores, guards,
JIT/map/sample and failed-postcheck recovery are retained in
byte-bicubic-one-newton-rejected.tar.xz, SHA256
5caecec35bf146cd1e2e2601baed5c19f5f500d8b84ac370a3f009855adb7a56,
mirrored with all 102 members verified. Reproduce the existing named case with
the [reference tooling](../tools/WarmReference/README.md).

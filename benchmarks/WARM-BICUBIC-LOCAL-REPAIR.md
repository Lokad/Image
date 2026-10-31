# Local bicubic rounding repair

The existing tight-memory byte reduction is **5.61% faster independently**,
with exact pixels and unchanged owned storage. All-actual elapsed improves
**5.12%**; the native-normalized point improves **6.93%**. The complete operation
now costs **622.835 µs**, **1.474× / upper 1.540**, **132,309 allocated B/op**
(+3 B/op measurement noise). It remains a gap against the ≤1× objective.

Uncertain SIMD pairs now replay only their two pixels in original y/x tap order,
using the already admitted weights and offsets. A cold exact-rational calculation
reproduces the whole frozen input/output and proves at least two near-half
ambiguities: alpha at (6,4) and blue at (142,94). The earlier rejected numerical
candidate abandoned the entire frame at such a refusal. Local repair changes
that dataflow; its reciprocal, one fused correction, range guards and 2^-16
certificate are reused unchanged. No threshold/width/FMA tuning or new profile.

The same 513×259 RGBA8 → 256×129 control retains its 667,660-byte local allowance:
source 531,468 + owned result 132,096 + 4 KiB scratch. No new buffer, cache, owner,
kernel file or public API. Tiny positive alpha still unwinds the strip before
using the existing exact-alpha scratch. Admission refusal remains terminal;
borrowed roots, cancellation, cleanup and independent output lifetimes remain.

One short pair gives 4.50% raw/5.29% normalized improvement. The default 5% short
decision is retained as false; explicit acceptance of a 4.1% gain authorizes one
independent confirmation. That confirmation clears the normal 5% gate on every
view. Raw and normalized confidence envelopes separately separate: candidate
upper 1.540 is below original lower 1.564. The existing original twelve is reused
after fresh-short stability of −1.52% filtered/−1.57% all/native +0.72%.
All actuals and native brackets remain; no score is repeated.

**5,285 checks per OS** pass (2,152 internal + 2,185 public + 948 live), plus
18 focused arithmetic/clipping/canary/local-repair/tiny-unwind/refusal/cancel/root
controls and 369 public controls normal/scalar and Linux with AVX512 disabled.
The **same actual Windows Release package/DLL** passes both OS consumers and
three examples. Packaged tight byte enlargement and reduction verify exact
frozen pixels, bounded scratch and independent surviving ownership. One core
and internal-test build per OS; zero public/differential/harness/selector/native
builds. Published releases remain immutable.

Only the named TightDown8 cost changes. Initial eligibility guards and dispatch
are unchanged; default reduction selects the earlier cached kernel, and other
named depth/axis/support cases reject this arithmetic. Other kernels, codecs,
inspection, text and drawing retain applicable evidence. This primitive is closed
after acceptance; the full 268-path objective remains open.

[Numeric evidence](warm-bicubic-local-repair-evidence.json) pins every actual,
confidence view, proof, producer and actual package. Raw
bicubic-local-rounding-repair-qualified.tar.xz, SHA256
ad1a1a900bdd9f02701a3c915d48ac7a8827f9bfa0df6d50ea4b93269ae1e008,
is mirrored with all 117 members verified, including actual builds/package,
tests, source, selectors, workers, scores and guards. Prior proof/source archives
are retained as dependencies. Reproduce the named existing control using the
[reference tooling](../tools/WarmReference/README.md).

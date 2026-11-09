# Exact byte-vector compositing

BCS646 replaces three scalar UInt64 color divisions per nontransparent pixel
with four-pixel AVX512 integer weights and correctly rounded float division.
On the frozen complete RGBA8 Composite control, **558.02 → 65.63 µs** saves
**492.39 µs / 88.24% raw / 88.22% reference-normalized**. FFmpeg measures
451.36 µs; ratio **0.145**, conservative upper **0.157**, passes this control.
The short screen was 68.79/451.63 µs. The independent confirmation retains all
twelve actuals (ten filtered); raw and normalized confidence separates from the
[current baseline](RGBA8-COMPOSITE-CURRENT-PACKAGE.md) in both views. No favorable
repeat was needed. This establishes no broader all-input or all-path CPU claim.

Both subjects perform the complete in-place operation on the frozen 513×259
source at (17,19). Source/canvas setup and reset before every operation are
excluded equally; invocation/unroll is one. Admission, cancellation checks and
context cleanup remain timed. The caller retains source and canvas ownership.
The exact rational managed frame, hidden RGB, zero-source-alpha identity and
untouched pixels remain unchanged. FFmpeg's approved native-only RGB error ≤1,
RMSE ≤0.5 and exact alpha/untouched/zero-source-alpha requirements pass.

For byte source/destination alpha As/Ad, coverage
d=255As+Ad(255−As)≤65025. A rounded color numerator is at most
255d+floor(d/2)≤16613887<2²⁴, so integer-to-float conversion is exact. A quotient
≤255.5 has float ULP at most 2⁻¹⁶, less than 1/65025, the smallest nonzero
distance to an integer. Correctly rounded division followed by truncation
therefore gives the original integer quotient. Exact integer quotients are
representable. Alpha uses exact rounded division by 255; transparent source
lanes select the original complete destination pixel. This proof preserves the
managed pixel policy; it does not introduce a visual tolerance or approximate
reciprocal. A benign denominator handles discarded zero-coverage lanes.

Clipping, admission and traversal direction remain shared. Each vector loads both
pixel groups before writing, preserving directional self-overlap. Cancellation
still polls every ≤256 pixels. The generic mixed-depth, sixteen-bit and
unsupported-hardware kernels remain unchanged; a 0–3 pixel suffix uses integer
arithmetic. There is no new public API, cache, pool, frame or retained state.

Allocation-only checks on the actual timed DLL remain **56 B/op**, three sets of
sixteen complete calls using the prepared diagnostic. Mutable-canvas timing
disables extra statistics passes. New owned workspace and shared-admission
capacity are both zero; retained operation scratch is zero. Source and canvas
each contain 531,468 logical borrowed bytes. Total allocation is not physical
peak capacity; native/physical peaks are unmeasured.

Two focused tests cover 49,939,200 nearest-integer lane comparisons across every
valid denominator and byte quotient, plus every alpha pair with four channel
patterns. Existing independent BigInteger public tests cover every depth pair,
clipped/wide/tail layouts, forward/backward self-overlap, limits, cancellation
and surviving owners. Windows/Linux each pass **5975 full/live checks**
(2710 internal + 2317 public + 948 differential), focused normal/scalar checks
and disabled-AVX512/AVX2 checks. Windows uses the portable path; Linux exercises
AVX512. The actual package consumer now covers a vector plus a scalar suffix.

One successful main/embedded build per OS is reused throughout. Timed Windows
DLL70bb690a48458d055f221151d3fcc9cc34ac795f65c969e217aa0643b885136e
is embedded in actual private Release 0.2.1 package
3507899d05a62f3bcb303a6baf4748cb605860f0187c1aede46a94d266e4fe77
(435,939 bytes). That same package passes consumers and all three usage examples
on Windows/Linux. Published releases remain immutable; publication is postponed.

[Structured evidence](byte-composite-exact-vector-evidence.json) pins current
costs, source/packet identities, both confidence views, allocation and package
qualification. Its mirrored, member-verified raw archive retains scripts,
immutable runtimes, actual package, guards and all observations; pinned baseline
and allocation-diagnostic archives complete reproducibility. Unchanged codec,
text, resizing and ownership paths reuse valid evidence. This passing control
stops; the full 283-path objective remains open.

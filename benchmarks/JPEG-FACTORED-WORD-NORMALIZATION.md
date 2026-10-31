# Exact factored JPEG word normalization

The existing governed RGBA16 JPEG420 control independently improves **4.56%**
in filtered elapsed, **4.72%** across all samples and **4.38%** native-normalized.
Raw, all-sample and normalized confidence intervals separate. This retains a
meaningful gain under the accepted 4.1% threshold; parity remains open.

| Existing complete operation | Managed µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| Governed original, independent | 2,170.186 | 1.179 / 1.197 | 819,489 |
| Governed candidate, short | 2,082.175 | 1.132 / 1.626 | 819,489 |
| Governed accepted, independent | 2,071.314 | 1.127 / 1.135 | 819,489 |
| Nongoverned, current independent | 1,907.949 | 1.036 / 1.049 | 388,215 |

Original 513×259 RGBA16 input, Q85/native qscale 5, 4:2:0 and background
0x204060 remain frozen. Both subjects normalize and flatten once, convert,
encode, acquire independent complete output and release it per operation.
Packets remain **52,115 / 52,117 bytes**, with identical coded content and
reconstructed pixels apart from approved bounded COM attribution. No quality
search or numeric-quality equivalence is involved. The governed operation keeps
the borrowed 32 MiB root, 1,062,936 B resident-source charge outside timing and
**1,704,856 B peak**. Temporary admission/refunds remain timed. Native allocation
is unmeasured; its API has no shared admission equivalent. The nongoverned row
is a current cost, without a causal comparison or budget-overhead attribution.

Cancelling 255 leaves `N = sample*alpha + background*257*(65535-alpha)` and
denominator `65535*257`. Convex `N ≤ 65535²` fits uint. Exact binary64 widening
precedes half addition, which could overflow uint. The upward-rounded reciprocal
cannot undershoot an integer; error <2⁻⁴⁴ is far below the minimum noninteger
distance 1/16,842,495. Four-pixel safe intrinsics retain original AVX2/scalar
fallbacks, aliases, tails and failure write prefixes. No capacity, owner or charge
is added. One new production-boundary test covers every alpha near byte-rounding
transitions; existing full-sample, overlap, allocation and frozen-packet tests pass.

Both OSes pass **5,331 checks** each: 2,169 internal, 2,214 public and 948 live.
Thirteen internal and 354 public checks pass normal/scalar on each OS and with
AVX512 disabled on Linux. The same actual Release package and three examples
pass both OSes, including frozen RGBA16 420/422/grayscale packets and shared
ownership. Package SHA256 `228818d7e0026933b6cc800ee58dcece0603e945f7f46bcb02397a63184203f9`; published packages remain unchanged.

One core/internal build per OS, zero public/differential/harness/native/selector
builds. Reused original independent evidence, workers, selectors and fixtures;
one candidate short, one independent confirmation and one companion confirmation.
Both quiet-v2 guards pass for every job on CPU0, optimized warm JIT and
nonconcurrent workstation GC. Prelaunch syntax/manifest failures precede scoring
and remain in raw evidence. No profile, new input, settings grid or timing rerun.

Only 16-bit subsampled/grayscale preparation executes this changed primitive;
RGBA8, caller-byte samples, RGBA16 444, decoders and other image operations retain
unchanged-path proof. Existing `Encode422_16`, `Encode420Stream16` and
`Encode420Cancelable16` still need current costs before CPU claims. No broader
grayscale or content claim follows. Close width/reciprocal/inlining variants.

[Numeric evidence](jpeg-factored-word-normalization-evidence.json) retains actuals,
confidence, identities and qualification. Mirrored/member-verified raw
`jpeg-factored-word-normalization-qualified.tar.xz` SHA256
`86c732f367a36ca05b9d92f1676cedb4a0fbef6af78a5c86a33454fcf366acbc` (3,695,052 B / 121 members)
preserves both producers, source, package, checks, failed prelaunches and
reproducing scripts/fixtures/worker, with J16C1/MGP1 dependencies retained.
The full 268-path objective remains open.

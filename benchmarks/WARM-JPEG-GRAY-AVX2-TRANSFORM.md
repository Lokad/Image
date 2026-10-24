# Exact vector grayscale JPEG transforms

The existing513×259 controls retain Q69, owned quantizers and raw-exact58,910-byte
packets. Caller-gray borrows resident gray samples; RGBA-gray normalizes against
0x204060. Complete operations include encoding, fresh independent owned output
and release, plus normalization where required. The approved reference is
SIMD-enabled standalone libjpeg-turbo3.1.4.1, benchmark tooling only.

| Quiet one-CPU run | Lokad µs | libjpeg-turbo µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| Caller-gray short | 2,805.02 | 1,078.78 | 2.600/2.739 | 154,839 |
| Caller-gray independent | 2,824.38 | 1,079.44 | 2.617/2.638 | 154,839 |
| RGBA-gray current short | 3,747.82 | 1,415.35 | 2.648/2.875 | 154,836 |

Independent caller-gray confirmation uses17.3% less elapsed than its
[preceding confirmation](WARM-JPEG-GRAY-QUANTIZATION.md). All12actual iterations
are retained,11selected; both sets contribute to the conservative upper. Both
versioned idle guards pass. RGBA-gray is a current screen without a separate
independent speedup claim.154,839/154,836B/op retain raw GC reporting, within2B
of preceding values; no per-call heap storage is added.

Two8×8integer transposes let AVX2 execute eight existing even/odd butterflies in
parallel. Both rounding stages and portable scalar fallback remain exact. The
same256-byte snapshot preserves overlapping/unaligned input/output; proven
intermediate and output bounds preserve signed16 publication. Shared/local
admission, work, cancellation and independent output ownership are unchanged.

Both platforms pass5,088checks each, including947live comparisons,7affected
scalar gray-stage checks and148scalar public JPEG checks.114,944original blocks
retain7,356,416frozen coefficients; bounds, overlapping guards, invalid lengths
and zero allocation pass. Existing2,145native block/quantization identities
remain exact. The same actual private0.2.0 package and all three examples pass
both platforms. Pre-package headroom reserves5MB plus margin.

[Evidence](warm-jpeg-gray-avx2-transform-evidence.json) records samples, producer
identities and exact reuse. Color JPEG, other codecs, decoders/editing/text remain
unchanged. VP8/VP8L now have the largest current encoder gaps. The full257-case
CPU-parity objective remains open.

E179 independently confirms the same RGBA-to-grayscale boundary on unchanged
production paths: 3,758.47 µs, 2.696×/upper2.711, 154,836 B/op and the same exact
58,910 B packets. Both idle guards pass; all 12 actual iterations are retained
and selected. The current core and package reuse E178 qualification. This resolves
an earlier short-run uncertainty; it is not a new production speedup claim.

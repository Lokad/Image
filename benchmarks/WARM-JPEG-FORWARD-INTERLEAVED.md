# Warm JPEG certified forward transform and quantization

Interleaving the independent sums in a certified separable transform reduces
complete JPEG encode time by about 3.7%, retaining every encoded byte.
The measured ratio moves from 2.02x to 1.95x FFmpeg on the frozen case.
The production optimization and its original correctness controls are retained.

The [numeric evidence](warm-jpeg-forward-interleaved-evidence.json) contains
original samples, confidence bounds, allocations, quality/source/output/DLL
identities and immutable source/raw archive hashes. Regenerate this report with
`python benchmarks/render-warm-jpeg-forward-interleaved.py`.

| Stage | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
| Retained baseline | 7638.30 | 3784.38 / 3778.61 | 2.0215x | 2.1956x |
| Rejected serial accumulators | 7635.35 | 3776.67 / 3774.69 | 2.0228x | 2.0916x |
| Interleaved screen | 7355.71 | 3780.91 / 3773.80 | 1.9492x | 2.1784x |
| Interleaved 12-sample confirmation | 7393.29 | 3784.19 / 3781.85 | 1.9549x | 1.9743x |

Two separable eight-term passes use the existing owned frozen basis and
separate products/additions. Independent rows and vertical frequencies are
interleaved while retaining each sum’s x/y order. A conservative 2^-30 margin
certifies the original rounded quantized integer; uncertain coefficients use
the original direct evaluation. Standalone forward coefficient bits remain
unchanged. The [independent proof](https://github.com/lokad/image/blob/39402e74e0f95c82ab209e5a950066a206c2af65/docs/jpeg-forward-quantization.md) defines
the domain and bound. The original 4096-unit atomic work/cancellation boundary
and fixed scratch admission remain; 512 stack bytes are reused and the existing
coefficient-array allocation is preserved. No lookup, cache or heap buffer is added.

The preliminary serial-accumulator implementation gives no useful gain.
An actual timed-DLL probe confirms fast-path selection and the original
half-boundary outputs. A private instrumented complete encode needs direct
fallback for 1,003 of 411,840 coefficients (about 0.24%). One VM JIT inspection
shows serial accumulator chains; the corrected scheduling is then screened
once and independently checked with 12 samples. The previous encode CPU profile
is reused. No additional profile, quality preparation or image is introduced.

Original constant blocks 2 and 3 expose quantizer half-boundary errors at
divisors 32 and 16. Twelve new controls pass before the production change.
Every generated divisor and mixed lanes are compared with the original direct
quantized integers. Controls cover both signs, unaligned spans, sentinels,
unchanged source/quantizers, parent work, tight atomic admission and cancellation.
Supported fast-path selection is asserted, and zero-divisor decline leaves
work/output untouched. All 37 final forward/quantization/admission controls pass
on both platforms with normal and disabled intrinsics. A private certificate
bypass mutation breaks both original half-boundary controls as expected.

The only benchmark raster is the existing original 513x259 RGBA8 input:
owned quality 69, native qscale 5, 4:4:4 sampling and background 0x204060.
Frozen encoded bytes, the <=0.25 dB source-error match, decoder/alpha bounds,
ownership and seven historical settings remain exact. Complete public encoding
and fresh MemoryStream creation/growth/disposal are timed. Native conversion,
flattening, encoding and independent output acquisition/release are included;
declared codec/conversion state reuse is unchanged. Startup, IPC, I/O, hashing
and validation are excluded. Jobs run sequentially in Release on the dedicated
EPYC 9V45 VM, CPU 0, one codec/conversion thread, thread type zero, normal SIMD,
.NET 10.0.8 / SDK 10.0.204, tiering disabled and non-concurrent workstation GC.

Both statistical prequiet checks pass; both postquiet checks fail. The longer
run’s conservative upper is below 2x, while the short run’s is not. Original
quietness failures and cooldowns remain recorded. Formal independent passing
confirmation remains open. Unsupported hardware and other paths are unmeasured.

All 2,063 tests pass on Windows and Linux (483 internal, 686 public, 894 live
FFmpeg comparisons). Both actual statistical DLLs and the fresh Linux build
match the same actual dependency-free Release package in every production
method body and implementation flag. Its DLL bytes equal the fresh Windows
build, and all three usage examples pass through that same package on both OSes.

The fixed 16-operation allocation totals and histograms equal the retained
package; raw per-operation sequences differ in the order of five 248-byte
extras, whose ownership is not attributed. An initial helper omitted its
required one-processor environment, and the recovery’s sequence assertion was
too strict. Both failures and raw arrays remain; only the missing environment
and evidence interpretation are corrected. No examples, tests or timings are
repeated for recovery. Actual package SHA256:
`5d192ebc15f226f60e3d701336901fed6a84078672ae1d1eb193729fa7b1c843`.

PNG16 decode is now the largest retained measured point gap near 2.0093x.
Unknown, unmatched, uncovered and unconfirmed required paths remain open;
uniform warm <=2x parity has not been achieved.

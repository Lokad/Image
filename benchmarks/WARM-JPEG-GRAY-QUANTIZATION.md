# Exact grayscale JPEG vector quantization

The existing513×259 controls retain Q69, owned quantizers and raw-exact58,910-byte
packets. Caller-gray borrows resident gray samples directly; RGBA-gray normalizes
the original RGBA8 input against0x204060. Complete operations include encoding,
fresh independent owned output acquisition and release, plus normalization where
required. The approved reference is SIMD-enabled standalone libjpeg-turbo3.1.4.1,
limited to benchmark tooling.

| Quiet one-CPU run | Lokad µs | libjpeg-turbo µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| Caller-gray short | 3,425.11 | 1,080.85 | 3.169/3.520 | 154,840 |
| Caller-gray independent | 3,415.99 | 1,078.78 | 3.167/3.183 | 154,840 |
| RGBA-gray current short | 4,155.49 | 1,416.07 | 2.935/3.280 | 154,838 |

Independent caller-gray confirmation uses22.8% less elapsed than its
[preceding screen](WARM-JPEG-GRAY-FACTOR.md). All12actual iterations are retained;
11selected results and all-actual statistics both contribute to the conservative
upper. Both versioned idle guards pass. RGBA-gray is a current screen without
a separate independent speedup claim.154,840/154,838B/op retain raw GC reporting,
within3B of preceding values; the implementation adds no per-call heap storage.

One prepared128-byte reciprocal stack replaces repeated scalar division in the
AVX2 path. Unsigned high products, one exact quotient correction and sign
restoration preserve every signed16 coefficient for every8-bit quantizer. The
scalar path retains division. No obsolete alias or mode is kept. Scratch fits
the existing8KiB workspace; shared/local admission, work, cancellation, borrowed
inputs and independent output ownership remain unchanged.

Both platforms pass5,088checks each, including947live comparisons,6affected
scalar gray-stage checks and148scalar public JPEG checks.16,711,680exhaustive
coefficient/quantizer pairs plus1,048,576mixed-lane coefficients retain the frozen
hash. Unaligned/in-place guards, borrowing, rejection, zero allocation and
unchanged admission pass. Existing2,145native gray block/quantization controls
remain exact. The same actual private0.2.0 package and all three examples pass
both platforms. Pre-package headroom reserves5MB plus margin.

[Evidence](warm-jpeg-gray-quantization-evidence.json) records samples, producer
identities, qualification and exact reuse. Color JPEG, other codecs, decoders,
editing and text are unchanged. The full257-case CPU-parity objective remains open.

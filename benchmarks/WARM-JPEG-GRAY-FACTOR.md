# Exact grayscale JPEG transform factorization

The existing513×259 grayscale controls retain Q69, owned quantizers and complete
raw-exact58,910-byte packets. The RGBA input is flattened on0x204060; the caller
control supplies resident gray samples directly. The reference is explicitly
approved SIMD-enabled standalone libjpeg-turbo3.1.4.1, limited to benchmark tooling.

| Quiet one-CPU run | Lokad µs | libjpeg-turbo µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| RGBA short | 7,379.95 | 1,410.55 | 5.232/5.692 | 154,840 |
| RGBA confirm | 7,498.78 | 1,397.74 | 5.365/5.410 | 154,840 |
| caller gray current short | 4,425.94 | 1,078.60 | 4.103/4.542 | 154,843 |

Independent RGBA confirmation uses23.7% less elapsed than the
[preceding screen](WARM-JPEG-SPARSE-SCAN.md), with unchanged154,840B/op.
Both versioned idle guards pass. All12actual iterations are retained. Complete
operations include normalization where required, encoding, fresh independent
output acquisition and release. Caller-gray is a current-cost screen, without
an independent speedup claim. Its3B/op reporting difference retains raw GC rounding.
The two public boundaries are not a precise method-cost subtraction.

An independently derived even/odd factorization replaces dense Q13 dot products.
Each8point line uses12general integer products instead of64, preserving both
rounding stages and every coefficient. The dense development mode/table is removed.
The same256-byte row snapshot preserves unaligned overlaps and fits the existing
fixed operation admission. Work, cancellation, shared/local admission and output
ownership remain. No per-call allocation, reservation, cache or upstream data is added.

Both platforms pass5,079 checks each, including947 live comparisons,7 scalar
gray-stage checks and148 scalar public JPEG checks.114,944original mathematical
blocks retain7,356,416exact frozen coefficients, including byte extrema, impulses,
endpoint tiles and seeded blocks. Algebraic/domain bounds, invalid lengths,
unaligned overlaps/guards and zero warm allocation pass. Existing2,145native
block/quantization identities remain exact. The same actual private0.2.0 package
and all three usage examples pass both platforms. The package headroom check
explicitly reserved5MB plus margin before packing.

[Evidence](warm-jpeg-gray-factor-evidence.json) retains samples, identities,
qualification and precise reuse. Color JPEG, other codecs, decoders/editing/text
are unchanged. Grayscale remains the largest measured gap; full CPU parity is open.

# Exact AVX2 JPEG row reductions

The existing 513×259 JPEG420 controls retain their original inputs, background
0x204060, quality85/native qscale5 and settings. Complete RGBA16 packets remain
52,115/52,117 B; RGBA8 remains 52,091/52,093 B. Only the approved attribution COM
differs. The caller-root control retains its 32 MiB root and private coefficients.

| Quiet one-CPU run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| Governed RGBA16 short | 6,200.16 | 1,837.72 | 3.374/3.700 | 803,968 |
| Governed RGBA16 confirm | 6,183.11 | 1,837.17 | 3.366/3.385 | 803,968 |
| RGBA16 short | 6,225.27 | 1,836.16 | 3.390/3.730 | 372,808 |
| RGBA8 short | 5,689.22 | 1,767.88 | 3.218/3.531 | 372,811 |

Independent governed confirmation uses 8.3% less elapsed time than the
[preceding 6,746.29-µs screen](WARM-JPEG-COOPERATIVE-OWNER.md), with unchanged
allocations. Both idle guards pass. The other two rows are current-cost screens,
without independent speedup claims. Complete operations include normalization,
encoding, admission, fresh owned output and release. Full CPU parity remains open.

AVX2 horizontal additions replace per-coefficient scalar extraction/reduction.
The weighted sum plus rounding bias fits Int32 for every byte input; frequency
order, bias, shift and signed saturation remain exact. Column math, weights,
SSE2/scalar behavior, fixed scratch, work/admission/cancellation and ownership
remain unchanged. The kernel introduces no heap allocation, cache or new mode.

Both platforms pass 5,068 checks each, including 947 live comparisons, six scalar
transform checks and 148 affected scalar public checks. 409,600 frozen coefficients,
the original 166 native vectors, range proofs, unaligned partial overlaps, invalid
lengths and zero warm kernel allocation pass. The same actual private 0.2.0 package
and all three usage examples pass on both. Only required producers were built;
`test.ps1 -NoBuild -Differential` ran the full suites against verified binaries.
Existing benchmark, native and generated hosts were reused.

[Evidence](warm-jpeg-avx2-rows-evidence.json) retains identities, raw samples,
selected/all-actual confidence and precise reuse. Other color-JPEG costs require
current evidence; unchanged grayscale, decoders, other codecs, editing and text
retain relevant confirmations. The finite scope remains 257 cases.

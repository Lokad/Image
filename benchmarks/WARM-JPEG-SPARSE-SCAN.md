# Exact JPEG sparse coefficient traversal

The existing513×259 governed JPEG420/RGBA16 control retains Q85/nativeq5,
background0x204060 and complete52,115/52,117-byte packets. Only the approved
attribution COM differs. UngovernedRGBA8 remains52,091/52,093B; grayscale is
raw-exact58,910/58,910B against the approved standalone SIMD libjpeg-turbo reference.

| Quiet one-CPU run | Lokad µs | Reference µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| governed short | 5,145.86 | 1,840.17 | 2.796/3.684 | 803,971 |
| governed confirm | 5,134.92 | 1,839.63 | 2.791/2.804 | 803,971 |
| rgba16 current short | 5,017.63 | 1,836.89 | 2.732/2.968 | 372,811 |
| rgba8 current short | 4,626.00 | 1,771.43 | 2.611/3.179 | 372,811 |
| gray current short | 9,829.15 | 1,409.55 | 6.973/7.422 | 154,840 |

Independent governed confirmation uses17.0% less elapsed than the
[preceding confirmation](WARM-JPEG-AVX2-ROWS.md). Both idle guards pass.
Complete operations include normalization/admission, encode, fresh independent
output and release. The reported3B/op color difference retains raw GC rounding;
the new primitive has zero warm managed allocation and no new reservation.
Ungoverned/gray screens establish current cost, without independent speedup claims.

AVX512BW builds an exact zigzag AC nonzero mask; set-bit traversal skips zeros
while retaining DC history, zero runs/EOB, categories, entropy/work and packets.
Compiled format indices add no managed table/cache. Existing fixed workspace,
borrowed inputs, cancellation, local/shared admission and output lifetimes remain.
Other hardware keeps the scalar traversal. Color and grayscale share this scanner.

Both platforms pass5,074 checks each, including947 live comparisons,2 scalar
mask and148 scalar public JPEG checks. Linux verifies295,169 independent mask
cases; Windows checks unsupported-ISA behavior and scalar public packets.
The original-method prototype checks298,599 blocks, including3,366 actual control
blocks. Unaligned inputs, tails, short input, borrowing and zero warm allocation pass.
The same actual private0.2.0 package and all three usage examples pass both platforms.

The earlier grayscale0.962× claim describes a replaced58,823-byte encoder, before
admitted raw-exact grayscale reference encoding in3f36e3f. Its packet/CPU evidence
cannot qualify current code. The current6.973× gap is the next priority. Prepared
quality metadata was refreshed solely for the current exact public decoder:
native packet/input/settings/source error stayed unchanged, and the previous
max1/SSE5451 decoder difference became exact zero. Existing quality gates remain.
Archived generated gray hosts and the same native worker were reused.

[Evidence](warm-jpeg-sparse-scan-evidence.json) retains samples, identities,
qualification and precise reuse. Other JPEG encoder costs remain prior-production;
PNG/WebP, all decoders, editing and text are unchanged. Full CPU parity remains open.

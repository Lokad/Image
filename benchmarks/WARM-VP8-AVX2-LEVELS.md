# Exact VP8 vector quantization

The existing513×259 RGBA8 control retains Q75/method4, the frozen reference
settings and its complete98,590-byte packet, including exact alpha.

| Quiet one-CPU run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| short | 166,241.00 | 53,468.53 | 3.109/3.521 | 24,080,952 |
| confirm | 166,543.29 | 53,562.51 | 3.109/3.128 | 24,080,952 |

Independent confirmation uses7.8% less elapsed time than the
[preceding confirmation](WARM-VP8-TEXTURE-BUTTERFLY.md), with unchanged allocations.
Both versioned idle guards pass. Complete operations include normalization where
required, alpha, encoding, fresh owned output and release. CPU parity remains open;
this result covers this fixed content/settings and operation boundary.

AVX2 computes eight coefficient levels at once. Checked reconstruction and
publication stay in their original scalar order, retaining overflow behavior.
The scalar hardware path remains available. Borrowed matrix/output aliasing is
rejected before mutation. Fixed64-byte stack scratch fits the existing admitted
workspace; work, cancellation, shared admission and output lifetimes remain.

Both platforms pass5,072 checks each, including947 live comparisons,14 scalar
quantization checks and29 scalar public reference-packet/wrapper checks. An
independent arithmetic prototype compares73,600 blocks/1,177,600 lanes with the
original method; tracked tests freeze the full coefficient domain, threshold,
rounding, sign-zero and Int32 boundaries. Checked failure prefixes, borrowed
storage, untouched tails and zero warm allocation pass. Existing native fragment
and14,025-block hashes remain exact. The same actual private0.2.0 package and all
three usage examples pass on both platforms.

[Evidence](warm-vp8-avx2-levels-evidence.json) records samples, producer identities,
qualification and unchanged-path reuse. JPEG, PNG, VP8L/lossless alpha, all decoders,
editing and text are unchanged. Immutable benchmark/native/confirmation workers
were reused throughout this bounded round.

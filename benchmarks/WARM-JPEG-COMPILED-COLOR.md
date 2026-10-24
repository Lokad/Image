# Exact compiled JPEG color matrix

Existing 513×259 JPEG420 controls retain frozen Q85/owned reference qscale5
quantizers, inputs, sampling and single-round normalization on RGB 0x204060.
Complete operations include normalization, encoding and fresh independently
owned output acquisition/release, on one CPU. Packets remain exact apart from
the approved attribution COM replacement: 52,115/52,117 B for RGBA16 and
52,091/52,093 B for RGBA8. No quality/settings search or new image cases.

| Quiet VM run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|
| Caller-root short | 3,993.91 | 1,837.47 | 2.174/2.338 | 804,075 |
| Caller-root independent | 3,988.50 | 1,839.84 | 2.168/2.184 | 804,075 |
| Nongoverned RGBA8 short | 3,802.65 | 1,765.18 | 2.154/2.499 | 372,856 |
| Nongoverned RGBA16 short | 3,919.58 | 1,842.55 | 2.127/2.509 | 372,856 |

Independent caller-root confirmation uses 7.9% less elapsed than
the [preceding confirmation](WARM-JPEG-VECTOR-WORD-NORMALIZATION.md).
All 12 actual/11 selected iterations contribute to the conservative upper; both
idle-v2 guards pass. Nongoverned rows are current short reconciliations without
independent speedup claims. Governed allocation increases 56 B/op; its cause is
unproven. Admitted peak remains 1,704,856 B, including 1,062,936 B surviving source.

The same nine rounded matrix coefficients are compile-time rational expressions;
existing luma/chroma helpers inline. Every integer operation/rounding remains.
No new storage, owner, allocation site, copy or allowance is added. Work,
cancellation, shared/local admission, refusal and output lifetimes remain.
An independent preintegration proof compared all 16,777,216 byte RGB triples and
66,867 paired controls on both platforms/normal/scalar. Its frozen hash and zero
warm kernel allocation are retained in the integrated test. FullRange already
folded/inlined before this change: no division-removal claim is made.

Both platforms pass 5,120 checks each, including 947 live comparisons, 13 scalar
matrix/normalization checks and 148 scalar public JPEG checks. The same actual
private 0.2.0 Release package and all three consumer examples pass both platforms.
The measured Linux core is the qualified core; benchmark/native/prepared hosts
and fixtures were reused.

[Evidence](warm-jpeg-compiled-color-evidence.json) records samples, identities
and qualification. Other affected colorJPEG CPU observations remain historical.
GrayscaleJPEG, other codecs, decoding, editing and text reuse unchanged evidence.
VP8 is the next larger measured gap; full 257-case/1× and readiness remain open.

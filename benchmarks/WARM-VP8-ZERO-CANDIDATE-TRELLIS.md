# Exact zero-candidate VP8 trellis shortcut

The existing513×259 opaque diagram/Q75/effort6 independently uses **5.3% less
elapsed**:15.45ms versus a fresh16.32ms baseline. It remains a **1.574× /
upper1.583** CPU gap. Complete packets stay exact at6,818B; allocations are
unchanged. These are separate sequential observations, not paired-sample
statistics or a general-content speed claim.

| Quiet one-CPU run | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Baseline, short | 15,972.12 | 9,800.85 | 1.630 / 1.806 | 754,344 |
| Baseline, confirm | 16,321.80 | 9,889.90 | 1.650 / 1.706 | 754,344 |
| Zero-candidate shortcut, short | 15,136.00 | 9,866.31 | 1.534 / 1.584 | 754,344 |
| Zero-candidate shortcut, confirm | 15,451.13 | 9,816.91 | 1.574 / 1.583 | 754,344 |

Both sides include source normalization, complete encoding and fresh independent
packet acquisition/release. Every actual, native bracket and idle-v2 guard passes
and is retained. Native allocation is unmeasured. Inputs/settings and exact
reconstructed pixels/alpha stay frozen; no compression/quality search or new grid.

Count-only evidence found60,284 of87,384 trellis calls with no nonzero candidate,
although none had entirely zero input coefficients. Safe AVX2 checks all used
integer products:0≤product≤65535 proves both candidate levels zero with the
existing shift17/bias65536. The DP then cannot select a nonzero terminal. The
shortcut clears the same regions and preserves I16 DC, tails, validation and
cancellation. The portable scalar search stays. No heap array, cache, reservation
or per-block scope is added; existing2048B operation scratch remains.

Before adoption,36,864 comparisons perOS match our accepted trellis source at
`38c3421`:threshold fragments, both luma types, two probability profiles, extreme
positive lambda, importance/tie cases, dequantization, DC/tails and ownership.
Focused tests additionally cover product/sign/wrap/vector boundaries, unaligned
spans, zero allocation and guards/cancellation before mutation. Both OSes pass
5,047 Release checks each, including947 live comparisons, plus21 internal and27
public scalar checks. The SAME actual private0.2.1 package and all three usage
examples pass both systems; published packages are unchanged.

One explicit immutable core build perOS serves the round. Cached harness/native/
short/confirmation hosts are reused; no new profile is needed. Only method5/6
luma-trellis callers change. Other named affected callers require current costs
before pass claims. Method4/effort2, other codecs, decoding, editing/text and
their valid evidence remain unchanged.

[Evidence](warm-vp8-zero-candidate-trellis-evidence.json) records all observations,
identities, proof and qualification. Mirrored/member-verified raw master:
`vp8-zero-candidate-trellis-qualified.tar.xz`, SHA256 `7cbcaa4e20f376392a4902c32eefb802f6d60a817b400c2c8f66cb821533b800`.
Stop this primitive; prioritize the larger remaining opaque-photograph/effort2/
JPEG caller-RGB420 gaps. Full267-path warm single-CPU ≤1× remains open.

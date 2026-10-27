# Exact parallel opaque chroma preparation

End-to-end costs are historical after [single-block reconstruction](WARM-VP8-SINGLE-BLOCK-RECONSTRUCTION.md). The preparation proof below remains valid.

The existing effort2 diagram independently measures 4.339 ms: 1.720× reference
time (upper 1.734×), exact 7,330 B and unchanged 741,342 B/op. Elapsed time is
11.43% below the preceding independent mean; separate runs are not a
paired baseline gain. Its 12 actual/filtered samples remain. The photo now has the
largest confirmed current gap: 17.077 ms, 1.755×/upper 1.774, 809,056 B/op, exact 17,688 B.
Its cached independent confirmation resolves the wide short; all 12 samples remain.
Other rows are guarded shorts, without independent gain/parity claims.

| Existing quiet one-CPU control | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Output bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| DiagramLossyDistortion8 short | 4,406.19 | 2,523.31 | 1.746 / 2.488 | 741,342 | 7,330 |
| DiagramLossyDistortion8 confirm | 4,338.57 | 2,522.55 | 1.720 / 1.734 | 741,342 | 7,330 |
| PhotographLossy8 short | 17,083.66 | 9,747.74 | 1.753 / 2.569 | 809,056 | 17,688 |
| PhotographLossy8 confirm | 17,076.96 | 9,728.47 | 1.755 / 1.774 | 809,056 | 17,688 |
| DiagramLossy8 short | 9,166.64 | 5,821.97 | 1.574 / 1.731 | 741,121 | 7,100 |
| DiagramLossyTrellis8 short | 16,609.72 | 9,835.02 | 1.689 / 1.748 | 754,328 | 6,818 |
| vp8 short | 77,283.89 | 53,477.53 | 1.445 / 1.625 | 23,258,168 | 98,590 |
| EncodeOpaque16 short | 26,973.16 | 15,619.34 | 1.727 / 1.832 | 1,201,536 | 72,380 |

The opaque RGBA8 path processes 16 source pixels into 8 chroma samples with safe
AVX512F/BW/VBMI intrinsics: exact generated gamma byte lookups, four-sample sums,
inverse-knot interpolation and the original signed matrix/byte rounding. Sums
stay ≤16380; inverse indices ≤31. Existing luma, scalar tails, repeated source
borders, alpha/16-bit and portable paths remain. No quality/sampling/policy change.

The 768-byte table value and temporaries are operation-local, admitted before
construction: 4 KiB on eligible opaque8 width≥16, 1 KiB otherwise. Temporary admission
rises 3 KiB on that path; retained capacity and measured heap B/op are unchanged.
No static/heap cache, extra frame copy or owner. Refusal remains terminal even
when scalar workspace would fit; row cancellation and output independence remain.

Both systems pass 4,948 Release checks (1,966 internal, 2,035 public, 947 live), 627 scalar
WebP and 63 focused checks. Eligible-hardware refusal/cleanup/recovery is tested, along
with native preparation hashes, odd borders, source/decision/packet identities
and existing resource/cancellation gates. The same actual private 0.2.0 package and
three usage examples pass both systems. One production build per OS was reused;
a real highwater-assertion update required only a linked-test rebuild, not another
production build. Source-analysis peak now preserves earlier preparation peaks.

Private non-timing proofs cover all 256 gamma inputs, 16,381 inverse sums and 256 two-row
cases/4,096 packed U/V samples before integration. [Evidence](warm-vp8-opaque-chroma-gamma-evidence.json)
retains source/core/native/fixture identities, samples and mirrored/member-verified
raw archive. Complete normalization, fresh output ownership and release stay timed.
Six named VP8 anchors replace earlier costs; unrelated paths keep valid evidence.
Stop chroma variants. Full 255-path upper ≤1× and replacement readiness remain open.

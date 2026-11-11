# Shared bicubic word preparation

The horizontal-height-identity and tight reverse-mixed word controls are now
owned by [exact identity-height execution](BICUBIC-IDENTITY-HEIGHT.md). Their
older costs here are history; unchanged execution paths retain source-valid evidence.

RPA503 shares SIMD divisor and conservative-margin preparation across pair,
quartet and cached mixed-word publication. Integer exponent construction is
bit-identical; hardware division, error bounds, rounding, repair and portable
fallbacks remain. No public API, owner, array, pool or session is introduced.
The cached caller retains its distinct acceptance checks.

Independent complete-operation comparisons establish these scoped changes:

- Mixed-axis: **1,058.51 → 1,016.78 µs**, saving **41.73 µs / 3.94% raw / 2.85%
  reference-normalized**. Native moves −1.13%.
- Cross-axis: **945.83 → 862.04 µs**, saving **83.80 µs / 8.86% raw / 8.94%
  normalized**. Native moves +0.08%.
- Tight vertical regresses **718.75 → 730.71 µs**, **11.96 µs / 1.66% raw / 1.82%
  normalized**. Native moves −0.15%. This tradeoff remains explicit.

Filtered/all-actual raw and normalized confidence separates for all three.
Allocation is unchanged on mixed/vertical; cross measures −1 B/op, without a
storage-saving claim. Refreshed other costs establish current parity, not causal
historical gains. Default and bounded 1027×519 enlargement now pass; six word controls pass
and stop, nine remain gaps. No blanket resizing speedup is claimed.

All controls use the original 513×259 RGBA16 input, existing output dimensions,
limits and zimg reference. Native graphs/scratch remain prepared as declared;
managed geometry, filtering, certified publication, independent output and disposal
remain timed. Exact managed rational pixels and existing native float bounds stay
unchanged. No shapes/settings are added.

| Existing case / output | Lokad µs | zimg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Up16, 1027×519 | 2,752.40 | 2,874.23 | 0.958 / 0.966 | 4,264,573 | Pass |
| TightUp16, 1027×519 | 2,751.64 | 2,875.47 | 0.957 / 0.963 | 4,264,573 | Pass |
| HorizontalUp16, 1027×259 | 1,448.29 | 1,459.92 | 0.992 / 0.997 | 114 | Pass |
| TightHorizontalUp16, 1027×259 | 1,489.69 | 1,455.63 | 1.023 / 1.030 | 114 | Gap |
| VerticalUp16, 513×519 | 1,521.56 | 1,675.81 | 0.908 / 0.912 | 114 | Pass |
| TightVerticalUp16, 513×519 | 1,527.18 | 1,670.07 | 0.914 / 0.918 | 114 | Pass |
| Down16, 256×129 | 548.29 | 574.61 | 0.954 / 0.959 | 199,951 | Pass |
| Horizontal16, 256×259 | 638.75 | 581.19 | 1.099 / 1.105 | 199,929 | Gap |
| Vertical16, 513×129 | 649.34 | 590.40 | 1.100 / 1.107 | 384,014 | Gap |
| Mixed16, 1027×129 | 1,016.78 | 841.41 | 1.208 / 1.217 | 114 | Gap |
| Cross16, 256×519 | 862.04 | 819.16 | 1.052 / 1.057 | 199,892 | Gap |
| TightHorizontal16, 256×259 | 662.01 | 586.01 | 1.130 / 1.137 | 113 | Gap |
| TightVertical16, 513×129 | 730.71 | 588.80 | 1.241 / 1.248 | 113 | Gap |
| TightMixed16, 1027×129 | 1,017.38 | 847.82 | 1.200 / 1.204 | 114 | Gap |
| TightCross16, 256×519 | 862.45 | 825.16 | 1.045 / 1.049 | 114 | Gap |

A qualified-DLL oracle freezes 24,576 exponent/mantissa/sign/threshold controls,
including 2,741 accepted outputs checked against independent scalar rounding.
Existing 1,008 pair controls and independent quartet decisions remain unchanged.
One private proof-tool refinement improved accepted-control coverage before any
production edit; its first version remains retained. Two coherent production
stages replace repeated setup; the initial misplaced cached edit failed compilation
and was corrected before measurements. No completed job is repeated.

Windows/Linux each pass **5,866 full/live checks**, plus **58 internal / 688 public**
per normal/scalar mode. The same actual Release package and all three examples pass
both OSes. One immutable main/embedded build per OS per stage; zero public/harness/
native/selector/fixture builds. No owned capacity or retention is added. Existing
admission/ownership qualification is reused; physical/admitted peaks are not newly
measured. Publication stays postponed.

[Evidence](bicubic-shared-word-preparation-evidence.json) retains source scope,
original/candidate actuals, adverse results, producer identities and qualification.
Raw `bicubic-shared-word-preparation-qualified.tar.xz`, SHA256
`1c9528ea15b55ccb329dd9b6a64fc8d9ab1af52192b368e867d087474ace77aa`,
18,238,164 B / 680 members, is mirrored/member-verified. All fifteen affected word
costs refresh; four displayed rows change and 121 displayed costs reuse unchanged
paths. This is not a 125-row actual-package release refresh.

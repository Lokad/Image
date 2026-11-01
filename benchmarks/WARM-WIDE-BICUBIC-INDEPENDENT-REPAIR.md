# Independent wider bicubic rounding repair

The existing RGBA8 513×259 → 128×64 control improves **6.68% elapsed**
(6.70% all-actual / **6.26% native-normalized**), preserving exact pixels and
owned storage. Both raw intervals separate; normalized candidate upper **1.445**
is below original lower **1.502**. The path remains a gap against ≤1×.

| Existing default-workspace control | Lokad µs | Ratio / upper | Managed B/op | Status |
|---|---:|---:|---:|---|
| Wide8 | 493.816 | 1.423 / 1.445 | 231,241 | Gap |
| Down64x32_8 | 386.868 | 1.059 / 1.081 | 205,898 | Confidence open |
| Wide16 | 445.240 | 1.062 / 1.083 | 264,001 | Gap |
| Down64x32_16 | 407.907 | 1.043 / 1.082 | 214,084 | Gap |
| Down32x16_8 | 342.094 | 0.903 / 0.927 | 199,369 | Pass / stop |
| Down32x16_16 | 339.742 | 0.821 / 0.853 | 201,414 | Pass / stop |
| Down16x8_8 | 323.903 | 0.713 / 0.737 | 201,739 | Pass / stop |
| Down16x8_16 | 325.970 | 0.652 / 0.678 | 202,248 | Pass / stop |

The 32-tap cache now repairs each uncertified pixel independently, using the
existing individual guards already used by larger caches. Cold analysis of the
original input finds 82 pixel replays, of which 41 neighbors are already certified.
This count motivates the trial; it is not a timing prediction. Thresholds,
coefficients, arithmetic, tap order, vector widths, packing and exact tiny-alpha
fallback remain. The larger-cache duplicate guard is removed. No new storage,
owner, reservation or cancellation mechanism. Primary allocation differs by
2 B/op measurement noise; substantial owned capacity is unchanged.

One short earns one independent primary twelve, reusing the original twelve:
all seven relevant production source pins match before the change. Seven other
existing shared-body controls receive one independent twelve each. Four passes
remain passing and stop. Three gaps remain; byte64×32 is confidence-open and its
score is not repeated. No gain claim is made for companions versus older costs.
The native bracket is 0.45% faster for the primary; normalized gain is separate.

All eight controls use the same two original rasters, frozen settings, complete
normalization/filtering and fresh output acquisition/release. Reference: zimg,
FFmpeg's zscale backend. Exact managed rational/frozen frames, original native
hashes and RGB/alpha max1/RMSE≤0.25, zero-alpha RGBzero, source independence,
surviving ownership and versioned quiet guards pass. Native allocation is
unmeasured. No new workload, fixture, profile or settings grid.

**5,376 checks per OS** pass: 2,176 internal + 2,252 public + 948 live. Existing
**221 public checks pass normal/scalar on both OSes**, also Linux without AVX512.
The same actual Windows Release package and three examples pass both OSes.
Its consumer verifies both frozen full byte/word frames under a shared root,
temporary-charge release and surviving output ownership. One core build per OS;
zero test/harness/native/selector builds. The immutable measured build serves
qualification. Preparation catches an older cached harness before timing;
the completed build/checks are reused with the current harness without reruns.

Only these eight wide-cache costs are invalidated. Tight-budget, axis, mixed,
enlarging and one-pixel controls reject before the changed body. Other applicable
evidence and published packages are preserved. The summary keeps 105 selected
comparisons and all **268 required material paths**; full parity remains open.
This is not a full immutable-package release refresh.

[Numeric evidence](wide-bicubic-independent-repair-evidence.json) retains all
actuals, brackets, confidence, scope, producers, qualification and actual package.
Raw `wide-bicubic-independent-repair-qualified.tar.xz`, SHA256
9d22a44b401bdcf6fb24e02b1bfedcf873c8df513a09f2b1e8ba86baab2a915c (2,162,904 B / 221 members),
is mirrored and member-verified, with sources, immutable DLLs, cached drivers/
workers, original fixtures, package/consumer, logs and reproduction scripts.
Cited dependencies retain the baseline and unchanged test assemblies.

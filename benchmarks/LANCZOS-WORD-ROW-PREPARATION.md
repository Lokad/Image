# Lanczos word source-row preparation

Exact word alpha products are now formed once per source row and reused by
overlapping horizontal windows. The existing operation/plan workspace owns the
admitted UInt32 capacity. Signed64 filtering, Int128/certified vertical execution,
Q30 coefficients, pixels and portable fallbacks stay unchanged. The qualified
byte kernel is restored; no pool, general cache or new admission model is added.

Prepared word reduction improves **639.53 → 613.92 µs**, **4.00% elapsed / 4.26%
reference-normalized**, saving 25.61 µs. Complete word one-shot improves
773.72 → 750.21 µs, 3.04% / 2.76%, saving 23.50 µs. Filtered/all-actual raw and
conservative normalized confidence separate. Claims cover these frozen reductions;
both prepared depths retain CPU gaps. Byte costs are refreshed without a
byte-kernel improvement claim.

| Complete 513×259 → 256×129 / zimg | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Plan retained B |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA8 one-shot | 720.25 | 1,566.64 | 0.460 / 0.462 | 138,643 | 0 |
| RGBA8 prepared | 614.45 | 492.45 | 1.248 / 1.255 | 139 | 139,800 |
| RGBA16 one-shot | 750.21 | 1,814.57 | 0.413 / 0.417 | 146,864 | 0 |
| RGBA16 prepared | 613.92 | 511.12 | 1.201 / 1.207 | 139 | 148,040 |

Each before/candidate confirmation has twelve actuals and adjacent native brackets,
one CPU, original inputs/settings, exact managed pixels and passing native/quiet
gates. One-shot includes preparation, storage, current input processing, independent
output ownership and disposal on both sides; prepared operations exclude explicit
preparation on both. Native float intermediates retain the frozen quality bounds.

Prepared allocation remains 139 B/call. Word plan retention rises **139,800 →
148,040 B**: 8,208 B of products plus coarse array allowance. One-shot word allocation
rises **138,608 → 146,864 B** (+8,256); byte one-shot rises by 24 B of metadata.
One-shot owned scratch is released; the existing unbudgeted output pool is separate
and unchanged. Physical heap peaks and native allocation remain unmeasured.
Governed capacity is reserved before allocation and stays charged through its
actual owner. Refusal is terminal; cancellation/exception cleanup releases storage.
Prepared images retain independent ownership after source/plan disposal.

The existing word-enlargement generalization screen is flat: 4,754.97 → 4,753.35 µs,
ratios 1.2177 → 1.2158, wide intervals; allocation is unchanged at 4,265,302 B/op,
plan retention 325,156 → 333,396 B. Three actuals establish no enlargement gain,
regression or parity. Do not extrapolate the reduction result.

Byte UInt32 preparation has a mixed screen. An exact-double paired implementation
is initially three times slower; instruction evidence exposes temporary vector
construction and a serial FMA chain. Correcting both restores near-baseline elapsed
but no useful normalized lead and higher retention. Both byte implementations are
removed. All adverse screens, exactness proofs, actual source/DLL stages and captures
remain in raw evidence; the abandoned implementation does not change pixel policy.

Four new workspace cases prove charges, optional capacity, terminal refusal,
cancellation cleanup and subsequent recovery. Windows/Linux each pass **5,529
full/live checks**, plus **72 internal / 15 public Lanczos checks per normal/scalar
mode**. Four frozen controls/eight boundaries pass normal/scalar on Linux. The same
actual Release package and all three examples pass both OSes, including retained
admission and asynchronous surviving outputs. Published packages remain unchanged.

One immutable final core per OS serves confirmation and qualification. Existing
workers, fixtures, selectors and quality checks are reused; no production rebuild
after timing. [Evidence](lanczos-word-row-preparation-evidence.json) pins current
costs, all adverse stages, boundary invalidation and package/source identities.
Raw `lanczos-word-source-row-preparation.tar.xz`: 33,872,700 B / 717 members,
SHA256 `c4834c33d827b7c5f1313c45225d5b7862a6f2bb7aa6b8149277b7ff4d1c02d2`,
mirrored/member-verified, with pinned qualified-baseline, worker and attribution
archives. This is a scoped development update, not a full release refresh.

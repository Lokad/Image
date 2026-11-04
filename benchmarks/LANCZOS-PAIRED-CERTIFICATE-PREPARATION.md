# Lanczos paired certificate preparation

The paired writer now prepares RGB quotients and certificate radii in the same
eight division lanes. Alpha's division by 2^60 becomes the identical exact
power-of-two scale for bounded normal values. RGB/radius divisions, clipping,
rounding, guard, masks and uncertain-pixel repair remain unchanged. No new array,
reservation, cache, API or ownership model.

One counterbalanced comparison of complete public prepared RGBA16 enlargement
confirms **175.99µs / 4.20% elapsed improvement**: 4,183.83→4,007.85µs. Twelve adjacent
original/candidate pairs alternate order, sixteen operations per subject, without
filtering. The conservative log-ratio interval maps to 0.95152–0.96441. Both immutable
builds execute in one process with independent identical sources/plans/outputs.
Setup/JIT/validation are excluded; source reads, row reset, result allocation and
disposal stay timed. This is incremental managed-build evidence, not FFmpeg parity.

Separate FFmpeg-backend confirmation has an adverse raw point: 4,201.44→4,375.20µs,
while native reference time is 8.87% slower. Its reference-normalized gain is 4.35%
with separated filtered/all-actual confidence. Preserve both facts; no standalone
causal raw speedup or regression follows. The bounded same-process comparison
resolves the missing comparable-clock evidence without repeating candidate jobs.

| Existing 513×259 control / zimg | Lokad µs | Reference µs | Ratio / upper | Managed allocated B/op | Retained plan B |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA8 reduction, one-shot | 711.35 | 1,588.55 | 0.448 / 0.457 | 138,647 | 0 |
| RGBA8 reduction, prepared | 563.84 | 489.93 | 1.151 / 1.158 | 139 | 139,800 |
| RGBA16 reduction, one-shot | 752.13 | 1,946.83 | 0.386 / 0.391 | 146,864 | 0 |
| RGBA16 reduction, prepared | 558.34 | 512.94 | 1.089 / 1.097 | 139 | 148,040 |
| RGBA8 enlargement, one-shot | 4,016.44 | 5,281.52 | 0.760 / 0.783 | 319,224 | 0 |
| RGBA8 enlargement, prepared | 3,775.71 | 4,085.82 | 0.924 / 0.946 | 148 | 325,156 |
| RGBA16 enlargement, one-shot | 4,337.24 | 5,079.02 | 0.854 / 0.860 | 4,592,777 | 0 |
| RGBA16 enlargement, prepared | 4,375.20 | 4,239.10 | 1.032 / 1.052 | 4,265,302 | 333,396 |

All eight affected costs are current; five still pass and stop, three prepared
cases remain gaps. Reuse the selected word-enlargement confirmation and refresh
the seven other existing controls once. Those refreshes establish current costs,
not further incremental claims; all actuals/brackets/clock movement remain.
One-shot includes plan/workspace preparation on both sides; prepared operations
retain their declared state. Native float does not relax exact managed Q30/rational
pixels, full 16-bit samples or native max 1/RMSE 0.25 gates.

A preceding count-only diagnostic finds zero repairs among 533,013 enlargement
pixels, 259 horizontal rows and 3,091 vertical accumulations. One two-second capture
of the original immutable binary has 1,696 samples: writer 600, vertical 502,
horizontal filter 467. Counts/sampling skid are attribution, not phase times.
The source-projection tool is excluded from production and performance numbers.

The second exact stage groups repair calls but has mixed/inconclusive independent
results and is removed. Both stages and adverse actuals remain. The selected first
stage is restored without rebuilding. This differs from the older scalar-reciprocal
trial: quotient/certificate divisions and their operation order stay identical.

Both OSes pass **5,631 full/live checks**, 76 internal/15 public affected checks per
normal/scalar/AVX512-off mode, and the SAME actual Release-package/all-three-example
verification. Two new theory cases compare 8,192 pair decisions against independent
single-pixel decisions; existing integer/rational envelope and frozen-output
checks remain. No production changes after measurement. Published packages stay
immutable; publication is postponed.

Capacity/ownership are unchanged. Word enlargement's 4,265,302 B/op includes its
required 4,264,104 B independent output; no incremental allocation saving or physical
peak claim. Caller budgets, cancellation and terminal refusal cleanup remain.
One selected core/internal build per OS; one extra Linux experimental version,
one diagnostic build, one private paired-tool build and one package. Cached
harness/native/selector/public-test artifacts are reused throughout.

[Evidence](lanczos-paired-certificate-preparation-evidence.json) pins every producer,
source, interval, gate and qualification result. Mirrored/member-verified
`lanczos-paired-certificate-preparation-qualified.tar.xz`: 8,248,688B /310 members,
SHA256 `8578848f8faf976e6be95054c6c7a01ae12c3d43e2de7f517815128ddda732b0`. Both stages, original modules,
all actuals/guards, diagnostic/paired tools and the actual package are retained
with frozen source/reference dependencies. Full 282-path objective remains open.

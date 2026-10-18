# Byte output across cached bicubic paths

Updated 2026-10-18. Five existing byte cache cases are **11.6–20.8% faster** after
reusing the proven byte-row emitter. Exact pixels, allocation/admission logic,
work and ownership stay unchanged. Vertical byte enlargement receives an
independent twelve-actual check and remains **1.020× / upper1.024×**, a small
measured gap. All other results below remain qualified three-actual gaps.

| Existing selector | Lokad.Image µs | Reference µs | Ratio / upper | Managed B/op | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| Cross16 | 1,056.31 | 819.48 | 1.289 / 1.388 | 1,263,255 | Qualified short gap |
| Cross8 | 994.41 | 840.45 | 1.183 / 1.259 | 731,753 | Qualified short gap |
| Down16 | 729.05 | 571.10 | 1.277 / 1.715 | 464,343 | Qualified short gap |
| Down8 | 635.08 | 416.65 | 1.524 / 1.610 | 332,178 | Qualified short gap |
| Horizontal16 | 866.64 | 579.18 | 1.496 / 1.646 | 730,691 | Qualified short gap |
| Horizontal8 | 734.08 | 574.67 | 1.277 / 1.365 | 465,374 | Qualified short gap |
| HorizontalUp8 | 2,211.79 | 1,516.35 | 1.459 / 1.596 | 1,930,617 | Qualified short gap |
| Up16 | 4,258.22 | 2,871.12 | 1.483 / 1.690 | 5,016,578 | Qualified short gap |
| Up8 | 3,919.91 | 3,036.45 | 1.291 / 1.357 | 3,244,646 | Qualified short gap |
| Vertical16 | 782.41 | 592.55 | 1.320 / 1.672 | 913,737 | Qualified short gap |
| Vertical8 | 624.02 | 583.77 | 1.069 / 1.273 | 648,997 | Qualified short gap |
| VerticalUp16 | 1,998.15 | 1,670.35 | 1.196 / 1.482 | 2,516,599 | Qualified short gap |
| VerticalUp8 | 1,776.78 | 1,742.77 | 1.020 / 1.024 | 1,664,251 | Independent gap |

The existing original513×259 RGBA8/16 rasters become256×129 (Down),1027×519
(Up),256×519 (Cross),256×259 (Horizontal),513×129 (Vertical),1027×259
(HorizontalUp) or513×519 (VerticalUp), using default workspace. Exactly the
thirteen existing cache selectors are retained; no new input, shape or settings
grid is introduced. Complete public Resize includes admission, geometry,
filtering, fresh independent output and disposal. The unchanged **zimg, FFmpeg
zscale backend** reference includes source conversion, timed clamped halo4,
encoded-space premultiplied float B=0/C=.5 filtering, owned float-planar/packed
output, unrounded-alpha division/quantization and release. Private contexts may
be warm; no output is cached. Startup, IPC, I/O, preparation, hashing and validation
are excluded. Native allocation is unmeasured; managed B/op includes harness
amortization and is reported from each actual job.

Only one dispatch predicate and a private helper name change: all nonmixed byte
cache rows now use the existing byte emitter, previously selected only for
horizontal enlargement. Its entire body is unchanged: vertical tap order,
sums/divisions, byte precision margins, range/normalizer/tiny-alpha guards,
original-order replay, raw odd-row offsets, clipping/rounding/packing, tails and
cancellation. Allocation, workspace reservation and logical work charges stay
unchanged. Reversing the predicate and rename recovers the entire predecessor
file. Mixed rows keep their existing separate emitters.

Newly specialized Down8/Up8/Horizontal8/Vertical8/VerticalUp8 improve11.6%,17.7%,
13.9%,20.8% and19.8% respectively (the last selects its independent confirmation).
Word, cross and already specialized HorizontalUp8 controls show no material
regression. All thirteen shared-cache cases are conservatively remeasured;
**194 other comparisons and all58 passing proofs** retain their paths. Original
hashes, exact managed pixels, source/owners and approved normal/scalar native
RGB/alpha max1 and RMSE≤.25 pass. Only the existing vertical-word enlargement
exception permits RGBmax2 where referencealpha≤16, max1 elsewhere, alpha max1
and bothRMSE≤.25. No reference build, identity reconciliation or policy adjustment
is needed; current P39 workers/readies/quality checks are reused.

VerticalUp8's short screen improves20.3% and reaches1.010×, with a wide interval
straddling parity. One independent twelve-actual run resolves that uncertainty
at1.020×/upper1.024×; every actual is retained. It does not pass parity and is not
repeated unchanged. Other materially slower cases receive no long confirmation.
One immutable producer/harness/fixture/reference build serves thirteen short
screens and that confirmation, in batches of at most three. Both retained
generated drivers are reused; no profile, selector or native build is added.
All means are read before one accepted-change qualification.

All **3,663 Release tests per OS** pass (1,352 internal,1,413 public,898 live
FFmpeg), plus209 scalar cache/reference checks per OS. Existing coverage already
exercises both axes/depths, tight/default admission, extremes, odd offsets and
held owners; no new test matrix is needed for this unchanged emitter body.
The same actual private package passes its consumer and all three examples on
Windows and Linux, with matching symbols. Screened Linux bytes survive
qualification unchanged; published0.1.0 packages and tag remain intact.

[Numeric evidence](warm-bicubic-byte-emitter-evidence.json) retains complete
statistics, all actuals/quiet guards, exact identities, scope and qualification.
Raw archive `parity-p40-qualified-evidence.tar.gz`, SHA256
`21114dd252c7f82cfcac745418a34eee442c4c3d3bf33ce6b71765b88250939d`.

# Current WebP photograph attribution

One one-second capture of the unchanged, qualified photograph encoder contains
756 workload samples. The immutable public-operation worker, original fixture,
libwebp exact=1 reference, complete 160,032-byte packet and source/ownership gates
are unchanged. Profiling durations are discarded; these are stack observations,
not component timings or available savings.

| Observed function | Exclusive samples | Fraction of workload samples |
| --- | ---: | ---: |
| Color-trial histogram construction | 118 | 15.6% |
| Prepared predictor entropy | 92 | 12.2% |
| Color-trial cost evaluation | 61 | 8.1% |
| Clustering population sums | 57 | 7.5% |
| Predictor histogram accumulation | 48 | 6.3% |
| Match-index construction/search | 38 | 5.0% |

Inclusive color selection has 238 samples, predictor selection 216 and clustering
150. Inclusive/exclusive counts overlap and must not be added together. Prepared
predictor entropy belongs to predictor selection, separate from color-trial cost.
The current photograph therefore directs attention to transform preparation and
scoring before another matching-loop trial. The separate transparent-control
profile retains its narrower attribution; it is not a photograph cost model.

Existing channel-plane, coefficient-contribution and dense entropy-table trials
remain inconclusive. This profile does not prove those implementations useful or
justify replaying them. Next, count repeated exact score terms within a tile before
considering lazy preparation shared by its trials. Retain original cost arithmetic,
candidate order, ties, cancellation, admission and complete packets. A useful
design must remove enough repeated work to pay for its preparation and lookups.

No production, harness, sampler or reference build and no new performance control.
Quiet guards pass. A post-capture metadata-field mismatch was corrected using
the existing receipt's `nativeReady.exactRgb`; the completed trace was analyzed
without repeating the capture or encode. Failed handoff logs remain in the raw
archive. The unchanged [qualified production/package](WEBP-TRACE-OUTPUT-STORAGE.md)
reuses its full Windows/Linux/scalar/consumer proof.

[Evidence](webp-photograph-current-attribution-evidence.json) retains the profile,
source identities and receipt. Raw webp-photograph-current-attribution.tar.xz:
1,177,428 B / 30 members, SHA256
73e403fee9ce992d600bf9e0c41436b404845e7cc1e79031d86a3452ab6d934b,
mirrored/member-verified, with qualified production and original input dependencies
pinned. Current benchmark costs are unchanged.

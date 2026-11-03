# WebP token-tail history workspace

Cache-history preparation now borrows unused capacity after an uncached candidate's
live token prefix. Its users finish before cache application or later token writes;
selected candidates keep independent ownership. Aligned spans share one history/
scoring computation. Dirty cache entries are cleared. Context, disposal and live-
source/seed alias guards protect the borrow. Insufficient capacity selects the
original allocation path before admission; refusal never triggers another attempt.
No pool, cache, owner framework or cross-operation retention is added.

| Existing complete encode | Allocated B/op before → current | Saving | Exact packet B |
| --- | ---: | ---: | ---: |
| Transparent RGBA8, Q75 / effort4 | 8,622,560 → 8,146,632 | 475,928 / 5.52% | 98,590 |
| Lossless text/diagram, Q85 / effort4 | 4,560,910 → 4,363,930 | 196,980 / 4.32% | 7,234 |

The actual-DLL transparent stream control's admitted peak falls **3,847,888 →
3,656,309 B**, saving **191,579 B / 4.98%**. The photograph keeps its original
allocation path and **3,470,059 B** admitted peak. Encoder scratch returns to zero;
caller roots survive. Total allocation, admitted capacity and retention are separate
metrics; physical peaks and native allocation are unmeasured. The
[capacity audit](WEBP-CACHE-HISTORY-CAPACITY.md) retains the original ownership lead.

There is **no established CPU gain or regression**. One independent alpha comparison
gives normalized ratios 1.2222→1.2255 with overlapping intervals; its 2.54% raw lead
tracks a 2.79% faster reference. The fresh photograph global ratio is adverse versus
history (1.176 versus 1.083). One targeted existing fallback comparison gives
1.1181→1.1120, also overlapping; raw 3.70% tracks a 3.17% faster reference. Preserve
the adverse global value as current. All 12 interleaved blocks are retained without
filtering, two managed operations per block and the fastest of six adjacent native
samples. These comparisons do not replace the displayed global-bracket costs.

All eight affected existing displayed comparisons receive one current 12 confirmation
on the same immutable core: original/diagram/depth/async pass and stop; photograph
and transparency retain gaps; opaque confidence is open. Other adaptive H costs
remain historical. Inputs/settings, complete preparation/normalization and independent
output/disposal stay timed. Every original packet/pixel/source/ownership/quiet gate
passes. No new performance control or numerical-policy change.

Each OS passes **5,537 full/live checks** and **808 internal / 194 public WebP checks
per normal/scalar mode**. Eight new cases cover independent cache choices, alignment,
live-prefix/tail guards, later writes, dirty/zero storage, zero allocation/admission,
context/cache/disposal/alias guards, refusal cleanup and subsequent success. The same
actual **0.2.1 development package** and all three examples pass Windows/Linux.
Published packages are unchanged; publication remains postponed. One production/
internal build per OS serves the round. Existing workers, selectors and paired tools
are reused. Both launch repairs stop before actuals; completed measurements are kept.

[Evidence](webp-token-tail-history-evidence.json) pins the eight current costs,
independent comparisons, admitted peaks and same-package qualification. Raw
`webp-token-tail-history-qualified.tar.xz`: 4,166,948 B / 370 members, SHA256
`f764f66ac2e53a286685b2b76536e43ab57355917f9e14b9b49df62b77a151c4`,
mirrored and member-verified. Full source, actual assemblies/package, adverse points,
launches and unchanged reference dependencies are retained. The 282-path objective
remains open; speed-only work follows concrete current attribution.

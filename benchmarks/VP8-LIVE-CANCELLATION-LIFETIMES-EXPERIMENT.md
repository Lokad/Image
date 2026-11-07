# Live VP8 cancellation lifetimes: unselected experiment

VDR518/VCP519 leaves production unchanged. A current two-second capture locates
1,435 of 1,998 samples in the live coefficient reader. Generated code gives each
inlined cancellation decision an addressable token copy. This motivates a storage
lifetime investigation; samples are neither phase times nor predicted savings.

Three bounded implementations retain every entry/decision checkpoint, arithmetic,
refill, model/category/sign order, successful cursor, exact pixels and ownership.

| Complete live opaque decode | Actuals | Managed / FFmpeg µs | Allocated B/op | Interpretation |
| --- | ---: | ---: | ---: | --- |
| A: guarded original inline throw | 3 | 5,028.45 / 4,785.23 | 228,864 | Unpromising screen |
| B: separate cold original throw | 3 | 5,087.27 / 4,788.78 | 228,864 | Unpromising screen |
| C: direct token-carrying exception | 3 | 4,948.00 / 4,792.72 | 228,864 | Promising screen; confirmation follows |
| Qualified original, independent | 12 | 5,019.61 / 4,792.23 | 228,864 | Contemporary original |
| C, independent confirmation | 12 | 5,030.66 / 4,792.96 | 228,864 | No useful gain established |

The independent point is 11.05 µs/0.22% slower, with overlapping filtered/all-actual
raw and reference-normalized intervals. It establishes neither a gain, a causal
regression nor equivalence. No further samples follow. The qualified current cost
remains owned by [direct coefficient classes](VP8-DIRECT-COEFFICIENT-CLASSES.md).

A retains the intended token homes. B removes them but creates arithmetic-state
spills across a possibly returning call. C removes both, reducing the local frame
from 248 to 152 bytes; its 3,161-byte body is slightly larger than the original
3,107 bytes. This code shape does not establish an elapsed-time or physical-peak
benefit. No variant introduces owned capacity, retained storage or a new allowance.

Windows/Linux normal/scalar checks pass: A uses 935 internal/239 public checks per
mode; B/C use 41 reader checks/239 public checks. Ordinal tests cover both actual
decision helpers, exact cursors and following reads. C also compares exception
type, message, HRESULT, inner exception and borrowed token against the original
.NET throw. Separate single public sync/async ownership checks capture actual
production code generation. All experimental sources and tests remain in raw
evidence; none is retained in production.

Every timing uses the original 72,380-byte packet, exact independently owned
531,468-byte RGBA output, complete preparation/decoding/filter/conversion/disposal,
one quiet CPU, frozen gates and the declared retained native contexts. Instrumented
profile elapsed is discarded. There is no broader decoder or fresh-context claim.

Qualified sources, main/embedded modules and dependent bindings are restored and
byte-verified on both OSes. The actual package stays unchanged. Reuse
[VHB517 qualification](VP8-FOUR-BLOCK-HISTOGRAM.md); no broad suite, package or
restoration rebuild is repeated. All 125 displayed costs remain valid and unchanged.
This failed implementation round is not a permanent cancellation/window family ban.

[Compact evidence](vp8-live-cancellation-lifetimes-experiment-evidence.json) pins
all observations, source applicability, exceptions, code generation and restoration.
Raw `vp8-live-cancellation-lifetimes-experiment.tar.xz`, SHA-256
`6e9f729613d33e1d3cc3d4cbf181d5c7c1c38b9f0b791718069edc4c0ab59fab`,
10,798,720 bytes/312 members, is mirrored and every member verified. Scripts,
minor pre-execution/postprocessing corrections and predecessor input providers
are retained. There were no native/harness/fixture builds or favorable repeats.
Independent PNG preparation attribution proceeds; publication remains postponed.

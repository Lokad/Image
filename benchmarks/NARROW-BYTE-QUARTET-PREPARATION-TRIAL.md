# Narrow-byte quartet preparation trial

**Inconclusive; no production change retained.** BQP839 groups four source pixels
through the existing exact UInt16 premultiplication primitive before widening to
the original binary64 values. Its complete short screens establish no useful gain.

| Complete513×259→256×129 RGBA8 Resize /4KiB | Lokad / zimg µs | Ratio / upper | GC B/op |
| --- | ---: | ---: | ---: |
| Immutable qualified baseline | 451.36 / 439.60 | 1.027 / 2.132 | 113 |
| Attempted quartet preparation | 448.57 / 421.15 | 1.065 / 1.096 | 113 |

Each screen has three actuals. Raw improvement is2.79µs/.62%, reference-normalized
point is adverse3.74%, and native moves−4.20%. Raw/all-actual/normalized confidence
overlap; the baseline screen is especially noisy. These are diagnostics, not
passing current costs or a proven regression. No independent confirmation or
unchanged timing repeat follows an unpromising screen.

The candidate reuses the qualified packed executor's four-pixel products, which
fit UInt16 through65025. Original two/one-pixel residuals, source/cache loans,
coefficient layout, horizontal/vertical arithmetic, certificates/repair and
cancellation remain verbatim; the entire original file reconstructs. Existing
two256-bit stores per pair remain, distinguishing this from the earlier unselected
decoded-store trial. No new array, owner, pool, reservation, capacity or retention.
4KiB admission and3424/3232/4080B explicit row/large/small payloads remain.

The producer inlines. Row/tail bodies grow6616→6801B /6562→6769B, with their
source preparation and stores included in elapsed time. The model's65536 exact
byte-product pairs and reduced grouping counts are not CPU-gain evidence.
Windows/Linux normal/scalar checks pass532internal/791public cases, including four
new ordered-value/high/zero/tiny/mixed-alpha, unaligned-input/canary/no-allocation
cases and existing actual row/tail/odd/root/owner/cancellation coverage. One core/
embedded build perOS, no public/harness/native/fixture build. An initial patch
anchor mismatch stops before mutation; no production compile failure.

Original source and the qualified60dd/6ceb core/embedded/public modules are restored
exactly; candidate-only tests are removed with the discarded producer. Attempted
sources/modules/tests remain in raw evidence. The unchanged actual packagebdbeb557
and [6,439-check Windows/Linux/SAME-package qualification](CONTIGUOUS-WIDE-SOURCE-VIEWS.md)
are reused; no broad/package repeat. Existing confirmed costs remain current.
This does not impose a permanent ban on a materially revised preparation design.

[Evidence](narrow-byte-quartet-preparation-trial-evidence.json) and mirrored/member-
verified raw `narrow-byte-quartet-preparation-trial.tar.xz` —1,382,620B/61members,
SHA256 `9a5263197fd30aa384c65ba559b9e1bea072c244f949360dffda859e05f2bf56` —
preserve both screens, identities, source/model/poison checks, JIT and restoration.
Dependencies retain the baseline and original reference. No new image/profile/
settings grid; all289material paths remain, publication postponed.

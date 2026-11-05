# Current lossless-photograph comparison

The current qualified producer has a small confirmed gap against explicitly
labeled **libwebp exact=1, FFmpeg's encoder backend**. Its conservative interval
is **1.000841–1.038225**; the earlier uncertain current cost is superseded.
This measures unchanged production and establishes no speed or allocation gain.

| Complete 513×259 lossless encode, effort4 | Lokad µs | Reference µs | Ratio / upper | Managed B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Photograph | 84,191.94 | 82,331.43 | 1.022598 / 1.038225 | 6,516,648 | 160,032 |

One job reuses the existing counterbalanced driver: twelve adjacent pairs,
alternating order, four complete operations per subject, four warmup pairs and
twelve native batches before/after. Every pair/bracket remains. The bound takes
the worse of paired log-ratio confidence and all-actual/diagnostic managed-mean
confidence against the faster before/paired/after native bracket. Both managed
views retain all twelve samples; no weaker criterion forces a decision.

The same 513×259 original photograph, managed quality85/native75, effort4 and
complete 160,032 B packet stay frozen. Equal quality numbers are not the matching
criterion. Managed source processing/admission, fresh output growth, complete
encode, independent ownership/disposal and delegate dispatch remain timed.
Native repacking, pixel import, encoding and independent packet acquisition/
release stay timed; documented wrappers/BGRA scratch are retained. IPC, fixture
decode, setup/JIT, hashes and validation are excluded. One quiet pinned CPU,
tiering off and nonconcurrent workstation GC. All original exact packet/sample/
alpha, source/owner, native/normal/scalar CLI and quiet gates pass before/after.

Total allocation uses the direct driver's four-operation batches. Its difference
from the prior BDN figure is a measurement/batching difference, not a production
saving. Owned capacity and lifetimes are unchanged; native allocation and complete
physical/admitted peaks are not newly measured.

The [earlier adjacent measurement](WEBP-PHOTOGRAPH-COUNTERBALANCED.md) remains
historical: its producer predates common parsing/history changes. The existing
driver, current harness and frozen reference worker are reused without any build.
Reuse qualified CWH323 main/package/full/scalar proof and the separately qualified
[retained reuse tests](WEBP-INDEX-INITIALIZATION-INCONCLUSIVE.md). Only this one
comparison row changes; the other 124 remain. Next attribute this demonstrated gap
before selecting a production change. No unchanged remeasurement or publication.

[Evidence](webp-current-photograph-comparison-evidence.json) pins source/module,
settings, all confidence views, exact gates and reproduction.

Mirrored/member-verified raw `webp-current-photograph-counterbalanced.tar.xz`:
1,027,504 B / 41 members, SHA256
`1bc198bd5409a1bb2c6e4d618c74dc807b0dcbfda10654182a1d3e72e4fbe848`. Dependencies retain the qualified
source/package, fixtures/reference tools and original driver; the retained-test
archive resolves the current test-source manifest delta.

# WebP cache-prefix aggregation

Cache scoring counts each eligible literal once by minimum-hit width and maximum
hash, then folds cumulative mass by hash prefix into each cache-width histogram.
This replaces repeated counter updates across widths. One bounded operation-owned
scratch array is reused for both seed scores; the maximum-width output histogram
supplies cumulative leaves. Short seeds retain direct counting. Histograms, exact
costs, cache-bit/candidate order, ties, pixels and packets remain unchanged.

| Complete warm encode | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Original RGBA8 / libwebp exact=1 | 129,622.35 | 120,894.60 | 1.072 / 1.086 | 25,329,304 | 183,812 |
| Opaque RGBA8 / libwebp exact=1 | 76,982.83 | 70,099.66 | 1.098 / 1.107 | 13,387,336 | 121,896 |
| Photograph / libwebp exact=1 | 92,822.92 | 82,933.52 | 1.119 / 1.127 | 13,777,464 | 160,032 |
| Text/diagram / libwebp exact=1 | 9,055.80 | 9,559.13 | 0.947 / 0.963 | 6,686,461 | 7,234 |
| Transparent VP8 / FFmpeg | 65,098.06 | 53,424.71 | 1.219 / 1.231 | 11,608,624 | 98,590 |

Five independent confirmations retain twelve actuals, native samples before/after
and both quiet guards. Every frozen packet/source/settings/reconstruction/alpha/
normal-scalar-native/ownership gate passes unchanged. Diagram remains a confirmed
1× pass and stops; four other controls retain gaps. Libwebp exact=1 is explicitly
FFmpeg’s encoder backend. Claims cover these frozen controls/settings.

Alpha saves2.18ms /3.25% raw (3.08% reference-normalized); photo saves2.04ms /
2.16% (2.01% normalized). Both raw and normalized confidence separate. Ordinary/
opaque raw point gains2.64% /1.22% have overlapping normalized intervals: no
incremental gain claim. Diagram raw managed time is flat; normalized cost rises
6.82% with a6.59% faster native clock and separated normalized loss confidence.
This adverse relative result is retained without a causal managed-regression
claim. Its current upper.963 still passes. No control/settings retuning or
favorable resampling. Other native-clock shifts ordinary/opaque/photo/alpha are
-.49% /-1.52% /-.15% /-.17%.

Extra scratch is at most40KiB payload plus32Badmitted header per history operation,
chosen before admission from token work. Direct counters add40,984B/photo and
81,968B/alpha; diagram adds0 and the authored small control280B. All four admitted
peaks stay unchanged (photo10,166,464B /diagram4,802,292B /alpha4,085,883B).
No encoder scratch survives disposal. BDN/direct small harness differences remain;
ordinary’s40,168B BDN delta is not an exact scratch size. Total allocation is not
peak live capacity. Ordinary/opaque admitted/physical peaks and native allocation
are unmeasured. Coarse capacity excludes borrowed input/caller streams.

Storage is reserved before allocation through both actual score users. Existing
cancellable/local-eligibility behavior is preserved. Shared refusal is terminal;
no lower-memory retry, cache/pool, new owner/governor or fixture-based dispatch.
One pre-score guard held ordinary timing on brief CPU0I/O wait; no samples ran.
The failed record is retained, other guarded jobs completed, and its first valid
confirmation then ran. No scored-job repeat or historical-load cooldown.

Windows/Linux each pass5,455full/live checks and300affected normal/scalar checks.
Existing24,558frozen histograms also compare folded/direct results. Five new checks
cover zero/cache-token/guarded spans, actual large/mixed seed dispatch, exact
121,744Badmission, shared refusal/recovery, second-seed failure cleanup and
unchanged cancellation selection. The same actual0.2.1Release package/consumer
and three examples pass both OSes. Its existing authored patch is enlarged to
128×96deterministic RGBA noise in the same513×259canvas/6MiBroot, exercising the
new path through packaged sync/async operations and independent surviving outputs.
One core/internal build perOS; no public/harness/native/selector builds; immutable
DLLs reused for focused/scoring/full --no-build checks. Consumer-only source is
indexed afterward without production rebuild or timing repeat.

Complete operation boundaries and native prepared-wrapper labels are unchanged:
normalization/preparation/candidates, fresh output and disposal stay timed; native
pixel import, encoding and independent packet ownership stay timed. Startup/I/O/
IPC/checks are excluded. Other affected WebP encoding costs stay historical;
opaque VP8, decoders, other codecs and editing/text reuse unchanged proof.

Next: reuse the unchanged history-building lead and simplify repeated prefix-node
address calculation. No new storage or numerical policy is proposed. Publication
is postponed; published packages remain immutable.

[Evidence](webp-cache-prefix-aggregation-evidence.json) retains all actuals, guards,
source/producers, packages, qualification and held pre-score record. Raw
webp-cache-prefix-aggregation.tar.xz: 24,345,968B /625members, SHA256
34e5bbd505797c124e14330bdfaa3e1cb0992c3c6f04a7fc9861d7b10e28cd06, mirrored/member-verified. Before dependencies:
webp-prepared-color-support.tar.xz (SHA256
4974a1aecef6f89d389ab193d91252ef7f24201bf27df10f3993c9c5d88f9406) and
current-webp-opaque-alpha-evidence.tar.xz (SHA256
6670dd13c8899470d696ac5fa6de8365afb7ff8c7c47a967b071fe5ddf28295a).

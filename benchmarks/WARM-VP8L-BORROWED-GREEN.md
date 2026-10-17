# Borrowed VP8L green lookup

Updated 2026-10-18. Native-input lossless decoding takes **13.5–13.7% less time**;
indexed input improves3.9%. Literal/run is2.1% above its preceding independent
mean; compressed-alpha controls are roughly flat. All seven are **qualified
three-actual gaps**, with all actuals and both quiet guards retained.

| Existing decode | Managed µs | FFmpeg µs | Ratio / upper | Managed B/op | Input B |
| --- | ---: | ---: | ---: | ---: | ---: |
| VP8 + compressed alpha | 10,801.06 | 5,965.33 | 1.811 / 1.925 | 1,966,616 | 98,590 |
| VP8 + compressed alpha / fresh Stream | 10,871.67 | 5,994.64 | 1.814 / 1.859 | 2,327,699 | 98,590 |
| Indexed16 VP8L | 793.06 | 543.37 | 1.460 / 1.701 | 1,351,972 | 11,810 |
| Native VP8L / live token | 4,980.60 | 3,030.56 | 1.643 / 1.707 | 1,216,180 | 183,812 |
| Literal/run VP8L | 3,553.60 | 2,148.63 | 1.654 / 1.735 | 1,074,301 | 533,100 |
| Native VP8L | 4,922.43 | 3,007.80 | 1.637 / 1.728 | 1,216,180 | 183,812 |
| Native VP8L / fresh Stream | 4,787.64 | 3,031.33 | 1.579 / 1.638 | 1,924,876 | 183,812 |

These are the existing two original513×259 RGBA8/16 rasters, projections and
frozen native packets. Complete operations include input acquisition, parsing,
entropy/reconstruction/conversion, admission, fresh independent pixel ownership
and disposal. Stream cases include wrapper/read/copy/EOF/release costs. Startup,
IPC, disk I/O, preparation/hashing and validation are excluded. Native allocation
is unmeasured; managed B/op includes harness amortization. Pixels, alpha, hidden
RGB, source and independent ownership remain exact under the existing gates;
VP8 retains its approved bounded RGB comparison. No settings or image grid is added.

Large green trees use a12-bit lookup in freshly zeroed, unwritten private pixels.
Decoding switches to the original canonical path before that scratch is reached,
including after overlapping copies cross the boundary. Every borrowed word is
overwritten before publishing output. Stable counting-sort replaces15 alphabet
scans; the saved14×alphabet iterations cover bounded construction for trees with
at least768 leaves. No heap, fields, memory reservation or logical work is added.
Speculative bit reads commit only after the original exact work charge, retaining
partial-budget, truncation and cancellation behavior through the old fallback.

Removing only the lookup/preparation/dispatch and restoring the grouping loop
recovers **all three predecessor production files byte for byte**. Exactly seven
older independent decode measurements are superseded. All **200 other comparisons
and58 passing proofs** retain their executed paths, including opaque/simple VP8,
encoders, metadata, other codecs and edits. The existing profile and bounded
codebook diagnostic are reused; no new timing profile is needed.

The ordinary lossless case now uses the already approved live/Stream-capable
FFmpeg worker: its resident mode retains the same packet reset/decode/conversion
and owned-output calls. Its older worker identity was reconciled with an archived
source diff. Ordinary VP8's optional scalar-CLI flag correctly records that this
control is not performed; the Stream scalar control passes. Original validator
refusals, corrected receipts and both original guards are retained. No benchmark
was repeated and no quality tolerance or operation boundary changed.

One immutable producer/harness/native/fixture build serves the seven screens;
both prepared selection drivers are reused without building. All means remain
above parity, so no twelve-actual confirmation or small-control retuning is run.
All **3,654 Release tests/OS** pass (1,348 internal,1,408 public,898 live FFmpeg),
plus43 scalar Huffman/lookup checks/OS. Four authored cases cover every symbol,
phase, long fallback, partial budget, truncation/cancellation and an independently
specified cache/copy packet crossing the scratch boundary at exact memory/work
limits. Its packet was checked against the predecessor before implementation.
The same actual private package passes its consumer and all three examples on
Windows/Linux; symbols match. Screened Linux production bytes stay identical
through qualification. Published0.1.0 package/tag and unaffected scalar proofs remain.

[Numeric evidence](warm-vp8l-borrowed-green-evidence.json) retains statistics,
receipts, identities and qualification. Raw archive
`parity-p30-qualified-evidence.tar.gz`, SHA256
`56236115e1af2e8952d3446235a447ce979e0205129db453b1f91389841dc0d9`.

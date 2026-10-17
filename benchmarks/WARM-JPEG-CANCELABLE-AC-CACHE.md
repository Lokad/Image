# Cancelable JPEG AC lookahead reuse

Updated 2026-10-18. Complete baseline444 decoding with a live token is **9.3%
faster**, with unchanged pixels, work charges and allocation. The result remains
a **qualified three-actual warm parity gap**. All actuals and both quiet guards
are retained; no twelve-actual confirmation is run for a demonstrated gap.

| Existing case | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Baseline444, native input / live token | 2,765.05 | 1,717.88 | 1.610 / 1.664 | 2,664,373 |

Both subjects decode the existing **133,041-byte JPEG444 packet**, prepared from
the original 513×259 byte raster. Complete public Decode includes admission,
container/entropy/transforms, full-range conversion, independently owned RGBA8
output and disposal. FFmpeg includes state reset, complete software MJPEG decode,
BT.601 conversion, output acquisition and release; contexts and packet/frame
wrappers may be warm. Native has no cancellation setting. Token setup, startup,
IPC, I/O, preparation, hashing and validation are excluded. No output is cached.
Native allocation is unmeasured; managed B/op includes harness amortization.

One six-second pre-change profile records 5,327 samples, 54.2% in
ReadPlainCancelableAc. That method now retains ordinary lookahead in a local
64-bit cache, safely refilling four bytes when fewer than 18 bits remain.
Unused whole bytes return before every exit. Prefix and amplitude retain their
separate exact work admissions and cancellation checkpoints, including completed
charges, partial coefficients, logical position and pending bits on failure.
Stuffing, markers, long/incomplete codes, invalid syntax and constrained cases
retain original traversal. No fields, table, heap workspace or charge is added.
Replacing only this method recovers the entire predecessor source text.

Only **NativeDecodeCancelable8** changes. Ordinary baseline and progressive
selectors use other methods, as do metadata, encoding, other codecs and editing:
**206 comparisons and all 58 passing proofs are reused**. Original encoded and
per-decoder output hashes, exact managed pixels, source/owner independence,
opaque alpha and normal/scalar native RGB maximum4/RMSE1.5 gates pass.

The approved current ordinary JPEG444 worker is reused for this live-token case.
Compared with the older cited worker, added direct-RGB modes, false RGB guards
and untimed ready metadata preserve this YUVJ444P/BT.601 operation. Common ready
fields, original packet/output hashes, compiler, flags and static archives match.
No native build, baseline-policy change or relaxed tolerance is introduced.

One immutable producer/harness/fixture/reference build serves the short screen;
the original static live444 selector is reused. The mean is read before one
accepted-change qualification. All **3,662 Release tests per OS** pass (1,352
internal, 1,412 public, 898 live FFmpeg), plus 32 scalar entropy-boundary and 64
scalar public JPEG checks per OS. Expanded scalar-oracle tests cover consecutive
symbols, every cached phase/budget/truncation, multiple blocks and canceled
lookahead; they pass on the predecessor first. No new performance input or grid.
The same actual private package passes its consumer and all three examples on
Windows and Linux, with matching symbols. Screened Linux bytes remain unchanged
through qualification; released 0.1.0 artifacts and tag are preserved.

[Numeric evidence](warm-jpeg-cancelable-ac-cache-evidence.json) retains statistics,
identities, native reconciliation, source scope and qualification. Raw archive
`parity-p38-qualified-evidence.tar.gz`, SHA256
`de5431226a9abfb5a0a92fc293a8b091c0c4471155c546bd8bad326ec755ca48`. Pre-change profile archive
`parity-p38-jpeg-live-profile.tar.gz`, SHA256
`8c99d5c1bc66d1c4ee98b85bfa58551f5ecbb45a44f0cd27f102750bae46665b`.

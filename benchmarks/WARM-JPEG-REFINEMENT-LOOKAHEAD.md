# Progressive JPEG live-token lookahead

Updated 2026-10-18. Complete progressive444 decoding with a live token is
**10.1% faster**, with exact managed pixels and unchanged work, cancellation,
partial-failure state and allocation. It remains a **qualified short parity gap**.

| Existing case | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Progressive444 / live token (changed) | 5,206.53 | 3,354.93 | 1.552 / 1.731 | 2,682,364 |
| Baseline444 / live token (unchanged control) | 2,740.40 | 1,724.07 | 1.589 / 1.680 | 2,664,373 |
| Progressive444 (unchanged control) | 5,336.07 | 3,356.11 | 1.590 / 1.645 | 2,682,364 |

Both subjects decode the existing **132,961-byte progressive444 packet** from
the original 513×259 byte raster. Complete public Decode includes admission,
container/entropy/transforms, full-range conversion, fresh independently owned
RGBA8 output and disposal. FFmpeg includes reset, complete software MJPEG decode,
BT.601 conversion, output acquisition and release. Contexts and wrappers may be
warm; no decoded output is cached. Token setup, startup, IPC, I/O, preparation,
hashing and validation are excluded. Native allocation is unmeasured; managed
B/op includes harness amortization. The baseline control uses its unchanged
133,041-byte packet. Native has no cancellation setting.

One six-second predecessor profile places 58.7% of samples in the fused live
refinement reader, with only 0.78% overall in mask construction. It rules out
mask caching and supports reducing repeated cursor/cache traffic. The new guarded
path keeps a local 64-bit cache without address-exposed state or enclosing
exception handling. Four-byte ordinary lookahead cannot cross stuffing/markers;
publication returns unused whole bytes. Original per-code/field/correction-bit
polls, work charges and immediate updates remain exact. Slow tails publish before
using the original reader. Cancellation, syntax and coefficient overflow publish
the original logical cursor, pending bits, partial coefficients and completed
work. No field, table, heap workspace, admission or logical charge is added.

Only **ProgressiveCancelable444** replaces selected timing. Removing the new
methods and dispatch restores both entire predecessor files. Ordinary progressive
and baseline readers retain their paths. Their control means differ by +0.44%
and −0.89%; their selected evidence is reused, as are **206 comparisons and all
58 passing proofs**. No performance input or settings grid is added. The old
profile is explicitly pre-change and cannot attribute the accepted reader.

Exact frozen managed samples, input/output hashes, opaque alpha, source/owner
independence and normal/scalar native RGB maximum4/RMSE1.5 gates pass. Two new
independent-reference tests cover consecutive blocks, pending phases, stuffing,
every truncation, marker tails and coefficient overflow. Existing checks retain
long codes, work limits, cancellation and immediate partial-state coverage.

All **3,680 Release cases per OS** pass (1,359 internal, 1,423 public, 898 live
FFmpeg), plus 465 scalar internal JPEG and ten scalar progressive-work checks.
The same actual private package passes its consumer and all three examples on
Windows/Linux, with matching DLL, symbols and documents. Screened Linux bytes
survive qualification unchanged. Released 0.1.0 artifacts and tag are preserved.

One immutable producer/harness/reference/fixture build serves three sequential
three-actual, 200ms short jobs with 50ms native brackets. Both five-second
`exclusive-vm-idle-v2` guards and all actuals are retained. No native, harness or
selector rebuild occurs during measurement. The mean is read before controls and
one accepted-change qualification. A demonstrated 1.552× gap does not warrant
a twelve-actual parity confirmation. Unchanged qualification is reused.

[Numeric evidence](warm-jpeg-refinement-lookahead-evidence.json) retains both
native brackets, conservative selected/all-actual confidence bounds, allocations,
source scope and qualification. Raw archive `parity-p44-qualified-evidence.tar.gz`,
SHA256 `52aef563bd4caa70701e0caf897f336748e34f5c61068fcdb6414a13852a4086`.
Pre-change profile `parity-p44-current-progressive-profile.tar.gz`, SHA256
`4d7b96ad6d64cb39134bc9f9e9c0c5f6f64309d8e8957cfbd2d0aa29e57e78b3`.
The fixed 207-case parity objective stays open: 58 independent passes, 145
demonstrated gaps and four confidence-open cases. Passing paths remain stopped.

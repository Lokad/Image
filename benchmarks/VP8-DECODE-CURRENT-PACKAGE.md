# VP8 decode on the current package

The first five owned-output VP8 captures were uncertain on the preceding 0.2.1
package. Partitioned sampling confirms both transparent cases. The same schedule
confirms simple-filter opaque decoding on the latest package. Ordinary opaque
and live-token decoding retain open timing confidence.

| Complete operation | Lokad.Image µs | FFmpeg µs | Conservative interval | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| Opaque with live cancellation token | 4,969.17 | 4,821.00 | 0.923035–1.080555 | 230,880 |
| Transparent, fresh seekable stream | 5,845.96 | 6,008.13 | 0.871249–1.019694 | 240,496 |
| Transparent, resident | 5,968.06 | 6,009.20 | 0.901183–1.044184 | 240,400 |
| Opaque with simple filter | 4,984.92 | 4,925.66 | 0.912091–1.066994 | 230,880 |
| Opaque, resident | 4,945.38 | 4,843.16 | 0.919620–1.074378 | 230,880 |

The inputs, outputs, fixtures and reference identities match the earlier
[fixed-position round](VP8-FIXED-CODE.md). The original paired driver runs twelve
alternating pairs of 128 complete operations, with twelve equal reference batches
before and after. Managed parsing, cancellation, normalization, fresh stream
acquisition where applicable, and independently owned output remain timed.
FFmpeg retains its declared reference state and decodes every image. The
original normal/scalar checks and both VM guards pass. The coding-only VP8
validator retains its evidence because it returns a descriptor and skips the
changed output pool.

In the live-token case the first native bracket averages 5,132.9 µs and the
last 4,821.0 µs. Managed and adjacent native batches also move together.
The paired interval is 0.982905–1.007425, while the full conservative rule
keeps the upper at 1.080555. In three other resident cases the native brackets
move in the same direction. The cause remains unverified. The stream and
transparent resident comparisons pass under paired intervals alone, while the
full gate remains open. These measurements establish neither a demonstrated
production slowdown nor passing parity. The partitioned captures below test closer timing alignment under the same gate.

The immutable package and driver are reused. Allocations match the previous
measurements. They are separate from the owned output capacity and from
unmeasured physical peaks. The mirrored `vp8-decode-current-package-confirmations.tar.xz`
contains 36 verified members and 34,744 B, SHA-256
`b56bde45b1a954159b2e9e65221be8075a5125aa05a4c8ba4395b01392671ab6`. It retains original and current samples,
selection commands, VM guards and quality/ownership receipts.
[Evidence](vp8-decode-current-package-evidence.json)

## Partitioned confirmation

| Complete operation | Lokad.Image µs | FFmpeg µs | Conservative interval | GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Opaque with live cancellation token | 5,198.14 | 5,194.48 | 0.984622–1.016033 | 230,880 | Confidence open |
| Transparent, fresh seekable stream | 6,082.74 | 6,494.10 | 0.924882–0.947445 | 240,496 | Pass |
| Transparent, resident | 6,184.82 | 6,437.62 | 0.947734–0.971896 | 240,400 | Pass |

The original live-token managed/native ratio means for the first and last six
pairs were 0.992 and 0.998, with a larger move between separate reference
brackets. The revised schedule interleaves before, paired and after reference
groups within each actual. Each is split into sixteen eight-operation
subdivisions. Four whole 128-operation warmups per side and twelve full
statistical units per group remain. All subdivisions are retained. The
conservative gate takes the worst paired and all-actual bounds with all three
reference groups. The original diagnostic on upper outliers is retained.

Both transparent cases pass and stop. The live-token interval narrows and still
straddles parity. The complete decode, output and disposal stay inside every
operation. CPU0, natural GC, exact pixels and original stream and owner checks
remain. Differences in measured clocks do not establish a causal production
effect.

An initial stream capture exceeded the post-run guard with 4.2% CPU0 I/O wait
and 1.048% aggregate I/O wait against the 1% ceiling. It remains unscored.
The next run uses memory-backed storage for untimed temporary files and saves
its log after the guard. Both corrected captures pass the original guards.
The timed DLL, harness, native worker and fixtures are identical.

The mirrored `vp8-partitioned-current-package-confirmations.tar.xz` has 35 verified members
and 102,940 B, SHA-256 `2b2174384ebc090ae57bf7a18f8e3428ef8db55691fbb87a0907156ba608dd14`. It retains
the rejected capture, all accepted subdivisions, identities, gates and
reproduction commands.

## Opaque resident confirmation

| Complete operation | Lokad.Image µs | FFmpeg µs | Conservative interval | GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Opaque, resident | 4,791.37 | 4,879.96 | 0.913911–1.052237 | 230,880 | Confidence open |
| Opaque with simple filter | 4,711.56 | 4,900.74 | 0.955311–0.964514 | 230,880 | Pass |

Both cases use the same original 128-call units, twelve actuals, four warmups,
sixteen eight-call subdivisions and all three interleaved reference groups.
The immutable package supplies the tested DLL. One tooling build changes only
the selected case allowlist and its diagnostic text. Fifty non-core runtime
files, the prepared reference, the original packets and alpha, exact output
and independent-owner checks all match. Untimed files stay in memory and both
idle guards pass.

The ordinary opaque paired interval is **0.976560–0.983253**. First-six to
last-six managed means move from 4,720.45 to 4,862.28 µs. The paired native
means move from 4,813.07 to 4,966.51 µs. The drift's cause remains unverified.
The conservative gate includes every actual and every native group. Ordinary
opaque remains uncertain under that rule. Simple-filter passes and stops.
The live-token case keeps its earlier partitioned evidence.

The mirrored `vp8-opaque-partitioned-current-package.tar.xz` contains
17 verified members and 65,572 B, SHA-256
`4137aa08c5b882e031335125a00c57bbe984aff14a6e00f0e1696c52df6b48cf`.
It retains the original receipts, tool and reference identities, idle checks,
exact-output evidence and all subdivisions.

## Opaque timing diagnostic

Across the twelve retained ordinary opaque actuals, managed time correlates
0.998, 0.996 and 0.995 with the before, paired and after native groups.
The largest individual-group spread is 8.64%, while adjacent point ratios
range from 0.97525 to 0.98440. The live-token trace also moves together,
with paired correlation 0.916 and point ratios from 0.99550 to 1.00369.
The full conservative bounds still apply to both cases.

One instrumented ordinary opaque capture keeps the same immutable package,
fixture, reference, 128-operation units, eight-operation subdivisions and
guards. It records Linux main-thread scheduled runtime and run delay
outside the individual operation clocks, plus CPU0 counters for each whole
actual. The original 8.64% swing did not recur. The new managed and native
wall-time spreads are at most 2.57%. Native wall and scheduled time
correlate 0.9994 or higher in all three groups. CPU0 reports zero I/O-wait
and steal ticks over those actuals. The counters therefore describe only
the smaller movement in this diagnostic. They do not identify the cause
of the previous swing. The managed wall/scheduled difference also
includes runtime threads and measurement boundaries.

All packet, exact pixel, source and owner checks pass. One tooling build
adds counters. Production and the reference worker are unchanged. Every
clock in this capture is diagnostic. Both earlier uncertain comparisons
stay open under the same parity gate. A repeat needs a concrete new
measurement or production hypothesis.
[Scheduler evidence](vp8-scheduler-diagnostic-evidence.json)

The mirrored `vp8-scheduler-diagnostic.tar.xz` contains 14 verified members
and 48,068 B, SHA-256
`88cff927d8cc5f209b5273d3701644e23102d78c6c5c54a182c69f9b9e8d90cd`.

## Fixed subblock geometry trial

The live-token candidate computes the 4×4 luma and 2×2 chroma coordinates
with fixed shifts and masks and selects the token partition once per
macroblock row. A geometry check covers all 24 subblocks and 4,096 supported
row/partition combinations. Entropy order, model state, reconstruction,
cancellation and memory ownership retain their original boundaries.

Full optimized JIT output for `ReadPlanes` on the current DLL shows seven
integer divide instructions. The candidate has zero. Three alternating
16-operation pairs measure **5,172.89 → 5,168.59 µs** at **230,880 B/op**
on both DLLs. The paired interval **0.953284–1.047272** gives no useful
gain to confirm. The candidate and the ordinary/live parity uncertainties
remain separate. No independent or broader candidate timing follows.

Both guards and frozen pixels, source, live-token cancellation and
independent-owner checks pass. The collector expected optional output
dimensions after timing. The scored wire and guard records survived, and
the corrected collector replays validation only with its clocks discarded.
The prior attempt, original samples, both JIT outputs and recovery commands
are retained. One private library and one actor build reuse the existing
reference, fixtures and package. The mirrored archive has 28 verified
members and SHA-256
`5d3cb342c482be94fff5973fe8e250a74afbd468de73e621778a0520aaa48c4a`.
[Trial evidence](vp8-decoder-fixed-geometry-trial-evidence.json)

## Sparse residual reconstruction trial

The private decoder keeps the existing nonzero flag from each token block
and the macroblock's Y2 block. Zero residuals leave prediction intact. A
luma block with no coded residual can still get DC from Y2. The trial writes
that constant residual with the original signed rounding and saturated add.
Prediction and loop-filter decisions, token order, cancellation and admission
are preserved. No scan of the coefficient block or new retained storage is
needed.

Three alternating 16-operation pairs on the original live-token opaque control
measure **4,779.61 → 4,800.50 µs**, ratio interval **0.949869–1.062018**,
with **230,880 B/op** for both modules. This gives no basis for confirmation.
Exact pixels, source, independent ownership, live-token checks and both quiet
guards pass. The full signed DC range matches the general inverse, with **39
focused Windows tests** passing in each normal and scalar mode. One private
core build reuses the existing actor and FFmpeg worker. Both original opacity
comparisons remain open on the qualified package.

[Trial evidence](vp8-decoder-sparse-residual-trial-evidence.json) retains the
source, runner and 28 verified members of the mirrored raw archive.

## Already normalized range trial

The private wide reader skips its normalization step when the arithmetic
range is already at least 128. Window construction and each shifted
normalization leave at least eight input bits buffered, so this branch
skips a zero shift and a no-op refill. Every live-token poll remains.

Three alternating 16-operation pairs on the original opaque live-token
control measure **5,030.37 → 5,288.06 µs**, interval **0.914629–1.208248**,
at **230,880 B/op** for both modules. The adverse point and inconclusive
interval give no basis for confirmation. Exact pixels, source, ownership,
and quiet guards pass. Windows passes 52 focused cursor, fixture and
validation tests in each normal and scalar mode.

The first preflight inherited an integer-divide assertion from an older
geometry trial. The assertion stopped before sampling. The corrected
runner uses the same built DLL and retains the original preflight evidence.
[Trial evidence](prepared-range-and-vp8-normalization-evidence.json)

## Fixed-probability coefficient signs

The private reader uses the probability-128 split for every nonzero
coefficient sign. After the original normalization, the left range is
`ceil(range / 2)`. The right range differs by one only for odd ranges.
The sign mask selects the new interval and applies the sign before the
unchanged dequantization. The existing live cancellation check runs before the
sign decision. Fallbacks, packet traversal, publication and ownership stay
exact. No new storage is required.

The first variant applies the sign change to both wide readers. In
independent managed pairs, live-token decode saves **142.35 µs (2.77%)**,
interval **0.965964–0.978772**. Ordinary opaque saves 11.94 µs, interval
**0.992979–1.002343**. The live-only follow-up restores the ordinary reader
and saves **121.00 µs (2.35%)** in independent pairs, interval
**0.968896–0.984242**. All allocate 230,880 B/op.

| Private DLL / complete FFmpeg comparison | Managed / FFmpeg µs | Conservative upper | Status |
| --- | ---: | ---: | --- |
| Both readers, live token | 4,590.79 / 4,812.23 | 0.961712 | Pass |
| Both readers, ordinary | 4,776.78 / 5,109.27 | 1.003126 | Uncertain |
| Live reader only, live token | 4,820.17 / 5,000.11 | 1.058311 | Uncertain |

The live-only paired reference interval is **0.960461–0.965690**. The
all-actual mean confidence against the native bracket controls the result.
The original thresholds stay in force. Both variants remain private.
Neither result establishes parity for the unchanged package. Do not repeat
the same timing to seek a narrower interval.

The proof covers 73,536 normalized interval/code/suffix combinations
and 4,228 signed magnitudes. Windows passes 52 focused tests in normal and
scalar modes per variant. The quiet VM retains exact pixel, source,
cancellation and independent-owner gates and all actuals for both frozen
drivers. The narrowed run's archive filename collided after sampling.
Recovery verifies and archives the completed records without repeating timing.
[Evidence and mirrored archives](vp8-coefficient-sign-trials-evidence.json)

### CPU frequency diagnostic and longer confirmation

One unscored live-only capture reads CPU 0's APERF and MPERF on 125 ms
intervals from CPU 1. The original four warmups and twelve 128-operation
units stay intact. Every complete decode, exact frame and owner check stays
in place. The whole-unit APERF/MPERF ranges from **1.6741 to 1.7503**.
Managed and all three FFmpeg group times correlate **−0.997 to −0.994**
with that ratio. A second alignment margin gives the same result.
Multiplying each actual's time by its counter ratio reduces the managed
coefficient of variation from 1.18% to 0.15%. Those adjusted times are
diagnostic only. This identifies frequency variation in the observed
capture without establishing its cause or extrapolating to prior captures.

One uninstrumented follow-up extends each actual from 128 to 512 operations
per subject and keeps four original 128-operation warmups. Twelve actuals,
all three reference groups, the eight-operation subdivisions and all
original correctness and quiet guards remain. The same immutable candidate
measures **4,855.68 / FFmpeg 5,032.66 µs**, at **230,880 B/op**. The adjacent
paired interval is **0.962852–0.965291** and the full interval is
**0.880090–1.057373**. The longer cadence does not resolve the uncertainty.
The candidate remains private and the production evidence stands.

Two orchestration builds, zero library, fixture or reference builds.
The archive retains all observations, commands, stages and member hashes.
[Clock and confirmation evidence](vp8-clock-attribution-evidence.json)

### Shared refill check for coefficient prefixes

This private follow-up prepares up to three coefficient decisions with one
refill check. The decision order, live cancellation checks and scalar
publication stay exact. The reservoir holds at most 53 future bits during
prefix preparation. The conservative 304-decision model bounds total reads
at 269 bytes within the existing 272-byte window guard. An independent model
also agrees on 622,592 decisions and 2,048 published cursors.

Three alternating quiet-VM pairs on the original live-token control measure
**4,970.84 → 5,057.70 µs**, interval **0.941919–1.099071**, with
**230,880 B/op** on both DLLs. The comparison adds only the refill change
to the private live-sign candidate. Both guards and original exact pixel,
source and ownership gates pass. The adverse point and overlapping interval
give no reason for independent confirmation. Production and its
qualification stay in place.
[Refill trial evidence](vp8-prefix-refill-trial-evidence.json)

### Sixteen-pixel horizontal filter

This private variant filters one horizontal luma edge in sixteen AVX2 lanes
instead of two eight-lane passes. It widens 16 input bytes without loss and
narrows with the same unsigned saturation. Thresholds, edge order,
cancellation and ownership remain exact. The eight-pixel, vertical and
portable implementations are unchanged. Thirty-five focused Windows tests
pass in each normal and scalar mode, including the whole-plane scalar oracle.

Three alternating quiet-VM pairs in each original complete operation give
ordinary opaque **4,749.99 → 4,672.63 µs**, interval **0.912060–1.060977**,
and live token **4,772.58 → 4,801.70 µs**, interval **0.805471–1.256657**.
Both allocate **230,880 B/op**. The exact frame, source, owner and quiet
guards pass. These mixed results give no basis to keep the specialized
kernel or repeat standalone timing. The production uncertainties remain.
[Filter trial evidence](vp8-horizontal-edge16-trial-evidence.json)

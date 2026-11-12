# Output-backed byte cross resizing

Width reduction with preserved/enlarged height now shares the existing full-row
prefix preparation, cache and ownership schedule. It borrows unpublished output
capacity, unwinds, then uses the measured eight-column strip kernel for the
remaining rows. Small outputs that cannot hold preparation use the strip kernel.
Horizontal capacity is statically specialized at preparation/row boundaries;
there is no runtime interface per pixel or separate copied full-row engine.

| Complete public RGBA8 Resize, extra 4 KiB | Before / current µs | Prepared zimg µs | Ratio / upper | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| 513×259 → 256×519 | 971.84 / 793.42 | 875.67 | 0.906 / 0.913 | 113 |
| 513×259 → 256×259 | 638.33 / 496.61 | 573.93 | 0.865 / 0.872 | 113 |

Both controls pass and stop. Independent savings are **178.42 µs / 18.36% raw /
17.02% normalized** and **141.71 µs / 22.20% raw / 16.65% normalized**.
Filtered, all-actual raw and normalized intervals separate; native clocks move
−1.61% / −6.66%. All original screens, twelve independent actuals and both native
brackets remain. The one-byte allocation-counter difference is not a design gain.

Complete managed preparation, filtering, certification/repair, admission,
independent output and disposal are timed. The labeled zimg backend retains its
graph/scratch; conversion, clamped halo, filtering, quantization and owned output
are timed. This is not a fresh-native-graph or FFmpeg CLI comparison. Frozen
inputs/settings, qualified managed pixels (including declared half decisions),
original native bounds, unchanged source, surviving owners and idle-v2 pass.

The prefix uses 91,168 B of already charged output on these controls. It completes
429/169 rows, then the original strip kernel writes the last 90. No new heap
capacity, retained scratch, pool, session or API. One existing 4,096 B reservation
covers sequential non-inlined prefix/tail frames, then retires before tiny-alpha
replay. Explicit arrays are 80 B prefix / 3,440 B Cross tail; enlargement's
4,000 B strip arrays stay unchanged. Physical/JIT/native peaks are unmeasured.
The result owner exists before borrowing; PrepareTail touches no completed row.
Odd-width alignment adds at most four bytes inside that owned output.

The source/count model finds 8,288→1,720 row-decode calls and 181,300→143,708
decoded source pixels on the primary control, plus 16,608→3,309 geometry reads/
preparations. Boundary overlap increases horizontal filtered pixels by 1,024.
These are preparation/work counts, not component times or speed predictions.
The same-height control reuses the existing cardinal vertical guard. General
certification, original repair/replay, ordered arithmetic and the strip body
(after start-row/PrepareTail remapping) remain unchanged.

All fourteen source-affected controls are reconciled: two Cross passes and eleven
enlargement passes / one uncertainty. HorizontalUp8's first affected capture is
1,486.10/zimg1,518.51 µs, upper1.025: uncertain, not a presumed gap or favorable
repeat. Four passing enlargement views have slower separated raw means; all raw/
native/all-actual effects remain explicit. The two largest discrepancies receive
one existing bridge parameterization: twelve alternating pairs of sixteen full
operations. VerticalUp8 ratio .99378 (.99010–.99746) improves in that scope;
TightUp16 .99965 (.99551–1.00380) is inconclusive, not proof of equivalence.
These scoped increments never replace the independent standalone native costs.

The earlier ordinary-publication implementation was 1.44% slower in paired
operations and restored. Typed cache views were inconclusive despite a favorable
historical raw capture and were restored. Their raw archives and the original
actual-package profile are dependencies below, without renderer dependencies.
Neither result bans materially different designs.

The complete root Resize caller retains all 1,620 resolved IL instructions and
the actual timed root's 9,388 B / 2,204 instruction/label lines and block offsets,
normalizing only process addresses and method-number prefixes. No whole-library
audit. Other 59 material resize costs remain source-valid: 53 base, five generated
and one DeepWord. Across 73 standalone controls: **60 pass / 5 gaps / 8 uncertain**;
scoped adjacent views stay separate. The five generated passes remain stopped.

Windows/Linux each pass **6,497 full/live checks** (3,168 internal / 2,381 public /
948 live), 569 internal / 791 public per normal/scalar resize mode and all three
named storage suites. Ten new cases cover prefix thresholds, odd alignment,
partial strips, rational/frozen pixels, poison output, low/full alpha, terminal
refusal, cancellation cleanup and recovery. Unchanged disabled-ISA/other-family
proof is reused after unchanged guards. The SAME actual Windows Release-package
consumer explicitly tests both frozen byte Cross controls, aggregate admission,
cancellation and independent survivors; all three usage examples pass both OSes.
One core/embedded build each OS, no frozen-harness/native/fixture/public build,
one diagnostic bridge build and one pack. Published releases remain immutable.

[Evidence](cross-bicubic-output-prefix-evidence.json) retains every result,
source/count/ownership proof, diagnostics and reproduction. Raw `cross-bicubic-output-prefix-qualified.tar.xz`,
SHA256 `287d74ff255a4c51efb454f0e39a11f9dd6fef6fc23ae8be7e0b62d7cc7f6239`
(1,100,044 B / 326 members), is mirrored/member-verified with
retained dependencies. Private development 0.2.1 package (455,507 B), SHA256
`a5e9e6a1fe75b716e8619ef3f451a48ba55b1f9063d92c2bea1b44483504156c`, contains the measured Windows DLL.

Next attribute the existing stored-word PNG post-GC four-operation gap (~96.49 µs)
on this actual package, retaining its separate passing natural-GC64-operation
scope. Remaining resize gaps include byte16×8 (~59.46 µs) and byte8×17 (~39.95 µs).
Resolve uncertainty before tuning; all 289 paths remain required and publication
is postponed. This round is not a release-wide performance refresh.

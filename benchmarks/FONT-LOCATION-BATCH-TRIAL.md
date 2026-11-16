# Batched font-location validation trial

**Rejected.** An isolated AVX2 prototype checks eight long-loca ranges/orderings
at once, then performs the original ordered eager header validation. Short loca,
scalar/tail/error paths, signed bounds, mapping, every glyph cancellation poll,
independent copy and owner cleanup remain. Invalid chunks return to the original
scan so a later bad location cannot hide an earlier bad header. No array, cache,
pool, parser-policy or public API change is proposed for adoption.

| Complete owned font load/dispose | Qualified / prototype µs | Interpretation |
| --- | ---: | --- |
| Three-actual short screen | 18.665 / 17.947 | Promising point; overlapping intervals |
| Independent twelve-actual captures, all actuals | 19.028 / 17.991 | Raw 1.037 µs gain conflicts with native clock movement |
| Twelve alternating 16,384-operation pairs | 17.616 / 17.901 | Prototype slower; ratio 1.016181, interval 1.012632–1.019742 |

FreeType 2.13.2 moves 14.345→13.142 µs across the independent captures (-8.39%).
The reference-normalized direction is adverse despite separated raw managed
intervals. The targeted alternating comparison resolves this contradiction before
any production promotion. It uses the original compiled `WarmFontLoad.Managed`
public operation, including copying, admission, all eager validation, glyph-count
consumption and disposal. Both processes warm 16,384 operations; one CPU0 operation
runs at a time under natural nonconcurrent GC. Two clock reads per batch, IPC,
allocation readings and original correctness-only reference checks stay outside.
Every pair/actual, native bracket and quiet guard is retained. These pairs identify
an incremental DLL effect; they do not replace the standalone reference gate.

The frozen face remains DejaVu Sans 2.37 Regular, 757,076 B / 6,253 glyphs.
FreeType copy/open/retain/dispose validates lazily, whereas managed loading checks
mapping/glyph headers eagerly. No parser equivalence or overall text slowdown is
inferred. Allocation stays 176 B/op; added active/retained capacity is zero. Active
font storage remains 757,076 B plus a coarse 256 B descriptor, with 757,076 B idle
payload in this default serial workload. Physical/native/JIT peaks are unmeasured.

Both DLLs pass 8,192 original-order validation cases each in normal/scalar modes:
short/long locations, 8-entry boundaries, signed/zero/empty/truncated headers,
unaligned bytes, large/reversed locations and conflicting earlier/later failures.
Public owned-source independence, refusal/cancellation cleanup, borrowed-root
charges and subsequent successful loads also pass. Actual JIT emits the intended
shuffle/comparisons; long-scan code grows 506→1,014 B. Code size/instructions are
not an attribution of elapsed time; instrumented clocks are discarded.

The paired run completed all samples, cleanup and its final guard before summary
code sought a statistics file that existed only locally. The record was resumed
from all twelve preserved wire pairs with the original parser's exact critical
value: zero repeated operations or timing captures. One isolated Linux core build,
one successful Windows oracle build (one prior compile failure before output) and
one small pair-driver build; zero canonical/embedded/public/harness/reference/
fixture/package builds. No complete-text retiming or broad campaign follows.

Qualified source, modules and package never change. Reuse Windows/Linux 6,750
full/live checks each, 268 internal/166 public focused normal/scalar checks and
the SAME actual Release-package consumer/all three examples. Unchanged
[font-loading costs](FONT-PACKED-BOUNDS.md) and
[complete public text passes](ORDINARY-WORD-TEXT-DISPATCH.md) remain current.
Further font work needs a distinct attributable hypothesis; remaining copy work
proceeds independently. Full 289-path parity and publication remain open.

[Evidence](font-location-batch-trial-evidence.json) retains both implementations,
original workers, all captures, oracle/source proof, code generation and unchanged
qualification. Mirrored/member-verified `font-location-batch-trial.tar.xz`:
SHA256 `4ea6495ce5130e75d45b9f7929aa31bb6c345fc88d8d34a01ebfb78998336d0d`, 19162540 B / 201 members. Archived `flb1146-*`
recipes and pinned dependencies reproduce the bounded study. The renderer does
not depend on this rejected trial.

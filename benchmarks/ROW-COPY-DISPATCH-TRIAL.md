# Row-copy dispatch trial

**Not retained.** Selecting grouped/BCL copying before traversal, through two
literal arguments to one force-inlined source loop, shows no useful complete
byte-copy gain. Twelve alternating 8,192-call pairs give **1.983879
to 1.983887 µs**, ratio interval **0.997982–1.002031**,
**0 B/op** on both sides. This is inconclusive, not a regression or FFmpeg parity.

The source-valid prior generated caller retains kernel selection inside each
chunk. The trial moves that invariant choice outside the maintained traversal;
the JIT is asked to specialize its true/false call sites. No new generated-code
capture proves whether that happened or attributes the null effect to code size,
register allocation or another cause. Stop without confirmation, neighboring
variants, other cases, native timing or broad/package qualification.

One shared source loop preserves exact byte/word samples, clipping, forward/backward
alias traversal, prepared row offsets, 4,096-pixel cancellation chunks and every
public validation/admission boundary. All grouped/BCL kernel bodies are unchanged.
No interface, delegate, generic pixel abstraction, cache, buffer, owner or API.
No added admission or retained storage; physical/native/JIT peaks are unmeasured.

Linux passes **25 internal/190 public** normal/scalar checks, including hidden RGB,
clipped/wide/overlapping/multi-chunk copies, cancellation, refusal and recovery.
Exact qualified source/core/embedded/public modules and source inventory are
restored without rebuilding. Windows modules and actual package never change.
Reuse **6,721 full/live checks per OS** and SAME actual-package/three-example proof.
One Linux core/embedded build, zero other builds. All actuals and quiet guards remain.

[Current copies](DRAWING-ROW-PREPARATION.md) and
[word-copy uncertainty](RECTANGLE-COPY-ADJACENT.md) stay unchanged.
[Evidence](row-copy-dispatch-trial-evidence.json) pins the trial and restoration.
Raw `row-copy-dispatch-trial.tar.xz` (1,753,468 B/58 members), SHA256
`677183f06b9d067eb56f825b4fe1769feb24a5ae902c6fe7575f0bc36844af45`, is mirrored/member-verified, with qualified baseline and
attribution dependencies retained. No current-cost renderer dependency or
permanent strategy ban. The full 289-path objective remains open; publication
stays postponed. Require a materially revised design or new attribution before
another copy-scheduling trial rather than repeating this screen.

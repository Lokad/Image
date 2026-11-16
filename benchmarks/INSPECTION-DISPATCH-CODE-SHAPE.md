# Inspection dispatch code shape

Read-only follow-up to the [rejected format-dispatch trial](INSPECTION-FORMAT-DISPATCH-TRIAL.md).
Both archived immutable DLLs run the original standalone async-JPEG harness for
generated-code inspection only. Every instrumented elapsed observation is discarded.

The old 90 B entry plus 3,452 B compiler dispatcher become a 507 B entry and
218 B post-prefix dispatch. The outer owner (733 B) and header reader (1,977 B)
have identical instructions after address normalization. Removing an async layer
does not by itself establish a performance gain or explain the adverse standalone result.

Retained standalone allocation diagnostics show 568 B/operation, no Gen1/Gen2
collections and similar Gen0 counts per operation on both subjects. Counts do
not establish equal GC time or exclude runtime/code-layout sensitivity. The
paired/standalone contradiction remains unresolved; the candidate stays rejected.

Original descriptor, prefix, reference and ownership gates pass; both idle guards
pass for each subject. There are no builds, new statistical timings, production
changes or qualification/package repeats. Source, modules and package remain
the qualified baseline. The current-cost renderer does not depend on this audit.

[Evidence](inspection-dispatch-code-shape-evidence.json) pins selected methods,
instruction normalization and original GC observations. Raw `inspection-dispatch-code-shape.tar.xz`
(54,440 B / 24 members), SHA256 `977178ef4f322e139f99646974613c8544ab4adc2014d02995d88d624e0dcc85`, is mirrored and
member-verified with the original trial dependency. Future dispatch work needs
new attributable evidence or a materially revised design; no favorable replay.

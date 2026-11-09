# Packed byte bicubic: current cost and preparation model

TDR647 confirms the existing 513×259 → 64×32 RGBA8 bicubic operation on the
qualified actual package DLL: **544.63 / zimg 373.78 µs**, ratio **1.457**,
conservative interval **1.338–1.499**. The remaining gap is 170.85 µs.
All twelve actuals and both native brackets pass idle-v2 guards. Older
545.55/370.06 µs clocks establish no causal gain. No production change, build,
new fixture, profile, settings grid or qualification repetition occurs.

This is a complete public resize, including normalization, preparation, working
storage, independently owned output and disposal. The native worker retains its
documented zimg graph and scratch; it is a prepared backend comparison, rather
than fresh graph construction. Exact rational/frozen managed frames, original
native RGB/alpha max1/RMSE≤0.25, transparent-pixel RGB and source/ownership checks
pass. Q30/rational/certified rounding requirements remain unchanged.

Allocation is now **113 B/op**. The older 8,329 B observation predates
commit d7fcbd41's complete-overwrite filtered-resize allocation and cannot describe
current storage. The local ceiling remains 543,756 B = 531,468 source + 8,192
owned result + 4,096 workspace; the existing kernel's stack payload is 3,992 B.
The result stays independently owned through disposal. No added capacity or
retained state is introduced. Existing idle pool capacity and physical/native
peaks are not newly measured; allocation is not a peak measurement.

The two-column, 36-slot kernel rebuilds vertical geometry across strips.
A count-only model observes **33,152 vertical weight evaluations**, 8,288 cached
source-row visits and 325,822 decoded source pixels. One illustrative result-tail
row-batch design cuts geometry evaluations to 3,020 but resets those row caches:
12,960 row visits and 509,490 decoded pixels, **56.37% more**. This is a lifetime
and work model, not CPU attribution, a selected design or proof of a regression.
The next step must attribute that tradeoff or revise scheduling before a
production experiment. Shared geometry preparation should simplify the wider
column families while retaining their measured kernels and portable fallbacks.

The timed Windows DLL70bb690a is the actual private Release 0.2.1 package
3507899d. [Existing qualification](BYTE-COMPOSITE-EXACT-VECTOR.md) supplies 5,975
full/live checks, relevant scalar/ISA checks and the same package's consumers
and all three examples on each OS. No resize source changed in that stage.
The first observer omitted this existing case from its whitelist; the completed
measurements were processed after correcting it, with no timing replay.

[Structured evidence](packed-byte-bicubic-current-package-evidence.json) pins
the sources, complete boundary, exact output, model and reused qualification.
Mirrored/member-verified raw packed-byte-bicubic-current-package.tar.xz contains
6,142,340 bytes / 118 members, SHA-256
034f9177d4e230ebbe8d34d7ac4465739e04d5f1b509d0346f80314033cd2f9c.
It retains immutable runtime, receipts, guards, scripts and count model, with the
qualified actual-package archive pinned as a dependency. Full 283-path parity
remains open; publication is postponed.

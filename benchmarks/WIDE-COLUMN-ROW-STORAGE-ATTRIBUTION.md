# Wide-column row storage attribution

WCA760 reuses the qualified actual-package DLL for one two-second CPU0 capture,
with sampling onCPU1. No production/test change, build, new workload or timing
confirmation; all instrumented elapsed is discarded. The source-valid complete
513x259->128x64 RGBA8/4KiB cost remains419.13/zimg349.29us, upper1.209,113Bop:
about69.84us remains. Wide-column, geometry and shared-math sources are unchanged.
Current6161full/live/scalar/ISA/SAME-package qualification is reused. Production
8e26f314 CI42800600405 is green in all three jobs.

The current four-column byte body is5936B, with1775of1998sample locations;
geometry Build/Read/Prepare has107/11/1, Kernel62 and Stride16. Indexed decode,
filtering, row lookups and publication remain visible. These are inlining/skid
locations, not phase times, removable percentages or a speedup forecast.
Frozen rational/native/source/ownership and idle-v2 gates pass.

## Proposed result-row loan

An eligible byte four-column operation could process its first42output rows in
sixteen-column strips. Borrow8704Bcache and2176Bweights from the final22unpublished
result rows (11264B capacity). Other working data needs3296B explicit stack under
the existing4096B admission. Top writes stop before the loan starts. Return that
frame before restoring metadata only in the unpublished tail, then compute the
tail with the original four-column workspace. Every result pixel is materialized.

| Primary logical work | Existing | Proposed |
| --- | ---: | ---: |
| Row visits | 2048 | 1040 |
| Source-row conversions | 8288 | 4448 |
| Decoded source pixels | 229474 | 189418 |
| Geometry builds, including preparation/restoration | 704 | 390 |
| Horizontal column-rows | 33152 | 34688 |

The wide17-tap footer is152B and its prepared prefix is88columns. The initial
model used the96B narrow footer; its448->260build count is superseded. Timing,
loan sizes and other counters are unchanged; the original cited raw is retained.

The extra1536horizontal column-rows are adverse work at the phase boundary.
Counters prove neither elapsed-time gain nor memory-bandwidth reduction. The
existing528x264->128x64 exact4.125 functional boundary has the same loan layout
and source-window bound. Existing odd/small/three-column controls retain fallback;
word kernels remain original. No new benchmark image/settings grid.

No array, capacity, owner, pool, session, public API or budget model is added.
The independently owned32768B result is already admitted; its storage is borrowed
only before publication. Cache/weight readers must retire before metadata restore
or tail writes. Restoration and repeated filtering stay inside complete timing.
Preserve original coordinates/coefficients/order, exact repair/tiny-alpha unwind,
terminal refusal, cancellation and independent output lifetime. No numerical change.

Next implement closed loan/tail modes in the existing shared executor and a
bounded tail-geometry preparation hook. Qualify actual bounds, canaries, loan
retirement/restoration, source/kernel bits, word erasure, zero new allocation,
aggregate admission/refusal, cancellation, survivors and recovery. Then inspect
code and run the existing TightWide8 short screen, with contemporary original and
independent confirmation only if useful. Reconcile word costs only if affected.
Broad Windows/Linux/scalar/ISA/SAME-package qualification belongs at a coherent
accepted stage. No general cache framework or extra pixel-loop abstraction.

[Evidence](wide-column-row-storage-attribution-evidence.json) pins source identity,
profile annotations, bounded ownership/address model and reused qualification.
Mirrored/member-verified raw `wide-column-row-storage-corrected-attribution.tar.xz`:
13924B/7members, SHA256
`a10ff7204363d0e42764e7de5bf21a32d2e775757258dc6bfff14ee8987e062b`. The
[shared publisher](BICUBIC-SHARED-BYTE-PUBLICATION.md) owns current wide costs;
[paired preparation](BICUBIC-PAIRED-COEFFICIENT-PREPARATION.md) owns the newer
narrow-byte gain. This attribution adds no renderer dependency or current-cost
change. All283paths remain required; publication stays postponed.

The subsequent [result-row implementation](BICUBIC-RESULT-ROW-LOAN.md) owns newer
wide-byte/word costs and qualification. The attribution above describes its
preimplementation checkpoint; its model/profile and original raw remain retained.

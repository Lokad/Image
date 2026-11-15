# Prepared text attribution

TWA1038 is a read-only profile of the current qualified Linux DLL `548befe0`;
production, builds, fixtures, current costs and package `026a6f23` are unchanged.
The [complete public pipelines](FONT-HEADER-VALIDATION.md) remain the end-to-end
comparison: one-shot and preparation-plus-eight-draw both pass. Loading and
prepared drawing have different validation/retained state across references;
neither this profile nor a sum of historical stages proves an overall slowdown.

The existing continuous public-API runner performed 27,245 prepared RGBA16 draws
on the frozen DejaVu Sans / LatinKerning control. Every draw resets its borrowed
canvas and checks every resulting sample. Existing layout, visual, full-word,
ownership and reference checks are retained. CPU0 application / CPU1 sampler,
the versioned quiet guard and the existing optimized single-CPU runtime were used.
No elapsed-time comparison is accepted from this instrumented run.

Of 1,996 sample locations, 687 locate Band, 200 Area, 171 polygon clipping,
90 Edge.X, 90 Draw, 73 InteriorRun and 69 sorting. Reset and validation locations
remain visible. Inlining, sampling skid and unresolved locations prevent treating
these counts as phase times, removable percentages or a speed forecast. The actual
module's scalar Edge.X has four separately rounded arithmetic operations;
Band calls it four times per trapezoid.

The first, insufficient capture is retained: the existing BDN reset-per-operation
selector invokes drawing once per iteration and finished before useful sampling.
It supplied only 18 locations. Its exact process exit codes were not recorded and
remain unknown. Terminal state was checked before the continuous runner was used;
no timing was repeated to obtain a favorable result.

The earlier [row-composition study](TEXT-ROW-COMPOSITION-INCONCLUSIVE.md) and
[geometry attribution](TEXT-CURRENT-GEOMETRY-ATTRIBUTION.md) remain relevant.
Defer replaying touched-range tracking. A bounded lead is paired edge endpoints
with each coordinate's scalar arithmetic order, portable fallback, unchanged
polygon order, checkpoints and owned storage. Resolve equivalent per-draw timing
boundaries before assuming that lead improves performance; no candidate is accepted.

[Machine-readable evidence](text-prepared-current-attribution-evidence.json)
identifies sources, reused qualification and reproduction. Raw
`tmp/text-prepared-current-attribution.tar.xz` is mirrored and all 88 members verified:
5,186,608 B, SHA-256 `bf5cff94d543c2906d0fb47fcab6ec5f42a3042c876cc1cb894cebe2f80b6282`.

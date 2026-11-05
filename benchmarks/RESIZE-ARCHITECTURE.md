# Resize preparation and execution boundaries

Source audit at `80e32dfa4ce91a5a75c406dae6115831069e1e0a`; no production change,
build, timing run or new qualification. Current costs and confidence remain in
[coverage](WARM-COVERAGE.md). This audit inventories the 22 bicubic implementation
files (5,010 lines), including three files that supply kernels rather than drivers.
Helpers such as `BicubicMath` and geometry views are separate.

| Existing family / files | Distinct execution or storage boundary |
| --- | --- |
| Cached, 2 | Shared byte/word plans, admitted row products/cache; support through 2,048. |
| Small column, 2 | 6/8-column strips, support ≤10; shared vertical geometry in unpublished result tails. |
| Wide column, 2 | Byte 3/4-column and word 3-column strips, support ≤20, different cache layouts within 4 KiB. |
| Packed column, 2 | Two-column strips; byte support ≤36 with UInt16 products, word ≤34 with its own product layout. |
| Deep column, 2 | One-column row cache, support ≤72; byte split reduction differs from word ordered additions. |
| Streaming, 4 | Two shared byte/word schedulers and four typed row/output kernels, supports through 304/544. |
| Generated, 2 | Larger supports generate geometry while executing instead of retaining full support arrays. |
| Mixed, 2 | Horizontal enlargement / vertical reduction; shared geometry view, distinct depth kernels. |
| Enlarging, 2 | Enlargement strips, with deferred exact repair after stack storage unwinds. |
| Cross, 2 | Horizontal reduction / vertical enlargement; distinct row scheduling and repair. |

The portable prepared/direct paths remain required. Optimized paths reserve before
stack/heap use, release before exact tiny-alpha replay where necessary, and never
catch shared-admission refusal to retry. Result-tail geometry is borrowed only
while those unpublished bytes cannot be overwritten. Output disposal and
surviving independent owners remain outside these temporary views.

Small byte/word column paths duplicate strip/row scheduling, but share preparation
already. Their measured filters differ (paired 512-bit byte versus 256-bit word),
as do reciprocal/full-word certificates, tail handling and replay guards. A shared
driver could remove source duplication without removing substantial executed
work; it must not replace these kernels or introduce depth dispatch inside tap
loops. There is no new speed claim or immediate blanket merge from this audit.
Existing [shared streaming](BICUBIC-SHARED-STREAMING.md) and
[word column preparation](BICUBIC-WORD-COLUMN-PREPARATION.md) retain useful designs.

Lanczos already shares one scheduler, admitted Q30 axes, row storage and ownership
between one-shot and retained-plan calls. Fast execution first forms exact Int64
horizontal rows, converts them in place to double, then uses certified FMA output
with exact repair. The native zimg worker uses float intermediates. Those are
different numerical contracts; neither consolidation nor a benchmark approval
relaxes the managed Q30/rational result.

A concrete remaining preparation lead is the repeated widening/permutation of
the same four Q30 coefficients for every horizontal source row. For the frozen
513→256 axis, exact support geometry gives 3,060 taps and 760 four-tap groups per
row; 259 rows would repeat 196,840 such preparations. These are geometry counts,
not measured phase times or a savings estimate. Both byte/word filters use them.
The earlier [execution attribution](RESIZE-CURRENT-EXECUTION-ATTRIBUTION.md) is
historical; its sample fractions are not current costs.

A bounded prototype may prepare coefficient lanes once in the existing axis/
workspace ownership, retain actual specialized kernels and compare the two
already-required prepared reduction controls. A straightforward packed layout
would add about 97,952 B for reduction; that is a material retained-memory cost,
not an allocation saving. Account for preparation, retained capacity, admission
refusal and complete one-shot execution before acceptance. Do not add it to a
passing enlargement merely to improve a fixed fixture. Avoid a general cache,
new numerical policy or fixture-dependent geometry. If sharing alone is neutral,
retain useful simplification only when the coherent completed design justifies
its costs; otherwise select an independently attributed codec task.

The [revised VP8L stage](VP8L-FUSED-PREPARED-COUNTING.md) already reassessed and
accepted fused body counting and SIMD/scalar histogram addition. Both mechanisms
remain in current production. Their earlier rejection thresholds are historical,
so replaying those unchanged implementations would add no new work.

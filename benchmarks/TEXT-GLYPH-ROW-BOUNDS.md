# Glyph-bounded ordinary text rows

[Fixed warm confidence](ORDINARY-TEXT-WARM-CONFIDENCE.md) supersedes the two ordinary drawing costs below; this report retains the accepted production change and unchanged complete integrations.

Ordinary drawing reuses existing conservative computed edge-X envelopes to clear
and round only the current glyph's columns, and borrows each destination row once.
Every band still executes; nonfinite envelopes retain full-row scans and empty
horizontal rows retain a direct cancellation checkpoint. Geometry, coverage and
blend arithmetic, ordering and full-row prepared capture remain unchanged.
Prepared drawing's pixel loop and font loading are unchanged.

Two doubles add 16 bytes per unique operation-private shape, within its existing
512-byte pre-allocation descriptor allowance. There is no new array, pool, cache,
owner or retained mask. This is a revised design, distinct from earlier per-band
touched-range tracking and blend-arithmetic trials.

Final independent complete ordinary16 pairs measure **53.114→50.660 µs**, **4.62% / 2.454 µs** saved, interval **0.938097–0.969755**. All-actual mean intervals separate. Twelve alternating 1,024-operation batches use natural nonconcurrent GC and one CPU0 operation; reset, validation and allocation reads are outside clocks. Prototype screen/confirmation and the final cancellation revision's observations are retained separately. This proves a scoped DLL effect.

| Public boundary / ImageSharp.Drawing 2.1.7, Fonts 2.1.3, ImageSharp 3.1.12 | Managed / reference µs | Upper / status | Managed GC B/op |
| --- | ---: | --- | ---: |
| Ordinary8 | 48.308 / 44.918 | 1.208 / uncertain | 83,216 |
| Ordinary16 | 51.825 / 46.034 | 1.244 / uncertain | 83,216 |
| Complete OneShot | 1151.490 / 6195.031 | 0.205 / pass | 306,168.00 |
| Complete PrepareDraw8 | 1249.351 / 6867.824 | 0.210 / pass | 315,892.17 |

Contemporary standalone ordinary16 baseline/candidate points are 55.445/51.825 µs;
they reconcile the controlled direction. Both ordinary depths remain uncertain,
not passing parity. Complete integrations include all 30 fitting trials and every
public layout call, fresh owned explicitly cleared RGBA16 canvases, independent
outputs and disposal. Preparation 1117.577/6132.818 µs is diagnostic; directly timed
eight-draw amortization 156.169/858.478 µs includes preparation. Retained working
state, eager/lazy font validation and GC-driven reference font release differ.
These are complete public-API integrations, never FFmpeg or bare shaping costs.
Historical stage timings are not added or used to claim overall text slowdown.

The bounded existing Arabic control measures 420.483→396.049 µs with separated
intervals; GC 590,552→590,824 B/op. Latin GC rises by 80 B to 83,216 B/op. Prepared
execution stays 56 B/op; retained Latin/fit capacities 2,112/4,160 B and governed
fit peak 822,460 B are unchanged. Preparation allocates 314,545.25 B. Approximate
full-GC prepared heap deltas 4,336/22,504 B exclude idle pools. Allocations, admitted
peak and retained capacity are distinct; physical/native/JIT peaks are unmeasured.

Forty-eight new sparse/clipped/overlapping canvas comparisons and all eight frozen
full-word frames pass, with exact managed pixels and unchanged visual/layout gates.
Each OS passes 6,745 full/live checks plus 268 internal/165 public focused checks
in normal/AVX-disabled/scalar modes. The same actual Release package and all three
consumer examples pass both OSs. Prototype and final revision each require a
core/embedded build per OS; public tests build once for the new cases. No harness,
reference or fixture builds; one NoBuild pack and no accepted-DLL rebuild after timing.

[Evidence](text-glyph-row-bounds-evidence.json) pins all producers and actuals. Package `924e81be4bb1ab477c705f54ff0e851d6552b26f3002983be360220652392b7c`, timed DLL `aa4a59ca8fe2ea623126d4b3a97793aec46625a59455da56d74a0f89e7a15d26`. Raw `text-glyph-row-bounds-qualified.tar.xz` (5,312,592 B/265 members), SHA256 `fe37d7b79882d4918770ea95d48717d5d0e5f74589011dc1f04abc26f59b123e`, is mirrored/member-verified. Archived gbc1120/gbc1120b recipes reproduce the bounded experiment with retained reference dependencies; captures are never restarted. [Previous clipping evidence](TEXT-FINITE-HORIZONTAL-CLIPPING.md) remains historical for affected ordinary/integration costs; unchanged font/prepared execution and codec/resize paths reuse their evidence. Publication stays postponed and published releases immutable. Full 289-path parity remains open.

# Ordinary text: longer integration

The two existing ordinary public-draw controls now establish **small scoped
gaps**, resolving their earlier uncertainty. Production and the qualified
package are unchanged; this is a measurement update, not a speedup.

| Borrowed 384×160 canvas / retained font and layout | Lokad / ImageSharp µs | Conservative interval | Lokad GC B/op |
| --- | ---: | ---: | ---: |
| RGBA8 ordinary draw | 46.510231 / 44.790422 | 1.025100–1.046179, gap | 83,216 |
| RGBA16 ordinary draw | 47.201747 / 46.256201 | 1.002524–1.027033, gap | 83,216 |

Absolute point gaps are **1.720 /0.946 µs**. One prespecified integration increase
from 4,096 to **16,384 draws per subject** lengthens roughly 200 ms samples to
800 ms. Both sides warm the same count before all statistical groups. The
existing worker already alternated managed/reference batches; a new pairing
scheme was unnecessary. Keep all twelve actuals and all three reference groups,
with the original worse-of-all-actual/paired confidence rule. No favorable replay.

The original public operations, fonts, inputs, full byte/word frames, glyph/layout/
visual gates and stock **ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 / ImageSharp 3.1.12**
identities remain. Both sides reset the borrowed canvas before EVERY operation,
outside its two clock reads. Every draw's full-frame validation and allocation
reads remain outside timing. This scoped prepared-layout drawing boundary does
not substitute for the separately passing complete one-shot and eight-draw
public integrations, nor establish overall font/text processing performance.

Managed public allocation remains **83,216 B/op** plus **24 B** of the existing
timed adapter. Reference allocation is unmeasured. Added owned/retained storage is
zero; caller canvas/font/layout storage and physical/native/JIT peaks are separate.
Outside-clock managed batch counters show 81–82 generation-0 collections per
16,384 operations; byte samples also have 11–12 generation-1 collections, word
samples zero, and neither has generation-2 collections. These are diagnostics,
not pause attribution or grounds to remove actuals.

Only the outer driver's fixed count, description and outside-batch GC counters
changed. Reversing those edits restores its original source exactly. One Windows
driver build; zero production/embedded/public/original-harness/reference/fixture/
package builds. Four normal/scalar original controls pass before timing. All
quiet-VM guards pass. An earlier source-record preflight failed before samples
because a committed documentation overlay had not reached its manifest; reconcile
that record only, without rebuilding or discarding statistical measurements.

The same immutable Linux DLL `fd31bf26` and actual package `1a1c30bc` retain
[6,760 full/live checks per OS and the SAME package/all-three-example proof](COMPOSITION-STORAGE-REUSE.md).
No new broad qualification is needed for unchanged production. The earlier
[word-dispatch gain](ORDINARY-WORD-TEXT-DISPATCH.md) and all uncertain/adverse
captures remain; historical clocks are not summed or reclassified.

[Evidence](ordinary-text-long-integration-evidence.json) pins cases, identities,
original gates and reused qualification. Mirrored/member-verified raw
`ordinary-text-long-integration.tar.xz`: SHA256
`aec8ddf8c7720b4aa106ade3b0e8f9d34fac3fdb07d1c372a988d0f7c7ddeafb`,
5,674,800 B /85 members. Archived `oti1152-*` recipes and pinned dependencies
reproduce the count-only comparison. The finite 289-path objective and publication
remain open. Stop unchanged captures; attribute material work/allocation before
another design. Font loading and remaining copy/owned-result paths remain
independent priorities, ranked by absolute savings and coverage.

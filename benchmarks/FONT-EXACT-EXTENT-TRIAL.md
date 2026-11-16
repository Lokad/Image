# Exact font-location extent trial

**Not adopted.** The already validated declared loca extent becomes an exact
borrowed typed span. Its Length bounds both the loop and indexed reads. Entries,
error order, eager header/mapping checks and every glyph cancellation poll remain.
No new allocation, capacity, owner, cache, pool, API or parser policy.

The current qualified DLL first receives one2second CPU0 profile with a CPU1
sampler:1992samples,1078managed,1003in glyph validation,898in libc. These are
instruction locations, not phase times or predicted savings; instrumented elapsed
is discarded. Six existing mapping/metrics controls provide current generated code
without rebuilding. Current uint/ushort scans are537/553B; scalar coordinate
extraction and a separate per-location span bound remain visible.

The prototype removes that repeated range check. Scan code becomes543/559B
because an exact-extent check is added once. It does not establish a CPU gain.

| Three-actual complete owned font load/dispose | Managed / FreeType µs | Managed GC B/op |
| --- | ---: | ---: |
| Qualified producer | 19.820619 / 13.697535 | 176 |
| Exact borrowed location extent | 19.724792 / 13.261023 | 176 |

The0.48%/0.096µs point is inconclusive. All actuals/native brackets remain;
no independent confirmation or favorable replay follows. Reference is explicitly
FreeType2.13.2copy/open/retain/dispose on frozen DejaVuSans2.37Regular, with eager
managed versus lazy native validation. This is a scoped diagnostic, not parser
equivalence or an overall text-processing slowdown. Complete one-shot/eight-draw
public comparisons remain qualified passes after restoration; stages are not added.

Font owned storage757076B plus256B descriptor allowance and default idle storage
are unchanged. Retained capacity is separate from allocation; physical/native/JIT
peaks remain unmeasured. Linux266internal/163public normal/scalar checks pass,
including existing malformed font, cancellation, admission and surviving-owner
behavior. One core/embedded build; zero other builds or new controls/settings.

Qualified source/tests/core/embedded/public modules and source inventory restore
exactly, without rebuilding. Windows modules/package never change. Reuse6730
full/live checks perOS and SAMEactualpackage/allthreeexamples. No broad/package
campaign or accepted performance change follows this small inconclusive result.

[Evidence](font-exact-extent-trial-evidence.json) pins profile/code-shape/screen
and restoration. Raw `font-exact-extent-trial.tar.xz` (1,909,136B/106members), SHA256
`7d36bb63ddebb25da95b86f48d5c962fc2ecbf01506e3f2caec25756b9bad024`, is mirrored/member-verified, including the perf trace, JIT output,
all timing observations and reproducible original tooling dependencies.
The renderer does not depend on this trial. Publication/full289completion stay open.

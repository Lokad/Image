# Compact cached bicubic geometry: unselected

This bounded round is closed without a production change. Smaller horizontal
geometry reduced declared wide workspace **198,432→178,464 B** (19,968 B),
independent of the unchanged power-of-two row cache. The completed designs did
not establish a worthwhile combined CPU/storage result. Current accepted costs
remain [cached-wide quartets](CACHED-WIDE-SHARED-QUARTETS.md) and
[narrow publication](CACHED-NARROW-SHARED-PUBLICATION.md).

| Complete operation / immutable variant | Lokad / zimg µs | Ratio / upper | GC B/op | Observation |
| --- | ---: | ---: | ---: | --- |
| predecessor / Wide8 | 374.60 / 347.52 | 1.078 / 1.086 | 198,451 | gap |
| predecessor / Horizontal16 | 553.68 / 594.43 | 0.931 / 0.970 | 199,933 | pass |
| predecessor / Vertical16 | 442.26 / 588.44 | 0.752 / 0.760 | 367,597 | pass |
| A / Wide8 | 363.15 / 348.33 | 1.043 / 1.054 | 178,476 | gap |
| A / Horizontal16 | 576.34 / 584.51 | 0.986 / 1.003 | 199,930 | uncertain |
| A / Vertical16 | 466.29 / 592.71 | 0.787 / 0.821 | 367,597 | pass |
| B / Wide8 | 394.99 / 376.81 | 1.048 / 1.061 | 178,477 | gap |
| B / Horizontal16 | 556.32 / 588.97 | 0.945 / 1.020 | 199,931 | uncertain |
| B / Vertical16 | 479.52 / 594.91 | 0.806 / 0.819 | 367,596 | pass |
| C / Wide8 | 384.24 / 349.14 | 1.101 / 1.135 | 178,474 | uncertain |
| C / Horizontal16 | 554.58 / 579.60 | 0.957 / 0.963 | 199,931 | pass |
| C / Vertical16 | 444.67 / 584.74 | 0.760 / 0.771 | 367,597 | pass |

The predecessor is contemporary unchanged production. A saves 11.45 µs/3.06%
raw/3.28% normalized on Wide8, with separated filtered/all-actual/normalized
intervals. Horizontal/vertical word raw costs rise 4.09%/5.43%; normalized
intervals overlap. B uses two compiled domains from one source scheduler, but
vertical word rises 8.42% raw/7.24% normalized; normalized intervals still overlap.
Its primary raw rise tracks a large native-clock rise. These are adverse
observations, not proven normalized regressions or acceptable overall gains.

C restores the original Workspace layout and one compiled scheduler, deriving
geometry extent from its owned array. Large nonwide raw adverse results disappear,
but independent Wide8 is 384.24 versus predecessor 374.60 µs: no useful CPU gain.
Its promising 361.11 µs short screen does not supersede that confirmation. No
successful job is repeated for favorable bounds. C stops after these three
confirmations; the other 21 affected costs and broad/package checks are not run.
A and B each retain all 24 existing affected complete costs, not new workloads.

Source inspection also found A/B could lose the wide pixel-offset convention
when only vertical support is wide. C preserves stride>16 and qualifies that
boundary. A/B are not claimed generally correct. All measured controls retain
exact managed pixels and original native gates, input/reference identities,
ownership/disposal and CPU0 idle-v2 guards. Ordinary native zimg retains its
graph/scratch; explicit one-shot/prepared comparisons keep equivalent boundaries.

Total GC, admitted capacity and retention remain distinct. Candidate primary GC
178,474–178,477 B/op and declared retained workspace 178,464 B are unselected;
the current production workspace remains 198,432 B. Physical/native/JIT peaks
are unmeasured. No owner, pool or admission model is introduced.

Production sources, DLLs and the actual package are restored exactly on both
OSes without rebuilding production. Ten retained public-plan cases verify
normal/low-alpha rational pixels, stack/heap row metadata, cancellation/recovery,
borrowed inputs/root and independent outputs surviving plan/source disposal.
Rebuilt embedded tests pass **452 resize/admission checks per normal/scalar/
AVX2-off/AVX512-off mode on each OS**, including all three named storage suites.
Earlier 6,351 full/live checks and SAME actual Release-package/all three examples
remain valid for unchanged production. No new full-suite count is claimed.

Three attempted core/embedded versions per OS, attempted sources, code generation,
all screens/confirmations, preflight/binding failures and zero-actual prepared-data
guard failure are retained. The isolated prepared-data copy changes only two
expected retained-capacity values; harnesses/native workers/fixtures stay unchanged.
Private-only dense tests/charge changes are removed with the implementation.
A materially revised design may revisit the idea; these results are not a ban.

[Evidence](cached-bicubic-dense-geometry-trial-evidence.json). Raw
`cached-bicubic-dense-geometry-trial.tar.xz`, SHA256 `73bbeaa9d5aa5c67f267aff1530f682577aa0286b735a709f905f8e0f5a8088b`,
3,346,556 B/685 members, mirrored/member-verified. Its pinned
dependencies retain original tools, fixtures, current package and qualification.
No renderer dependency: current costs are unchanged. All 289 paths remain required;
publication stays postponed.

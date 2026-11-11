# Exact cubic coefficient preparation experiment

CBG791 batches eight horizontal coefficients while keeping every unfactored cubic
operator, scalar normalizer order and original tail. The model matches 21,243
weights, 2,073 normalizers and 4,128 boundary/random lanes bit for bit. Actual
helper/canary/allocation checks and 364 internal/776 public resize checks pass
on Windows/Linux, normal and scalar, including all three named storage suites.

**This implementation is not retained.** Three-actual complete-operation screens
show no convincing benefit. They are development screens, not current costs or
independent confirmations. The broad intervals do not prove a regression.

| Existing complete resize screen | Before / candidate µs | Reference before / candidate µs | Normalized point change |
| --- | ---: | ---: | ---: |
| Down8 | 439.65 / 477.52 | 420.05 / 456.76 | -0.12% |
| Wide8 | 394.97 / 431.49 | 346.31 / 366.89 | +3.12% |

Positive change is slower. Managed preparation, normalization, exact filtering/
repair, independently owned output and disposal stay timed; the explicitly labeled
zimg backend retains graph/input/scratch as before. Source/output/native hashes,
approved native bounds and idle-v2 guards pass on CPU0. No shape/settings grid,
quality gate or numerical policy changes.

Reusing the unchanged [public-plan diagnostic](CACHED-NARROW-PLAN-ATTRIBUTION.md)
finds creation 37.48–38.38 µs before and31.00–31.38 µs in the candidate, while
unchanged execution moves487–488→450–451 µs. Each side has three200msbatches;
allocation stays200,056 B/op. These instrumented boundaries differ from default
Resize and have no native bracket. They suggest a modest preparation lead after
execution-clock movement, not a complete-operation gain or time to subtract from
the benchmark. The extra helper, weight reread and separate geometry loop do not
justify this implementation. A materially revised preparation design remains open.

No owner/capacity/cache/pool/API is added. Complete screen totals stay about
199,980–199,983 B/op for Down8 and198,447–198,450 for Wide8; the few-byte aggregate
variation is not an allocation-saving claim. Peak/retained capacities are unchanged
by construction, not newly measured physical peaks. Selected candidates, exact
pixels, cancellation and admission are unchanged.

Both production source/builds and embedded test assemblies are restored to the
byte-verified accepted CNP789 artifacts without rebuilding. The candidate-only
helper test and implementation survive in raw evidence. Accepted Windows/Linux
6,248 full/live checks, affected ISA checks and SAME actual Release-package/all
three examples remain valid; they are not repeated for this rejected candidate.
The private package565088e0 and published releases are unchanged. Current benchmark
costs/renderer gain no dependency on this experiment.

[Evidence](cached-cubic-weight-preparation-experiment-evidence.json) records all
screens, numerical/source proofs, phase batches, modules, restoration and reuse.
Mirrored/member-verified raw `cached-cubic-weight-preparation-experiment.tar.xz`: 7,761,524 B / 385 members,
SHA256 `014f9ffc9d6bb73f56c343fe5607edbd3c57ae74949e63f23677734b141c4e9f`. It retains source, both candidate builds, actuals, guards
and reproduction tools; pinned CNP789/CPA790 archives supply unchanged references
and qualification. The next bounded task attributes the existing 4KiB narrow
byte resize's ~55µs declared point gap on the current immutable package.
Publication stays postponed and all289 required paths remain in scope.

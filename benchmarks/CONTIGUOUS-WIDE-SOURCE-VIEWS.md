# Contiguous source views for bounded wide bicubic

The complete 513×259→128×64 RGBA8 Resize saves **14.15µs /
3.81% raw / 4.01% reference-normalized**.
Filtered/all-actual raw and normalized confidence separate. Native moves
+0.21%. This is a verified incremental gain; byte parity remains open.

| Complete Resize / input+output+4KiB | Lokad / zimg µs | Ratio / upper | GC B/op |
| --- | ---: | ---: | ---: |
| Immutable baseline, RGBA8 | 371.48 / 350.00 | 1.061 / 1.066 | 113 |
| Contiguous views, RGBA8 | 357.33 / 350.73 | 1.019 / 1.033 | 113 |
| Immutable baseline, RGBA16 | 402.43 / 414.87 | .970 / .975 | 113 |
| Current companion, RGBA16 | 404.77 / 407.07 | 0.994 / 1.012 | 113 |

The word companion is **uncertain**, superseding its older pass. Raw cost grows
2.35µs/.58%, native moves -1.88%, and the normalized point is adverse
2.51%; normalized and all-actual raw confidence overlap. No word gain or established
normalized regression is claimed. Both confidence views and all actuals remain.
Do not optimize a presumed word gap or repeat unchanged timing for a passing bound.

The [current profile](WIDE-BICUBIC-PREPARED-ATTRIBUTION.md) identified repeated
source-offset loads and range checks in the horizontal loop. Existing once-per-tile
proofs now select two prebounded contiguous spans. Each output keeps its ordered
AVX256 t0…t15 Add(Multiply); clamped/odd/count17/other modes keep their actual indexed
paths. No reassociation, FMA, new numerical policy or certificate/repair change.
Both original production files reconstruct after removing the new producer/dispatch.

The first implementation's 512-bit return introduced staging loads/stores before
cache publication. Its short screen/JIT are retained; it was revised before
confirmation. The accepted producer returns two independent sums directly.
Row code is 6,498B versus 6,204B; byte-tail code remains 6,512B. Although the new
guard folds away for word, importer locals change its generated layout/prologue
6408→6384B / 712→728B. An exact-code identity assumption was disproved, prompting
the existing word control's complete baseline/candidate confirmation before qualification.
Four functional word checks pass; their captures are reused after a parser fix.

Two candidate versions/core+embedded builds per OS; unchanged public tests and
benchmark/native/selector/fixture tools are reused. Primary candidate screens
precede one immutable baseline and one independent candidate confirmation;
the word companion is refreshed once per build. All frozen input/settings,
exact rational/managed pixels, native bounds, source/owners and CPU0/idle-v2 gates
pass. All setup, preparation, filtering, repair, independent output and disposal
remain timed. The labeled zimg3.0.6 reference retains graph/scratch; no one-shot
setup or wider content/geometry claim is made.

Allocation remains113B/op and retained operation scratch zero. Existing3296B
row/4068B tail payloads and4KiB admission remain. Independently owned byte/word
results are32768/65536B. No new capacity, heap buffer, pool, reservation or owner;
physical/native/JIT peaks and default-pool retention are unmeasured. Logical
offset/check counts explain the hypothesis, not phase costs or predicted gains.

Windows/Linux each pass **6,439 full/live checks**:3110internal/2381public/948live.
Affected528internal/791public pass normal/scalar/AVX2-off/AVX512-off, including
all three named storage suites. Four new exact-bit/poison/no-allocation cases and
existing actual row/tail/generalization/refusal/cancellation/ownership checks pass.
One broad/package milestone follows affected performance reconciliation; controller
preflights/analysis failures are retained, with no production compile failure.

The **same actual Release package** and all three examples pass both OSes.
Private0.2.1 SHA256 bdbeb55726a8863035b157ee8d6a6515d5d951515c65a079ae00e288596d0c88 (454,324B), timed/packaged
DLL 60dd553a555885a8d16a7331e937dc65f9aa6eac7281fe5539ad904cea30bfc4; separately qualified Linux
DLL 6ceb90136823195d6495fd5a95c5319a866ec18219be77a6ddf4c15587e04543. Published releases remain immutable.

[Evidence](contiguous-wide-source-views-evidence.json) and mirrored/member-verified
raw contiguous-wide-source-views-qualified.tar.xz — SHA256 faa3bcdb364d10bdb8f9ee293d5fa4339f6ddf1c6400a87df640951ca196402e
(2,754,152B / 191members) preserve both versions, modules,
actual package/symbols, all measurements/checks, JIT, source proofs and reproduction.
Dependencies preserve the baseline/reference and prior inconclusive experiments.
No new material path/grid: all289 required paths remain; publication is postponed.

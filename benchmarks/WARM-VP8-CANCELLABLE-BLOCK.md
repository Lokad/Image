# VP8 blocks with live cancellation

The existing live-token operation improves 11.6% while retaining every original
per-decision cancellation check. Allocations stay 760,572 B/op. All five affected
existing VP8 operations refresh; the four control point means are0.5â€“1.8% slower
than the prior short results. These small movements are retained, without retuning
or a claim of statistically confirmed regression. All remain parity gaps.

| Complete existing operation | Lokad Âµs | FFmpeg Âµs | Ratio / upper | Lokad allocated B/op | Elapsed change |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| OpaqueCancelable8 | 6,687.81 | 4,793.83 | 1.395 / 1.437 | 760,572 | -11.6% |
| SimpleOpaque8 | 6,888.59 | 4,890.64 | 1.409 / 1.450 | 760,572 | +1.8% |
| Opaque8 | 6,638.33 | 4,795.62 | 1.384 / 1.443 | 760,572 | +0.5% |
| Native8 | 8,353.50 | 5,968.68 | 1.400 / 1.436 | 1,966,648 | +0.6% |
| StreamNative8 | 8,284.99 | 5,977.38 | 1.386 / 1.435 | 1,966,744 | +0.9% |

Each scored short retains all three actuals, native pre/post brackets, both official
idle-v2 guards and exact original input/frame/source/owner checks. Frozen packets
remain 72,380/76,128/98,590bytes, exact alpha and the original RGB<=4/RMSE<=1.5 native
gate. Fresh Stream input includes the same AVIO/acquisition/copies/EOF/ownership
boundary. One pinned CPU/operation and one native codec/conversion thread; startup,
IPC, I/O and checks excluded. Native allocations are unmeasured. No longer repeat
runs above one, new fixture, settings grid, harness, selector or native build.

A separate live helper reuses the existing 32-bit window arithmetic. Selection
requires 304 available local work units and 272 partition bytes, with the complete
private model/block and valid scalar state. A token poll immediately precedes
every decision/work increment. A finally publishes exactly the completed work and
scalar cursor even after cancellation, without a cleanup poll. Initial cancellation
consumes nothing. Short/tight/invalid states keep the original reader; the original
noncancelable helper remains unchanged. No heap/table/cache/root/admission is added.
The [proof](https://github.com/lokad/image/blob/27fc0425be9e21d913e7bd049fe3ce629bf28955/docs/vp8-cancellable-block.md) states the bounded prefix and ownership
requirements; no cancellation granularity or pixel precision is relaxed.

Both OSes pass **4,484 Release tests**: 1,660 internal + 1,892 public + 932 live FFmpeg,
132 VP8 and 36 root controls normally/scalar, and 305 affected public scalar controls.
Tests compare 1,024 coefficient compositions to the original reader and stop
deterministically before every ordinal 0..304, checking token identity, completed
work, all cursor fields and 19 following reads. Maximum blocks cover all cursor
phases; guards, tight/short/invalid eligibility, root borrowing and initial cancel
remain checked. The same actual private 0.2.0 package and all three usage examples
pass Windows/Linux, including real PNG/VP8 default/live-token pixels and independent
charged survivors. One production/internal-test build per OS is frozen throughout;
public/differential assemblies and unchanged package consumer tooling are reused.

The shared dispatcher affects all five VP8 operations, so all five current rows
refresh. PNG's accepted materialized cursor, JPEG, VP8L/encoders, editing/Lanczos3,
text/drawing and admission/lifetime code retain their identified scopes. Coverage
stays 254=238 numeric+16 unmeasured,72 passes/165 gaps/one confidence-open,38 current and
200 historical. No historical observation is relabeled. Full parity and release
readiness remain open; published 0.1.0 is immutable.

Next: one bounded Reduce8 Lanczos diagnostic, then feasibility of a wider FP32
horizontal strategy with rigorous conversion/FMA bounds and exact fallback. The
required byte 1.548x/word 1.588x gaps remain; rejected word strategies stay retired.
[Evidence](warm-vp8-cancellable-block-evidence.json) retains statistics, allocations,
unchanged reference boundaries, identities, qualification and mirrored raw archives.

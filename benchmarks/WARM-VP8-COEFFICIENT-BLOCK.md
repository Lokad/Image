# Block-scoped VP8 coefficient reader

Reading an admitted coefficient block through a private32bit window saves
**11.2%** on current SimpleOpaque8. Exact pixels, logical work, original
cursor state and allocations are preserved. All five affected existing cases
now have current-producer evidence; all remain above parity.

| Existing complete operation | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| SimpleOpaque8 | 6766.54 | 4890.49 | 1.384 / 1.459 | 760,572 |
| Opaque8 | 6603.84 | 4795.63 | 1.377 / 1.419 | 760,572 |
| OpaqueCancelable8 | 7561.39 | 4794.93 | 1.577 / 1.659 | 760,572 |
| Native8 | 8307.22 | 5964.81 | 1.393 / 1.425 | 1,966,648 |
| StreamNative8 | 8210.18 | 5993.82 | 1.370 / 1.407 | 1,966,744 |

The quantified gain uses the matched current SimpleOpaque8 baseline. The other
four rows replace historical producers; no matched current-baseline speedup is
claimed. Native allocation is unmeasured. Supplied-root timing remains separate.

RFC6386 coefficient paths need at most304boolean decisions per16sample block.
Only noncancelable calls with304remaining logical work,272partition bytes and a
valid scalar state are eligible. A16bit refill maintains an8..24bit lookahead;
the exact scalar range/value/byte/bit cursor and actual work are published once
perblock. No input copy, heap/table/cache/root or new reservation. The small
private stack state fits the existing8192B coarse allowance. Tight work, short
input and live cancellation retain the original perbit reader before any fast
work. No resource refusal is caught or retried. Earlier per-Read/window/local-
state/local-work trials remain rejected; this changes the block boundary.

The original three packets (72,380/76,128/98,590B), managed/native full-frame
hashes, exact alpha, native RGBmaximum4/RMSE1.5 and case-specific normal/scalar
CLI policy pass. Resident or fresh non-exposed seekable input, complete decode/
conversion, acquisition/disposal and source/independent-owner gates remain.
Stream native AVIO, copy, length/EOF and owned packets match the latest input
reconciliation. The live-token row also refreshes because its entry dispatch
changes, though its arithmetic reader stays original.

Each short retains three actuals, native pre/post brackets and both official
idle-v2 guards. One immutable producer is reused, with no new harness/selector/
native build, raster/settings grid, profile or long repeat above parity.

Qualification: **4,478 Release tests per OS** (1,654 internal,1,892 public,932 live
FFmpeg),129focused VP8 checks normally/scalar and305public WebP/VP8/memory scalar
checks per OS. New original-reader comparisons cover coefficient/dequant/sign/
EOB composition, guarded storage, all cursor fields, exact work and following
reads; the maximal block consumes exactly304work within272available bytes.
Public/differential test assemblies are reused with the current frozen core;
all suites run through test.ps1-NoBuild without rebuilding production.

The same actual private0.2.0 package passes both consumers and all three examples,
including a real VP8 packet decoded identically through fast/noncancelable and
original/cancelable paths, with independent charged survivors. Published0.1.0
is unchanged. Full254-case warm parity and release readiness remain open.

Next: bound a distinct prepared-double/FMA horizontal strategy for the larger
current LanczosReduce16 gap, before one existing-case trial. Unchanged JPEG,
Lanczos, other codec and encoder confirmations are reused. [Evidence](warm-vp8-coefficient-block-evidence.json)
retains all statistics, allocations, state/work proof, source/package identities
and mirrored raw hashes.

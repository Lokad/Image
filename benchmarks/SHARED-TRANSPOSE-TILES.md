# Shared transpose and counterclockwise tiles

All four original byte/word transpose and counterclockwise controls pass warm
single-CPU parity. The byte turn independently saves **11.84 µs / 29.12% raw**,
**28.85% reference-normalized**, against a contemporary **40.65 / FFmpeg 38.02 µs**
baseline. Both confidence views separate; the reference clock moves **−0.39%**.
Exact pixels and **112 B/op** remain unchanged.

| Complete original 513×259 → 259×513 operation | Lokad / FFmpeg µs | Conservative upper | GC B/op |
| --- | ---: | ---: | ---: |
| Counterclockwise RGBA8 | 28.81 / 37.88 | .769 | 112 |
| Plain transpose RGBA8 | 27.70 / 38.27 | .758 | 112 |
| Counterclockwise RGBA16 | 35.65 / 44.07 | .880 | 112 |
| Plain transpose RGBA16 | 34.98 / 44.11 | .855 | 112 |

The existing SSE2 UInt32 4×4 transpose now also places counterclockwise output.
One shared byte/scalar tile schedule replaces the duplicate scalar rotation loop;
the accepted AVX2 UInt64 matrix method remains verbatim. Closed generic row order
selects increasing/decreasing destination rows. Complete pixel bits, alpha and
hidden RGB are never interpreted. Scalar tails/fallbacks, 32×32 cancellation
checkpoints, admission, explicit owners and exception cleanup remain intact.

The primary uses a cheap existing 12×64-operation adjacent screen, then one
contemporary baseline and independent confirmation. The same immutable build
supplies all four current costs. Every pair and three native brackets are retained;
source/native identities, exact managed/normal-scalar CLI pixels, independent
outputs and idle-v2 guards pass. Companion cost changes from the
[earlier word stage](COUNTERCLOCKWISE-WORD-MATRIX.md) remain historical comparisons,
with overlapping normalized bounds; they establish no causal companion speedup.
Stop all four passing controls. No new fixture, settings grid or favorable repeat.

Complete public admission, allocation, materialization and disposal stay timed.
The FFmpeg public transpose reference retains graph/source/frame wrappers;
independent pixel allocation/processing/release is timed, exposing padded rows
without a redundant packing copy. Startup, transport and validation are excluded.
This is the stated complete warm boundary, not FFmpeg CLI startup.

No extra image, pass, workspace, reservation, cache or owner is introduced.
The byte primary borrows 531,468 B input and owns 531,468 B output; retained
operation scratch is zero. Allocation is separate from live/retained capacity.
Governed operations bypass the unchanged two-array/4 MiB idle pool. Ownership
tests peak at 15,540 / 31,080 B for byte/word source plus two outputs. Physical,
native and JIT peaks remain unmeasured; there is no capacity-saving claim.

**6,587 full/live checks per OS** pass: 3,236 internal / 2,403 public / 948 live.
Focused **13 internal / 248 public** checks pass normal/scalar on both OSes.
Seven additional public cases cover byte 4×4/32×32 boundaries and complete pixel
payloads, hidden RGB, terminal refusal, admission-triggered cancellation cleanup
and independent surviving outputs. Full encoder packets, decoder pixels and
editing remain correct. Unchanged codec/resize/text/ISA evidence is reused.
One core/embedded/public build per OS; no harness/native/fixture build or
production rebuild after timing.

The **same actual Release package** contains the timed Linux DLL and passes both
OS consumers and all three examples, including byte/word rotation cancellation,
refusal, tails and surviving ownership. One Release/--no-build pack goes directly
to artifacts/nugets. Published packages remain immutable; publication is postponed.
The current 191 owner views have **166 passes / 9 gaps / 16 uncertainties**. All
289 required paths remain; these controls establish no full-library parity claim.

[Evidence](shared-transpose-tiles-evidence.json) retains source scope, complete
boundaries, every sample, confidence, identities, immutable producers, qualification
and actual package. Raw `shared-transpose-tiles-qualified.tar.xz`, SHA256
`1d736d7c0ec7aa91a84cd946265ebd05f1481373cc459a55609112127b2ab936` (2,436,132 B / 129 members),
is mirrored/member-verified. Its dependency retains the earlier word stage,
preceding actual package and original reference inputs/replay recipes.

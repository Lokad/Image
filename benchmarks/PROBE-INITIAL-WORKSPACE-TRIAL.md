# Probe initial workspace trial

IWP1057 is **not retained**. Starting dimension-only acquisition with 32 bytes and
growing to 256 bytes before bulk skipping saves 224 allocated bytes for early
seekable prefixes. The existing nonseekable JPEG needs bulk storage before SOF,
so the extra initial array instead adds 56 bytes. General/header 8,448-byte workspace,
shared parser, exact descriptors and all public input/output boundaries stay fixed.

| Complete original JPEG boundary | Baseline / candidate µs | Paired ratio interval | Allocated baseline / candidate B/op |
| --- | ---: | ---: | ---: |
| seekable | 0.235819 / 0.229077 | 0.856388–1.101904 | 568 / 344 |
| nonseekableAsync | 0.310053 / 0.322666 | 0.998511–1.084630 | 568 / 624 |

Both three-pair CPU screens are inconclusive. These immutable-DLL comparisons
are scoped diagnostics, not FFmpeg results or current accepted costs. The candidate's
extra promotion/admission machinery and asynchronous allocation tradeoff do not
justify retaining it. No CPU regression is established and smaller buffers remain
a possible revised design; there is no family ban or fixed percentage threshold.
No independent confirmation, PNG/WebP/header timing, broad correctness or package
campaign followed this decision. Existing current benchmark rows remain unchanged.

Each OS passes 10 internal/153 public focused checks in normal and scalar modes,
including aggregate admission, terminal growth refusal, cancellation/recovery and
storage held until uncooperative reads finish. Candidate initial admission is 288 B,
bulk 512 B, with no retained operation workspace. These are logical ownership charges,
not physical peaks. Physical/native/JIT peaks are unmeasured.

Exact original sources, production DLLs and embedded/public test main modules
are restored from verified snapshots, with no restoration build. Reuse 6,721 full/live
checks perOS and the same actual private package/all three examples. One core,
embedded and public build perOS; zero helper/harness/native/fixture builds. No
candidate code or candidate-specific admission test expectations remain.

The first idle guard rejected CPU0 I/O wait after building, with zero timing samples
and no overlapping job. That guard remains in raw evidence. One distinct idle-ready
retry and the async screen pass both versioned guards; no successful run is repeated.

[Evidence](probe-initial-workspace-trial-evidence.json) records every observation,
source/producer identity, restoration and reuse. Raw `tmp/probe-initial-workspace-trial.tar.xz`:
793,780 bytes/71 members, SHA256 `4bdbee58d1db7bca3f7222fb948827347468de34c1830d453c71a0cfd1613d18`, mirrored/member-verified.
Its dependency retains the qualified baseline/package. This trial is not a renderer
dependency. All 289 material paths remain required; publication stays postponed.

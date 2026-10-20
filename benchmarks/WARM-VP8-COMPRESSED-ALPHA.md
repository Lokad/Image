# Exact compressed alpha for lossy WebP

The [bounded alpha-match successor](WARM-VP8-ALPHA-MATCHES.md) refreshes the four
unrestricted q0 packet/cost rows. The work32M packet/work is unchanged; earlier
producer-specific results below remain historical evidence.

The three existing alpha controls each save **18,504 complete-packet bytes** with
identical VP8 payloads, decoded RGBA, source quality and exact alpha. This is an
incremental compression gain: all three packets remain above the frozen 98,590 B
FFmpeg/libwebp reference. Managed 100/native 75, effort 4, original 513×259 inputs,
the 0.25 dB source-quality gate and decoder tolerances are unchanged.

| Existing control | Before → current / reference packet B | Managed / reference µs | Managed allocated B/op | Short CPU ratio / upper |
| --- | ---: | ---: | ---: | ---: |
| Encode8 | 286,532 → 268,028 /98,590 | 21,368.74 / 53,430.11 | 2,174,702 | 0.400 / 0.475 |
| Encode16 | 286,532 → 268,028 /98,590 | 21,613.20 / 53,776.89 | 2,174,520 | 0.402 / 0.425 |
| EncodeBudget8 | 471,436 → 452,932 /98,590 | 23,542.60 / 53,383.44 | 2,681,364 | 0.441 / 0.514 |

ALPH payload decreases from 132,868 to 114,363 B (including its one-byte header).
Packet savings are 6.46% for 8/16-bit input and 3.93% for the 32M-work case. The latter
still uses its fixed coefficient model and completes within 32M (31,961,577 units).
The first conservative work screen compressed only the unconstrained controls;
the final bound accounts one alpha sample rather than four RGBA channels, plus
every performed writer call. It includes both passes and fixed-header headroom.
No pre-existing source conversion, prediction or coefficient work is discounted.

The independent encoder uses RFC 9649 horizontal residuals, existing fixed prefix
coding and previous-pixel copies in a headerless green plane. Count then write
borrows the owned alpha plane, adding only 2 KiB of admitted workspace; no filtered
plane, full RGBA intermediate or encoded-alpha buffer is allocated. Raw alpha is
chosen before shared admission when local allowances cannot afford compression,
or after a successful count when compression does not reduce padded bytes.
Shared refusal and failures remain terminal. Workspace charges survive writing
and are released explicitly, including cancellation/output failure.

Both platforms pass **4,613 tests** (1,718 internal, 1,961 public, 934 live FFmpeg),
146 internal + 3 public scalar checks and the same actual private 0.2.0 package/all
three examples. Ten focused regressions cover exact edges/long copies, no-gain
and local-work decisions, aggregate shared refusal, cancellation, retained charge
and subsequent success. Frozen decoder corpus packets are unchanged: its replay
test permits smaller compressed ALPH only with identical VP8 and decoded pixels.
The initial stale hash failures and their correction are retained. An additional
internal-only build adds final emission-cleanup regressions; production remains
immutable. One accepted core build/OS, one earlier Linux work-bound candidate;
no differential/reference/harness/native/selector rebuilds or post-freeze rebuild.

Three sequential short cost screens retain all three actuals and six idle-v2
guards. Complete operations include normalization, output growth/ownership and
disposal; there is no timed ToArray. Native allocations remain unmeasured.
CPU parity is deferred; these supersede three prior independent observations
without claiming current independent timing passes. No confirmation repeat or
new workload is added. The other four lossy controls and unrelated algorithms
retain their unchanged relevant evidence.

The [seven-control ledger](WARM-VP8-SIZE-COVERAGE.md) still has five procedural
size gaps. Alpha coding and q0 coefficient coding remain material opportunities;
the two primary opaque content controls continue to pass at their documented
matched quality. No broader compression claim follows.

[Structured evidence](vp8-compressed-alpha-evidence.json) retains original/current
packets, exact hashes, source-quality checks, allocation/timing costs, failures,
qualification and raw-archive identities.

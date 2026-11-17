# Current bilinear reference reconciliation

The original RGBA16 enlargement remains **uncertain** on the current immutable
qualified package. The full conservative interval crosses one; there is no pass,
demonstrated gap or production speedup claim.

| Original complete 513×259→1027×519 Resize /retained FFmpeg graph | Managed /reference µs | Conservative ratio interval | GC B/op |
| --- | ---: | --- | ---: |
| Bilinear RGBA16, current package | 1,826.192830 /2,013.583393 | 0.818235–1.009894 /uncertain | 4,498,558 |

The earlier capture has consecutive native group means of
2,030.336 /1,924.568 /1,972.721 µs. Its paired confidence passes while the full
bracket envelope remains uncertain. The explicit tooling correction distributes
the same 36 native actuals around twelve alternating pairs, one before and after
each pair. Fixed 256-operation batches, four complete warmups, every actual and
the conservative all-actual/outlier-diagnostic/paired-log gate stay unchanged.
This addresses a sampling limitation without assigning a host cause, normalizing
clocks, dropping samples or repeating an identical capture for a favorable result.

The resize kernel and geometry sources are unchanged. ImageBuffer, operation
context and shared-pool sources have changed since the older producer, so this
capture measures the current qualified DLL rather than assigning old costs to it.
No causal speedup, regression or allocation gain follows from historical clocks.
Allocation remains 4,498,558 B/op. Source **1,062,936 B**, independent result
**4,264,104 B**, geometry **102,892 B** and row cache **131,520 B** stay separate.
The output exceeds the unchanged 4 MiB idle ceiling; no fixture-specific retention
increase. Caller-root ownership remains explicit; physical/native/JIT peaks are
unmeasured.

Managed timing includes complete public validation/admission, geometry/workspace,
exact filtering/repair, independent output and disposal. The FFmpeg/zimg reference
retains graph/source/frame wrappers and geometry; full filtering/conversions,
output acquisition and release stay timed. This remains an **asymmetric diagnostic**,
distinct from the [passing equivalent one-shot](BILINEAR-EQUIVALENT-ONE-SHOT.md).
No necessary managed work leaves timing. Frozen source/settings, exact managed
rational/full-word pixels, native-only RGB/alpha maximum 1 and RMSE ≤.25,
zero-alpha behavior, native normal/scalar CLI, source and surviving owners pass.
CPU0 Release, natural nonconcurrent GC and all idle-v2 guards remain.

Initial setup rejects reference source/build files that no longer match the frozen
binary identity, before warmups or actuals. Restore the original hash-verified
source, build recipe and unchanged binary under an isolated ignored root, with
the physical root marker required by the frozen harness. No reference rebuild,
upgrade or gate relaxation. The full capture then completes; postprocessing
incorrectly expects nested metadata instead of the original flat schema. Correct
only the recorder and validate the completed log/frame/guards. **Zero completed
measurements repeat.** Both failures and recovery recipes remain.

One small driver build; zero production, embedded, public, harness, native,
fixture or package builds and no broad qualification repeat. Reuse **6,823 checks
per OS/SAME actual package/all three examples**. No new workload or settings grid.
Passing paths stop; this uncertain diagnostic does not block independent work.

[Evidence](bilinear-current-interleaved-reference-evidence.json) retains current
costs, exact identities, source changes, patch, every actual and both recoveries.
Mirrored/member-verified `bilinear-current-interleaved-reference.tar.xz`:
**1,369,816 B /54 members**, SHA256
`26635fbb6a4a5b78ecd0857baedae4b207b0b2c880c4b104eb59179d5036832a`.
Archived `bir1231-*` recipes and declared dependencies reproduce the round.
The full 289-path objective/final immutable-package refresh remain open.

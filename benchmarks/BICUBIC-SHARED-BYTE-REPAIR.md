# Shared original-order bicubic byte repair

WBR665 shares the existing small-column byte repair with wider strips. Uncertain
byte pairs and singletons now replay only those pixels in the original source
tap order. The repair body, wide certificates and word repair are unchanged.
Tiny positive alpha still retires the 4 KiB workspace before rational replay.
No numerical policy, default dispatch, buffer, pool, session or owner is added.

One already-authored RGBA8 4097×2051→1024×512 control demonstrates the benefit:
compact execution falls **643.41→30.78 ms**, saving **612.63 ms / 95.22%**,
or **20.90×**. Original three-actual evidence is retained; the candidate short and
independent twelve-actual confirmation agree. Filtered and all-actual intervals
separate. Full-frame normal, scalar and previously qualified pixels match exactly.
This is a managed-only complete-operation comparison, without native parity or
broader shape claims. The benchmark class and input generator remain verbatim.

| Current large complete operation | Elapsed ms | Allocated B/op | Admitted workspace B |
| --- | ---: | ---: | ---: |
| Existing default cached | 25.869 | 1,585,375 | 1,585,440 |
| Existing compact working limit | 30.785 | 153 | 4,096 |

Each retains a separately owned 2,097,152-byte result until disposal. Governed
peaks are 3,682,592 / 2,101,248 bytes, excluding borrowed input; scratch retention
is zero. Physical peaks and idle pool capacity are unmeasured. Minor allocation
differences among diagnostic runs reflect amortization, not intrinsic storage
savings. Default cached execution remains faster, so selection stays unchanged.

| Current frozen complete resize / labeled zimg backend, µs | Lokad | Reference | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA8 →128×64, input+output+4 KiB | 476.04 | 348.99 | 1.364 / 1.378 | 113 |
| RGBA8 →256×129, input+output+4 KiB | 542.51 | 417.07 | 1.301 / 1.321 | 113 |
| RGBA16 →128×64, input+output+4 KiB | 453.66 | 403.32 | 1.125 / 1.134 | 113 |

These use the existing 513×259 inputs, settings and exact/native gates, with
three-actual screens then independent twelve-actual confirmations. All remain
gaps. The wide-byte control regresses from451.12 µs by **24.93 µs / 5.53% raw**;
both raw confidence views separate. Keep this adverse result alongside the large
gain. The small-byte observation improves from574.56 µs, but its earlier native
brackets drift; do not claim a causal companion gain. Current word brackets are
413.52/403.32 µs, so the displayed conservative native cost uses403.32, without
a causal word gain claim. No completed job is repeated for favorable sampling.

Managed timing includes public Resize, preparation/workspace, independent output
and disposal. Zimg, FFmpeg's scaler backend, retains graph/scratch before timing;
input preparation, filtering, quantization and independent output remain timed.
This is not fresh graph-setup parity. Exact managed pixels and the approved
native max1/RMSE≤0.25 gate remain distinct.

Windows/Linux each pass **6,006 full/live checks**, plus227 internal/703 public
per normal/scalar/disabled-AVX512/disabled-AVX2 mode. New half-tie/transparent cases
require successful wide execution, including a singleton tail, direct pixels,
poison and shared-root cleanup. Existing tests preserve tiny-alpha retirement,
terminal refusal, cancellation and independent survivors. The SAME actual
Release package and all three examples pass both OSes. One main/embedded build
per OS is reused; one necessary diagnostic build makes the expected DLL identity
configurable, preserving benchmark code. No native or standard selector rebuild.

[Evidence](bicubic-shared-byte-repair-evidence.json) pins all outputs, sources,
actuals, guards and package identities. Mirrored, member-verified raw archive
`bicubic-shared-byte-repair-qualified.tar.xz` has12,801,560 bytes / 361 members,
SHA256 `62b95267dcca616931caa99b5920d40c3e63ec89da2feabdf4c11eccba010422`.
It retains the [adverse generalization](ARCHITECTURAL-PREPARATION-AND-STORAGE.md)
and [earlier wider consolidation](BICUBIC-SHARED-WIDE-COLUMNS.md) as dependencies.
Three displayed costs refresh; other unchanged paths reuse evidence. The full
283-path objective remains open; this is not a release timing refresh. Publication
is postponed.

The subsequent [guarded geometry-view experiment](BICUBIC-GUARDED-GEOMETRY-VIEWS.md)
finds no useful robust complete-operation lead and restores production exactly.
It retains one real crossing-strip pixel case; current costs above are unchanged.

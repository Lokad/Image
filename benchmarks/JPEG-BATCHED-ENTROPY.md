# Batched JPEG entropy inspection

JSC1137 batches the search for unescaped JPEG marker prefixes. The original
`InspectHeaders/Jpeg420FreshStream` control now **passes and stops** on the actual
timed/package DLL. No encoder packet, decoded/editing pixel, API or limit changes.

| Complete operation | Current result |
| --- | --- |
| Independent current reference | 1.999 / 2.799 µs; ratio interval 0.695889–0.730193, pass |
| Controlled original/current DLL pair | 4.013 → 2.102 µs; 47.63% / 1.912 µs saved; ratio interval 0.516566–0.530917 |
| Managed allocation | 8,768 B/op standalone; 8,776 B/op on both paired DLLs |

All 12 actuals and both 12-sample reference brackets remain. Short current screen
also passes at 2.019/2.791 µs, upper 0.867760. Quiet VM, CPU 0, one processor,
optimized JIT and natural nonconcurrent workstation GC; no favorable retries.
The paired and standalone stream wrappers differ by 8 B, not an allocation gain.

The reference remains **FFmpeg n8.1 AVIO/owned descriptor APIs plus an original
bounded structural/EXIF adapter**, not public packet-parser performance. Both sides
include a fresh opaque seekable source, complete entropy/late metadata/EOF scan,
independently owned nine-field output and disposal. Frozen input/settings and
worker identities are unchanged. No pixels, whole-input copy or retained parser.

One safe managed helper loads a bounded 32-byte vector and derives prefix and
zero masks. `prefixes & ~(zeros >> 1)` skips complete `FF00` data pairs together.
Final lanes, filler, restart/real markers and partial prefixes return to the
existing suffix parser at the same position. The portable span fallback preserves
behavior. No new fields, heap workspace, cache or ownership model. Removing this
helper/imports and restoring one entropy-only search reconstructs the original
reader exactly; fill/read/skip, cancellation, admission and pending users remain.

Two new public controls move stuffing/filler/restart/end prefixes across all 32
lanes and initial/bulk storage boundaries. They compare sync/pending-async read
requests and verify late EXIF, exact typed output, truncation/container failures,
borrowed inputs and shared-budget cleanup. They pass old/new builds in normal and
scalar modes, plus AVX2-disabled current execution. The existing 20 same-packet
grammar controls and 10 internal/158 public focused checks per normal/scalar mode
pass. Each OS then passes **6,749 full/live checks** (3,294/2,507/948). The SAME
actual Release package passes consumer verification and all three examples on
Windows/Linux, with no timed-artifact rebuild.

Admission remains 512 B initially / 8,448 B maximum; no new retained workspace.
Native bounded scratch remains 16,416 B excluding contexts/descriptors. Allocation,
logical capacity and unmeasured physical/native/JIT peaks remain distinct. Unchanged
dimension-prefix, PNG/WebP, codec, editing and text costs retain their original
identified producers. Their gaps/uncertainty are not relabeled or retimed.

The earlier buffered-loop front end was inconclusive (ratio 0.991589, wide interval)
and is unaccepted. The materially revised batched classifier separates in its own
prototype confirmation and the actual production comparison. A single two-second
baseline profile supplied a lead; only 403/1,992 leaf locations map to managed
methods, so it establishes no phase cost. All instrumented clocks are discarded.
Both implementations and every actual remain in raw evidence.

One core/embedded/public build per OS, two isolated Windows prototypes, zero
existing helper/reference/fixture builds, one NoBuild pack. Runner discovery and
an existing controls-directory collision were corrected before statistical timing,
without production rebuilds or restarted captures.

[Evidence](jpeg-batched-entropy-evidence.json) pins current/baseline sources,
producers, tests and package. Mirrored `jpeg-batched-entropy-qualified.tar.xz`:
SHA-256 `fd8bca4fd4a894affdb84a16a8514c6d5b014588371933aa2224c34d6061b6ba`,
8,471,312 B / 237 verified members. It retains both prototypes, exact producers,
all guards/actuals, platform/package logs and reproducible controllers. Extract
the cited baseline dependency too; use archived `jsc1137-measure.py` for the fixed
reference boundary and `jsc1137-pair.py` for adjacent DLL effects. Published packages
remain immutable; publication is postponed and the full 289-path objective is open.

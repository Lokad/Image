# Bilinear reference with partitioned actuals

**Original RGBA16 enlargement passes/stops** on the same source-valid immutable
qualified C620/f034 DLL/package. This resolves measurement uncertainty on unchanged
production code; no historical-clock speedup, regression or host/CPU cause is claimed.

| Complete513×259→1027×519 Resize /retained FFmpeg graph | Managed /reference µs | Conservative interval | Managed GC B/op |
| --- | --- | --- | --- |
| Bilinear RGBA16 | 1,847.738228 /2,024.227756 | .877761–.944044 /pass | 4,498,558 |

The [preceding capture](BILINEAR-CURRENT-INTERLEAVED-REFERENCE.md) retains native
actuals spanning1.866–2.218ms and mostly1.811–1.817ms managed actuals. Whole-batch
group interleaving leaves its full gate uncertain despite a passing paired bound.
This explicit refinement partitions each256-operation actual into sixteen
16-operation subdivisions, interleaving the original before/paired/after groups
within every actual. Each original operation completes independently, including
its preparation, output and disposal; no work leaves timing. All192managed and
576native subdivisions remain raw and aggregate into **twelve statistical units
per group**, without more confidence samples. Four whole256-operation warmups
per side and conservative all-actual/outlier-diagnostic/paired-log statistics stay.
Paired interval .881716–.938012; native group means2,024.228/2,032.287/2,033.629µs.
Every actual remains; no identical favorable replay or gate relaxation occurs.

Both sides have more batch-clock reads; their overhead stays in measured spans.
Native IPC/client work remains outside operation clocks and occurs more often.
Diagnostic buffers are prepared before actuals; detailed reporter objects are
created afterward. Natural nonconcurrent GC, optimized JIT, sequentialCPU0 and
idle-v2 guards pass. The numerical/ownership contract and public operation bodies,
frozen inputs/settings, harness/native binaries and baseline preparation remain.
Managed timing includes fresh geometry/workspace, admission, exact rational
pixels, independent result acquisition/materialization/disposal. FFmpeg retains
its graph/source/frame wrappers and geometry, with all filtering/conversions and
independent output acquisition/release timed. This **asymmetric diagnostic** stays
distinct from the unchanged passing equivalent one-shot comparison.

Original full-word rational frames, native RGB/alpha max1/RMSE≤.25, zero-alpha
behavior, normal/scalarCLI, input and surviving owners pass. GC allocation is
unchanged; source1,062,936B/result4,264,104B/geometry102,892B/rowcache131,520B remain
separate. No retained operation scratch or idle-ceiling increase; physical/native/
JIT peaks remain unmeasured. Reuse6,823checks eachOS/SAMEC620actualpackage/all3
examples and unchanged currentC5/539 qualification6,824eachOS. One small driver
build; zero production/embedded/public/harness/native/fixture/package builds or
broad repeats. The final165comparison immutable-package refresh stays outstanding.

Preflight stops before workers/warmups/actuals because an old frozen manifest was
hard-linked to later canonical updates. Restore its exact archived7122cb41 bytes
as an independent file; current9353063b manifest, production and package remain
unchanged. Reference code/binaries never rebuild or change. Original failure,
exact correction, source patch, counters and every segment remain reproducible.

[Evidence](bilinear-partitioned-reference-evidence.json) pins the current scope
and all checks. Mirrored/member-verified `bilinear-partitioned-reference.tar.xz`:
**287,976 B /28 members**, SHA256 `246a60c19a22b1a0f5acc61ccea1d3fa0c7e2b3462c2099c833bc053ec5e2029`.
Archived bpt1240 prepare/deploy/resume/run/record scripts and declared source-valid
runtime/reference/package archives reproduce this bounded capture. Passing resize
paths stop; font/copy/inspection gaps and finite289-path completion remain open.

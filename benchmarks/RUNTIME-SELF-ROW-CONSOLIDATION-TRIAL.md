# Runtime copy and self-row consolidation trial

**Not retained.** Reusing runtime memmove for bounded separate byte rows makes
both complete byte controls slower. No confirmation, promotion or passing-path/
broad/package campaign follows. Production and current benchmark costs stay unchanged.

| Existing complete operation /E1 baseline →private DLL | Baseline →candidate µs | Paired ratio interval | GC B/op |
| --- | ---: | ---: | ---: |
| Separate RGBA8, 256×129 | 1.966704 →2.048770 | 1.005153–1.079630 | 0 →0 |
| Original clipped RGBA8, 254×129 effective | 1.958303 →1.984772 | 1.005282–1.021818 | 0 →0 |

The proposed end state uses runtime copying for bounded separate rectangles,
as the existing word path does, and the measured shared-backing self-row helper
at both depths. This removes the two-span bulk helper instead of adding another
kernel. Wider byte chunks, portable fallback, snapshot order, cancellation,
admission and owners remain. Older custom-helper gains precede later preparation
changes; they justify retaining evidence, not assuming the reversal helps now.
The current complete screens reject this implementation: **4.17% /1.35% slower**.

Reuse the original compiled harness and small actor. Three alternating pairs per
case, fixed 65,536-operation warmup/batches, two batch timestamps, CPU0, natural
nonconcurrent GC and idle-v2 guards remain. Exact full source/canvas, hidden RGB,
alpha, untouched pixels, frozen hashes/settings and original owner checks pass.
These are managed-DLL incremental screens, not FFmpeg parity measurements.
No storage is allocated, admitted or retained; caller pixels and unmeasured
physical/native/JIT peaks remain separate from the zero managed allocations.

Both OSes pass **10 internal /190 public** checks per normal, AVX-512-disabled
and scalar mode. The four new cases exercise clipping, distinct strides, tail/
chunk boundaries, cancellation, a borrowed zero-capacity root and independent
surviving output. Their initial test ceiling incorrectly counts only the larger
borrowed buffer. The existing production limit correctly rejects it before
mutation. Correct only that test to the exact sum of both buffers; preserve the
failure logs and unchanged production bounds. Rebuild only the embedded test
assembly; reuse all three completed Linux public modes. No timing replay occurs.

One private core build and two embedded test builds; zero canonical/VM/public/
harness/actor/native/fixture/package builds. The qualified E1 library/package and
6,819 full/live checks per OS/SAME package/all three examples remain unchanged.
Do not turn this adverse result into nearby width/ISA/kernel sweeps or infer
that every future consolidation is impossible. Resolve remaining meaningful
owned-operation uncertainty independently; passing paths stop.

[Evidence](runtime-self-row-consolidation-trial-evidence.json) retains design,
sources, checks, actuals, correction and identities. Mirrored/member-verified
`runtime-self-row-consolidation-trial.tar.xz`: **1,909,112 B /61 members**, SHA256
`cb93f3a96601c2c4eee02afa89d1160dd62d1c7a0527d17146f195aeaad1c504`.
Archived `rsc1226-*` recipes and declared dependencies reproduce it. This
unpromoted experiment adds no renderer dependency or required workload.

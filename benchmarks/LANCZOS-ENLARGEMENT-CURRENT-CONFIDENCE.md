# Current prepared Lanczos enlargement confidence

The prepared RGBA16 enlargement passes on the current immutable package DLL.
Together with seven source-valid controls, **all eight declared Lanczos comparisons
pass and stop**. This resolves uncertainty without a production change or speed claim.

| Existing 513×259 → 1027×519 prepared operation | Managed / zimg µs | Conservative ratio interval | Managed allocated B/op | Retained managed plan B |
| --- | ---: | ---: | ---: | ---: |
| RGBA16 enlargement | 4,117.26 / 4,212.39 | 0.956–0.990 | 4,264,760 | 333,396 |

The reference is **zimg 3.0.6, FFmpeg's scaler backend**, with its original prepared
graph/input/scratch boundary. Both sides prepare plans outside timing, then read
fresh input, normalize/filter, create an independent output and dispose it inside
every operation. Preparation has not silently moved across the timed boundary.
Managed Q30/rational pixels remain exact; native float RGB/alpha errors ≤1 and
RMSE ≤0.25 remain separately qualified. Frozen input/output hashes, full samples,
quality, source and independent-survivor checks pass unchanged.

Twelve fixed alternating pairs run 16 complete operations per subject, with twelve
native batches before and after. Every actual and the unchanged conservative
worse-of paired/three-bracket confidence rule remain. CPU0, sequential Release,
nonconcurrent workstation GC and both idle-v2 guards pass. The old 3989.78/3911.18 µs
uncertain observation remains history; different clocks do not prove a speedup.
No favorable repeat follows this passing result.

An independent result owns **4,264,104 B**, separate from the **333,396 B** plan and
the borrowed **1,062,936 B** input. The result exceeds the unchanged two-slot/4 MiB
pixel-store ceiling, so allocation remains timed. Native scratch is **274,240 B**;
native graph/input retention is unmeasured. No capacity or retention policy changes,
and the lower direct allocation counter versus the old aggregate counter is not
an intrinsic allocation saving. Physical/native/JIT peaks are unmeasured.

One private driver build adapts the existing adjacent tool to the frozen harness's
native protocol. Production, embedded tests, original harness, worker and fixtures
are reused without builds. The actual package contains the timed DLL; unchanged
6,600 full/live checks per OS, scalar/ISA and SAME-package consumer/all three examples
are reused. No broad/package qualification campaign occurs.

[Evidence](lanczos-enlargement-current-confidence-evidence.json) retains the complete
capture and reproduction. Raw `lanczos-enlargement-current-adjacent.tar.xz`:
44,364 B / 24 members,
SHA256 `855f3fe32238126e11b24e3e82701ad1a0d6a0c1cf591a451aa87750537216eb`, mirrored and member-verified.
Dependencies retain the original sources, inputs, oracle/worker and qualified
package. All 289 material paths remain required; publication stays postponed.

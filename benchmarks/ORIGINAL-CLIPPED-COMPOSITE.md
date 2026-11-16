# Original clipped RGBA8 compositing

The original `Composite/Rgba8StraightAlpha` passes and stops on the current
package DLL: **78.567 / FFmpeg 526.201 µs**, ratio **.149311**, interval
**.139438–.159260**, **0 B per public operation**. No production change or speedup
is claimed. All six inherited drawing recipes are now measured; copy gaps remain.

One original 513×259 source and separate canvas at (-3,5), effective 510×254 pixels.
The earlier (17,19) control stays separate. Both include complete public mutation,
with the canvas reset before EVERY operation outside timing. Managed invocation/
unroll 1 and iteration reset/validation preserve that boundary. Native uses two
clock reads per operation around the complete FFmpeg mutation, excluding reset.

The reference is explicitly **FFmpeg n8.1 retained single-thread overlay graph
and static source**, with unique canvas-buffer transfer and preserved source.
Necessary graph/frame/source preparation is outside this repeated borrowed-canvas
comparison. It is not a one-shot graph-preparation cost. No snapshot/full-frame
copy is inserted into either timed operation; native frame ownership/cleanup is
included and verified. Source/canvas setup, IPC and pixel verification are excluded equally.

Managed output is exact against an independent integer rational oracle with
half-up rounding. Native approved bounds pass:RGB max 1, full-frame RMSE .403307,
active-region RMSE .408453; alpha exact. All 3,327 untouched and 264 zero-source-alpha
pixels are exact. Input/source survive unchanged. Native output is frozen only
after comparison, then checked deterministically throughout timing. Normal/scalar
current-package checks agree exactly; no precision/quality contract is relaxed.

Three-actual short screen precedes independent 12-actual confirmation on quiet CPU0,
sequential Release, optimized JIT and natural nonconcurrent workstation GC.
All actuals, both native brackets and 99.9% Student-t confidence remain; the upper
retains the worse filtered/all-actual view. Short 72.109/490.720 µs, upper .164302
stays recorded. No favorable repetition or historical-stage sum.

The custom BDN driver emits no GC line. A separate warmed public Composite+reset+
validation diagnostic measures three 128-operation samples in each normal/scalar
mode: all 0 B. New operation-owned admission/retained storage remains 0. Prepared
caller pixels are 1,062,936 B; native source/canvas pixel payload is at least 1,077,440 B,
excluding graph/allocator storage. Default idle-pool policy is unchanged;
physical/native/JIT peaks are unmeasured. Zero peroperation storage is not zero
total retained storage.

Two managed-tool builds, two native builds(one rejected), zero production,
embedded/public-test, package or fixture builds. Reuse unchanged SMP1130 evidence:
6,747 full/live checks per OS and SAME actual Release-package consumer/all 3 examples.
The immutable core hash/timestamp remain unchanged throughout.

Initial native linking selected a codec-only FFmpeg build without overlay; retain
that rejected binary and use the already-qualified overlay archive identities.
An executable-runtimeconfig launch and a metadata-record failure are recovered
without repeating completed checks. The older observer's composite schema failure
is recovered from the completed short capture. All accepted timings run once.

[Evidence](original-clipped-composite-evidence.json) pins current producers,
sources, oracle/native outputs, quality gates, prepared state and all costs.
Raw `tmp/original-clipped-composite-confirmation.tar.xz`, SHA-256
`c203f65dae6f61ebbca222ba5f0e6ae15f9954e4f2906f141b3ed0138e461cda`,
7,538,124 B / 107 members, is mirrored/member-verified with the SMP1130 dependency.
It retains authored harness sources/generated BDN driver, accepted/rejected native
workers, inputs, checks, scripts and failures. Reuse the prepared runtime with
`ocm1133-measure.py`; tools reference the immutable package DLL without rebuilding production.

Six original authored-font operations remain unmeasured. All 289 required paths,
remaining gaps and uncertainties stay open; publication remains postponed.

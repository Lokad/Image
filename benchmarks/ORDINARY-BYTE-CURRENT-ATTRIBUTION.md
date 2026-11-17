# Current ordinary RGBA8 attribution

One two-second diagnostic uses the same immutable `e1ae64a3…` package DLL and
original ordinary-draw worker as the [resolved comparison](ORDINARY-DRAW-SYMMETRIC-CHECKS.md).
It completes 36,951 exact checked draws; all original frame/layout/reference and
ownership checks pass, with zero timed reference samples or builds. CPU0 executes
the workload, CPU1 collects; both idle-v2 guards pass. No RGBA16 passing path is
remeasured. Reuse 6,819 checks per OS/SAME package/all three examples.

The 1,996 sampled locations include Band 475, Area 185, BuildEvents 129, Add 124,
MergeBuffered 116, Partition 84, InteriorRun 71 and BuildEdges 70. They include
reset, validation, GC, inlining and skid: **not phase costs, removable percentages
or speed forecasts**. Instrumented clocks are discarded; historical profile counts
are not subtracted. The source shows a possible shared-preparation opportunity:
crossing descriptors order edge starts, then event sorting orders those starts
again alongside ends and intersections. Audit whether those orders can be shared
before selecting another implementation; no gain is established.

A prospective design must preserve the exact resulting events, equal/signed-zero/
nonfinite handling, arithmetic, event limits, cancellation and admitted ownership.
Prove safe workspace/merge lifetimes first. No implicit layout cache, numerical
relaxation, fixed-font shortcut or automatic replay of old sort trials is selected.

Preparation initially finds an already copied matching runner, before any guard
or worker. The single actual capture completes; postprocessing then looks under
the old validation root. Read the completed root-specific validation and existing
trace/map/log without replaying the worker or collector. Both failures and recovery
recipes remain. [Evidence](ordinary-byte-current-attribution-evidence.json) retains
the scope and identities; full receipts, JIT, trace, samples, guards and source
recipes are in mirrored/member-verified `ordinary-byte-current-attribution.tar.xz`:
**134,016 B /27 members**, SHA256
`4ad8d478566ad3e9c774399ff26a4525f8b912207e23f4664574a075b9ccac0c`.
Restore its declared dependency and run archived `ora1218-*` recipes, using recovery
only for completed postprocessing. No renderer dependency or current cost changes.

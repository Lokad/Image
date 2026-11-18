# Native copy scheduling and refresh preparation

**Initial static review: no production change, build or benchmark operation.** The
current copy worker `911f1946` calls libc `memcpy` from FFmpeg's plane copier.
Current libc `755a371e` and the retained2,048-byte function body `3c87561d` match
the earlier mapped word-copy evidence. Reuse that static strategy insight; it
does not newly identify the current byte invocation's IFUNC target or phase cost.

The retained implementation switches beyond512bytes to alignment-aware bulk
logic. Both original byte rows exceed that threshold. Earlier safe-hint word
prototypes had adverse points with per-row geometry and with two prepared plans;
their23.77–23.79ns hint diagnostic is separate from complete-operation timings.
Neither an automatic byte port nor another persistent cache/pin mechanism follows.
[Current byte attribution](WHOLE-ROW-COPY-PREPARATION-TRIALS.md#current-separate-byte-attribution)
locates bulk/tail instructions; it does not establish their stall causes or a gain.

Final-refresh preparation identifies the providers for all **165 existing rows**:
62codec,82editing/text and21inspection. Four reused aliases are resolved against
the actual renderer owners. No fixture/settings/control is added, no historical
clock becomes current-package evidence, and no timing acceptance changes.
Executable runtime/reference mapping remains; affected nondisplayed paths also
need reconciliation. The165-row refresh alone does not establish all289 material
paths. This is a one-off ignored preparation recipe, not new repository machinery.

Production/package `7b59eabe`/`f4c5b5db` remain unchanged. Reuse6,844full/live checks
per OS, the SAME actual package and all three examples. Publication and complete
performance qualification remain open. [Evidence](native-copy-scheduling-review-evidence.json)
pins identities, exact disassembly, source review and the ignored provider inventory.
Mirrored/member-verified `native-scheduling-refresh-preparation.tar.xz`:
**43,852 B /10 members**, SHA256
`31bf4f44f809bc58ea8bbce2d7e3fd0032c83ff5ca4c9766475a8b333660286d`.
Named dependencies retain the original native code and adverse trials.

## Executable refresh preparation

Seven frozen JPEG commands now bind the actual `f4c5b5db` package's `7b59eabe`
DLL in five private runtime clones. All442non-core runtime files match their
receipts; every original runtime file remains unchanged. Untimed checks invoke
the original setup, generated single-operation consumer and cleanup. All seven
preserve the original packet/source/quality/ownership/native checks: forward-only
444, callerRGB420, governedRGBA16, RGBAgray, callergray and both async boundaries.
Qualified preparation-file overrides are restored byte-for-byte. All clocks are
discarded; these checks supply no new costs or performance passes. Four archived
WebP receipts are also recovered with their selected producer identities verified.

The inspected frozen JPEG generated job leaves `GcMode.Force` unset. Its pinned
BenchmarkDotNet module resolves that setting to true and forces collections
before/after iterations. Retain this historical boundary; final natural-GC
orchestration must reuse the generated workload loops without silently omitting
public work. This finding is scoped to the inspected binaries, not every old job.
Two small temporary metadata/check-tool builds; no production, benchmark-harness,
reference, fixture or package rebuild. Current costs and qualification stay valid
within their recorded scopes; no passing path is optimized or retimed here.

The VM ran out of disk during cloning. Removing13duplicate plain transfer files
after full hash verification recovered1,199,093,760B; independent identical local
copies remain. Reference sources, release packages and cited compressed evidence
are preserved. Preparation resumed from the verified partial clones without
rebuilding or scoring. Existing strict copy gates and all289required paths remain.
Recover the remaining30special recipe bindings and reconcile affected nondisplayed
storage paths before scoring the final refresh. Its165rows do not replace that
finite coverage checklist. Publication remains postponed.

[Evidence](benchmark-refresh-runtime-preparation-evidence.json) pins the commands,
actual-package bindings, original gates, GC-policy inspection, cleanup and failures.
Mirrored/member-verified `benchmark-refresh-runtime-preparation.tar.xz`:
**484,996B /86members**, SHA256
`d95ced41a470a91f393b3004aa983b3240d268a5038269a3d8f79085af218be5`.
Named dependencies retain unchanged runtimes, reference identities and packages.
The rejected-copy record `effb376b` passes all three CI jobs (run44256244286).

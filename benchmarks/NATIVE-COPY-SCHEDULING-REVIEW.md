# Native copy scheduling and refresh preparation

**Read-only review; no production change, build or benchmark operation.** The
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

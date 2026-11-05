# VP8 trellis preparation audit

A count-only projection of current owned source reuses the frozen complete
513×259 opaque diagram, Q75/effort6. One warmup and one observed public encode
preserve the exact 6,818-byte packet, original pixels, borrowed budget and cleanup.
Both quiet guards pass. No production change or timing/allocation/peak claim.

| Observed complete effort6 encode | Count |
| --- | ---: |
| Frames / preparation passes | 1 / 1 |
| Macroblocks | 561 |
| Luma16 importance derivations | 561 |
| Luma4 importance derivations | 561 |
| Method5 reconstruction derivations | 0 |
| Header retries / initial limit | 0 / 65,536 |
| Model derivations / exponentials | 1,122 / 17,952 |

The sixteen Gaussian weights depend only on coefficient coordinates. Preparing
them once per frame would reduce these counts to one model/sixteen exponentials.
This establishes repeated work, **not elapsed savings**. Extra span/stack liveness,
call boundaries and complete-operation cost must be measured before acceptance.

The intended design borrows immutable frame-prepared importance through nested
luma/reconstruction, preferably using existing frame stack/model storage. Preserve
checked/oracle inputs, mode/candidate order, strict ties/EOB, cancellation, exact
packets and admission. No global cache, new owner or quota. Internal signature
cleanup needs no forwarding wrappers. If shared call boundaries change, retain
an existing non-trellis control as a concrete regression guard; do not grow a grid.

The [old fixed-importance trial](VP8-FIXED-IMPORTANCE-DEFERRED.md) retains a 2.2%
short lead rejected under superseded rules. Its timing and closure language are
historical, not current proof or a permanent ban. This audit verifies actual
remaining repetition; it does not authorize blind replay of that fixed-literal/
validation-removal candidate.

One isolated diagnostic build; zero production, benchmark, native-worker, selector
or package builds. Current comparison costs stay unchanged. The qualified
[luma4 checkpoint](VP8-LUMA4-PREPARATION.md) retains 5,639 full/live checks per OS
and actual-package/all-three-example proof, plus the two separately qualified
production-row additions. The full282-path objective remains open; publication
is postponed. Diagnostic telemetry is outside production accounting.

[Evidence](vp8-trellis-preparation-audit-evidence.json) pins complete observations,
source/producer identities, guards and qualification reuse. Reproduce with retained
vtp305 scripts/current owned sources, instrumented replacements/driver, compiler
response, assembly, original diagram/manifest and frozen packet receipt.

Raw `vp8-trellis-preparation-audit.tar.xz`: 635,740 B / 264 members,
SHA256 `ad27bf589e6a51ada533bb46ca6ddb60a2b0adf267eff4156adfd4b51a6c4664`, mirrored/member-verified locally and on the VM.
The pinned qualified dependency retains unchanged runtime/source/package proof.

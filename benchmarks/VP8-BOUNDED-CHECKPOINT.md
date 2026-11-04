# VP8 bounded-checkpoint investigation

A two-second diagnostic on the actual qualified producer records 1,969 samples,
369 in coefficient emission. Its 3,931-byte JIT body has 13 inlined cancellation
throw sites. Samples/skid are neither phase times nor predicted savings.

The prototype replaces per-boolean dispatch with bounded coefficient/mode/header
checkpoints, retaining borrow, literal, word publication, carry, admission/growth,
finish and delivery checks. No packet/event, allocation, admission or ownership
policy changes. Both existing complete controls retain original inputs/settings,
normalization, fresh managed output acquisition and disposal. The native worker
retains codec/frame/packet wrappers; repacking and complete encoding stay timed.

| Existing complete encode, three actuals | Baseline µs | Candidate µs | Candidate ratio / upper | B/op, unchanged | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Opaque RGBA16 | 20,207.62 | 20,546.67 | 1.320 / 1.366 | 1,201,552 | 72,380 |
| Opaque photograph | 13,751.98 | 13,784.00 | 1.420 / 1.580 | 809,072 | 17,688 |

No useful short lead: opaque16 is adverse by point, photograph flat/noisy.
These are inconclusive measurements, not causal regression or parity proofs.
No independent confirmation or repeat of unchanged points follows.

Each OS/mode passes 818 internal/357 public focused checks, including exact
packets, writer limits, actual frame refusal/cancellation cleanup and surviving
ownership. A stale count assertion stops after the first 818 passes; remaining
checks resume on the same build without repeating them. No production test fails.

Qualified source, core/PDB/XML, internal assemblies and actual package are
restored byte-for-byte on both OSes, without builds. Reuse the accepted milestone's
5,569 full/live checks and same-package/all-three-example proof. Displayed costs
remain valid; no current performance row changes. Earlier
[local-state](VP8-COEFFICIENT-STATE-INCONCLUSIVE.md) and
[shared-event](VP8-SHARED-EVENT-STREAM-INCONCLUSIVE.md) variants also lack a useful
lead. Their unchanged-path evidence remains reusable; a materially different
design needs its own justification before more profiling or timing.

[Evidence](vp8-bounded-checkpoint-evidence.json) retains all actuals/native brackets,
both quiet guards, producer/source identities and restoration. Mirrored/member-
verified raw `vp8-bounded-checkpoint-investigation.tar.xz`: 2,621,728 B / 221 members,
SHA256 `906a95281c102a6a06553711d6225dd48c54b6fe5c43a33b7c1cfbb4779b6256`. Raw capture/JIT/instructions, isolated producers,
adverse/flat runs, sources and reproduction remain. Pinned dependencies preserve
qualified actual packages and the unchanged native reference. No new image,
reference/harness/selector/package build, favorable resampling or publication.
The full 282-path objective remains open; revised architectures remain eligible.

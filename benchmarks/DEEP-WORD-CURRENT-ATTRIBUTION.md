# Deep word resizing: current attribution

Current follow-up: [paired word weights](DEEP-WORD-VECTOR-WEIGHTS.md).
Costs and next-action statements below retain their original checkpoint.

Bounded513×259→32×16RGBA16 is now the largest remaining resize gap:
553.89/zimg446.20µs,upper1.268,114B/op. Its source-valid complete cost is reused;
the first actual-current-package profile changes no performance claim.

One two-second CPU0operation/CPU1sampler capture records1,997locations:
1,957in the3,050B DeepWord core and36in its488B publisher. Instruction mapping
identifies horizontal unsigned decoding, paired weight construction and ordered
filtering, rather than costly exact repair. Inlining/skid locations are not phase
times, CPU forecasts or removable percentages.

The bounded next stage replaces an eight-scalar repeated weight construction with
two broadcast halves, already used in shared kernels. This preserves every lane
bit and performs no new floating arithmetic. Keep coefficient/sample/tap order,
cache/geometry/normalization, exact certification/repair and4096B preadmission.
The old file must reconstruct after one expression substitution. Existing arrays
remain4032B, no retained storage or API; physical/JIT peaks are unmeasured.
JIT register selection may offset instruction simplification; complete evidence
decides. Broader multi-column/source-row reuse remains an architectural lead if
a material gap remains, not a reason to keep an ineffective implementation.

All original exact/native/settings/source/owner/idle-v2 gates pass. Instrumented
elapsed is discarded. Reuse current immutable package/build/fixtures/harnesses,
6,480checks perOS and SAMEpackage/all3examples: zero builds, new workloads,
statistical captures or qualification/package repeats. The VM source view is
reused without resynchronizing unrelated documentation.

[Evidence and bounded hypothesis](deep-word-current-attribution-evidence.json)
retain source/module identities, JIT/instruction mapping, samples and reproduction.
Raw `deep-word-current-attribution.tar.xz`, SHA256
`8339fb0bf7510ba603461275b3f89d45d50bdbdc0d838f172a934b8139194b5b`
(103,000B/27members), is mirrored/member-verified and
depends on the qualified byte-overlap round. All289paths remain required;
publication is postponed.

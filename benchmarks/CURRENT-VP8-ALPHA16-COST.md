# Current RGBA16 VP8-with-alpha cost

The existing 513×259 word-input control independently confirms a **1.311× gap /
upper 1.365×**, with an exact **98,590-byte packet**. This fills missing current
evidence without changing production or making a speedup/regression claim.

| Complete warm one-CPU operation | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Packet B, both |
|---|---:|---:|---:|---:|---:|
| RGBA16 → VP8 + alpha | 71,441.148 | 54,514.339 | 1.311 / 1.365 | 21,350,352 | 98,590 |

Both subjects normalize every original UInt16 channel with `(sample+128)/257`
inside each operation, encode VP8/4:2:0 and compressed alpha at the frozen
Q75/effort4 settings, then acquire and release fresh independent output.
Exact complete packets establish equivalence; equal numerical quality alone
does not. The original word raster, normalized raster, reference packet, recipe
and worker remain frozen. Native allocation is unmeasured.

All twelve actual iterations, filtered/unfiltered confidence, both native
brackets, source/output ownership, exact input normalization and alpha,
reconstruction checks and idle-v2 guards pass. The conservative lower bound is 1.245. Old timing passes used
different encoder implementations and packets, including 471,436 B; they remain
historical. Subsequent packet-exact encoding and QIV1 quantization invalidate
those CPU observations for the current path. Existing valid RGBA8, opaque-word,
photograph, diagram and unrelated confirmations are reused without retiming.

Zero production/harness/host/native/selector builds, fixture regeneration,
new workloads, profiles or repeated full/package checks. The cached selector is
recovered from its verified original archive and runs beside the same immutable
current DLL. Applicable 5,369 checks/OS, 948 live comparisons and the SAME actual
Release package/three examples remain valid. One launch failed before warmup
because an environment filter removed the FFmpeg path; it is retained. Correcting
that launch configuration produced the single valid twelve, without rescoring.

This adds an existing required comparison to the summary, now 104 rows in three
tables. All 268 required paths remain in scope. This case is a confirmed gap;
full warm parity and replacement readiness remain open.

[Evidence](current-vp8-alpha16-cost-evidence.json). Mirrored/member-verified
current-vp8-alpha16-cost.tar.xz, 2,607,988 B/59 members,
SHA256 561f8ade1794219e4cce48fc02f223d83c97c0b96e9cfed0841ec1c6dcf910fb.
Raw retains every sample, guard, receipt, the failed launch, actual cached
binaries/selector/worker, original fixtures and pinned source/qualification archives.

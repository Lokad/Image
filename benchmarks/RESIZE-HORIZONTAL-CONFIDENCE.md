# Horizontal enlargement confidence

The existing 513×259 RGBA8→1027×259 bicubic control passes on the unchanged
qualified package: **1,431.47 / prepared-zimg 1,514.24 µs**, ratio 0.945,
conservative range **0.934955–0.953372**. Stop this passing warm control.
This is confidence evidence, with no production change or CPU-gain claim.

One capture retains 12 alternating managed/native pairs of 64 complete operations
and 12 native batches before/after. Native bracket means are
1,514.89/1,518.18/1,514.24 µs; the fastest governs the result. All actuals,
paired-log confidence and conservative all-actual/diagnostic bounds remain.
The earlier standalone 1,486.10/1,518.51 µs, range 0.854–1.025, stays separately
visible with its post-GC conditioning. No broader conditioning or arbitrary
repeat-count claim follows, and historical means are not subtracted into phases.

Managed timing includes complete public Resize, geometry, filtering/exact repair,
independent output materialization/disposal and delegate dispatch. The explicitly
labeled FFmpeg zscale **zimg backend** retains graph/input/scratch; normalization,
halo, filtering, packing and independent pixel acquisition/release stay timed.
This is the existing prepared-reference boundary, not equivalent fresh-graph
or FFmpeg CLI performance. Setup, fixture decoding, JIT, validation and IPC are
excluded. One CPU0/thread, optimized JIT, nonconcurrent workstation GC and idle-v2
guards pass.

Frozen inputs/settings, exact managed rational/qualified-original pixels, approved
native bounds, source immutability and independent owners pass. Output SHA256:
`ef9e67095fcbc4a0d9a4fb2591577e9141a188e6b6323a44073cfd2ea41b7724`.
Direct allocation is 112 B/op in every pair; the older 114 B BDN boundary does
not establish savings. Output payload remains 1,063,972 B, admitted scratch 4 KiB,
retained operation scratch zero. Existing default pool/ownership behavior and
two-array/4 MiB ceiling are unchanged; occupancy and physical/native/JIT peaks
are unmeasured.

Reuse the [forwarding milestone's](PNG-STORED-FORWARDING.md) actual package,
6,517 Windows/Linux full/live checks, SAME-package/all-three-example consumers
and unchanged resize ISA/storage qualification. Zero production, embedded/public
tests, driver, harness, native, fixture or package builds and no broad repeat.
Current 73 named warm controls have 66 passes/zero gaps/seven uncertainties, with
this case's adjacent conditioning stated explicitly. All 289 required paths and
postponed publication remain; uncertainties are not presumed optimization targets.

[Evidence](resize-horizontal-confidence-evidence.json) pins current sources,
module/package/reference identities, every actual and original quality/owner gate.
Raw `resize-horizontal-confidence.tar.xz`, SHA256
`28fabd85d6751b402fafb87a301f688c9e127d9e50b629ac519437e84ac8a538`,
36,120 B/19 members, is mirrored and member-verified; dependencies preserve the
frozen driver, workers, original standalone evidence and actual package.

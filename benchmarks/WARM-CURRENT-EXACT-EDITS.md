# Current single-owner exact-edit evidence

The shared overwrite-storage pool change affects complete crop/clone/permutation
operations even though their pixel kernels retain valid witnesses. Resolve each
named operation on the existing immutable qualified production build, then stop
the passing pair. No production optimization is needed for the confirmed cases.

| Existing fixed case | Lokad.Image / FFmpeg µs | Ratio / upper | Managed B/op | Actual / selected |
| --- | ---: | ---: | ---: | ---: |
| Clone RGBA8 | 9.50 / 7.95 | 1.195 / 1.209 | 96 | 12 / 11 |
| Horizontal flip RGBA8 | 11.64 / 9.52 | 1.223 / 1.235 | 128 | 12 / 10 |
| Clone RGBA16 | 19.06 / 17.02 | 1.120 / 1.141 | 96 | 12 / 11 |
| Horizontal flip RGBA16 | 21.83 / 27.03 | 0.807 / 0.811 | 128 | 12 / 12 |

4/20 named single-owner cases currently have independent confirmations.
The separately confirmed two-live-output flip is reused and is outside this
single-owner count. The two original 513x259 byte/word rasters, fixed crop and
permutation geometries, native workers and exact output hashes are unchanged.
No new image, shape/depth grid or composed edit matrix is introduced.

Each managed call includes public admission/metadata, output-storage acquisition,
complete pixel materialization and disposal. Native public FFmpeg graphs reuse
graph/source/frame wrappers, but acquire independent owned output pixels and
release them every operation. View-producing filters include timed allocation
and full copying; no shared view is substituted for owned output. Padded native
rows are exposed directly without a redundant packing copy. Exact RGBA including
alpha and hidden RGB, placement, source preservation and simultaneous independent
owners pass before/after timing. Existing normal/scalar CLI checks are preserved.

Each case has a three-actual screen followed by an independent twelve-actual
confirmation on unchanged production/harness/native binaries. Every actual is
retained; conservative uppers cover selected/all-actual intervals against the
faster native bracket's lower confidence bound. Both versioned quiet guards pass
every scored run. Sequential Release uses CPU0, one filter thread, normal SIMD,
tiering disabled and workstation nonconcurrent GC. Managed allocations include
harness costs; native allocations are unmeasured. Startup, IPC, I/O, generation,
hashing and correctness checks are outside measured intervals.

Zero production, harness or native rebuilds are needed. Existing single-case
selectors require two selection drivers per case; horizontal flip shares its
two existing dual-depth drivers. Unchanged cached children,
including WarmDry, receive the verified production/harness assemblies. Existing
qualification remains valid: 3,483 Release tests per OS including 898 live
comparisons, plus the consumer and all three examples through the same actual
Release package on Windows/Linux. No repeated qualification, profile or
whole-library method audit. Horizontal flip reuses its existing exact normal/
scalar CLI controls against the same input/output hashes and unchanged native
worker; current full managed/native exact-pixel and ownership checks still run
before/after every measurement.

The first clone8 screen's private postcheck expected the decoder-style output
byte count. This older filter worker reports zero in that optional sample field,
as in its prior receipt. Exact full output geometry/pixels and independent
ownership are checked separately. The postcheck now requires the original value;
the original refusal/configuration, timings, guards and receipt are retained.
No timing rerun or workload change was needed.

[Numeric evidence](warm-current-exact-edits-evidence.json) retains the current
samples, allocations, guards, original witnesses, output/native/binary identities,
postcheck correction and reused qualification. Stop confirmed pairs. Outstanding
named single-owner edits, bicubic reconciliation and identification boundaries
remain open; these results do not establish uniform required warm parity.

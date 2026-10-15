# Cancelable RGBA8 PNG decoding at the working-memory limit

Complete exact-memory live-cancelable RGBA8 decoding improves from **2.475x**
FFmpeg to independently confirmed **1.867x / upper 1.873x**, with unchanged
**4,800 managed B/op**. Managed time falls25.0%, to2,559.30µs. Exact samples,
alpha/hidden RGB, admission, cancellation and atomic bit-work contracts remain intact.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Accepted three-actual screen | 2,561.52 | 1,371.55 | 1.868 / 1.951 | 4,800 |
| Independent twelve-actual confirmation | 2,559.30 | 1,371.11 | 1.867 / 1.873 | 4,800 |
| Noncancelable exact-limit regression screen | 2,857.30 | 1,465.13 | 1.950 / 2.814 | 4,800 |
| Default decoding regression screen | 2,034.33 | 1,368.65 | 1.486 / 1.705 | 4,456 |

One six-second profile places33.5% in general framing,17.6% in cancelable literal
counting and29.5% in BCL native inflation. The bit reader now keeps exact admitted
work locally for each compressed block and publishes completed charges on every
exit, including failure/cancellation, without a cleanup poll. A separate counting
loop removes materialized-output dispatch from this path. Original cancellation
polls, token validation, history/window bounds, work and error precedence remain.
No heap workspace, unsafe code, native binding, dependency or output cache is added.

Local work accounting alone confirmed2.153x/upper2.258x and was not accepted;
it received no full suite/package qualification. Its observations remain retained.
The final candidate passes both shared regression means before full qualification.
These are screens; the exact-limit screen's upper remains above2. Its
[earlier independent confirmation](WARM-PNG-TIGHT8.md) stays linked rather than
being presented as a new current-source certification.

The original 513x259 RGBA8 raster and frozen None/Optimal PNG are unchanged.
The logical limit remains1,235,173 bytes: input+IDAT+output+262,144-byte inflater
allowance+4,104-byte rows. Optional whole-filter/8KiB read buffers cannot fit.
One byte less is rejected. A live token is never signaled during timing;
pre-canceled rejection and token identity are checked outside it.

Both subjects time complete identical-input decoding and independent output
acquisition/release, including managed admission/framing/BCL inflation. Native
codec/input packet/frame wrappers are reused, with complete owned decoding/release
each operation, no conversion and no corresponding caller admission limit. No
output is cached. Startup, I/O, IPC, validation and immutable-option creation are
excluded. Exact normal/scalar native pixels, source preservation and two live
owners pass. Native allocations are **unmeasured**.

Sequential Release runs use one pinned CPU, normal SIMD, one codec/conversion
thread, disabled tiering and workstation nonconcurrent GC. Both quiet guards
pass on every retained run. All twelve final actuals are selected and retained.
The conservative upper includes selected and all-actual intervals against the
faster native bracket's lower bound. Only three workload types and the original
byte raster are used. Production/harness/tests remain frozen across final runs
and qualification; the harness/native worker are not rebuilt for production-only trials.

**3,374 Release tests per OS** pass:1,090 internal+1,386 public+898 live comparisons.
Focused113 framing+127 public PNG checks pass normal/scalar on both OSes, plus
two live checks each. The same actual Release package's consumer and all three
examples pass on Windows/Linux. An independent three-block/every-budget control
checks work publication, exact output, errors and BCL inflation. Measured and
qualified owned inputs are identical; publication documentation is updated afterward.

[Numeric evidence](warm-pngtightcancel8-evidence.json) retains all actuals,
allocations, guards, profile fractions, rejected trial, source/assembly/job/native
identities and exact package qualification. Stop this confirmed pair; remaining
material/current-source coverage and unmatched references stay open.

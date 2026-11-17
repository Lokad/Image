# Prepared vertical-copy execution

Both original owned vertical flips now **pass and stop** on the final immutable
timed/package DLL. Reuse the existing rectangle row helper; no new kernel,
workspace, pool, pin or API is introduced.

| Complete original 513×259 operation /FFmpeg owned output | Managed /reference µs | Conservative upper /status | GC B/op |
| --- | ---: | --- | ---: |
| RGBA8 vertical flip | 8.915508 /9.146290 | 0.988240 /pass | 56 |
| RGBA16 vertical flip | 18.762985 /19.311144 | 0.975846 /pass | 56 |

Controlled complete pairs on the same final DLL independently establish the
scoped effects: RGBA8 **8.945790→8.705113 µs**, saving **0.240677 µs /2.69%**,
ratio interval **0.970400–0.975799**; RGBA16 **18.090952→17.117228 µs**, saving
**0.973724 µs /5.38%**, interval **0.944773–0.947581**. Do not add private or
historical gains. The private byte confirmation passes; its word confirmation is
inconclusive at upper 1.000026. All earlier observations remain.

For Vector512 rows at least 256 bytes wide and at most 1,024 pixels, the existing
`CopyRows` prepares bulk/tail geometry once and traverses opposite row steps.
This removes repeated pixel getters, chunk selection and tail preparation. The
helper stays verbatim, including grouped loads/stores, bounded vector ends and
row checkpoints. These widths already require one cancellation chunk per row.
Wider, small and portable execution, output admission, failure cleanup, orientation
and independent ownership remain. Earlier getter-hoisting/flat-row/runtime-copy
trials retain per-row setup or change the kernel; this is shared prepared execution.

Every original frozen input/output hash, full sample, alpha, hidden RGB, source,
CLI/scalar and surviving-output check passes. CPU0 Release, natural nonconcurrent
GC, twelve alternating pairs, fixed 16,384-operation warmups/batches, three
interleaved reference groups and idle-v2 guards remain. Complete public validation,
admission, output materialization and disposal stay timed. FFmpeg retains its
graph/source/frame wrappers and allocates/copies/releases an independent output
from its negative-stride view each time. Setup, JIT, checks and IPC stay outside.
The [reference-scheduling correction](OWNED-FLIP-INTERLEAVED-REFERENCE.md) remains
explicit; no reference upgrade, fixture change or favorable retry occurs.

Allocation remains **56 B/op**, with no added workspace or retained operation
scratch. Source/result payload is 531,468 B per byte frame or 1,062,936 B per word
frame. Actual caller-governed result capacity stays charged until disposal;
governed calls bypass the unchanged four-array/4 MiB shared idle pool. Allocation,
live admission and idle retention are distinct; physical/native/JIT peaks remain
unmeasured.

Windows/Linux each pass **6,823 full/live checks** (3,363 internal,2,512 public,
948 differential), plus **10 internal/190 public** checks per normal,
AVX-512-disabled and scalar mode. Four new cases cover both depths/tokens,
eighteen widths including 1,024/1,025, tails, cancellation, aggregate refusal,
release, recovery and independent survivors. Existing 95-pixel packaged vertical
copy controls exercise the new path at both depths. The SAME actual Release
package and all three examples pass both OSes.

Each final core/embedded assembly builds once per OS; public/harness/native/
fixture builds are reused. Affected performance reconciles before broad checks;
one NoBuild Release pack contains the unchanged timed DLL. Original PNG pipelines
use normal orientation, so their passing paths stay stopped. Other codec, resize,
font/text, drawing and copy bodies remain unchanged and retain identified evidence.
Twenty owned-edit/composition views now pass; the asymmetric bilinear diagnostic
and separate font/text/copy requirements keep the full 289-path objective open.
The final complete 165-comparison immutable-package refresh remains outstanding.

[Evidence](vertical-prepared-rows-evidence.json) pins sources, exact artifacts,
all private/final observations, memory boundaries and qualification. Mirrored/
member-verified `vertical-prepared-rows-qualified.tar.xz`:
**3,668,888 B /167 members**, SHA256
`a6b2dba621c42eb4eaafaaee9490728bb72966e268a010f5d74da475e6692ca8`.
Archived `vpr1228-*` recipes and declared dependencies reproduce the round.
Publication remains postponed; published packages are unchanged.

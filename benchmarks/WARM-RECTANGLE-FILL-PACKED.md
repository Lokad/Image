# Exact packed rectangle fills

Both required fill controls pass the ≤1× conservative-upper target and stop.
Native-order complete-pixel filling reduces elapsed time by **94.1% for RGBA8**
and **88.3% for RGBA16**, with unchanged **56 B/op** and exact full canvases.

| Complete clipped fill | Previous Lokad µs | Current Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op |
|---|---:|---:|---:|---:|---:|
| RGBA8 | 20.202 | 1.188 | 1.565 | 0.759 / 0.775 | 56 |
| RGBA16 | 19.576 | 2.291 | 4.196 | 0.546 / 0.567 | 56 |

Use the existing original 513×259 canvases. Request (-17,19,273,129), clipped to
(0,19,256,129), with RGBA8 (23,57,91,128) or RGBA16
(12345,23456,34567,45678). The byte color is supplied as its exact 16-bit expansion.
Both subjects include clipping, required color normalization and complete
mutation of a borrowed canvas. Managed admission, validation, cancellation and
scope cleanup remain included. Native public FFmpeg av_image_fill_color performs
its normalization and clipping inside every operation. Canvas preparation/reset,
startup, IPC, I/O and verification are excluded equally; identical overwrites are
idempotent. Neither subject allocates an output or copies a whole frame. Native
allocation is unmeasured.

One initial independent twelve per depth exposes 12.84× / 4.97× gaps. One byte
short then one independent twelve qualifies this single primitive; one current
word twelve completes its finite affected scope. Raw/all-actual and normalized
confidence separate for both gains. Both idle-v2 guards, all original full-canvas
hashes, alpha, hidden RGB, untouched pixels and ownership checks pass. All actuals
and native pre/post brackets are retained. No profile, score rerun, extra image,
shape/color grid or further width/chunk/ISA variant is needed.

After the existing admission and conversion, read one native-order 32/64-bit pixel
from four/eight stack bytes. Span.Fill then replicates complete pixels through
the runtime's SIMD or portable kernel. Four scalar channel stores per pixel are
removed. Existing 4096-pixel chunks/checkpoints, clipping, rounding, alpha,
memory bounds and ownership stay intact. No unsafe production code, dependency,
heap/frame allocation or cache is added.

Both platforms pass **5,378 checks each**, including 948 live comparisons and
72 focused public checks in normal/scalar modes. Two new behavioral cases verify
wide clipped fills across multiple chunks, hidden RGB at zero alpha, tails and
untouched pixels at both depths. The **same actual Release package** and all
three usage examples pass both platforms. Its consumer verifies frozen partial/
zero-alpha full canvases with shared admission limited to exactly one canvas,
no extra reservation, caller ownership and final release to zero.

Only FillRectangle changes. CopyRegion, Composite and private helper source
remains identical. Their costs, the original 268 codec/geometry/read controls and
other text/drawing evidence remain applicable. Four audited drawing controls
now have current costs; ten remain unmeasured. The summary has 109 selected
comparisons in three tables. These two passes do not establish full warm parity.

[Evidence](rectangle-fill-packed-evidence.json) records all samples, confidence,
scope, producers and actual package hashes. Retained
rectangle-fill-packed-qualified.tar.xz: 2,340,816 B / 160 members,
SHA256 93a1e91bdf349d7af12cde4a8300873dfeabfe5cb6d2a327e24b3262e2351e27;
mirrored and member-verified, with its retained copy-round dependency.
Actual producers/drivers, source, original inputs, worker, protocol controls,
guards and full/package receipts are archived. Reproduce with
build-fill-rectangle-linux.sh, verify-fill-rectangle-linux.py and the archived
immutable selector/commands. Fresh tooling selects --warm-family FillRectangle
with --warm-cases DrawingFill8 or DrawingFill16.

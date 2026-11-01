# Exact rectangle copies with borrowed spans

Borrowing validated pixel spans once reduces complete rectangle-copy time by
**24.5% for RGBA8** and **17.4% for RGBA16**, with unchanged **56 B/op**.
Both independent comparisons separate raw/all-actual and native-normalized
confidence. RGBA8 remains a confirmed gap; RGBA16's interval crosses parity.

| Complete copy | Previous Lokad µs | Current Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op |
|---|---:|---:|---:|---:|---:|
| RGBA8 | 2.818 | 2.127 | 1.933 | 1.100 / 1.114 | 56 |
| RGBA16 | 4.709 | 3.888 | 3.828 | 1.016 / 1.052 | 56 |

Only the existing original 513×259 rasters are used. Copy the 256×129 rectangle
at (127,65) to (17,19) in a separate existing canvas at the same depth. Both
subjects borrow resident source/canvas storage and mutate it completely.
Managed admission, validation, cancellation checkpoints and scope cleanup are
included. The native reference uses public FFmpeg av_image_copy_plane.
Preparation, reset, startup, IPC and verification are excluded equally; repeated
identical overwrites are idempotent, so neither subject needs a reset.
Neither operation allocates an output or copies a whole frame. Native allocation
is unmeasured. Full canvas/source hashes, alpha, hidden RGB and untouched pixels
are exact before/after each run.

These are the first two measured controls from the bounded text/drawing audit,
within its 282 required paths. One baseline twelve per depth establishes the gaps.
One byte short then one independent twelve qualifies the single primitive; the
word companion receives one current twelve. All actuals, both native brackets,
filtered/unfiltered confidence and both idle-v2 guards are retained. No score is
repeated. Driver setup repairs and a verified orphan MSBuild-node refusal occur
before scoring; completed builds/checks are reused.

The implementation borrows packed byte spans and strides once, eliminating
per-chunk row/depth/span derivation. Byte copying preserves both sample depths
exactly. Clipping, reverse row/chunk traversal, overlap-safe copying, 4096-pixel
chunks, cancellation, admission and ownership stay intact. Existing tests cover
overlap beyond two chunk boundaries at both depths, clipping and failure cleanup.
No new production allocation, unsafe code, native dependency or cache is added.

Both platforms pass **5,376 checks each**, including 948 live comparisons and
70 focused public checks in normal/scalar modes. The **same actual Release
package** and all three usage examples pass both platforms. Its new consumer
checks frozen full byte/word canvases with the shared budget limited to exactly
the two resident image buffers, unchanged source, surviving canvas after source
disposal and final release to zero.

Only CopyRegion changes. Its two baseline costs are superseded; overlapping-copy
CPU evidence is still missing. Existing codec, geometry, fill, composite and text
production paths remain unchanged and reuse valid evidence. The summary has
107 selected comparisons in three tables; twelve audited paths remain unmeasured.
Stop this span-hoisting round without width/chunk/inlining variants or repeated
word timing to chase significance. Full warm parity remains open.

[Evidence](rectangle-copy-spans-evidence.json) includes samples, costs, confidence,
ownership, qualification, source/producer hashes and the exact package identity.
Retained rectangle-copy-spans-qualified.tar.xz: 2,327,012 B / 182 members,
SHA256 6d2df323037db0f6508240e425d94353af3e54b14dddb89530726120b60a91a5;
mirrored and member-verified, with its audit/BPR48 dependencies. The archive
contains actual immutable producers, drivers, native worker/source, original
inputs, raw logs/brackets/guards, full/package receipts and failed preparation
logs. Reproduce using build-copy-region-linux.sh, verify-copy-region-linux.py
and the archived commands/selector; reuse the verified immutable build.
A fresh standalone run selects --warm-family CopyRegion with --warm-cases
DrawingCopy8 or DrawingCopy16.

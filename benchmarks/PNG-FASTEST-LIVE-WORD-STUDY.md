# Live-word Fastest PNG: attribution and hash trial

PLW552/PFH553, 2026-11-08. **Unselected; production restored exactly.**
[Evidence](png-fastest-live-word-study-evidence.json) retains the complete capture,
trial, short screen, source/module snapshots and restoration. Current production
costs remain in [the original Fastest comparison](CURRENT-PNG-FASTEST-COSTS.md).

One two-second profile of the unchanged complete live RGBA16 encode has 1,937
samples: 1,473 in ProcessFast and its inlined helpers, 162 in token writing, 85 in
block preparation, and 156 in Huffman construction/helpers. These locate work,
not precise phase times or predicted savings. Instrumented elapsed is discarded.
The existing adaptive selector, frozen inputs, native worker and current immutable
DLL are reused with zero builds. CPU0 operation/CPU1 sampler, idle-v2, full packet,
pixel, alpha, source, ownership and fresh one-shot reference gates pass.

The bounded trial reuses the already qualified lazy sequential-hash helper in the
greedy sweep and short-match backfill. Two sweep-local integers reseed after skips
and slides, removing two repeated input-byte reads on consecutive insertions.
It leaves matching, chain order, ties, depth, tokens, publication and cancellation
checkpoints unchanged. Earlier rolling prototypes predate the acquired views and
the successful lazy-only recurrence; their results remain separate.

The live-word three-actual screen is **19,249.25 / reference 18,662.26 µs**,
ratio 1.031, upper 1.060. The original's two-launch measurements are approximately
18,800 µs. This is unpromising, without an independently confirmed causal loss or
equivalence claim. No byte/plain expansion, independent candidate confirmation,
favorable repeat, broad test suite or package run follows.

Both OSes pass 43 internal / 246 public focused checks in each normal/scalar mode,
including exact packets, slides/overlap/origins/chunks, cancellation, writer failure
and shared admission. One immutable main and embedded-test build per OS; no
harness, selector, native, fixture or public-test build. The complete 968,216-byte
packet remains exact. Admission stays 384 KiB, retained scratch zero. The short
GC ceiling 2,437,586 B/op versus original confirmation 2,437,660 reflects batching;
it does not establish an allocation saving. Physical/native peaks are unmeasured.

Original sources, main/embedded modules, bindings and Linux manifest are restored
byte for byte without rebuilding. The actual package stays unchanged. Reuse the
accepted 5,879 full/live checks per OS, affected scalar and same-package/three-example
proof, including the newest Fastest consumer checks. A restoration-script syntax
failure occurred before any mutation; the corrected script and failure log are
retained. No benchmark or test was repeated because of it.

Ignored raw `png-fastest-live-word-study.tar.xz`, SHA-256
`8ecf5f3c99828f9fad701efd2180e75487d814a6a52c0c598dc025942980eb0d`
(11,594,980 B / 187 members), is mirrored and member-verified. It pins the current
producer, original inputs/packets, capture/JIT/commands, all actuals/brackets,
source/build/check logs and restoration. It supplies no displayed result and is
not a renderer dependency. No general algorithm-family ban follows.

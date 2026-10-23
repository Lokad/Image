# VP8 input preparation

Private managed preparation now matches the pinned backend's ordinary undithered
RGBA-to-YUVA samples and transparent-area cleanup. This is an intermediate stage; the public lossy encoder is
unchanged and complete VP8 packet replication remains open.

The seven existing encoding rows have four reference packet identities:

| Original 513Ã—259 input | Existing rows | Complete reference bytes |
|---|---|---:|
| Opaque procedural | RGBA8 / RGBA16 | 72,380 |
| Alpha procedural | RGBA8 / RGBA16 / work-budget boundary | 98,590 |
| Photograph | RGBA8 | 17,688 |
| Text/diagram | RGBA8 | 7,100 |

These remain FFmpeg n8.1/libwebp1.6.0, quality75, method4, preset none, one codec
thread and BGRA delivery. Three missing loose reference packets were materialized
at their unchanged original settings; all preexisting complete hashes matched.
The retained alpha packet was reused. Earlier managed quality anchors describe
their producers; they are not behaviors to retain or new quality-search targets.

The first numerical divergence precedes transform coding: the current opaque
luma formula differs at9,153 of132,867 samples. At `(3,0)`, RGB `(144,235,72)`
produces178 versus179. The new primitive generates its fixed-point matrix from
BT.601 weights and inclusive limited-range code counts. Gamma knots and bounded
alpha reciprocals are derived mathematically; no upstream table is incorporated.

All logical Y/U/V/alpha streams match six native observations: the four original
rasters plus existing singleton and two-color inputs. The last two isolate tiny
edges and fully transparent gamma reduction. The original two16-bit procedural
inputs normalize to the same native streams, byte for byte. Managed tests freeze
the complete stream hashes and check replicated macroblock padding independently.
The observer uses libwebp's public conversion API before transparent-area cleanup,
matching the stage called by the pinned FFmpeg backend. Cleanup is qualified separately below; neither these samples nor smaller packets
imply complete encoding.

One admitted output buffer owns padded planes and optional exact alpha. The source
is borrowed and normalized on access, without a normalized-frame copy. Temporary
gamma workspace is charged and released before returning; retained capacity stays
charged until disposal through the same operation/root. The initial13focused checks
passed on Windows/Linux with the same assembly, normal and SIMD disabled, including
shared output/scratch refusal, recovery, cancellation and late work cleanup.

[Structured evidence](vp8-byte-parity-evidence.json) records native/managed hashes,
original source identities, exact scopes and raw receipts. No public codec body
changed, so the completed E62 full/differential/package proof remains applicable
to its frozen producer. No timing, allocation measurement or new packet equality
is claimed.

Transparent-area cleanup now matches native `exact=0` preprocessing in place.
Mixed8×8 blocks smooth only invisible luma; full transparent blocks share the
first block's Y/U/V across a horizontal run. Visible blocks and new bands reset
that run; incomplete edge blocks only smooth. One original33×17 control covers
this distinct full-block branch omitted by existing short transparent controls,
including run reuse/reset and partial edges. It adds no timed case or settings grid.
Alpha and visible luma stay exact, and padding is regenerated from cleaned planes.

The current same assembly passes25checks per platform, normal and SIMD disabled:
all13conversion checks plus12cleanup checks. Cleanup adds no storage or admission
calls. Retained owners block another operation on the shared root until disposal;
cancellation/late work failure keep storage charged until that owner releases it.
No public encoder body changed. Next are integer transforms, quantizers and
macroblock decisions, then entropy/framing, preserving the four fixed targets.

# VP8 encoder preparation

Private managed input conversion, transparent-area cleanup and forward4×4DCT
match the pinned backend at their declared stages. The public lossy encoder is
unchanged; complete VP8 packet replication remains open.

The seven existing encoding rows have four reference packet identities:

| Original 513×259 input | Existing rows | Complete reference bytes |
|---|---|---:|
| Opaque procedural | RGBA8 / RGBA16 | 72,380 |
| Alpha procedural | RGBA8 / RGBA16 / work-budget boundary | 98,590 |
| Photograph | RGBA8 | 17,688 |
| Text/diagram | RGBA8 | 7,100 |

References remain FFmpeg n8.1/libwebp1.6.0, quality75, method4, preset none, one
codec thread and BGRA delivery. Materializing three missing loose packets at
unchanged settings reproduces their preexisting complete hashes; the alpha packet
was reused. Earlier managed quality anchors describe their historical producers.

Ordinary undithered conversion matches six complete native logicalYUVA streams:
the four original rasters and existing singleton/two-color inputs. The original
two16-bit procedural inputs normalize to identical native streams. Matrix weights,
gamma knots and bounded alpha reciprocals are generated mathematically; no upstream
table is incorporated. The current public opaque luma formula differs at9,153 of
132,867 samples, before any transform coding. The private primitive matches native.

One admitted output buffer owns padded planes and exact alpha; the source is
borrowed and normalized on access. Gamma workspace releases before returning;
retained output stays charged until disposal through the same operation/root.

Cleanup matches native `exact=0` preprocessing in place. Mixed8×8 blocks smooth
invisible luma; complete transparent blocks share their firstY/U/V across a
horizontal run. Visible blocks and new bands reset the run; incomplete edges only
smooth. One original33×17 control covers this omitted complete-block branch,
including resets and partial edges, without a new timed case or settings grid.
Alpha and visible luma remain exact; padding extends the cleaned logical planes.
Cleanup adds no storage or admission calls. Retained owners block another shared
operation until release; cancellation/late work keep storage charged until disposal.

The forwardDCT matches all215,424 native coefficients from13,464 paddedY/U/V blocks
of the opaque control against neutral128 prediction. Thirty-six original
signed/extreme arithmetic fragments also match, including native zero-residual
rounding at index1. Native SIMD/scalar coefficient streams agree byte for byte.
This qualifies transform arithmetic, not actual encoder predictor/quantizer choices.

The DCT uses independent integer matrix projections and generated sine/cosine
weights. Borrowed input/output may overlap; fixed64-byte stack scratch has no
retained lifetime or per-block admission machinery. The encoder admits work and
cancellation at its block boundary. The same assembly passes64checks per platform,
normal and SIMD disabled, including all25conversion/cleanup checks. No public
encoder body changed; prior frozen producer evidence retains its stated scope.

[Structured evidence](vp8-byte-parity-evidence.json) records sources, native/managed
hashes and raw receipts. No new timing/allocation or complete-packet claim is made.
Next are Y2Walsh, quantizers and macroblock decisions, then entropy/framing and
ALPH coding, preserving the four fixed packet targets.

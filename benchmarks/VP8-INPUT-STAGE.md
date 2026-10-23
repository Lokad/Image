# VP8 encoder byte compatibility

The public managed encoder now uses the qualified reference-policy pipeline.
Twenty complete pinned libwebp1.6.0 packet identities match across 22 original
input/settings rows. This is a finite tested scope, with no WebP metadata
exception or claim about every possible image, setting or reference version.
The original sources and hashes are in `tools/Shared/OriginalVp8Controls.cs`;
[the evidence ledger](vp8-byte-parity-evidence.json) preserves all earlier
component milestones, native observations, producers and raw masters.

The principal controls use 513×259 inputs, quality75 and method4:

| Original input | Qualified source depth | Complete bytes, managed = reference |
|---|---|---:|
| Opaque procedural | RGBA8 / RGBA16 normalization | 72,380 |
| Alpha procedural | RGBA8 / RGBA16 normalization | 98,590 |
| Photograph | RGBA8 | 17,688 |
| Text/diagram | RGBA8 | 7,100 |

The named policy cases reuse those sources:

| Method | Diagram, quality75 | Alpha procedural, quality75 |
|---:|---:|---:|
| 0 | 9,980 | 93,386 |
| 1 | 9,244 | 106,588 |
| 2 | 7,330 | 102,582 |
| 3 | 7,082 | 102,502 |
| 4 | 7,100 | 98,590 |
| 5 | 7,026 | 98,602 |
| 6 | 6,818 | 85,796 |

Diagram method4 quality1/85/100 also matches 2,304/8,898/15,172 bytes;
method0 quality100 matches 27,668 bytes. Default options select quality85/method4.
These cases cover fast and full analysis, distortion decisions, RDO, selected-mode
and all-candidate trellis, high-quality DC behavior, and effort-dependent ALPH
compression. They do not form an exhaustive quality/depth/shape grid.

The reference remains FFmpeg n8.1/libwebp1.6.0 with frozen BGRA inputs, one codec
thread, preset none, SNS50, filter strength60, sharpness0, strong filtering,
autofilter0, pass1, preprocessing0, partitions0, partition limit0, alpha
compression1/filtering1/quality100, exact0, low-memory0 and sharp-YUV0.
Reference revisions, worker hashes and native configuration records are in the
ledger. No native source, table or packet is a production build input.

The single existing 5632×5632 opaque header-overflow control also matches its
complete 16,787,132-byte packet. E97 both-platform normal/scalar component evidence
remains valid; E99 actual-package public dispatch reproduces that packet. Its
explicit caller work allowance is512 billion units. Method6 alpha uses64 billion;
its default2 billion allowance and the existing32 million boundary correctly
refuse with structured Work failure. These are accounting units. No default was
increased, work discounted or fallback encoder introduced to make a case pass.

One admitted input owner normalizes samples on access. Shared admission follows
all nested owners; source/analysis/quantization/frame storage ends before output.
Owned partitions and ALPH remain charged through actual synchronous/asynchronous
stream use. Delivery makes no joined-frame or async staging copy. Caller roots,
inputs and streams are borrowed; surviving outputs have independent lifetimes.
Refusal, cancellation, IO failure, local/output limits and subsequent recovery are
qualified. Superseded unpublished encoder implementations and their experiment-only
tests are removed; useful public tests and the original decoder corpus remain.

E99 passes2,038 internal,2,092 public and940 live FFmpeg checks on each of Windows
and Linux, plus29 affected scalar checks per platform. The SAME actual0.2.0
package passes22 public VP8 rows, synchronous/asynchronous delivery, the existing
codec/resource/font consumer and all3 usage examples on both platforms. The
package is private qualification material, not a published release. Raw logs
identify the test-only quota correction and obsolete Linux source cleanup;
unchanged successful evidence was reused.

No current CPU time or GC allocation claim is established here. Earlier public
VP8 timing rows are labeled **Prior production; reconfirm** in
[the benchmark summary](../docs/benchmark.md). Exact decoder samples, the broader
functional scope and deferred warm single-CPU parity remain open. Passing encoder
cases are stopped; the next work audits existing decoder comparisons for exactness.

# Resource admission

## Caller-owned cooperative memory (0.2.0 development)

Set `ImageLimits.MemoryBudget` to an adapter implementing `IMemoryBudget` in
`Lokad.Image`. Forward the same borrowed root in nested/concurrent operations;
the package supplies no governor implementation or host dependency. Child scopes
share its allowance. Zero is valid, negatives throw, refusal is immediate and
leaves accounting unchanged, and disposed owners reject admission/resizing.
Scope/reservation disposal is idempotent and nonthrowing; scope disposal refunds
and invalidates descendants without canceling work or disposing runtime objects.

Owned storage is admitted before allocation, including retained capacities and
old/new replacement peaks. Caller inputs/budgets are borrowed; copies are charged
separately. Dispose images, fonts and prepared layouts to release their charges
promptly. Prepared outlines survive font disposal. Keep the supplied ancestor
alive and synchronize its disposal with every operation and retained result.
Async cancellation keeps storage charged until the actual I/O user finishes.
Governed calls use uncached storage and do not populate unowned global pools.
Minor allocations are approximate; keep headroom for runtime storage and delayed GC.

Refusal raises `ImageLimitException` (`WorkingBytes`, `SharedBudget`), stops the
operation and releases relinquished storage. It never waits, retries or selects
a fallback after refusal. Cancellation and allocation failure remain distinct.
Local limits and cumulative `ImageBudget` work quotas continue independently.

`ImageLimits` bounds one operation's admitted buffers and algorithm work. It is
not a process RSS, GC-heap, elapsed-time or global-cache quota. Supplied pixel or
encoded buffers, results and live codec workspaces count toward admission;
releasing admission does not force garbage collection. [SPEC.md](../SPEC.md)
defines the detailed codec and pixel contracts.

## Defaults

All limits must be positive. Codec format limits also apply.

| Limit | Default |
| --- | ---: |
| Maximum side length | 32,768 pixels |
| Maximum pixel count | 32,000,000 |
| Encoded input / output | 64 MiB each |
| Working memory | 512 MiB |
| Optional metadata | 4 MiB |
| Container items / EXIF directory entries | 100,000 |
| Charged work | 2,000,000,000 units |

Geometry limits apply to codec input and newly created outputs. A transform may
read a larger existing buffer to produce a smaller admitted result; that source
still counts toward working memory. Work units are deterministic accounting,
not CPU instructions or a time limit.
Optional codec eligibility does not promise that a whole operation fits its work
limit. Variable work is charged as performed; exhaustion stops the operation and
releases temporary storage without retrying another encoding strategy.

## Shared budgets

`ImageBudget` provides `Identify`, `InspectHeaders`, `Decode` and `EncodePng/Jpeg/Webp` entry points
with the same codec options, pixels and stream ownership. Calls reserve the full
configured `MaxWorkingBytes` and `MaxWork` atomically before codec execution.
Choose per-operation limits that fit alongside other live reservations; this
coarse admission can reject a call whose actual needs would have been smaller.
Completion refunds temporary memory and unconsumed work, including on failure
or cancellation. Charged work is cumulative across calls. Concurrent calls share
the allowance safely; each codec operation itself uses one CPU thread.

Decode returns a `BudgetedImage`. Use its `Image` and dispose the outer owner
after all pixel uses; its retained pixel charge then ends. Each result owns its
storage and reservation independently. Disposing `Image` directly leaves the
reservation held until the outer owner is disposed or the image is collected.
`Reserve(bytes, work)` lets caller-managed storage and work use the same budget;
dispose that reservation after the storage's lifetime. Its explicit work charge
is not refunded. Caller streams/output storage and later unbudgeted image copies
remain caller-managed. Existing `ImageCodec` entry points use per-operation limits.

## What working memory includes

| Operation | Main admitted storage |
| --- | --- |
| Exact edits / nearest resize | Supplied pixels plus an independent result; orientation may require a second full result |
| Drawing / compositing | Each distinct supplied canvas/source once; no snapshot raster, including self-overlap |
| TrueType face load / lookup | Load: caller input plus one owned copy and 256-byte descriptor; lookup: immutable font StorageBytes |
| Text layout / measurement | Font, borrowed UTF-16 input, private placement/line capacities and decoded geometry; temporary component/flag/kerning storage and overlapping growth capacities, with no raster allocation |
| Text rendering | Canvas/layout storage, prepared edges/events/crossings, one coverage row, 4 KiB descriptor/frame allowance and 1600-byte subdivision stack; overlapping growth capacities, no frame mask |
| Bilinear / bicubic resize | Source, result, admitted optional geometry/row caches, and 4 KiB for exact low-alpha rounding when needed; tight limits select the original bounded path |
| Lanczos3 resize | Source/result, Q30 plans, up to 64 horizontal rows and one output-sum row; eligible SIMD uses in-place Int64-to-double row conversion and certified FMA sums. Tight local work/memory selects the original integer kernel before admission; shared refusal is terminal |
| Decode Stream input | 8 KiB scratch, simultaneous old/new growth capacities and actual retained capacity throughout decoding; no final full-input copy |
| Synchronous Identify Stream input | 8 KiB scratch and conservative growth/final-copy admission; parses a private input prefix without the final copy |
| Container / EXIF inspection | Encoded input and bounded metadata/parser state; dimensions are checked without allocating a decoded image |
| InspectHeaders / InspectHeadersAsync | Fixed 8,448-byte scratch/parser allowance; no whole-input, metadata-block or raster allocation |
| PNG decode | Encoded input, concatenated IDAT, result, pass rows/palette/transparency, 4 KiB framing scratch and 256 KiB inflater allowance; optional filtered storage is separately admitted |
| PNG encode | Source, packed row, 64 KiB IDAT storage and 1 MiB BCL compression allowance, admitted before the signature |
| JPEG decode | Encoded input, parser/tables, padded coefficient/sample planes and RGBA result; orientation is admitted in the same operation. Certified sparse SIMD uses 8 KiB of original immutable module data, with no heap table or new workspace |
| JPEG caller-sample decode | Supplied encoded/destination spans and the same parser/planes; orientation writes directly into caller storage without an RGBA frame |
| Four-component JPEG | The same bounds with four coefficient/sample planes and descriptors; K is color, and shared admission counts caller CMYK storage |
| JPEG encode | Source plus 8 KiB fixed workspace; eligible adaptive coding additionally admits event capacity and 64 KiB scratch |
| JPEG caller-sample encode | Supplied gray/RGB span plus 8 KiB workspace and admitted optional event cache; bounded RGB rows, without RGBA conversion storage |
| VP8L decode / encode | Decode admits trees/cache/transform rasters and simultaneous conversion buffers; encode borrows source and admits the fixed 8 KiB path, 64 KiB cached baseline, separately admitted predictor/color maps, optional uncached literal models, code-cost predictor-map replacement with a shared color owner, bounded reference-token/index/prefix capacities, seeded copy models with borrowed channel trees and a bounded residual history/lookahead ring and spatial-group analysis with tile counts/indices, map/ranking and overlapping tables; counts output before streaming |
| VP8 decode / encode | Decode admits models/modes, padded YUV, alpha and RGBA phases; encode admits source/reconstruction planes, coefficient caches, alpha and optional bounded alpha history/tables, plus arithmetic-buffer capacity and old/new growth overlap |

These are logical admission charges, including conservative BCL allowances,
rather than measured native or managed allocation totals. Optional fast paths
must fit their complete workspace before selection; fallback preserves pixels,
work, cancellation and failure ordering.
Drawing applies geometry limits to the clipped affected rectangle. Copy/fill charge
four work units per pixel in chunks up to 4096 pixels; compositing charges sixteen
in chunks up to 256. Limits/cancellation can leave completed chunks in traversal
order. Argument, geometry and memory validation precede mutation.
Caller-owned encoded output storage is separate from codec workspace;
`MaxOutputBytes` limits actual writes. Eligible stored RGBA PNG encodes may
reserve plain `MemoryStream` capacity after work/output preflight, preserving
the original writes and limit checks.
Tight noninterlaced PNG RGBA decoding can keep 64 KiB of raw history inside the
existing inflater allowance; its original result/two-row peak stays unchanged.

## Streams, ownership and failures

Input is read through EOF using bounded buffering, including nonseekable streams;
caller streams remain open. `Identify(Stream)` may reuse a fully written private
prefix instead of making an extra exact-length copy, while preserving the same
conservative growth/final-copy admission. Its idle pool retains less than 8 MiB;
live operations never share a lease. Pixel and codec storage can also be reused
after release, as specified in [SPEC.md](../SPEC.md#pixels-and-ownership).

`Decode(Stream)` also parses a private input prefix without a final copy. Its actual
retained capacity stays charged during decoding, with simultaneous capacities
admitted during growth and capacity capped by `MaxInputBytes`. Pixel outputs remain
independent after the input lease is released; shared admission retains only pixels.

`IdentifyAsync`/`DecodeAsync` use asynchronous reads through EOF, leaving input
open. They admit 8 KiB scratch plus simultaneous growth capacities and retain
the full input capacity during CPU parsing/decoding; no final input copy is made.
Shared reservations span awaits and settle on every exit. Identification still
validates the full container/EXIF structure.

`InspectHeaders`/`InspectHeadersAsync` scan dimensions/orientation through EOF with
fixed workspace. Seekable inputs can skip opaque payloads; other inputs are drained.
Read-ahead and skipped bytes still consume input/work quotas. Dimension/pixel and
metadata/item limits apply, without a hypothetical decoded-array ceiling. PNG CRCs
and compressed pixels are outside this API's validation scope; use `Identify` for
fuller container validation and `Decode` for pixels. Shared reservations span awaits.

`EncodePng/Jpeg/WebpAsync` stage encoded bytes privately, then use caller
`WriteAsync` in blocks up to 64 KiB. Charge actual staging capacity, 256 bytes
of stream bookkeeping and 64 bytes per linked chunk, plus existing codec workspaces.
JPEG retains adaptive coding with an admitted bounded stock-memory destination;
resource/cancellable fallbacks remain. A separate private output pool retains at
most two exact arrays totaling 4 MiB. Shared reservations remain held across writes.
Unused tails are cleared before handing array-backed memory to callers. Work
includes codec work, staging-copy, cleared-tail and delivery-attempt bytes. No final
output copy is made before delivery; failures during delivery may leave a prefix.

Dispose image buffers and stop using their row spans before disposal. Transforms
return independent owners. Mutable input buffers require caller synchronization.
Limits and cancellation are checked during long loops and before associated
large allocations. Optional optimizations preserve the original resource checks.

Exhausted quotas raise `ImageLimitException`; malformed data raises
`ImageDataException`; unsupported features raise `UnsupportedImageException`.
`ImageLimitException.Limit` identifies the exhausted `ImageLimits` category or
the managed buffer-length ceiling. `UnsupportedImageException.Feature` identifies
the unsupported feature and `Format` identifies the recognized codec when known.
Legacy message-only constructors report `Unspecified`; library failures provide
typed categories. These properties do not require exception-message parsing.
`ImageLimitException.Scope` distinguishes per-operation exhaustion from shared
budget exhaustion; shared failures report `WorkingBytes` or `Work` as the limit.
Cancellation raises `OperationCanceledException` with the supplied token. Invalid
public arguments use standard argument exceptions. Stream encoding can leave
partial output on failure; stage output when atomic publication is required.

## Validation

JPEG coefficient reuse starts with three idle exact-size slots; owned full-
resolution CMYK can grow it to four, still totaling at most 4 MiB. Leases are
zeroed before parsing and returned on success or failure;
the original live working-memory and work charges remain unchanged.

Authored boundary tests cover exact admission and one-byte-below rejection,
limited work, cancellation, malformed input, overlapping owners and unchanged
sources. Windows/Linux correctness and live FFmpeg suites validate behavior;
the actual Release package is exercised by an isolated consumer and all usage
examples. [Benchmark results](benchmark.md) report the separate quiet-VM timing
and managed-allocation evidence. Changes to workspaces or accounting require
updating the relevant boundary tests and contract.

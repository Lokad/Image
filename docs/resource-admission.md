# Resource admission

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

## What working memory includes

| Operation | Main admitted storage |
| --- | --- |
| Exact edits / nearest resize | Supplied pixels plus an independent result; orientation may require a second full result |
| Bilinear / bicubic resize | Source, result, admitted optional geometry/row caches, and 4 KiB for exact low-alpha rounding when needed; tight limits select the original bounded path |
| Encoded Stream input | 8 KiB read scratch; old/new capacities during growth; conservative capacity + final-copy peak before parsing |
| Container / EXIF inspection | Encoded input and bounded metadata/parser state; dimensions are checked without allocating a decoded image |
| PNG decode | Encoded input, concatenated IDAT, result, pass rows/palette/transparency, 4 KiB framing scratch and 256 KiB inflater allowance; optional filtered storage is separately admitted |
| PNG encode | Source, packed row, 64 KiB IDAT storage and 1 MiB BCL compression allowance, admitted before the signature |
| JPEG decode | Encoded input, parser/tables, padded coefficient/sample planes and RGBA result; orientation is admitted in the same operation |
| JPEG encode | Source plus 8 KiB fixed workspace; eligible adaptive coding additionally admits event capacity and 64 KiB scratch |
| VP8L decode / encode | Decode admits trees/cache/transform rasters and simultaneous conversion buffers; encode admits source plus 8 KiB fixed workspace and counts output before streaming |
| VP8 decode / encode | Decode admits models/modes, padded YUV, alpha and RGBA phases; encode admits source/reconstruction planes, models, alpha and arithmetic-buffer capacity, including old/new growth overlap |

These are logical admission charges, including conservative BCL allowances,
rather than measured native or managed allocation totals. Optional fast paths
must fit their complete workspace before selection; fallback preserves pixels,
work, cancellation and failure ordering.

## Streams, ownership and failures

Input is read through EOF using bounded buffering, including nonseekable streams;
caller streams remain open. `Identify(Stream)` may reuse a fully written private
prefix instead of making an extra exact-length copy, while preserving the same
conservative growth/final-copy admission. Its idle pool retains less than 8 MiB;
live operations never share a lease. Pixel and codec storage can also be reused
after release, as specified in [SPEC.md](../SPEC.md#pixels-and-ownership).

Dispose image buffers and stop using their row spans before disposal. Transforms
return independent owners. Mutable input buffers require caller synchronization.
Limits and cancellation are checked during long loops and before associated
large allocations. Optional optimizations preserve the original resource checks.

Exhausted quotas raise `ImageLimitException`; malformed data raises
`ImageDataException`; unsupported features raise `UnsupportedImageException`.
Cancellation raises `OperationCanceledException` with the supplied token. Invalid
public arguments use standard argument exceptions. Stream encoding can leave
partial output on failure; stage output when atomic publication is required.

## Validation

Authored boundary tests cover exact admission and one-byte-below rejection,
limited work, cancellation, malformed input, overlapping owners and unchanged
sources. Windows/Linux correctness and live FFmpeg suites validate behavior;
the actual Release package is exercised by an isolated consumer and all usage
examples. [Benchmark results](benchmark.md) report the separate quiet-VM timing
and managed-allocation evidence. Changes to workspaces or accounting require
updating the relevant boundary tests and contract.

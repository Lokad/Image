# Resource admission

Pass a caller-owned `IMemoryBudget` through `ImageLimits.MemoryBudget` in codec,
editing and text options. Nested/concurrent calls forward the same borrowed root;
the package supplies no governor, independent allowance or cumulative CPU quota.
`MaxWorkingBytes` remains a separate per-operation storage bound, counting borrowed
inputs as well as results and temporary capacity. Shared admission charges owned
storage/copies, not caller-owned input storage again.

`ValidatePixels` retains no raster charge. It still admits substantial codec
storage (PNG compressed input/rows/inflater, progressive JPEG coefficients, WebP
planes/history). Bounded indexed PNG validation reuses certified filtered bytes;
larger or uncertain streams retain the inflater path. Baseline JPEG validates with
block scratch; baseline decoding
reconstructs admitted component sample planes without full-frame coefficients.
Owned JPEG 4:1:1/4:1:0 rendering also admits 1,536/1,792 bytes of fixed stack workspace
alongside its live component planes and output.
Stream acquisition stays charged through validation; successful metadata results
contain only scalar facts. Its memory bounds can therefore differ from Decode's.
Non-indexed VP8L decoding on little-endian systems uses the returned image's
storage for its pixel history; validation still needs that history without a result.

`IMemoryScope` starts empty and shares its ancestor's allowance. Reserve capacity
before allocating, including old/new overlap during replacement. `TryReserve` and
`TryResize` accept zero, reject negative values, and leave accounting unchanged on
refusal. Admission/resizing is thread-safe; disposed owners reject it. Dispose
reservations/scopes idempotently without throwing; scope disposal refunds and
invalidates descendants. It does not cancel operations or dispose their storage.
Callers synchronize ancestor disposal with all users.

Images, fonts and layouts own retained charges until explicit disposal. Prepared
layouts own decoded outlines and remain usable after font disposal. Finish all
row-span/async users before disposing their owner. No refund depends on collection,
finalizers or weak-table tracking. Caller roots, inputs and streams stay borrowed.
Copies are charged separately. Temporary owners release on every exit; growing
buffers cover retained capacity and replacement overlap. Asynchronous owners stay
charged until actual reads/writes finish, including cancellation and failure.
Encoded capacity retained by a caller-owned stream belongs to that caller's
storage owner. Keep its admission through later consumers/publication; codec
completion does not admit arbitrary retained copies. Stream failure can leave
partial output; publication/atomic replacement is outside the codec operation.

Shared refusal throws `ImageLimitException` with `WorkingBytes` and `SharedBudget`,
terminates the affected operation and releases unused/relinquished reservations.
There is no retry, wait, eviction or lower-memory fallback after refusal. A local
workspace-dependent path may be chosen before admission; that does not make shared
refusal recoverable. Cancellation and allocation failure remain distinct.

Meaningful local bounds remain positive: dimension32768, pixels32million,
input/output64MiB each, working512MiB, metadata4MiB, container/EXIF items100000.
Codec/managed buffer-length ceilings also apply. Cancellation is cooperative,
with direct checkpoints in long loops and before substantial allocations;
it is not a CPU/time quota. Drawing/stream failure may leave a completed prefix.

Substantial ownership boundaries include codec planes/tables/trees/history,
input/output capacity, resize tables/caches/results, font copies, prepared text
placements/outlines and rendering edges/rows. Memory admission is coarse logical
capacity rather than measured GC allocation or process RSS. Minor objects/runtime
storage are approximate; allow runtime/collection headroom. Calls with a shared
root bypass uncharged global pixel/coefficient/input/output pools. Caller-root
storage never enters those caches. Other calls retain bounded idle caches.

Dimension-only probes admit512 bytes; other probing and complete header inspection
use8448-byte workspace without a raster. Identify/Decode stream
buffers retain actual capacity through parsing, avoiding a final encoded copy.
Async encoding retains staged chunks/prepared partitions through actual delivery.
Returned results have independent lifetimes. See [SPEC](spec.md) for codec,
stream validation and pixel details and [migration](migration.md) for0.2 API removals.

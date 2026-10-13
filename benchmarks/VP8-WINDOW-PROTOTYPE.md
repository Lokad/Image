# VP8 arithmetic-window correctness prototype

An original 32-bit arithmetic-window prototype passes **5,418,833 reference
checks per OS and mode**: Windows/Linux, with intrinsics enabled and disabled.
Opaque and alpha-bearing frozen fixtures retain every decoded pixel and
independent held output. Production is unchanged. **No timing, allocation or
two-times parity claim is made.**

The code word holds the logical eight-bit interval value in its high byte and
up to 24 future bits below it. Shifting the whole word consumes normalization
bits; bounded batch refills replenish the fractional part. Logical byte/bit
position remains explicit. A short partition or span falls back to the original
recurrence, retaining partially advanced state and failure order. This design
is independently derived from the arithmetic-coding model in
[RFC 6386 §7.2](https://datatracker.ietf.org/doc/html/rfc6386#section-7.2).
The pinned FFmpeg implementation was inspected for strategy only; no upstream
code or tables are imported.

The checks compare **4,239,185** transitions/composites against the retained
scalar recurrence and **1,179,648** transitions directly against the **actual
tested package's reader**. They cover all 256 probabilities, normalized interval
boundaries, every bit position, nonzero partition offsets, changing span lengths,
long deterministic sequences, literal/tree/token decoding, malformed probability
spans, work exhaustion and cancellation. Returned values, full logical cursors,
remaining work, exception types and messages agree. The direct-package bridge
uses development-only reflection; no production diagnostic API is added.

An isolated development assembly replaces only the reader when decoding the
existing 72,380-byte opaque packet and 98,590-byte alpha packet. Both expected
managed pixel hashes and held-output mutation checks pass in all four runs.
No original raster, encode-quality sweep or performance workload is added.

The reader's private state grows from **20 to 24 bytes**: at most **32 extra
bytes** across eight token partitions. Integration must account for this state
within fixed workspace admission, preserve all public resource/cancellation and
pixel contracts, and retain meaningful tests of the logical cursor. The
[rejected leading-zero trial](VP8-NORMALIZATION-REJECTED.md) remains rejected;
this is a different representation, with no speed established yet.

The [numeric evidence](vp8-window-prototype-evidence.json) records all four
identical proofs, source hashes and the frozen prototype/source/package archive.
Next, integrate one safe primitive and measure the complete warm operation on
the dedicated quiet VM. Opaque VP8, JPEG420 RGBA8's upper-bound gap and the rest
of the required material contract remain open.

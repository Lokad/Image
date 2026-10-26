# Materialized Deflate block cursor

PNG decoding can use a local bit cursor for a complete-Huffman, noncancelable
block in an already admitted private output span. Cancelable calls, counting
and rolling-row inflation retain their separate paths. There is no new heap
buffer, table, cache or owner, and no work/fuel admission.

For requests up to16bits, refilling while fewer than16bits are present leaves
at most47meaningful bits in a ulong cache after a32-bit load. Short tails use
byte refills. Long prefixes use the canonical decoder, publishing/restoring
cursor state even on failure. Finally publishes all state on block exit.
Reserved codes, history/window bounds, overlap-safe copies and excess output
retain their validation order. Checksum position and trailing-IDAT behavior
remain unchanged; no admission refusal is recoverable.

Authored tests compare per-symbol materialization across block phases, extras,
overlapping history, deep codes, every truncation, malformed tokens, output
guards and checksum positions. Independent BCL, cancellation and cooperative
ownership controls remain. Historical timings are in [benchmark state](benchmark.md).

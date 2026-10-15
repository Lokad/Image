# RGBA16 PNG decoding at the working-memory boundary

Complete warm decoding confirms **1.834× FFmpeg / upper 1.852×**, at
**5,290.75 µs** and **8,896 B/op**, under **3,254,193 admitted working bytes**.
The short screen measures 1.841×; the independent repeat retains all twelve
actuals on the same frozen core and harness. Every versioned quiet guard passes.
Stop this fixed pair.

The original 513×259 RGBA16 raster and frozen 960,565-byte PNG are unchanged.
The managed limit is exactly encoded input (960,565), concatenated IDAT (960,340),
output (1,062,936), original inflater allowance (262,144) and two rows (8,208).
One byte below this total raises `ImageLimitException`; the exact boundary
preserves every word, alpha and hidden RGB, with independent outputs and unchanged
source. Normal/scalar native CLI output is exact.

This budget exercises the original BCL/unbuffered fallback: neither the optional
whole-filter buffer nor the optional 8 KiB decompressed-output buffer fits.
The BCL's own internal buffers remain covered by its existing allowance. Admission
checks, framing, inflation, endian conversion, output acquisition and disposal
are timed in the public managed operation. Immutable limit/options setup is
excluded. The existing original native worker times complete decoding into an
owned RGBA64BE frame, full conversion into a second owned RGBA64LE frame and both
releases; packet/context/frame wrappers are reused. Native exposes no matching
caller admission setting. This is a per-operation managed working limit; it does
not bound process RSS or idle-cache/GC retention.

Sequential Release runs use one pinned CPU, one codec thread, normal SIMD,
tiering off and workstation nonconcurrent GC. Startup, preparation, I/O, IPC,
hashing and validation are excluded. Native allocations are unmeasured.

Only one benchmark input changes. Qualified production, native worker, raster and
packet are reused. Existing **3,332 full-suite Release tests per OS**, including
**896 live FFmpeg comparisons**, plus the additional complete-long-code control
and consumer/three examples through the same actual package remain applicable.
No production/native rebuild, full suite, package proof or method audit is repeated.
[Numeric evidence](warm-png-tight16-evidence.json) links that qualification and
immutable archives containing receipts, actual generated jobs, four child
identities, packet/raster and source inventories. Remaining material public and
current-source warm coverage stays open.

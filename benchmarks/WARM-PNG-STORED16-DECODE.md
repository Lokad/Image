# Stored-Deflate RGBA16 PNG decoding

Complete warm decoding confirms **1.524× FFmpeg / upper 1.556×**, at **472.57 µs**
and **1,080,706 B/op**. The short screen measured 1.539×; the independent repeat
retains all twelve actuals with the same frozen production core and harness.
Every versioned quiet guard passes. Stop this fixed pair.

Both subjects decode the same frozen PNG from the
[RGBA16 encoding round](WARM-PNG-STORED16-ENCODE.md), using the original 513×259
word raster. Stored-only Deflate, filter None, every word, alpha and hidden RGB
are checked exactly. The input contains 1,063,366 IDAT bytes; its SHA-256 is
`98b4c931725ffce5ac6ca86f42d2e2d44ca6f32ceac2463fcc4f63a24ae3b405`.
Normal/scalar FFmpeg CLI pixels, source preservation and independent outputs pass.

Managed timing covers public Decode and output disposal; released pixel storage
may be reused. The existing original native worker times complete decoding,
owned big-endian frame acquisition, full-frame word-order conversion to native
little-endian samples, a second owned frame acquisition and both releases.
Contexts and wrappers are reused; no output is cached. CPU0, one codec thread,
normal SIMD, tiering off and workstation nonconcurrent GC are used. Startup,
preparation, I/O, IPC and validation are excluded. Native allocations are unmeasured.

The short supervisor originally expected the encoder's filter metadata value.
Decode correctly reports `-1`; actual input scanlines are validated as filter None.
The original failed receipt is retained, and every identity, pixel, job and quiet
check is revalidated with the corrected metadata expectation, without retiming.
Confirmation validates directly.

Only one benchmark input changes. Qualified production, native worker, raster and
encoded packet are reused. Existing **3,318 Release tests per OS**, including
**896 live FFmpeg comparisons**, and consumer plus three examples through the
same actual package remain applicable; no full suite, package proof or method
audit is repeated. [Numeric evidence](warm-pngstored16-decode-evidence.json) links
the qualification and immutable archives containing source inventories, original
receipts, actual generated jobs and four child identities. Remaining material
public/current-source coverage and explicit baseline refusals remain open.

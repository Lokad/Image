# Managed PNG compression policy qualification

All four production policies use independent safe managed streaming Deflate.
Fourteen existing513×259 controls match the entire frozen FFmpeg packets,
including pHYs,4096-byte IDAT chunks, CRCs, zlib/Adler and entropy. No metadata
exception, changed reference setting or new image grid is involved.

| Policy / source | 8-bit packet B, managed = reference | 16-bit packet B, managed = reference |
| --- | ---: | ---: |
| Optimal / grayscale | 72,586 | 265,433 |
| Optimal / RGB | 168,729 | 739,472 |
| Optimal / grayscale-alpha | 120,366 | 506,887 |
| Optimal / RGBA | 210,973 | 963,202 |
| NoCompression / RGBA | 533,444 | 1,066,552 |
| Fastest / RGBA | 236,828 | 968,216 |
| SmallestSize / RGBA | 210,973 | 963,202 |

Stored level0, greedy level1 and lazy levels6/9 share one bounded owner reserved
before allocation. History/dictionary/token capacity plus bounded tree scratch
fit the conservative1MiB workspace; packed row/chunk and async staging owners
remain separate. Refusal is terminal. Cancellation/writer failure disposal does
not flush output. Stored flush thresholds preserve the complete scanline, including
its filter byte; a regression covers the32767-byte pending prefix.

The same immutable Windows-built core passes4,807 Release checks per platform
(1,822internal/2,045public/940live) and69 affected scalar checks on Windows/Linux.
All14 packet/pixel controls, the named forward/live-token/async/ImageBudget
boundaries, aggregate admission, cleanup, subsequent success and independent
outputs pass. The same actual private0.2.0 package verifies14PNG/10JPEG packet
controls and all three usage examples on both systems. One production build,
no Linux production build, no rebuild after freeze and no package amendment.

Current CPU time and allocated B/op are **unmeasured**. All17 historical BCL PNG
encoder rows require current-producer confirmation after exact codec work.
Decoder/transform evidence retains its original encoded inputs; four affected
16-bit preparation paths now explicitly read and hash-check the frozen packets.
No new timing, native build or native encode accompanied integration.

[Numeric evidence](png-byte-parity-evidence.json),
[17-row packet ledger](png-complete-packet-audit-evidence.json) and
[benchmark state](../docs/benchmark.md) retain scope, hashes and qualification.
Other codec exactness, full CPU parity and release readiness remain open.

# Six-byte VP8 arithmetic refills

The reader replenishes six bytes per refill using one bounded 64-bit load.
The initial load seeds the scalar tail and the first six future bytes.
Subsequent loads include two previously cached bytes and discard them. The code
byte stays at the same position. Token decisions, scalar cursor publication,
every live cancellation check and the short-partition fallback are unchanged.

An independent model checks all eight entry phases, the consumption bounds
and published positions. The maximum read ends at byte 271 in the existing
272-byte single-block window, 109 for a mode window, and 6,385/6,655 for the
24/25-block windows guarded at 6,528/6,800 bytes. The focused tests check
maximum renormalization, unread tails, cancellation ordinals, coefficient and
mode cursors, and all 164 frozen frames.

Independent 12-pair screens save **65.28 µs (1.27%)** for ordinary opaque
decoding and **56.92 µs (1.10%)** for live-token opaque decoding. Their paired
ratio intervals are **0.982795–0.991808** and **0.981690–0.996282** at
230,880 B/op. Both initial three-pair screens are retained. These gains are
scoped to the complete managed-DLL pairs.

| Complete operation | Lokad.Image / FFmpeg µs | Conservative upper | Managed B/op |
| --- | ---: | ---: | ---: |
| Opaque, resident | 5,055.25 / 5,212.60 | 0.974767 | 230,880 |
| Opaque with live token | 5,145.23 / 5,231.03 | 0.989704 | 230,880 |
| Simple-filter opaque | 5,191.33 / 5,324.73 | 0.981286 | 230,880 |
| Transparent, resident | 6,163.77 / 6,483.69 | 0.957587 | 240,400 |
| Transparent, fresh seekable stream | 6,072.82 / 6,492.78 | 0.941000 | 240,496 |
| Coding-only validation, span | 4,570.02 / 5,882.99 | 0.786854 | 15,600 |

All six controls pass on the same timed DLL. Full decodes retain twelve
128-operation actuals and all three interleaved reference groups. Stream
acquisition, decoding, normalization, independent output and disposal are
included where applicable. The coding-only case retains its original twelve
BDN actuals, two native brackets and descriptor output. FFmpeg retains its
declared codec and conversion context. Exact RGBA frames or descriptors,
source integrity, cancellation, ownership and the quiet-VM guards all pass.
A preparation attempt selected the resident worker for the stream identity
and stopped before any samples. The stream capture uses its original worker.
Both validator means are higher than in the earlier capture. The independent
paired gains above cover the two opaque decode operations.

The change adds no heap allocation, shared admission or retained capacity.
Encoder packet paths and VP8L decoding reuse their source-valid evidence.
Windows and Linux each pass 6,856 full/live tests and 1,444 focused normal
and scalar tests per mode. Both consumers and all three usage examples pass
on the same Release package `87f1ae4a` with the timed library `9c8653dd`.
Earlier [VP8 comparisons and trial history](VP8-DECODE-CURRENT-PACKAGE.md)
remain available. The [timing and qualification evidence](vp8-six-byte-refill-evidence.json)
identifies the mirrored archives, frozen producers and actual package.

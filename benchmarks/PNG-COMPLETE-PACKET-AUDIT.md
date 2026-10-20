# PNG complete-packet compression audit

The existing 17 PNG encoding rows contain **15 retained packet-size passes and two
gaps**. Both gaps encode the same RGBA8 level-6 control: **218,775 / 210,973 B**
(managed / FFmpeg), a 7,802 B excess. Async and synchronous boundaries remain
separate rows; no input, reference setting or compression policy is changed.

Acceptance requires exact samples and **complete packet bytes ≤ reference**, including
all chunks and framing. The historical 5% IDAT tolerance established timing workload
equivalence only. A passing timer or IDAT gate cannot close a packet-size gap.

| Existing control | Managed / reference packet B | Packet size |
| --- | ---: | --- |
| [PNG EncodePngAsync, fresh nonseekable Stream / level6](WARM-PNG-ASYNC-ENCODE.md) | 218,775 / 210,973 | Open |
| [PNG RGBA8, level 6 / None](WARM-PNG-CHUNK-CRC.md) | 218,775 / 210,973 | Open |
| [PNG RGBA8, NoCompression / None](WARM-PNG-STORED-CAPACITY.md) | 531,971 / 533,444 | Pass (retained) |
| [PNG RGBA16, NoCompression / None](WARM-PNG-STORED-CAPACITY.md) | 1,063,615 / 1,066,552 | Pass (retained) |
| [PNG RGBA8, Fastest managed2/native1 / None](WARM-PNG-CHUNK-CRC.md) | 231,465 / 236,828 | Pass (retained) |
| [PNG RGBA16, Fastest managed2/native1 / None](WARM-PNG-CHUNK-CRC.md) | 963,045 / 968,216 | Pass (retained) |
| [PNG RGBA16, Fastest managed2/native1 / live cancelable token](WARM-PNG-CHUNK-CRC.md) | 963,045 / 968,216 | Pass (retained) |
| [PNG RGBA16, Fastest managed2/native1 / forward-only output](WARM-PNG-CHUNK-CRC.md) | 963,045 / 968,216 | Pass (retained) |
| [PNG RGBA16, level 6 / None](WARM-PNG-CHUNK-CRC.md) | 960,565 / 963,202 | Pass (retained) |
| [PNG RGBA8, SmallestSize9 / native9](WARM-PNG-CHUNK-CRC.md) | 209,407 / 210,973 | Pass (retained) |
| [PNG RGBA16, SmallestSize managed9/native9 / None](WARM-PNG-CHUNK-CRC.md) | 960,650 / 963,202 | Pass (retained) |
| [PNG RGB8, Optimal managed9/native6 / None](WARM-PNG-BYTE-PACKING.md) | 163,358 / 168,729 | Pass (retained) |
| [PNG RGB16, level 6 / None](WARM-PNG-CHUNK-CRC.md) | 737,426 / 739,472 | Pass (retained) |
| [PNG gray8, Optimal managed9/native6 / None](WARM-PNG-BYTE-PACKING.md) | 72,373 / 72,586 | Pass (retained) |
| [PNG gray16, level 6 / None](WARM-PNG-CHUNK-CRC.md) | 264,939 / 265,433 | Pass (retained) |
| [PNG gray-alpha8, Optimal managed9/native6 / None](WARM-PNG-BYTE-PACKING.md) | 120,036 / 120,366 | Pass (retained) |
| [PNG gray-alpha16, level 6 / None](WARM-PNG-CHUNK-CRC.md) | 505,483 / 506,887 | Pass (retained) |

[Evidence](png-complete-packet-audit-evidence.json) identifies each selected receipt,
its path and hash, complete sizes and IDAT sizes. This is reconciliation of retained
byte/correctness evidence, not a new current-code execution or timing confirmation.
Historical producers retain their original validity scope; changed PNG production
paths require affected revalidation. VP8-only changes invalidate none of these PNG
packet receipts. No all-image, default-policy or complete-library compression claim
follows. Current compression work prioritizes lossy WebP, then lossless WebP; CPU
parity enforcement is deferred while the compression gates remain open.

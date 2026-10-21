# PNG complete-packet compression audit

All17 existing PNG encoding rows now pass their declared complete-packet gates.
[C161 Optimal RGBA8](PNG-OPTIMAL-RGBA8.md) closes the two changed sync/async
boundaries at **209,407 /210,973 B** (managed/reference), saving9,368 B each.
The public Optimal policy now uses managed backend9; the frozen FFmpeg reference
stays at6. Inputs, filter None and exact samples remain unchanged. Other15 paths
retain their existing evidence. This is not an equal-effort or all-image claim.

Acceptance requires exact samples and **complete packet bytes ≤ reference**, including
all chunks and framing. The historical 5% IDAT tolerance established timing workload
equivalence only. A passing timer or IDAT gate cannot close a packet-size gap.

| Existing control | Managed / reference packet B | Packet size |
| --- | ---: | --- |
| [PNG EncodePngAsync, Optimal managed9/native6](PNG-OPTIMAL-RGBA8.md) | 209,407 / 210,973 | Pass |
| [PNG RGBA8, Optimal managed9/native6 / None](PNG-OPTIMAL-RGBA8.md) | 209,407 / 210,973 | Pass |
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

[Evidence](png-complete-packet-audit-evidence.json) identifies each current packet
and reused receipt. Actual normal/scalar sync/async output and the same private
package pass on both platforms. Other15 controls remain retained observations
whose relevant production paths are unchanged; their historical timing producers
are not relabeled as current. Only defaultRGBA8 encode costs are superseded in
the finite warm ledger; decoder input bytes remain frozen. CPU confirmation is
deferred while JPEG compression gates remain open. Exact samples and complete
packet sizes remain distinct from CPU performance and historical IDAT tolerance.

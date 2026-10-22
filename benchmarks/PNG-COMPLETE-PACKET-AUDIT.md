# PNG complete-packet qualification

Nine existing delivery rows, covering all eight Optimal color/depth controls,
now match complete frozen FFmpeg packets raw-exact. The eight other policy/delivery
rows retain historical complete-size passes; their byte-exact qualification remains
open. Inputs, native levels and sample contracts are unchanged.
See [managed encoder qualification](PNG-MANAGED-OPTIMAL.md).

| Existing control | Managed / reference packet B | Verification |
| --- | ---: | --- |
| [PNG EncodePngAsync, fresh nonseekable Stream / Optimal managed6/native6](PNG-MANAGED-OPTIMAL.md) | 210,973 / 210,973 | Raw exact |
| [PNG RGBA8, Optimal managed6/native6 / None](PNG-MANAGED-OPTIMAL.md) | 210,973 / 210,973 | Raw exact |
| [PNG RGBA8, NoCompression / None](WARM-PNG-STORED-CAPACITY.md) | 531,971 / 533,444 | Historical size pass; bytes pending |
| [PNG RGBA16, NoCompression / None](WARM-PNG-STORED-CAPACITY.md) | 1,063,615 / 1,066,552 | Historical size pass; bytes pending |
| [PNG RGBA8, Fastest managed2/native1 / None](WARM-PNG-CHUNK-CRC.md) | 231,465 / 236,828 | Historical size pass; bytes pending |
| [PNG RGBA16, Fastest managed2/native1 / None](WARM-PNG-CHUNK-CRC.md) | 963,045 / 968,216 | Historical size pass; bytes pending |
| [PNG RGBA16, Fastest managed2/native1 / live cancelable token](WARM-PNG-CHUNK-CRC.md) | 963,045 / 968,216 | Historical size pass; bytes pending |
| [PNG RGBA16, Fastest managed2/native1 / forward-only output](WARM-PNG-CHUNK-CRC.md) | 963,045 / 968,216 | Historical size pass; bytes pending |
| [PNG RGBA16, Optimal managed6/native6 / None](PNG-MANAGED-OPTIMAL.md) | 963,202 / 963,202 | Raw exact |
| [PNG RGBA8, SmallestSize9 / native9](WARM-PNG-CHUNK-CRC.md) | 209,407 / 210,973 | Historical size pass; bytes pending |
| [PNG RGBA16, SmallestSize managed9/native9 / None](WARM-PNG-CHUNK-CRC.md) | 960,650 / 963,202 | Historical size pass; bytes pending |
| [PNG RGB8, Optimal managed6/native6 / None](PNG-MANAGED-OPTIMAL.md) | 168,729 / 168,729 | Raw exact |
| [PNG RGB16, Optimal managed6/native6 / None](PNG-MANAGED-OPTIMAL.md) | 739,472 / 739,472 | Raw exact |
| [PNG gray8, Optimal managed6/native6 / None](PNG-MANAGED-OPTIMAL.md) | 72,586 / 72,586 | Raw exact |
| [PNG gray16, Optimal managed6/native6 / None](PNG-MANAGED-OPTIMAL.md) | 265,433 / 265,433 | Raw exact |
| [PNG gray-alpha8, Optimal managed6/native6 / None](PNG-MANAGED-OPTIMAL.md) | 120,366 / 120,366 | Raw exact |
| [PNG gray-alpha16, Optimal managed6/native6 / None](PNG-MANAGED-OPTIMAL.md) | 506,887 / 506,887 | Raw exact |

A smaller packet, a5% IDAT tolerance or a timer pass does not establish exact
encoding. [Numeric evidence](png-complete-packet-audit-evidence.json) links current
packet identities and retained producers. Changed default-encoder CPU/allocations
are unmeasured; unchanged algorithms retain evidence on their frozen inputs.

# Current VP8L cache-history confirmation

The existing 513×259 RGBA8 control retains its pinned libwebp reference with
`exact=1`, preserving hidden RGB and alpha. One CPU; complete operations include
normalization, encoding and fresh independent owned output acquisition/release.
The complete packet stays raw-exact at 183,812 B. Inputs/settings are unchanged.

| Independent quiet VM confirmation | Lokad µs | Reference µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|
| Current VP8L | 297,488.76 | 122,398.05 | 2.431/2.456 | 48,184,648 |

All 12 actual iterations are retained and selected; both idle-v2 guards pass.
This resolves the [earlier short uncertainty](WARM-VP8-CACHE-HISTORY.md), without
a separate speedup claim. Production is unchanged. Its cache-history allocation
cost remains included. The exact measured DLL already passes 5,117 checks per
platform, live comparisons and the SAME actual private Release package with all
three examples. Unchanged scalar lossless/history and ownership/resource proofs
are reused. No rebuild, repeated full qualification or new controls.

[Evidence](warm-vp8l-cache-history-evidence.json) records identities and reuse.
VP8L now has the largest confirmed encoder gap; full warm parity remains open.

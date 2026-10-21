# JPEG complete-packet coverage

All 19 existing encode rows are reconciled on the qualified C165 producer:
**18 default boundaries plus one optional CompactColor boundary**. Thirteen default
cases pass size and quality; five remain open. CompactColor passes separately.
These procedural controls do not establish photographic or diagram compression claims.

[Precision-grid decisions](JPEG444-PRECISION-GRID.md) reduce the existing
4:4:4 packets to 135,630/135,576 B for 8/16-bit input, within unchanged quality
tolerances. Frozen source/reference/settings, Original DQT and alpha gates
remain. Actual packets exactly match the corrected independent prototypes. DC
and selected low AC may change; unaffected controls retain their existing evidence.

| Existing case / boundary | Managed / reference packet B | Size gate |
| --- | ---: | --- |
| Encode8 | 135,630 / 133,041 | Open |
| Encode422_8 | 66,050 / 71,168 | Pass |
| Encode420_8 | 51,379 / 52,093 | Pass |
| Encode16 | 135,576 / 133,043 | Open |
| Encode422_16 | 66,071 / 71,210 | Pass |
| Encode420_16 | 51,471 / 52,117 | Pass |
| Encode420Restart8 | 51,428 / 58,012 | Pass |
| Encode420Stream16 | 51,471 / 52,117 | Pass |
| Encode420Cancelable16 | 51,471 / 52,117 | Pass |
| Encode444Stream8 | 135,630 / 133,041 | Open |
| Encode444Cancelable8 | 135,630 / 133,041 | Open |
| JpegBudgetEncode4208 | 51,379 / 52,093 | Pass |
| Jpeg8AsyncNonSeekable | 135,630 / 133,041 | Open |
| JpegRgb420AsyncSamples | 51,379 / 52,093 | Pass |
| EncodeGray8 | 58,823 / 58,910 | Pass |
| CallerGray | 58,823 / 58,910 | Pass |
| CallerRgb420 | 51,379 / 52,093 | Pass |
| CompactColorRgb420 | 48,701 / 52,093 | Optional pass |
| Encode420Stream8 | 51,379 / 52,093 | Pass |

The largest remaining demonstrated gap is default 4:4:4, about 1.95% excess.
[Default 4:2:0](JPEG420-AC-TRELLIS.md) now passes all nine affected boundaries. Passing 4:2:2, grayscale, 4:2:0,
optional CompactColor and all declared WebP controls stay stopped.

[Current qualification and cost](JPEG444-PRECISION-GRID.md) records both platforms,
the actual package and one short quiet-VM observation. The finite256-case warm
objective remains open; CPU parity and speed-only work are deferred. Historical
fixed-stream costs are not asserted current after adaptive planning.

[Numeric ledger](jpeg-packet-ledger-evidence.json) identifies every current packet
and reused quality fact. The original C125 audit remains mirrored in
`c125-jpeg-packet-ledger.tar.gz`, SHA256
`27bde5ce6fc22f0ec02a351c3504c5821c7ec2c792ed60702dd8d1c550c0c35c`.

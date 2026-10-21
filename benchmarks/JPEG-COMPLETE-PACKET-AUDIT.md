# JPEG complete-packet coverage

All 19 existing encode rows are reconciled on the qualified C142 producer:
**18 default boundaries plus one optional CompactColor boundary**. Five default
cases pass size and quality; thirteen remain open. CompactColor passes separately.
These procedural controls do not establish photographic or diagram compression claims.

[Source-constrained trellis](JPEG-CONSTRAINED-AC-TRELLIS.md) reduces the existing
4:4:4 packets to 142,580/142,544 B for 8/16-bit input, within unchanged quality
tolerances. Frozen source/reference/settings, Original DQT/DC and alpha gates
remain. Actual packets exactly match the independent C141 prototypes;
unaffected controls retain their existing evidence.

| Existing case / boundary | Managed / reference packet B | Size gate |
| --- | ---: | --- |
| Encode8 | 142,580 / 133,041 | Open |
| Encode422_8 | 66,050 / 71,168 | Pass |
| Encode420_8 | 55,797 / 52,093 | Open |
| Encode16 | 142,544 / 133,043 | Open |
| Encode422_16 | 66,071 / 71,210 | Pass |
| Encode420_16 | 55,834 / 52,117 | Open |
| Encode420Restart8 | 55,897 / 58,012 | Pass |
| Encode420Stream16 | 55,834 / 52,117 | Open |
| Encode420Cancelable16 | 55,834 / 52,117 | Open |
| Encode444Stream8 | 142,580 / 133,041 | Open |
| Encode444Cancelable8 | 142,580 / 133,041 | Open |
| JpegBudgetEncode4208 | 55,797 / 52,093 | Open |
| Jpeg8AsyncNonSeekable | 142,580 / 133,041 | Open |
| JpegRgb420AsyncSamples | 55,797 / 52,093 | Open |
| EncodeGray8 | 58,823 / 58,910 | Pass |
| CallerGray | 58,823 / 58,910 | Pass |
| CallerRgb420 | 55,797 / 52,093 | Open |
| CompactColorRgb420 | 48,701 / 52,093 | Optional pass |
| Encode420Stream8 | 55,797 / 52,093 | Open |

The largest remaining demonstrated gap is default 4:4:4, about 7.2% excess.
The 4:2:0 controls retain about7.1% excess. Passing 4:2:2, grayscale, restart4:2:0,
optional CompactColor and all declared WebP controls stay stopped.

[Current qualification and cost](JPEG-CONSTRAINED-AC-TRELLIS.md) records both platforms,
the actual package and one short quiet-VM observation. The finite256-case warm
objective remains open; CPU parity and speed-only work are deferred. Historical
fixed-stream costs are not asserted current after adaptive planning.

[Numeric ledger](jpeg-packet-ledger-evidence.json) identifies every current packet
and reused quality fact. The original C125 audit remains mirrored in
`c125-jpeg-packet-ledger.tar.gz`, SHA256
`27bde5ce6fc22f0ec02a351c3504c5821c7ec2c792ed60702dd8d1c550c0c35c`.

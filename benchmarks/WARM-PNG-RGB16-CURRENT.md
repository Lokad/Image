# RGB16 with shared PNG improvements

One existing noninterlaced/None/zlib level 6 RGB16 PNG projects the original
513x259 RGBA16 raster: full RGB samples remain unchanged and alpha becomes 65535.
Both subjects decode identical resident bytes into independent owned RGBA16.
Native timing includes one-thread RGB48BE-to-RGBA64LE conversion and output
acquisition/release; contexts and frame wrappers are reused. Managed Decode/
disposal uses current released-storage reuse and admitted buffered inflation.
Startup/I/O/IPC/preparation/validation stay outside timing. Native normal/scalar
pixels, full samples, source and retained owners are exact. No original performance
image or production primitive was added.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Prior gap | 4,484.58 | 2,226.47 | 2.014 / 2.187 | 1,807,712 |
| Current three-actual screen | 4,020.43 | 2,209.43 | 1.820 / 2.126 | 15,024 |
| Independent twelve-actual confirmation | 4,020.98 | 2,210.00 | 1.819 / 1.823 | 15,024 |

This fixed pair is **confirmed: 1.819x / upper 1.823x**. Both guards pass;
all twelve actuals and twelve filtered results remain retained. Upper takes the
larger filtered/all-actual bound. Allocation falls 99.2%, to 15,024 B/op; it includes
harness overhead, and native totals remain unmeasured. Stop tuning this pair.
First-use, retained-output and tight-budget boundaries remain distinct.

Production and package are unchanged from [palette8 SIMD](WARM-PNG-PALETTE-SIMD.md).
After synchronization, forced fresh and both timed children match all 595 method
bodies and flags to that same actual Release package. Its 2,669 tests per OS,
focused normal/scalar PNG controls, original fixtures and all three usage examples
remain validation. No full suite or pack was repeated for this evidence-only round.

The [numeric evidence](warm-pngrgb16-current-evidence.json) retains every actual,
bracket, source/input/raster hash, four actual child identities, immutable raw
archives and package proofs. [Earlier RGB16 observations](WARM-PNG-RGB16-ROW.md)
remain unchanged. JPEG420's upper-bound gap and the complete required/material
warm scope remain open; this confirmation is not uniform warm parity.

# Gray8 with shared PNG improvements

One existing gray8 PNG, noninterlaced/filter None/zlib level 6, projects the
original 513x259 RGBA8 raster: red is gray, RGB equal gray, alpha=255. Identical
resident bytes decode to independently owned RGBA8. Native timing includes GRAY8
unpacking/one-thread conversion/output acquisition and release; wrappers are
reused. Managed Decode/disposal uses the current exact-size input/output reuse
and admitted buffered inflation. Startup/I/O/IPC/preparation/validation stay
outside timing. Native normal/scalar pixels, source and held owners are exact.
No original performance image was added.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Prior gap | 836.63 | 372.62 | 2.245 / 3.003 | 620,246 |
| Current three-actual screen | 565.71 | 365.59 | 1.547 / 1.774 | 9,904 |
| Independent twelve-actual confirmation | 564.59 | 361.22 | 1.563 / 1.580 | 9,904 |

This fixed pair is **confirmed: 1.563x / upper 1.580x**. Both guards pass;
12 actuals / 12 filtered results remain retained. Upper takes the larger filtered/
all-actual bound. Allocation falls 98.4%, to 9,904 B/op; it includes harness
overhead, and native allocation totals remain unmeasured. Stop tuning this pair.
First-use, retained-owner and tight-budget boundaries remain distinct.

Production is unchanged from [buffered inflation](WARM-PNG-INFLATE-BUFFER.md).
After synchronization, forced fresh and both timed children match all 591 method
bodies/flags of the same actual Release package. Its 2,663 tests per OS, 48 focused
normal/scalar PNG controls, original fixtures and all three package examples remain
validation. No full suite or pack was repeated for this evidence-only change.

The [numeric evidence](warm-pnggray8-current-evidence.json) retains every actual,
bracket, input/raster/source hash, four child identities, raw archives and package
proofs. Earlier [gray8 observations](WARM-PNG-GRAY8-ROW.md) remain unchanged.
Palette8/RGB16/JPEG420 and the full required material scope remain open; this
confirmation is not a uniform warm-parity claim.

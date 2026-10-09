# Warm single-CPU PNG: filter-None dispatch

Skipping prediction work for unchanged rows reduces complete PNG decode cost
from the [eight-byte CRC checkpoint](WARM-PNG-CRC.md). Both retained depths
remain above 2× FFmpeg. Other PNG profiles and warm families remain open.

The [numeric snapshot](results/warm-png-none-2026-10-09.json) retains original
statistics, actual managed measurements, native counters/times, exact checks,
component identities and quietness. The same two original 513x259 RGBA8/RGBA16
rasters are reused, including zero-alpha hidden RGB.

| Case | Before managed ms | After managed ms | Before worst mean ratio | After worst mean ratio |
| --- | ---: | ---: | ---: | ---: |
| Decode8 | 4.03 | 3.38 | 2.75× | 2.36× |
| Decode16 | 8.06–8.63 | 6.83–7.59 | 2.76× | 2.36× |

RGBA16 has two candidate launches; RGBA8 is an affected-path check in the second
launch. Each requests three managed iterations and three native samples before
and after them, at a 200 ms target. Ratios use the faster native bracket mean and
retain the worst launch. Conservative 99.9% intervals and all actual samples
are included in the snapshot. These short screens guide iteration; they do not
certify uniform parity.

An earlier same-RGBA16-input diagnostic attributed about 1.19 ms to the unchanged
filter-None loop. That historical cost guided selection of this primitive; other
stage costs from that diagnostic precede several optimizations and are not current
attribution. The complete decoder screens above measure the candidate's benefit.

## Changed primitive

PNG section 9.2 specifies that filter None stores the original bytes. The decoder
now checks cancellation and returns from row unfiltering without rewriting those
bytes or reading neighbour rows. Other filters retain their prediction paths.
Existing row work/memory admission and every container, framing, checksum and
ownership check remain intact. No buffer, cache, dependency or unsafe code is added.

Focused controls check unchanged bytes and source guards at empty, short and
64 KiB boundary lengths, independence from neighbour rows, and pre-cancellation.
The complete PNG matrix covers both depths, all filters and Adam7 interlace.

Windows and Linux each pass 1792 Release tests with live FFmpeg: 212 internal,
686 public API and 894 differential. The same actual dependency-free Release
package passes both platforms and all three usage-guide examples. SHA256:
`18204e4a42e83534c9903b01ec552ce8bdc91cea7d192220729252bed25e47eb`.

The complete warm boundary remains identical resident PNG bytes to independent
owned RGBA output, including acquisition, decode, required native endian
conversion and release. Startup, IPC, file I/O and exact checks are excluded.
Both subjects use CPU 0 and one effective codec thread; managed execution uses
processor count one, workstation GC, no background GC and tiering disabled.
Pre-run quiet checks pass; post-run failures and cooldown observations remain
preserved. Measurements, builds and profiling are sequential on the dedicated VM.

After building/verifying the native worker and capturing source identity:

```powershell
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases Decode16 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases Decode8,Decode16 -Cpu 0 -NoBuild
```

Review quietness around each launch and repeat independently. Regenerate this
report with `python benchmarks/render-warm-png-none.py`.

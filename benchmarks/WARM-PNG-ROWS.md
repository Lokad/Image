# Warm single-CPU PNG: contiguous RGBA rows

Writing contiguous RGBA rows directly reduces the measured PNG8 decode gap by
about one-third from the [literal lookup checkpoint](WARM-PNG-LOOKUP.md).
Both depths remain above 2× FFmpeg, at roughly 4.3–4.4×. Other PNG profiles and
warm families remain open; this round does not establish uniform parity.

The [numeric snapshot](results/warm-png-rows-2026-10-09.json) retains original
statistics, actual managed measurements, native counters/times, exact checks,
component identities and quietness. The same two original 513x259 RGBA8/RGBA16
rasters are reused, including zero-alpha hidden RGB.

| Case | Before managed ms | After managed ms | Before worst mean ratio | After worst mean ratio |
| --- | ---: | ---: | ---: | ---: |
| Decode8 | 9.04 | 6.16–6.70 | 6.34× | 4.34× |
| Decode16 | 16.25–16.38 | 14.22 | 5.56× | 4.41× |

RGBA8 has two candidate launches; RGBA16 is the affected-path check in the second
launch. Each requests three managed iterations and three native samples before
and after them, at a 200 ms target. Ratios use the faster native bracket mean
and retain the worst launch. Both subjects slowed in the second launch; raw
latency and ratios are preserved. Conservative 99.9% intervals are included in
the snapshot. These short screens guide iteration, not certification.

An isolated diagnostic on the same RGBA8 input measured about 3.0 ms in owned
pixel materialization and 3.0 ms in framing. The pixel diagnostic uses original
method bodies with only accessibility changed in an ignored copy, includes
output allocation/release and validates every byte before timing. Production
visibility is unchanged. This is attribution, not native parity.

## Changed primitive

Contiguous RGBA passes use direct safe span copies at 8-bit and span endian
reversal at 16-bit on little-endian hosts. The framework selects SIMD or scalar
execution. RGBA already has the destination channels and precision, so no
per-pixel scaling or row lookup is necessary. Dense Adam7 rows qualify too;
sparse passes and other color types keep the existing mapper.

All channels, alpha and hidden RGB are preserved. Copying checks cancellation
at each 16 KiB chunk, at least as frequently as the previous per-4096-pixel loop.
Working-memory admission, per-row work charges, allocation/ownership and strict
container/framing/Adler validation are unchanged. There is no added buffer,
cache, production dependency, unsafe code or native binding.

Tests cover chunk endpoints and partial tails, source preservation, destination
guards, cancellation before any write, wide rows with all five filters and both
interlace settings, and the existing full authored PNG matrix.

Windows and Linux each pass 1700 Release tests with live FFmpeg: 120 internal,
686 public API and 894 differential. Each platform also passes 82 focused tests
with AVX disabled, then with all hardware intrinsics disabled. The same actual
dependency-free Release package passes both platforms and all three usage-guide
examples. SHA256:
`17cc5c4ea6229985740da36d78222659052467bbb61ad31d887eb091e0847f12`.

The complete warm boundary remains identical resident PNG bytes to independent
owned RGBA output, including acquisition, decode, required native endian
conversion and release. Startup, IPC, file I/O and exact checks are excluded.
Both subjects use CPU 0 and one effective codec thread; managed execution uses
processor count one, workstation GC, no background GC and tiering disabled.
Pre-run quiet checks pass; post-run failures and cooldown observations remain
preserved. Measurements, builds and profiling are sequential on the dedicated VM.

After building/verifying the native worker and capturing source identity:

```powershell
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases Decode8 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases Decode8,Decode16 -Cpu 0 -NoBuild
```

Review quietness around each launch and repeat independently. Regenerate this
report with `python benchmarks/render-warm-png-rows.py`.

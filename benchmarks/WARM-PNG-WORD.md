# Warm single-CPU PNG: inlined word refill

Safe word refills reduce the cost of strict Deflate framing. The complete RGBA16
decode screens improve from the [contiguous-row checkpoint](WARM-PNG-ROWS.md);
RGBA8 remains essentially unchanged. Both depths remain above 2× FFmpeg. Other
PNG profiles and warm families remain open; this round does not establish uniform parity.

The [numeric snapshot](results/warm-png-word-2026-10-09.json) retains original
statistics, actual managed measurements, native counters/times, exact checks,
component identities and quietness. The same two original 513x259 RGBA8/RGBA16
rasters are reused, including zero-alpha hidden RGB.

| Case | Before managed ms | After managed ms | Before worst mean ratio | After worst mean ratio |
| --- | ---: | ---: | ---: | ---: |
| Decode8 | 6.16–6.70 | 6.17 | 4.34× | 4.32× |
| Decode16 | 14.22 | 11.73–12.93 | 4.41× | 4.06× |

RGBA16 has two candidate launches; RGBA8 is the affected-path check in the second
launch. Each requests three managed iterations and three native samples before
and after them, at a 200 ms target. Ratios use the faster native bracket mean
and retain the worst launch. Conservative 99.9% intervals and all actual samples
are included in the snapshot. These short screens guide iteration, not certification.

An isolated same-input framing comparison measured 4.94 ms for accepted byte
refill, 4.37 ms for word refill, and 3.83 ms for explicitly inlined word refill.
This diagnostic copies only original owned reader bodies into ignored files;
it adds no source raster and is attribution, not native parity. Initial complete
noninlined screens were inconsistent; the snapshot retains those observations.

## Changed primitive

The reader refills its cache with one safe little-endian 32-bit span read when
four bytes remain, otherwise using the bounded byte tail. A 64-bit cache holds
at most 47 valid bits because reads request at most 16. Explicit JIT inlining
keeps refill logic within its callers. Only consumed bits are charged; unused
lookahead bytes return to the input at stored-block and checksum boundaries.

Canonical short/long/unassigned-prefix behavior, work limits, cancellation,
4096-byte scratch admission and all strict framing checks remain unchanged.
There is no new heap buffer, cache, dependency, unsafe code or native binding.

Six original fixed/stored/fixed streams cover every word-boundary stored tail,
unused trailing bytes, every truncation, exact work admission and agreement with
the BCL inflater. Existing alignment, long-code, malformed and complete authored
PNG tests remain active.

Windows and Linux each pass 1706 Release tests with live FFmpeg: 126 internal,
686 public API and 894 differential. The same actual dependency-free Release
package passes both platforms and all three usage-guide examples. SHA256:
`ddd2a94fea31f1f4f8adc3caf4aed43cde05f66590522ef01d387d4b01c481d3`.

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
report with `python benchmarks/render-warm-png-word.py`.

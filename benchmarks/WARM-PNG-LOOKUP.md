# Warm single-CPU PNG: literal prefix lookup

A generated nine-bit Huffman lookup reduces the measured RGBA16 decode time by
about 26% from the [previous reader checkpoint](WARM-PNG-FRAMING.md). RGBA8
changes little and becomes the largest measured warm gap. Both paths remain
above 2× FFmpeg; other PNG profiles and warm families remain open.

The [numeric snapshot](results/warm-png-lookup-2026-10-09.json) retains original
managed statistics and every actual measurement, native counters/times, component
identities, exact checks and quietness observations. The same two original
513x259 RGBA8/RGBA16 rasters are reused, with zero-alpha hidden RGB preserved.

| Case | Before managed ms | After managed ms | Before worst mean ratio | After worst mean ratio |
| --- | ---: | ---: | ---: | ---: |
| Decode8 | 9.35 | 9.04 | 6.59× | 6.34× |
| Decode16 | 22.09–22.13 | 16.25–16.38 | 7.58× | 5.56× |

RGBA16 has two independent launches on each side of the change; RGBA8 has one
before/after screen. Each launch requests three managed iterations and three
native samples before/after them, at a 200 ms target. The ratio uses the faster
native bracket mean and the worst launch. Conservative 99.9% mean intervals
remain in the snapshot; these short screens guide priority, not certification.

The preceding stage diagnostic attributed 10.3 ms of a 21.8 ms RGBA16 decode to
framing validation. The new 512-entry ushort lookup is generated independently
from validated canonical code counts/symbols, following RFC 1951. No upstream
table, source or binary is copied. Long codes and unassigned incomplete paths
use the complete canonical fallback. Distance and code-length readers remain
unchanged. The table is regenerated per compressed block; no image or entropy
result is cached.

The lookup and existing/nested arrays occupy 3992 bytes within the unchanged
4 KiB scratch allowance, with no heap allocation or global cache. Construction
admits 8192 bounded work units per fixed/dynamic block. Stored blocks build no
table. Actual consumed prefix-bit charges, output/history/window checks,
checksum positioning and per-symbol cancellation remain intact.

New analytical tests exercise every fixed literal category, a legal incomplete
end-only tree and its unassigned path. Existing 15-bit, dynamic repeat, malformed,
truncation, all-eight-alignment and exact memory/work tests pass. The work-limit
boundary now includes the explicitly documented lookup construction charge.

Windows and Linux each pass 1690 Release tests with live FFmpeg: 112 internal,
684 public API and 894 differential. The same actual dependency-free Release
package passes both platforms and all three usage-guide examples. SHA256:
`5468ad2f5ff3e50525c80496fa879fd7ab0c979218d2cd00cdf8e52bdb6d22f0`.

Workload boundaries and native output adaptation are unchanged from the previous
round: identical resident PNG bytes to independent owned RGBA output, with
acquisition, decoding, required native 16-bit endian conversion and release timed.
Startup, IPC, file I/O and validation are excluded. Both subjects use CPU 0 and
one effective codec thread; managed execution has processor count one,
workstation GC, no background GC and optimized JIT with tiering disabled.
All pre-run quiet checks pass; failed post-run observations and cooldowns are
preserved. Measurements, builds and profiling run sequentially on the same VM.

After building/verifying the original native worker and capturing source identity:

```powershell
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases Decode16 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases Decode8,Decode16 -Cpu 0 -NoBuild
```

Review quietness before/after each launch and repeat independently. Regenerate
this report with `python benchmarks/render-warm-png-lookup.py`.

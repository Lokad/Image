"""Render the original narrow PNG Adler snapshot."""
import json
from pathlib import Path
import sys

sys.stdout.reconfigure(encoding='utf-8')
root=Path(__file__).resolve().parents[1]
data=json.loads((root/'benchmarks/results/warm-png-adler-2026-10-09.json').read_text())
def cases(phase,name):
    return [c for r in data['observations'][phase] for c in r['cases'] if c['caseId']==name]
def span(values):
    return f'{min(values):.2f}–{max(values):.2f}' if len(values)>1 else f'{values[0]:.2f}'
print('''# Warm single-CPU PNG: safe SIMD Adler

Exact chunked Adler accumulation and safe AVX2 sums reduce complete PNG decode
cost from the [distance-prefix checkpoint](WARM-PNG-DISTANCE.md). Both depths
remain above 2× FFmpeg. Other PNG profiles and warm families remain open;
this round does not establish uniform parity.

The [numeric snapshot](results/warm-png-adler-2026-10-09.json) retains original
statistics, actual managed measurements, native counters/times, exact checks,
component identities and quietness. The same two original 513x259 RGBA8/RGBA16
rasters are reused, including zero-alpha hidden RGB.

| Case | Before managed ms | After managed ms | Before worst mean ratio | After worst mean ratio |
| --- | ---: | ---: | ---: | ---: |''')
for name in ('Decode8','Decode16'):
    before,after=cases('baseline',name),cases('adler',name)
    assert before and after
    print(f"| {name} | {span([c['managedStatistics']['Mean']/1e6 for c in before])} | {span([c['managedStatistics']['Mean']/1e6 for c in after])} | "
        f"{max(c['worstBracketRatio'] for c in before):.2f}× | {max(c['worstBracketRatio'] for c in after):.2f}× |")
print('''
RGBA16 has two candidate launches; RGBA8 is the affected-path check in the second
launch. Each requests three managed iterations and three native samples before
and after them, at a 200 ms target. Ratios use the faster native bracket mean
and retain the worst launch. Conservative 99.9% intervals and all actual samples
are included in the snapshot. These short screens guide iteration, not certification.

An isolated same-RGBA16-input diagnostic measured framing at 3.61 ms, the
original Adler row loop at 2.13 ms, and CRCs at 1.70 ms. The checksum prototype
measured 2.129 ms original, 0.257 ms chunked scalar and 0.032 ms safe AVX2,
with no allocations. Original recurrence, actual encoded Adler and deterministic
tail/overflow controls agree. This is attribution, not native parity.

## Changed primitive

The independently derived RFC 1950 recurrence reduces modulo 65521 at chunk
endpoints, at most 4096 bytes apart. With normalized initial sums, the largest
intermediate b is 65520 + n*65520 + 255*n*(n+1)/2 <= 2408052720, below uint.MaxValue.
The result is exact regardless of where rows, interlace passes or calls divide the input.

Safe AVX2 blocks sum 32 bytes and their weights (32-i). Each byte contributes to
a once and to b with its recurrence weight; adjacent signed pairs peak at 16065,
so saturation cannot change the result. Scalar fallback and tails use the same
recurrence. Filter bytes and all channels remain part of the checksum.

Cancellation is checked at least every 4 KiB. Existing per-row work and memory
admission remain unchanged. There is no additional array, heap buffer, static
cache, dependency, unsafe code or native binding; span loads check their bounds.

Independent recurrence controls cover vector/chunk boundaries and tails,
maximum normalized sums, all-255 and original varying bytes, partition/state
carry, unaligned spans, source guards and cancellation. The complete existing
PNG matrix covers both depths, colors, filters and interlace settings.

Windows and Linux each pass 1772 Release tests with live FFmpeg: 192 internal,
686 public API and 894 differential. Each platform passes 90 focused tests with
AVX disabled, then 90 with all hardware intrinsics disabled. The same actual
dependency-free Release package passes both platforms and all three usage-guide
examples. SHA256:
`'''+data['validation']['packageSha256']+'''`.

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
report with `python benchmarks/render-warm-png-adler.py`.
'''.rstrip())

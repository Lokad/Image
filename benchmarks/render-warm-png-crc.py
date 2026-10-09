"""Render the original narrow PNG CRC snapshot."""
import json
from pathlib import Path
import sys

sys.stdout.reconfigure(encoding='utf-8')
root=Path(__file__).resolve().parents[1]
data=json.loads((root/'benchmarks/results/warm-png-crc-2026-10-09.json').read_text())
def cases(phase,name):
    return [c for r in data['observations'][phase] for c in r['cases'] if c['caseId']==name]
def span(values):
    return f'{min(values):.2f}–{max(values):.2f}' if len(values)>1 else f'{values[0]:.2f}'
print('''# Warm single-CPU PNG: eight-byte CRC

A generated wider CRC update reduces complete PNG decode cost from the
[safe SIMD Adler checkpoint](WARM-PNG-ADLER.md). Both depths remain above
2× FFmpeg. PNG encoding is also screened because it shares CRC. Other PNG
profiles and warm families remain open; this round does not establish uniform parity.

The [numeric snapshot](results/warm-png-crc-2026-10-09.json) retains original
statistics, actual managed measurements, native counters/times, exact checks,
component identities and quietness. The same two original 513x259 RGBA8/RGBA16
rasters are reused, including zero-alpha hidden RGB.

| Case | Before managed ms | After managed ms | Before worst mean ratio | After worst mean ratio |
| --- | ---: | ---: | ---: | ---: |''')
for name in ('Decode8','Decode16'):
    before,after=cases('baseline',name),cases('crc',name)
    assert before and after
    print(f"| {name} | {span([c['managedStatistics']['Mean']/1e6 for c in before])} | {span([c['managedStatistics']['Mean']/1e6 for c in after])} | "
        f"{max(c['worstBracketRatio'] for c in before):.2f}× | {max(c['worstBracketRatio'] for c in after):.2f}× |")
encode=cases('crc','Encode8')[0]
print(f"\nThe affected Encode8 screen is {encode['managedStatistics']['Mean']/1e6:.2f} ms, "
      f"{encode['worstBracketRatio']:.2f}× its native bracket. The earlier encode screen was already below 2×; confirmation remains pending.")
print('''
RGBA16 has two candidate launches; RGBA8 and Encode8 are affected-path checks
in the second launch. Each requests three managed iterations and three native
samples before and after them, at a 200 ms target. Ratios use the faster native
bracket mean and retain the worst launch. Conservative 99.9% intervals and all
actual samples are included in the snapshot. These short screens guide iteration,
not certification of passing encode or uniform parity.

A separate same-RGBA16-input CRC prototype reduced chunk CRC time from 1.71 ms
to 0.44 ms with no per-operation allocations. Its results match actual chunk CRCs
and an independent bitwise recurrence on original boundary/tail inputs. This is
attribution, not native parity. It adds no benchmark raster.

## Changed primitive

The independently generated reflected PNG polynomial table combines eight byte
contributions into one safe little-endian word update, followed by a byte tail.
Slice k represents a byte followed by k zero bytes. All entries derive from the
format polynomial; no implementation table or upstream source is imported.

The fixed shared table grows from 1 KiB to 8 KiB and is read-only after initialization.
It adds no per-image buffer or image cache. Existing work/memory admission and
all CRC validation remain intact. Cancellation is checked every 4096 bytes as
before. There is no dependency, unsafe code, pointer or native binding.

Independent bitwise controls cover every byte value at every word position,
unaligned spans, word/chunk boundaries, tails, all-255 and original varying bytes,
source guards and pre-cancellation. Existing PNG container, encoder and complete
format tests remain active, including malformed CRC rejection.

Windows and Linux each pass 1790 Release tests with live FFmpeg: 210 internal,
686 public API and 894 differential. The same actual dependency-free Release
package passes both platforms and all three usage-guide examples. SHA256:
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
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases Encode8,Decode8,Decode16 -Cpu 0 -NoBuild
```

Review quietness around each launch and repeat independently. Regenerate this
report with `python benchmarks/render-warm-png-crc.py`.
'''.rstrip())

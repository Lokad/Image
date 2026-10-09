"""Render the original narrow PNG distance-prefix snapshot."""
import json
from pathlib import Path
import sys

sys.stdout.reconfigure(encoding='utf-8')
root=Path(__file__).resolve().parents[1]
data=json.loads((root/'benchmarks/results/warm-png-distance-2026-10-09.json').read_text())
def cases(phase,name):
    return [c for r in data['observations'][phase] for c in r['cases'] if c['caseId']==name]
def span(values):
    return f'{min(values):.2f}–{max(values):.2f}' if len(values)>1 else f'{values[0]:.2f}'
print('''# Warm single-CPU PNG: distance prefix lookup

A generated five-bit distance prefix lookup reduces strict Deflate framing cost.
The complete decode screens improve from the [word-refill checkpoint](WARM-PNG-WORD.md).
Both depths remain above 2× FFmpeg. Other PNG profiles and warm families remain
open; this round does not establish uniform parity.

The [numeric snapshot](results/warm-png-distance-2026-10-09.json) retains original
statistics, actual managed measurements, native counters/times, exact checks,
component identities and quietness. The same two original 513x259 RGBA8/RGBA16
rasters are reused, including zero-alpha hidden RGB.

| Case | Before managed ms | After managed ms | Before worst mean ratio | After worst mean ratio |
| --- | ---: | ---: | ---: | ---: |''')
for name in ('Decode8','Decode16'):
    before,after=cases('baseline',name),cases('distance',name)
    assert before and after
    print(f"| {name} | {span([c['managedStatistics']['Mean']/1e6 for c in before])} | {span([c['managedStatistics']['Mean']/1e6 for c in after])} | "
        f"{max(c['worstBracketRatio'] for c in before):.2f}× | {max(c['worstBracketRatio'] for c in after):.2f}× |")
print('''
RGBA8 has two candidate launches; RGBA16 is the affected-path check in the second
launch. Each requests three managed iterations and three native samples before
and after them, at a 200 ms target. Ratios use the faster native bracket mean
and retain the worst launch. Conservative 99.9% intervals and all actual samples
are included in the snapshot. These short screens guide iteration, not certification.

A separate same-RGBA8-input diagnostic measured framing at 2.71 ms, the original
Adler row loop at 1.16 ms, and chunk CRCs at 0.42 ms. An isolated prototype
comparison reduced framing from 2.50 ms to 1.81 ms. It uses original owned
reader bodies in ignored files, no extra raster, and is attribution, not native parity.

## Changed primitive

Distance codes use a 32-entry, five-bit prefix lookup generated from the validated
canonical tree. Each entry contains the decoded symbol and exact bit length.
Long codes and unassigned prefixes keep the complete canonical fallback;
reserved symbols, backward-history and zlib-window checks remain unchanged.

The extra table occupies 64 stack bytes. All arrays and nested construction
scratch total 4056 bytes, within the existing 4096-byte admission. Construction
charges an additional conservative 512 work units per compressed block; actual
consumed bits keep their exact charge and cancellation checks. There is no new
heap buffer or static cache, dependency, unsafe code or native binding.

Original authored streams cover every fixed distance symbol, reserved codes
30/31, full 32 KiB history, complete dynamic prefix depths 1..15, both sides of
the five-bit lookup boundary, short/long unassigned prefixes, exact work and
memory admission, truncated tails, and BCL inflation agreement. Existing stored
alignment, checksum-boundary and complete PNG tests remain active.

Windows and Linux each pass 1756 Release tests with live FFmpeg: 176 internal,
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
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases Decode8 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases Decode8,Decode16 -Cpu 0 -NoBuild
```

Review quietness around each launch and repeat independently. Regenerate this
report with `python benchmarks/render-warm-png-distance.py`.
'''.rstrip())

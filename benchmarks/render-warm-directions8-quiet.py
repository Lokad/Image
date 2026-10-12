"""Render retained three-direction RGBA8 evidence; never run benchmarks."""
import json,math,re,sys
from pathlib import Path
r=Path(__file__).resolve().parent;e=json.loads((r/'warm-directions8-quiet-evidence.json').read_text(encoding='utf-8'));rows=[]
for key,label in [('clockwise','Clockwise quarter turn'),('counterclockwise','Counterclockwise quarter turn'),('transverse','Mirrored transpose')]:
 o=e['cases'][key]['observations'][-1];s=o['managed']['statistics'];native=min(b['mean'] for b in o['nativeBrackets']);gc=re.fullmatch(r'// GC:\s+\d+\s+\d+\s+\d+\s+(\d+)\s+(\d+)',o['managed']['rawGc']);assert gc
 allocated=math.ceil(int(gc[1])/int(gc[2]));assert all(g['passed'] for g in o['quietness'].values()) and o['conservativeRatioUpper']<=2
 rows.append(f'| {label} | {s["Mean"]/1000:.2f} | {native/1000:.2f} | {o["worstBracketRatio"]:.5f} / {o["conservativeRatioUpper"]:.5f} | {allocated:,} | {len(o["managed"]["actual"])}/{s["N"]} | Confirmed |')
content=f'''# Warm RGBA8 quarter turns and mirrored transpose

All three fixed RGBA8 cases independently confirm below 2x FFmpeg, including
conservative uncertainty and both quiet guards. Production and benchmark code
are unchanged. Stop tuning these points; full required warm coverage stays open.

| Operation | Lokad.Image us | FFmpeg us | Ratio / upper | Managed allocated B/op | Actual/filtered | Status |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
{chr(10).join(rows)}

One existing original 513x259 RGBA8 raster produces owned 259x513 output through
each complete public operation. Independent integer coordinate permutations,
all RGBA samples, alpha, hidden RGB, source integrity and two live output owners
pass before/after timing. Normal/scalar public FFmpeg CLI checks also pass.
No new image, profile, shape, quality sweep, CPU profile or production primitive.

All six scored runs use owned source SHA-256
`90eccc1bc6eb9e1d42aaf26183658850f655b1db0ab433984807cc1a616c06b7`
at revision `24d742fef739eda4d3a2b099378ec61d4bc364fa`. Each pair retains its
three-actual screen and independent twelve-actual confirmation. Native pre/post
brackets target 50ms; managed iterations target 200ms. Every actual remains
retained; upper uses the larger filtered/all-actual conservative bound.
`exclusive-vm-idle-v2` checks live overlap and five-second aggregate/CPU0 busy
<=5%, I/O wait <=1%, steal <=1%. All twelve scored guard samples pass; no retries,
threshold changes, historical cooldowns or other operator waits are used.

Timing includes public rotation/transpose, admission, independent pixel
acquisition, full materialization and disposal. Released managed storage may be
reused; simultaneous live owners stay independent. Native transpose graphs
allocate/filter/release an independent raster each call. Graph/source/frame
wrappers are reused and padded output rows are exposed directly, without an
extra packing copy. No output is cached. Startup, I/O, IPC, generation, hashing
and validation are excluded. CPU0, one filter thread, normal SIMD, tiering off
and nonconcurrent workstation GC remain fixed. Native allocation is unmeasured;
managed allocation is raw GC bytes / operations rounded up, including harness.

Each actual child's four files and original receipt, full owned source and
accepted package are frozen before another preparation or source mutation.
All 571 actual-child production method bodies and flags match the accepted
Release package. Its preceding 2,410 tests and 19 scalar chroma/admission controls
per OS, plus all three examples through the same actual package on both OSes,
remain applicable and were not repeated without production/package changes.

[Numeric evidence](warm-directions8-quiet-evidence.json) retains all actuals,
brackets, guards, source/input/output/child/package identities, exact boundaries
and immutable archive digests. See the [current three-table summary](../docs/benchmark.md)
and [measurement recipe](../tools/WarmReference/README.md).
Regenerate with `python benchmarks/render-warm-directions8-quiet.py`;
`--check` verifies this report. Other unmatched/uncovered/unconfirmed cases stay open.
'''
p=r/'WARM-DIRECTIONS8-QUIET.md'
if '--check' in sys.argv:
 assert p.read_text(encoding='utf-8')==content,'RGBA8 direction report is stale.'
 print('RGBA8 direction report is current.')
else:p.write_text(content,encoding='utf-8',newline='\n')

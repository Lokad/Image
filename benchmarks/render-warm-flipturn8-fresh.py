"""Render retained fresh flip/half-turn confirmations without measuring."""
import json,math,re,sys
from pathlib import Path
r=Path(__file__).resolve().parent;e=json.loads((r/'warm-flipturn8-fresh-evidence.json').read_text(encoding='utf-8'));rows=[]
for key,label in [('vertical','Vertical flip RGBA8'),('half','Half turn RGBA8')]:
 o=e['cases'][key]['observations'][-1];s=o['managed']['statistics'];native=min(b['mean'] for b in o['nativeBrackets']);gc=re.fullmatch(r'// GC:\s+\d+\s+\d+\s+\d+\s+(\d+)\s+(\d+)',o['managed']['rawGc']);assert gc
 allocated=math.ceil(int(gc[1])/int(gc[2]));assert all(g['passed'] for g in o['quietness'].values()) and o['conservativeRatioUpper']<=2
 rows.append(f'| {label} | {s["Mean"]/1000:.2f} | {native/1000:.2f} | {o["worstBracketRatio"]:.5f} / {o["conservativeRatioUpper"]:.5f} | {allocated:,} | {len(o["managed"]["actual"])}/{s["N"]} | Confirmed |')
content=f'''# Warm RGBA8 vertical flip and half turn: fresh confirmations

Both fixed pairs independently confirm below 2x FFmpeg, including conservative
uncertainty and both quiet guards. Production and benchmark code are unchanged.
Stop tuning these points; full required warm coverage remains open.

| Operation | Lokad.Image us | FFmpeg us | Ratio / upper | Managed allocated B/op | Actual/filtered | Status |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
{chr(10).join(rows)}

The [preceding round](WARM-DIRECTIONS8-QUIET.md) supplied twelve fresh passing
quiet checks. One fresh independent confirmation per pair therefore proceeds
without changing any threshold or reclassifying earlier failures. Vertical
flip's first precheck fails and launches no benchmark; its source and four child
files are archived before one bounded fresh attempt, which passes both guards.
Half turn's fresh confirmation passes both guards without a retry. All earlier
screens, failed confirmations and failed retries remain retained in the
[original copy/direction report](WARM-COPIES8-QUIET.md).

One existing original 513x259 RGBA8 raster is reused. Complete RGBA, alpha,
hidden RGB, dimensions, source integrity, two live owners and normal/scalar
public FFmpeg CLI checks remain exact before/after timing. No new input, shape,
quality sweep, profile or production primitive. All scored runs use owned source
`2686498178bbc1d27bca0960ba139ee2e511c7b27fb0d38137f616aa6c4daaee`
at recorded revision `edff94a5d1f573883aed7dd93cd4c0b3e057bfc2`.

Each independent confirmation retains twelve actuals and twelve samples per
native bracket; upper is the larger filtered/all-actual conservative bound.
Native samples target 50ms and managed iterations 200ms. CPU0, one filter thread,
normal SIMD, tiering off and nonconcurrent workstation GC remain fixed.
`exclusive-vm-idle-v2` checks live overlap plus five-second aggregate/CPU0 busy
<=5%, I/O wait <=1%, steal <=1%. Load is diagnostic only; no historical cooldown,
other operator wait or failed-interval recategorization.

Timing includes complete public FlipVertical/Rotate(Half), admission, independent
pixel acquisition, full materialization and disposal. Released managed pixel
storage may be reused; simultaneous live results remain independent. Native
vertical flip returns a shared view, then allocates/copies owned top-to-bottom
pixels inside each operation. Native half turn materializes its horizontal flip
and reverses that owned raster's stride without another packing copy. Graph/
source/frame wrappers are reused; output pixels are not cached. Startup, I/O,
IPC, generation, hashing and validation are excluded. Managed allocation is raw
GC bytes / operations rounded up including harness; native allocation is unmeasured.

Four actual child identities, original receipts, complete source and package
are frozen before another preparation or mutation. All 571 accepted actual-child
production method bodies and flags match the same Release package. Its preceding
2,410 tests and 19 scalar chroma/admission controls per OS, plus all three examples
through the same actual package on both OSes, remain applicable and are not
repeated without production/package changes.

[Numeric evidence](warm-flipturn8-fresh-evidence.json) retains confirmations,
all actuals, brackets, guards, exact boundaries, input/output/source/child/package
identities and immutable archive digests. See the
[current three-table summary](../docs/benchmark.md) and
[measurement recipe](../tools/WarmReference/README.md).
Regenerate with `python benchmarks/render-warm-flipturn8-fresh.py`;
`--check` verifies this report. Other unmatched/uncovered cases remain open.
'''
p=r/'WARM-FLIPTURN8-FRESH.md'
if '--check' in sys.argv:
 assert p.read_text(encoding='utf-8')==content,'Fresh RGBA8 flip/turn report is stale.'
 print('Fresh RGBA8 flip/turn report is current.')
else:p.write_text(content,encoding='utf-8',newline='\n')

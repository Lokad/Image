"""Render retained independent warm RGBA8 bilinear confirmation."""
import json,math,re,sys
from pathlib import Path
r=Path(__file__).resolve().parents[1]
e=json.loads((r/'benchmarks/warm-bilinear8-quiet-evidence.json').read_text());last=e['observations'][-1]
assert e['independentQuietConfirmationThisPair'] and e['all566TimedMethodsAndFlagsMatchSameActualPackage']
assert last['conservativeRatioUpper']<=2 and len(last['managed']['actual'])==12
assert all(g['passed'] and g['protocol']=='exclusive-vm-idle-v2' for g in last['quietness'].values())
gc=re.fullmatch(r'// GC:\s+\d+\s+\d+\s+\d+\s+(\d+)\s+(\d+)',last['managed']['rawGc']);allocation=math.ceil(int(gc[1])/int(gc[2]))
rows=[f'| {o["label"]} | {o["managed"]["statistics"]["Mean"]/1000:,.2f} | {o["worstBracketRatio"]:.5f}x | {o["conservativeRatioUpper"]:.5f}x | Pass |' for o in e['observations']]
text=f'''# Warm bilinear RGBA8: independent quiet confirmation

Complete public bilinear resize independently confirms at
{last['worstBracketRatio']:.3f}x FFmpeg, conservative upper
{last['conservativeRatioUpper']:.3f}x. Both idle guards pass. This fixed pair
clears 2x; full required warm coverage remains open. Production is unchanged.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{chr(10).join(rows)}

One existing original 513x259 RGBA8 raster is resized to 256x129. Managed
samples match the independent exact rational reference. Native alpha matches
exactly, zero-alpha RGB is zero, and the unchanged RGB maximum <=1/RMSE <=0.25
bound passes. Input, managed/native output hashes, native CLI/deterministic
pixels, source and independent owners are unchanged. No image, geometry,
filter or strategy enumeration. RGBA16 remains a separate open case.

Managed timing includes public Resize, geometry preparation, admission,
allocation/materialization and disposal. The native public FFmpeg graph includes
packed/planar conversion, zscale widened triangle filtering, encoded-space
premultiplication/unpremultiplication, final RGBA conversion and independent
output acquisition/release. Graph/source/frame wrappers and geometry contexts
are reused; every call filters the full input. Native padded output rows stay
exposed. No output cache. Startup, I/O, IPC, hashing and validation are excluded.
CPU0, one filter thread, thread type zero, normal SIMD, disabled managed tiering
and nonconcurrent workstation GC remain fixed.

Protocol `exclusive-vm-idle-v2` checks live jobs and five-second aggregate/CPU0
samples: busy <=5%, I/O wait <=1%, steal <=1%. Historical load is diagnostic
only. Preparation/smoke checks occur outside timing. The earlier
[packed-alpha round](WARM-RESIZE-QUAD-ALPHA.md) retains its original failed guards.

The independent repeat retains 12 actual / {last['managed']['statistics']['N']}
filtered measurements and 12 native samples per bracket. Its upper uses the
larger filtered/all-actual bound. Managed allocation is **{allocation:,} B/op**,
raw GC bytes / operations rounded up, including harness overhead. Native
allocation totals are unmeasured. Retained owners/cold allocation need separate coverage.

Both timing children match all 566 production method bodies/flags in the same
accepted Release package. Preceding 2,387 tests and 90 scalar framing controls
per OS, plus same-package/all-three-example checks on both OSes, remain
applicable: no production or benchmark implementation changed.
[Numeric evidence](warm-bilinear8-quiet-evidence.json) retains measurements,
receipts, source/input/output/child/package identities, guards and immutable
raw archive digests. Regenerate with `python benchmarks/render-warm-bilinear8-quiet.py`;
`--check` verifies this report. See the [current summary](../docs/benchmark.md).
Stop tuning this passing pair and confirm the largest remaining gap.
'''
target=r/'benchmarks/WARM-BILINEAR8-QUIET.md'
if '--check' in sys.argv:
 assert target.read_text(encoding='utf-8')==text,'Bilinear8 confirmation report is stale.'
 print('Bilinear8 confirmation report is current.')
else:target.write_text(text,encoding='utf-8',newline='\n')

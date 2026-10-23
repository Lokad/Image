"""Render the retained independent warm VP8 decode confirmation."""
import json,math,re,sys
from pathlib import Path
r=Path(__file__).resolve().parents[1]
e=json.loads((r/'benchmarks/warm-vp8decode-quiet-evidence.json').read_text());last=e['observations'][-1]
assert e['independentQuietConfirmationThisPair'] and e['all566TimedMethodsAndFlagsMatchSameActualPackage']
assert last['conservativeRatioUpper']<=2 and len(last['managed']['actual'])==12
assert all(v['passed'] and v['protocol']=='exclusive-vm-idle-v2' for v in last['quietness'].values())
gc=re.fullmatch(r'// GC:\s+\d+\s+\d+\s+\d+\s+(\d+)\s+(\d+)',last['managed']['rawGc']);allocation=math.ceil(int(gc[1])/int(gc[2]))
rows=[f'| {o["label"]} | {o["managed"]["statistics"]["Mean"]/1000:,.2f} | {o["worstBracketRatio"]:.5f}x | {o["conservativeRatioUpper"]:.5f}x | Pass |' for o in e['observations']]
text=f'''# Warm VP8 decode: independent quiet confirmation

Complete public VP8/ALPH decode independently confirms at
{last['worstBracketRatio']:.3f}x FFmpeg, conservative upper
{last['conservativeRatioUpper']:.3f}x. Both idle guards pass. This fixed pair
clears 2x; full required warm coverage remains open. Production is unchanged.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{chr(10).join(rows)}

The existing original 513x259 RGBA8 raster and identical frozen 98,590-byte
native-quality-75 VP8/ALPH keyframe are reused. No new images, profiles, effort
or quality enumeration. Both subjects decode the same encoded input; encoder
quality matching is irrelevant here. Fixed managed/native output hashes,
RGB maximum <=4/RMSE <=1.5, exact alpha, source and independent owners pass.
Native CLI pixels match the registered reference pipeline exactly.

Both time complete decode to independently owned RGBA8 and disposal. Native
decode/reset, centered bilinear chroma expansion, limited BT.601 conversion
through RGBA16, reduction, alpha merge and scratch/output allocation are included.
Codec, packet/frame wrappers and conversion contexts are reused; every pixel is
reprocessed. Padded rows stay exposed. Startup, I/O, IPC, hashing, validation
and encoding are excluded. CPU0, one codec/conversion thread, thread type zero,
normal SIMD, disabled tiering and nonconcurrent workstation GC are fixed.

Protocol `exclusive-vm-idle-v2` checks live jobs and five-second aggregate/CPU0
samples: busy <=5%, I/O wait <=1%, steal <=1%. Exclusive VM use requires no
historical-load cooldown. Load averages are diagnostic only. The earlier
[color-conversion round](WARM-VP8-COLOR-BLOCK.md) retains its earlier failed
guards; these are not reclassified. Preparation/smoke checks occur outside timing.

The independent repeat retains 12 actual / {last['managed']['statistics']['N']}
filtered measurements and 12 native samples per bracket. The upper uses the
larger filtered/all-actual bound. Managed allocation is **{allocation:,} B/op**,
raw GC bytes / operations rounded up, including harness overhead. Native
allocation totals are unmeasured.

Both actual timing children match all 566 production method bodies/flags in
the same accepted Release package. The preceding 2,387 tests and 90 scalar
framing controls per OS, plus same-package/all-three-example checks on both
OSes, remain applicable: no production or benchmark implementation changed.
[Numeric evidence](warm-vp8decode-quiet-evidence.json) retains measurements,
receipts, source/input/output/child/package identities, guards and immutable
raw archive digests. Regenerate with `python benchmarks/render-warm-vp8decode-quiet.py`;
`--check` verifies this report. See the [current summary](../docs/benchmark.md).
Stop tuning this passing pair; confirm the largest remaining matched gap.
'''
target=r/'benchmarks/WARM-VP8-DECODE-QUIET.md'
if '--check' in sys.argv:
 assert target.read_text(encoding='utf-8')==text,'VP8 decode report is stale.'
 print('VP8 decode confirmation report is current.')
else:target.write_text(text,encoding='utf-8',newline='\n')

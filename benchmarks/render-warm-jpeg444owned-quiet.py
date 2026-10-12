"""Render retained independent warm owned-input JPEG 4:4:4 decode confirmation."""
import json,math,re,sys
from pathlib import Path
r=Path(__file__).resolve().parents[1];e=json.loads((r/'benchmarks/warm-jpeg444owned-quiet-evidence.json').read_text());last=e['observations'][-1]
assert e['independentQuietConfirmationThisPair'] and e['all568TimedMethodsAndFlagsMatchSameActualPackage']
assert last['conservativeRatioUpper']<=2 and len(last['managed']['actual'])==12
assert all(g['passed'] and g['protocol']=='exclusive-vm-idle-v2' for g in last['quietness'].values())
gc=re.fullmatch(r'// GC:\s+\d+\s+\d+\s+\d+\s+(\d+)\s+(\d+)',last['managed']['rawGc']);allocation=math.ceil(int(gc[1])/int(gc[2]))
rows=[f'| {o["label"]} | {o["managed"]["statistics"]["Mean"]/1000:,.2f} | {o["worstBracketRatio"]:.5f}x | {o["conservativeRatioUpper"]:.5f}x | {"Pass" if all(g["passed"] for g in o["quietness"].values()) else "FAIL"} |' for o in e['observations']]
text=f'''# Warm owned-input JPEG 4:4:4: quiet confirmation

Complete public decode independently confirms at
{last['worstBracketRatio']:.3f}x FFmpeg, conservative upper
{last['conservativeRatioUpper']:.3f}x. Both idle guards pass. This fixed pair
clears 2x; full required warm coverage remains open. Production is unchanged.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{chr(10).join(rows)}

The existing original 513x259 RGBA8 raster and frozen 324,002-byte owned
quality-69 baseline JPEG are reused. Both subjects decode the identical input;
encoder quality matching is irrelevant to this comparison. Input and
managed/native output hashes remain exact. Normal/scalar native CLI agreement
stays RGB maximum <=4/RMSE <=1.5, with exact opaque alpha, unchanged source
and independent owners. No image, shape, quality, scan-strategy or effort enumeration.

Timing includes complete public Decode, admission, entropy reconstruction,
inverse transform, color conversion, owned output allocation and disposal.
Native software decode resets state, converts full-range BT.601 to RGBA8, and
acquires/releases independent output each time. Codec/conversion/packet/frame
wrappers are reused; every pixel is reprocessed. Padded rows stay exposed.
Startup, I/O, IPC, preparation, hashing and validation are excluded. CPU0, one
codec/conversion thread, thread type zero, normal SIMD, tiering off and
nonconcurrent workstation GC remain fixed.

The short run's post-run I/O-wait guard fails and remains failed; its raw
evidence is frozen before confirmation. Protocol `exclusive-vm-idle-v2` checks
live jobs and five-second aggregate/CPU0 samples: busy <=5%, I/O wait <=1%,
steal <=1%; historical load is diagnostic only. The independent repeat retains
12 actual / 12 filtered measurements and 12 native samples per bracket.
Upper = larger filtered/all-actual bound. Managed allocation is **{allocation:,} B/op**,
raw GC bytes / operations rounded up including harness overhead. Native
allocation totals are unmeasured.

Both timing children match all 568 production method bodies/flags in the same
accepted Release package. The preceding 2,388 tests and 254 scalar JPEG controls
per OS, plus same-package/all-three-example checks on both OSes, remain applicable.
No production or benchmark implementation changed. The
[earlier block-local work evidence](WARM-JPEG-LOCAL-WORK.md) remains retained.
[Numeric evidence](warm-jpeg444owned-quiet-evidence.json) preserves measurements,
receipts, source/input/output/child/package identities, guards and immutable
archive digests. Regenerate with `python benchmarks/render-warm-jpeg444owned-quiet.py`;
`--check` verifies this report. See the [current summary](../docs/benchmark.md).
Stop tuning this passing pair and cover the largest remaining material gap.
'''
target=r/'benchmarks/WARM-JPEG444-OWNED-QUIET.md'
if '--check' in sys.argv:
 assert target.read_text(encoding='utf-8')==text,'Owned JPEG444 confirmation report is stale.'
 print('Owned JPEG444 confirmation report is current.')
else:target.write_text(text,encoding='utf-8',newline='\n')

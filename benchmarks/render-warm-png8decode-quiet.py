"""Render retained independent warm PNG RGBA8 decode confirmation."""
import json,math,re,sys
from pathlib import Path
r=Path(__file__).resolve().parents[1];e=json.loads((r/'benchmarks/warm-png8decode-quiet-evidence.json').read_text());last=e['observations'][-1]
assert e['independentQuietConfirmationThisPair'] and e['all568TimedMethodsAndFlagsMatchSameActualPackage']
assert last['conservativeRatioUpper']<=2 and len(last['managed']['actual'])==12
assert all(g['passed'] and g['protocol']=='exclusive-vm-idle-v2' for g in last['quietness'].values())
gc=re.fullmatch(r'// GC:\s+\d+\s+\d+\s+\d+\s+(\d+)\s+(\d+)',last['managed']['rawGc']);allocation=math.ceil(int(gc[1])/int(gc[2]))
rows=[f'| {o["label"]} | {o["managed"]["statistics"]["Mean"]/1000:,.2f} | {o["worstBracketRatio"]:.5f}x | {o["conservativeRatioUpper"]:.5f}x | {"Pass" if all(g["passed"] for g in o["quietness"].values()) else "FAIL"} |' for o in e['observations']]
text=f'''# Warm PNG RGBA8 decode: quiet confirmation

Complete public PNG decode independently confirms at
{last['worstBracketRatio']:.3f}x FFmpeg, conservative upper
{last['conservativeRatioUpper']:.3f}x. Both idle guards pass. This fixed pair
clears 2x; full required warm coverage remains open. Production is unchanged.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{chr(10).join(rows)}

One existing original 513x259 RGBA8 raster and its identical 218,775-byte PNG,
level 6/filter None/noninterlaced, are reused. All RGBA samples, alpha, hidden RGB,
source and independent owners match exactly. Normal/scalar native CLI checks
pass. No new image, filter, shape or quality enumeration.

Timing includes complete public Decode, framing/checksums, inflation, row
reconstruction, admission, allocation and disposal. Native decode resets state
and acquires/releases independently owned output each time. Codec/frame wrappers
are reused; every pixel is decoded. Padded rows stay exposed. Startup, I/O, IPC,
preparation, hashing and validation are excluded. CPU0, one codec/conversion
thread, thread type zero, normal SIMD, tiering off and nonconcurrent workstation GC.

The screen fails its post-run CPU0 I/O-wait guard (2.004%) and remains failed.
Its raw evidence is frozen before the independent confirmation. Protocol
`exclusive-vm-idle-v2` checks live jobs and five-second aggregate/CPU0 samples:
busy <=5%, I/O wait <=1%, steal <=1%; historical load is diagnostic only.
The repeat retains 12 actual / {last['managed']['statistics']['N']} filtered
measurements and 12 native samples per bracket. Upper = larger filtered/all-actual
bound. Managed allocation is **{allocation:,} B/op**, raw GC bytes / operations
rounded up including harness overhead. Native allocation totals are unmeasured.

Both timing children match all 568 production method bodies/flags in the same
accepted Release package. The preceding 2,388 tests and 254 scalar JPEG controls
per OS, plus same-package/all-three-example checks on both OSes, remain applicable.
There is no production or benchmark implementation change.
[Numeric evidence](warm-png8decode-quiet-evidence.json) retains measurements,
receipts, source/input/output/child/package identities, guards and immutable
archive digests. Regenerate with `python benchmarks/render-warm-png8decode-quiet.py`;
`--check` verifies this report. See the [current summary](../docs/benchmark.md).
Stop tuning this passing pair and cover the largest remaining material gap.
'''
target=r/'benchmarks/WARM-PNG8-DECODE-QUIET.md'
if '--check' in sys.argv:
 assert target.read_text(encoding='utf-8')==text,'PNG8 confirmation report is stale.'
 print('PNG8 confirmation report is current.')
else:target.write_text(text,encoding='utf-8',newline='\n')

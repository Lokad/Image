"""Render retained independent warm VP8L decode confirmation."""
import json,math,re,sys
from pathlib import Path
r=Path(__file__).resolve().parents[1];e=json.loads((r/'benchmarks/warm-vp8ldecode-quiet-evidence.json').read_text());last=e['observations'][-1]
assert e['independentQuietConfirmationThisPair'] and e['all568TimedMethodsAndFlagsMatchSameActualPackage']
assert last['conservativeRatioUpper']<=2 and len(last['managed']['actual'])==12
assert all(g['passed'] and g['protocol']=='exclusive-vm-idle-v2' for g in last['quietness'].values())
gc=re.fullmatch(r'// GC:\s+\d+\s+\d+\s+\d+\s+(\d+)\s+(\d+)',last['managed']['rawGc']);allocation=math.ceil(int(gc[1])/int(gc[2]))
rows=[f'| {o["label"]} | {o["managed"]["statistics"]["Mean"]/1000:,.2f} | {o["worstBracketRatio"]:.5f}x | {o["conservativeRatioUpper"]:.5f}x | Pass |' for o in e['observations']]
text=f'''# Warm VP8L decode: quiet confirmation

Complete public lossless WebP decode independently confirms at
{last['worstBracketRatio']:.3f}x FFmpeg, conservative upper
{last['conservativeRatioUpper']:.3f}x. Both idle guards pass. This fixed pair
clears 2x; full required warm coverage remains open. Production is unchanged.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{chr(10).join(rows)}

The existing original 513x259 RGBA8 raster and frozen owned VP8L input
({last['receipt']['inputBytes']:,} bytes) are reused. This declared literal/run
profile has no transforms, color cache or spatial entropy groups. All RGBA
samples, alpha, hidden RGB, input/output hashes, native CLI pixels, source and
independent owners remain exact. No new raster, profile, shape or quality sweep.
Other material VP8L decode profiles remain open; this is one fixed case.

Timing includes complete public Decode, admission, entropy reconstruction,
owned output acquisition and disposal. Native software WebP decode resets
state, converts ARGB to RGBA and acquires/releases independent output each time.
Codec/conversion contexts and packet/frame wrappers are reused; every pixel is
decoded and converted. Padded rows stay exposed. Startup, I/O, IPC, input
encoding/preparation, hashing and validation are excluded. CPU0, one codec/
conversion thread, thread type zero, normal SIMD, tiering off and nonconcurrent
workstation GC remain fixed.

Protocol `exclusive-vm-idle-v2` checks live jobs and five-second aggregate/CPU0
samples: busy <=5%, I/O wait <=1%, steal <=1%; historical load is diagnostic
only. The independent repeat retains 12 actual / 12 filtered measurements and
12 native samples per bracket. Upper = larger filtered/all-actual bound.
Managed allocation is **{allocation:,} B/op**, raw GC bytes / operations rounded
up including harness overhead. Native allocation totals are unmeasured.

Both timing children match all 568 production method bodies/flags in the same
accepted Release package. The preceding 2,388 tests and 254 scalar JPEG controls
per OS, plus same-package/all-three-example checks on both OSes, remain applicable.
No production or benchmark implementation changed. The
[earlier reader evidence](WARM-VP8L-READ-INLINE.md) and its failures remain retained.
[Numeric evidence](warm-vp8ldecode-quiet-evidence.json) preserves measurements,
receipts, source/input/output/child/package identities, guards and immutable
archive digests. Regenerate with `python benchmarks/render-warm-vp8ldecode-quiet.py`;
`--check` verifies this report. See the [current summary](../docs/benchmark.md).
Stop tuning this passing pair and establish the missing required encode baseline.
'''
target=r/'benchmarks/WARM-VP8L-DECODE-QUIET.md'
if '--check' in sys.argv:
 assert target.read_text(encoding='utf-8')==text,'VP8L confirmation report is stale.'
 print('VP8L confirmation report is current.')
else:target.write_text(text,encoding='utf-8',newline='\n')

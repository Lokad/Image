"""Render three retained RGBA16 direction confirmations; never benchmark."""
import json,math,re,sys
from pathlib import Path
r=Path(__file__).resolve().parents[1];e=json.loads((r/'benchmarks/warm-directions16-quiet-evidence.json').read_text());rows=[]
for c in e['cases'].values():
 o=c['observations'][-1];assert c['independentQuietConfirmationThisPair'] and c['all569TimedMethodsAndFlagsMatchSameActualPackage']
 assert len(o['managed']['actual'])==12 and o['conservativeRatioUpper']<=2 and o['certifiedThisPair']
 assert all(g['passed'] and g['protocol']=='exclusive-vm-idle-v2' for g in o['quietness'].values())
 gc=re.fullmatch(r'// GC:\s+\d+\s+\d+\s+\d+\s+(\d+)\s+(\d+)',o['managed']['rawGc']);allocation=math.ceil(int(gc[1])/int(gc[2]))
 rows.append(f'| {c["label"]} | {o["managed"]["statistics"]["Mean"]/1000:,.2f} | {min(b["mean"] for b in o["nativeBrackets"])/1000:,.2f} | {o["worstBracketRatio"]:.5f} / {o["conservativeRatioUpper"]:.5f} | {allocation:,} | 12 / {o["managed"]["statistics"]["N"]} | Pass |')
text=f'''# Warm RGBA16 direction coverage: quiet confirmations

Counterclockwise quarter turn, mirrored transpose and vertical flip independently
confirm below 2x FFmpeg, including conservative uncertainty and both quiet guards.
Production and benchmark implementations are unchanged. Full required warm
coverage remains open; no averaging establishes uniform parity.

| Fixed pair | Lokad.Image microseconds | FFmpeg microseconds | Ratio / upper | Managed allocated B/op | Actual / filtered | Quiet guards |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
{chr(10).join(rows)}

All measurements use owned source SHA-256
`a23ea8c24a2d3457c2334de1aedbe710c1bc0d6f5d6edb403879fe56e2725be8`,
revision `93d17416eacd295ae0603b7685a6b242abbcba2a`. One existing original
513x259 RGBA16 raster is reused, without additional images, profiles, geometry
searches or quality calibration. Quarter turn and mirrored transpose produce
259x513 output; vertical flip retains 513x259. Every UInt16 channel, alpha,
hidden RGB, source integrity and independent owned result remains exact.
Both normal and scalar public FFmpeg CLI comparisons pass before timing.

Each pair retains one three-actual screen and one independent twelve-actual
confirmation, with three/twelve samples per native pre/post bracket at the
declared 50 ms target. Managed iterations target 200 ms. All actuals are retained;
upper is the larger filtered/all-actual conservative bound. All six scored runs
pass `exclusive-vm-idle-v2`, without retry or threshold changes. The vertical-flip
screen's wide three-sample upper bound did not qualify it; its independent
confirmation resolves that uncertainty. Earlier failed guards remain failed.
Historical load is diagnostic only; no historical cooldown or other operator wait.
Four actual child identities and original receipts are frozen before subsequent
preparation or owned-source mutation.

Timing includes complete public Rotate/Transpose/FlipVertical, admission,
independent output acquisition, full materialization and disposal. Released
managed pixel storage may be reused; simultaneously live owners are independent.
Native quarter-turn/transpose graphs process and release independent output.
Native vertical flip begins with a negative-stride shared view, then allocates
and materializes an owned top-to-bottom raster inside each operation before
release. Native graph/source/frame wrappers are reused; no output is cached.
Padded rows remain exposed. Startup, I/O, IPC, preparation, hashing and validation
are excluded. CPU0, one filter thread, normal SIMD, tiering off and nonconcurrent
workstation GC remain fixed.

Managed allocation is **128 B/op** for every pair, raw GC allocated bytes / timed
operations rounded up including harness overhead. Native allocations are
unmeasured. All 569 production method bodies and flags in both actual children
for every pair match the same accepted Release package. Its preceding 2,390 tests
and 39 scalar Huffman controls per OS, plus actual-package/all-three-example
checks on both OSes, remain applicable; these checks were not repeated without
production/package changes.

[Numeric evidence](warm-directions16-quiet-evidence.json) retains screens, all
actuals, brackets, source/input/output/child/package identities, guards and
immutable archive digests. Compiler prefix paths are normalized only in public
copies; original receipts remain bound by their digests. Earlier
[counterclockwise](WARM-COUNTERCLOCKWISE-TILES.md),
[mirrored-transpose](WARM-TRANSVERSE-TILES.md) and
[vertical-flip](WARM-VFLIP-ROWS.md) evidence stays historical at its reported
sources, without a newly certified before/after speedup claim. See the
[current three-table summary](../docs/benchmark.md) and
[measurement recipe](../tools/WarmReference/README.md).
Regenerate with `python benchmarks/render-warm-directions16-quiet.py`; `--check`
verifies this report. Stop tuning passing points and cover a remaining case.
'''
target=r/'benchmarks/WARM-DIRECTIONS16-QUIET.md'
if '--check' in sys.argv:
 assert target.read_text(encoding='utf-8')==text,'RGBA16 direction report is stale.'
 print('RGBA16 direction report is current.')
else:target.write_text(text,encoding='utf-8',newline='\n')

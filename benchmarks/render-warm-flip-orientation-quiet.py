"""Render retained flip/orientation coverage, including failed quiet guards."""
import json,math,re,sys
from pathlib import Path
r=Path(__file__).resolve().parents[1];e=json.loads((r/'benchmarks/warm-flip-orientation-quiet-evidence.json').read_text());rows=[]
for c in e['cases'].values():
 o=c['observations'][-1];assert c['all569TimedMethodsAndFlagsMatchSameActualPackage'] and len(o['managed']['actual'])==12
 quiet=all(g['passed'] for g in o['quietness'].values())
 if c['independentQuietConfirmationThisPair']:assert quiet and o['conservativeRatioUpper']<=2 and o['certifiedThisPair']
 else:assert not o['certifiedThisPair']
 gc=re.fullmatch(r'// GC:\s+\d+\s+\d+\s+\d+\s+(\d+)\s+(\d+)',o['managed']['rawGc']);allocation=math.ceil(int(gc[1])/int(gc[2]))
 status='Confirmed' if c['independentQuietConfirmationThisPair'] else 'Guard failed / OPEN'
 rows.append(f'| {c["label"]} | {o["managed"]["statistics"]["Mean"]/1000:,.2f} | {min(b["mean"] for b in o["nativeBrackets"])/1000:,.2f} | {o["worstBracketRatio"]:.5f} / {o["conservativeRatioUpper"]:.5f} | {allocation:,} | 12 / {o["managed"]["statistics"]["N"]} | {status} |')
text=f'''# Warm horizontal flip and normal-orientation coverage

Horizontal RGBA8 flip and normal RGBA16 orientation independently confirm below
2x FFmpeg, including conservative uncertainty and both quiet guards. Horizontal
RGBA16 flip remains **OPEN**: its measured confirmation failed the post-run guard,
and the one bounded fresh attempt failed its pre-run guard, launching no benchmark.
Production and benchmark implementations are unchanged. Full required warm
coverage remains open; no averaging or numeric-only acceptance is used.

| Fixed pair | Lokad.Image microseconds | FFmpeg microseconds | Ratio / upper | Managed allocated B/op | Actual / filtered | Status |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
{chr(10).join(rows)}

Failed-guard numbers above are retained observations, not a certified performance
claim. All measurements use owned source SHA-256
`ff8fb63d1fc2732be8f13cd2e4b3ab2541ad077d3d0986230b64661c76343a63`,
revision `07d88658e530e9d83817e34aad866bc9c4b8749d`. The same two original
513x259 RGBA8/16 rasters are reused, without new images, profiles or geometry.
All four channel samples, alpha, hidden RGB, source integrity and independent
owned results remain exact. Separate native worker and normal/scalar public CLI
controls reproduce original whole-pixel horizontal placement at both depths.
Normal-orientation setup checks both CLI modes and exact complete UInt16 copies.

The generated horizontal-flip job prepares both depths once; dispatch inspection
proves benchmark ID 0 selects depth 8 and ID 1 depth 16. Each case then runs
separately and sequentially under fresh guards, using that verified prepared child.
Every pair retains its three-actual screen and twelve-actual confirmation, with
three/twelve samples in each native pre/post bracket at the declared 50 ms target.
Managed iterations target 200 ms. All actuals are retained; upper is the larger
filtered/all-actual conservative bound. Four actual child identities and original
receipts are frozen before another preparation or owned-source mutation.

Protocol `exclusive-vm-idle-v2` keeps live-job checks and five-second aggregate/
CPU0 samples: busy <=5%, I/O wait <=1%, steal <=1%. Failed intervals have clear
CPU0 and live-job checks but aggregate I/O wait above 1%. No threshold is relaxed
and no failed interval is recategorized. A separate three-second user-readable
process I/O diagnostic found no activity; it cannot establish absence of system
or hypervisor activity. Normal orientation's initial pre-run failure launched
nothing, was archived and its one bounded fresh screen passed. Historical load
remains diagnostic only; there is no historical cooldown or other operator wait.

Timing includes complete public FlipHorizontal/AutoOrient, admission, independent
output acquisition, full materialization and disposal. Released managed pixel
storage may be reused; simultaneously live owners remain independent. Native
horizontal flip executes the public FFmpeg graph and releases its materialized
result. Native null orientation starts with a shared graph view, then allocates
and copies an independent raster inside every operation before release. Native
graph/source/frame wrappers are reused; no output is cached. Padded rows remain
exposed. Startup, I/O, IPC, preparation, hashing and validation are excluded.
CPU0, one filter thread, normal SIMD, tiering off and nonconcurrent workstation
GC remain fixed.

Allocation is raw GC allocated bytes / timed operations rounded up, including
harness overhead. Native allocation totals are unmeasured. All 569 production
method bodies and flags in both actual children for every pair match the same
accepted Release package. Its preceding 2,390 tests and 39 scalar Huffman controls
per OS, plus actual-package/all-three-example checks on both OSes, remain
applicable; production/package checks were not repeated without changes.

[Numeric evidence](warm-flip-orientation-quiet-evidence.json) retains screens,
all actuals, brackets, source/input/output/child/package identities, CLI controls,
generated dispatch, guards and immutable archive digests, including failures.
Compiler prefix paths are normalized only in public copies; original receipts
remain bound by their digests. Earlier [horizontal-flip](WARM-HORIZONTAL-FLIP.md)
and [normal-orientation](WARM-NORMAL-ROWS.md) evidence stays historical at its
reported sources, without a newly certified before/after speedup claim.
See the [current three-table summary](../docs/benchmark.md) and
[measurement recipe](../tools/WarmReference/README.md).
Regenerate with `python benchmarks/render-warm-flip-orientation-quiet.py`;
`--check` verifies this report. Stop tuning passing points; cover a remaining case.
'''
target=r/'benchmarks/WARM-FLIP-ORIENTATION-QUIET.md'
if '--check' in sys.argv:
 assert target.read_text(encoding='utf-8')==text,'Flip/orientation quiet report is stale.'
 print('Flip/orientation quiet report is current.')
else:target.write_text(text,encoding='utf-8',newline='\n')

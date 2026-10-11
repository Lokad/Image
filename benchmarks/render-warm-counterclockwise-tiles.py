"""Render one original RGBA16 counterclockwise-quarter-turn performance round."""
import json,sys
from pathlib import Path
folder=Path(__file__).resolve().parent
e=json.loads((folder/'warm-counterclockwise-tiles-evidence.json').read_text())
assert e['project']=='Lokad.Image' and not e['uniformWarmParityComplete']
screen=e['observations'][1];repeat=e['observations'][-1]
# Failed screen guards are retained.
assert repeat['conservativeRatioUpper']<2
quiet=all(g['passed'] for g in repeat['quietness'].values())
assert e['independentQuietConfirmationThisPair']==quiet
confirmation=(
 'The independent twelve-sample repeat passes both unchanged quiet guards and\n'
 'certifies this fixed pair only.' if quiet else
 'The twelve-sample repeat clears the numerical bound but fails a quiet guard;\n'
 'independent quiet confirmation remains open. A later cooldown cannot certify\n'
 'the failed interval.')
rows='\n'.join(
 f"| {o['label']} | {o['managed']['statistics']['Mean']/1000:.2f} | "
 f"{o['worstBracketRatio']:.5f}x | {o['conservativeRatioUpper']:.5f}x | "
 f"{'Pass' if all(g['passed'] for g in o['quietness'].values()) else 'Post-run failed'} |"
 for o in e['observations'])
text=f'''# Warm counterclockwise quarter turn: bounded pixel tiles

The fixed RGBA16 counterclockwise quarter turn improves {e['screenedMeanGainPercent']:.2f}%
by screened mean, from {e['observations'][0]['worstBracketRatio']:.2f}x FFmpeg to {screen['worstBracketRatio']:.5f}x. The short
screen has conservative upper
{screen['conservativeRatioUpper']:.5f}x. Failed quiet guards remain failed.
{confirmation}
Only one original 513x259 RGBA16 raster rotated to 259x513 is measured.
Uniform warm parity and other material cases remain open.

| Implementation/run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{rows}

One counterclockwise-turn primitive replaces per-pixel mapping callbacks and repeated
pixel views with cache-local complete-pixel placement. A single cast exposes
UInt32 RGBA8 or UInt64 RGBA16 pixels, whose channel bytes are copied intact.
Tiles are at most 32x32 pixels; each contiguous output-row slice receives a
forward source column. Partial tiles, single rows and single columns use the same exact
coordinate equation. The kernel has no workspace or extra raster pass.

The complete-overwrite allocator uses the existing single idle raster slot,
retaining at most 4 MiB; implementation and retention policy are unchanged.
Each operation processes every pixel into an independently owned result and
disposes it. Released storage may be reused; outputs are never cached instead
of processed. Both depths preserve swapped geometry, source orientation, full precision,
alpha and zero-alpha hidden RGB. Admission counts both complete buffers and
four logical samples per output pixel. Work/cancellation checks occur before
each tile, bounded to 1,024 pixels. A failed operation disposes its private
result before publication. Direct Rotate(CounterClockwise90) retains orientation;
AutoOrient resets it after the same placement. Other orientation kernels remain
unchanged.

Fourteen new controls include ten independent forward-placement cases covering
both depths, multi-axis partial tiles, long rows, single rows/columns, exact
memory/work, cancellation, source integrity and independent live outputs.
Two controls poison returned storage and reject a partial copy before checking
the next complete result. Two EXIF controls verify direct and automatic
orientation. Focused controls pass on both OSes with intrinsics enabled/disabled:
390 baseline and 404 final. All 2,209 Release tests per OS pass (533 internal,
780 public API, 896 live differential), as do all three examples through the
same actual dependency-free Release package. Timed and fresh production
methods/flags match that package. Only Orient's dispatch changes among existing
bodies; CounterclockwiseTurn.Apply and its generic typed copy method are added
(549 to 551). All other bodies and remaining flags are equivalent with metadata
resolved.

The original native adapter uses the public FFmpeg transpose filter with
counterclockwise placement and passthrough disabled. Every timed operation allocates
and processes independent output pixels, then releases them. Graph, source and
frame wrappers are reused. Padded output rows are exposed directly, with no
extra packing or materialization copy. Fourteen protocol, affinity and input
controls pass. Every UInt16 sample, alpha, hidden RGB and source matches the
independent coordinate reference and pinned normal/scalar CLI output. Two held
outputs prove mutation isolation. The output stride gate explicitly
checks the swapped 259-pixel width before timing.

The tile-local strategy follows the independently authored clockwise kernel.
The library kernel and public-API adapter are independently authored, with no
upstream source, tables or corpus copied into tracked files. This adds no
performance image, geometry, quality or profile sweep.

Runs are sequential Release jobs on the dedicated VM, CPU0, one filter thread,
thread type zero, normal SIMD, tiering off and non-concurrent workstation GC.
Native complete-operation batches use the declared 50 ms calibration target,
with three pre/post samples for screens and twelve for confirmation. Managed
samples retain the 200 ms target. The confirmation has twelve actual managed
iterations and eleven filtered results under the declared BDN outlier policy; all
original measurements remain in the numeric evidence. Startup, IPC, I/O, generation, hashing and
validation are excluded. Prepared generated children avoid compilation inside
scored windows; preparations execute their actual fixed jobs for validation
only, with one native operation before/after and null ratios. Cooldown scheduling
precedes fresh unchanged five-second guards and never certifies an old interval.

The [numeric evidence](warm-counterclockwise-tiles-evidence.json) retains managed
measurements, GC/threading lines, all native samples, guards, identities and
immutable archives. Conservative bounds divide managed confidence upper by
the smaller native confidence lower, using the managed critical value. The
pinned BDN parser reproduces retained official statistics at three/twelve
iterations. Regenerate with `python benchmarks/render-warm-counterclockwise-tiles.py`;
add `--check` to verify. See the
[worker/replay recipe](../tools/WarmReference/README.md).
No further tuning of this numerically passing pair is planned while larger
gaps remain to be screened.
'''
path=folder/'WARM-COUNTERCLOCKWISE-TILES.md'
if '--check' in sys.argv:
 assert path.read_text(encoding='utf-8')==text,'Counterclockwise report differs.'
 print('Counterclockwise report matches numeric evidence.')
else:path.write_text(text,encoding='utf-8',newline='\n')

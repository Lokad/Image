"""Render one original RGBA16 normal-orientation performance round."""
import json,sys
from pathlib import Path
folder=Path(__file__).resolve().parent
e=json.loads((folder/'warm-normal-rows-evidence.json').read_text())
assert e['project']=='Lokad.Image' and not e['uniformWarmParityComplete']
baseline,screen,repeat=e['observations']
quiet=all(g['passed'] for g in repeat['quietness'].values())
assert e['independentQuietConfirmationThisPair']==quiet
assert repeat['conservativeRatioUpper']<2
rows='\n'.join(f"| {o['label']} | {o['managed']['statistics']['Mean']/1000:.2f} | {o['worstBracketRatio']:.5f}x | {o['conservativeRatioUpper']:.5f}x | {'Pass' if all(g['passed'] for g in o['quietness'].values()) else 'Failed'} |" for o in e['observations'])
confirmation=('Both quiet guards pass for the independent twelve-sample repeat, certifying this fixed pair.' if quiet else 'The independent repeat clears the numerical bound but fails a quiet guard; quiet confirmation remains open.')
text=f'''# Warm normal orientation: bounded row copies

The fixed RGBA16 AutoOrient/Normal case improves {e['screenedMeanGainPercent']:.2f}%
by screened mean, from {baseline['worstBracketRatio']:.2f}x FFmpeg to
{screen['worstBracketRatio']:.5f}x. The baseline post-run load guard failed and
remains failed; its ratio ranks the substantial opportunity, without certification.
{confirmation}
One original 513x259 raster is measured; uniform warm parity remains open.

| Implementation/run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{rows}

One normal-orientation primitive replaces per-pixel coordinate callbacks and
pixel views with bounded row copies. Each chunk contains at most 1,024 complete
pixels, retaining every RGBA8/RGBA16 channel byte, alpha and hidden RGB. Both
complete buffers remain admitted, with four work units per pixel and the same
bounded work/cancellation checks. A private partial result is disposed on failure.
AutoOrient returns Normal orientation and leaves the source unchanged.

Complete-overwrite allocation uses the existing single idle raster slot, bounded
to 4 MiB. Each operation copies every pixel into an independently owned output,
then disposes it; released storage may be reused, processed outputs are never
cached. Storage policy, Clone and all other orientation kernels are unchanged.

Twelve new controls cover both depths, long rows and partial chunks, single axes,
exact memory/work limits, cancellation, hidden RGB and independent live owners.
They also poison released storage before partial failure and verify explicit
Normal EXIF. Focused checks pass on Windows and Linux with intrinsics enabled
and disabled: 378 baseline, 390 final. All 2,195 Release tests per OS pass
(533 internal, 766 public API, 896 live differential), along with all three
examples using the same actual dependency-free Release package. Timed and fresh
production methods and flags match the package. Only Orient's dispatch changes
among existing methods, with one NormalOrientation.Apply method added
(548 to 549); metadata-resolved comparison confirms all other bodies and flags.

The original public-API FFmpeg adapter runs a null graph, which emits a shared
view. Every timed operation allocates independent storage with av_frame_get_buffer
and copies the view with av_frame_copy, then releases the result. Graph/source/
frame wrappers are reused. Padded rows need no redundant packing copy. Fourteen
negative protocol/input/affinity controls pass. Every channel byte matches the
original raster and pinned normal/scalar CLI output; two held outputs prove
mutation isolation and the source remains unchanged.

Runs execute sequentially in Release on the dedicated VM, CPU0, one filter thread,
thread type zero, normal SIMD, tiering off and non-concurrent workstation GC.
Native batches use the declared 50 ms calibration target, three pre/post samples
for screens and twelve for confirmation. Managed samples retain the 200 ms target.
Prepared generated children avoid builds during statistical windows. Preparation
executes the actual fixed job for correctness only, with one native operation
before/after and null ratios. Cooldown scheduling precedes unchanged five-second
quiet guards; a later cooldown never certifies a failed interval. Startup, IPC,
I/O, generation, hashing and validation are excluded.

The [numeric evidence](warm-normal-rows-evidence.json) retains managed/GC/threading
results, native samples, guards, source and actual-child identities, plus immutable
archives. Conservative bounds divide managed confidence upper by the smaller
native confidence lower using the managed critical value. Regenerate with
`python benchmarks/render-warm-normal-rows.py`; add `--check` to verify.
See the [worker/replay recipe](../tools/WarmReference/README.md).
'''
p=folder/'WARM-NORMAL-ROWS.md'
if '--check' in sys.argv:
 assert p.read_text(encoding='utf-8')==text,'Normal-orientation report differs.'
 print('Normal-orientation report matches numeric evidence.')
else:p.write_text(text,encoding='utf-8',newline='\n')

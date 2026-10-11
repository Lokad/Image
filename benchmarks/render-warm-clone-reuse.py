"""Render one original RGBA16 Clone performance round."""
import json,sys
from pathlib import Path
folder=Path(__file__).resolve().parent
e=json.loads((folder/'warm-clone-reuse-evidence.json').read_text())
assert e['project']=='Lokad.Image' and not e['uniformWarmParityComplete']
baseline,screen,repeat=e['observations']
quiet=all(g['passed'] for g in repeat['quietness'].values())
assert e['independentQuietConfirmationThisPair']==quiet
assert repeat['conservativeRatioUpper']<2
rows='\n'.join(f"| {o['label']} | {o['managed']['statistics']['Mean']/1000:.2f} | {o['worstBracketRatio']:.5f}x | {o['conservativeRatioUpper']:.5f}x | {'Pass' if all(g['passed'] for g in o['quietness'].values()) else 'Failed'} |" for o in e['observations'])
confirmation=('Both quiet guards pass for the independent twelve-sample repeat, certifying this fixed pair.' if quiet else 'The independent repeat clears the numerical bound but fails a quiet guard; quiet confirmation remains open.')
text=f'''# Warm Clone: complete-overwrite storage

The fixed RGBA16 Clone case improves {e['screenedMeanGainPercent']:.2f}%
by screened mean, from {baseline['worstBracketRatio']:.2f}x FFmpeg to
{screen['worstBracketRatio']:.5f}x. Both baseline quiet guards pass. Failed later guards remain failed.
{confirmation}
One original 513x259 raster is measured; uniform warm parity remains open.

| Implementation/run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{rows}

One Clone primitive replaces fresh full-array allocation and zeroing with the
existing admitted complete-overwrite allocator. Every operation still copies
all source pixels. Its original row loop, exact sample work count and at most
16,384 bytes between work/cancellation checks are unchanged. Both complete
buffers remain admitted; complete channel bytes, alpha, hidden RGB, geometry,
source orientation and independent live ownership are preserved. A private
partial result is disposed on failure. The existing single idle raster slot
retains at most 4 MiB; storage policy and zero-initialized Create remain unchanged.
Released storage may be reused; processed outputs are never cached.

Fourteen new controls cover both depths, long rows, exact memory/work limits,
cancellation, hidden RGB and independent live owners. They poison returned
storage before partial failure, verify EXIF retention and check identity Resize
and ResizeToFit results, which call Clone. Focused checks pass on both OSes with
intrinsics enabled and disabled: 507 baseline, 521 final. All 2,251 Release tests
per OS pass (533 internal, 822 public API, 896 live differential), along with all
three examples through the same actual dependency-free Release package. Timed
and fresh production methods/flags match that package. Only existing Clone's
body changes, with no added or removed methods (555 retained). All other bodies
and flags are equivalent with metadata references resolved.

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
for screens and twelve for confirmation. Managed samples retain the 200 ms target. The repeat has twelve actual managed
iterations and ten filtered results under the declared BDN outlier policy;
all original measurements remain in the evidence.
Prepared generated children avoid builds during statistical windows. Preparation
executes the actual fixed job for correctness only, with one native operation
before/after and null ratios. Cooldown scheduling precedes unchanged five-second
quiet guards; a later cooldown never certifies a failed interval. Startup, IPC,
I/O, generation, hashing and validation are excluded.

The [numeric evidence](warm-clone-reuse-evidence.json) retains managed/GC/threading
results, native samples, guards, source and actual-child identities, plus immutable
archives. Conservative bounds divide managed confidence upper by the smaller
native confidence lower using the managed critical value. Regenerate with
`python benchmarks/render-warm-clone-reuse.py`; add `--check` to verify.
See the [worker/replay recipe](../tools/WarmReference/README.md).
'''
p=folder/'WARM-CLONE-REUSE.md'
if '--check' in sys.argv:
 assert p.read_text(encoding='utf-8')==text,'Clone report differs.'
 print('Clone report matches numeric evidence.')
else:p.write_text(text,encoding='utf-8',newline='\n')

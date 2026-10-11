"""Render one original RGBA16 half-turn performance round."""
import json,sys
from pathlib import Path
folder=Path(__file__).resolve().parent
e=json.loads((folder/'warm-halfturn-pixels-evidence.json').read_text())
assert e['project']=='Lokad.Image' and not e['uniformWarmParityComplete']
screen=e['observations'][1];repeat=e['observations'][-1]
assert all(g['passed'] for g in screen['quietness'].values())
assert all(o['conservativeRatioUpper']<2 for o in e['observations'][1:])
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
text=f'''# Warm half turn: complete-pixel reversal

The fixed RGBA16 half turn improves {e['screenedMeanGainPercent']:.2f}% by screened mean,
from 53.87x FFmpeg to {screen['worstBracketRatio']:.5f}x. The short screen passes both
unchanged quiet guards, with conservative upper {screen['conservativeRatioUpper']:.5f}x.
{confirmation}
Only one original 513x259 RGBA16 raster is measured. Uniform warm parity and
other material cases remain open.

| Implementation/run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{rows}

One half-turn primitive replaces generic per-pixel coordinate callbacks and
pixel views with complete-pixel reversal of the tightly packed raster. Safe
span-based SIMD shuffles reverse UInt32 RGBA8 or UInt64 RGBA16 pixels, retaining
each channel's exact bytes. Scalar fallback and final partial vectors preserve
the same placement. The complete-overwrite allocator uses the existing single
idle raster slot, retaining at most 4 MiB; its implementation and retention
policy are unchanged. Each operation processes every pixel into an independently
owned result and disposes it. Released storage may be reused; pixel outputs
are never cached instead of processed.

Both depths retain geometry, source orientation, full precision, alpha and
zero-alpha hidden RGB. Admission still counts both complete buffers and four
logical samples per output pixel. Work/cancellation checks are bounded to 1,024
pixels across the packed raster, including its final partial chunk. A failed
operation disposes its private result before publication. Direct Rotate(Half)
retains orientation metadata; AutoOrient resets it after the same placement.
Quarter turns, transposes and other orientation kernels are unchanged.

Twelve new controls include eight independent forward-placement cases covering
both depths, long partial chunks, single rows/columns, exact memory/work,
cancellation, source integrity and simultaneously live independent outputs.
Two controls poison returned storage and reject a partial copy before checking
the next complete result. Two EXIF controls verify direct and automatic
orientation. Focused controls pass on both OSes with intrinsics enabled/disabled:
199 baseline and 211 final. All 2,169 Release tests per OS pass (533 internal,
740 public API, 896 live differential), as do all three examples through the
same actual dependency-free Release package. Saved timed and fresh production
methods/flags match that package. Only Orient's dispatch changes among existing
bodies; HalfTurn.Apply and its two typed reversal methods are added (543 to 546).
All other bodies and remaining flags are equivalent with metadata resolved.

The original native adapter uses public FFmpeg hflip/vflip filters. Hflip
allocates independent output pixels; vflip's allocation callback lets hflip
write directly into reversed rows, then adjusts the final view. The observed
owned output has positive padded stride. This strategy processes pixels in
one pass, so no redundant full copy is added to force positive stride. The
adapter accepts either stride sign, verifies every output pixel and source,
holds two live outputs to prove mutation isolation, then releases each output
inside the timed operation. Graph, source and frame wrappers are reused.
The pinned FFmpeg vflip source was inspected for this allocation strategy;
the library kernel and development adapter are independently authored.

Fourteen native protocol, affinity and input controls pass. Every UInt16
sample, alpha and hidden RGB matches the original integer placement reference
and pinned normal/scalar CLI output. The initial harness assumption that the
owned output must have negative stride was rejected before timing; the corrected
gate checks independent pixels and accepts either sign. This adds no performance
image, geometry, quality or profile sweep.

Runs are sequential Release jobs on the dedicated VM, CPU0, one filter thread,
thread type zero, normal SIMD, tiering off and non-concurrent workstation GC.
Native complete-operation batches use the declared 50 ms calibration target,
with three pre/post samples for screens and twelve for confirmation. Managed
samples retain the 200 ms target. Startup, IPC, I/O, generation, hashing and
validation are excluded. Prepared generated children avoid compilation inside
scored windows. Preparations execute their actual fixed jobs for validation
only, with one native operation before/after and null ratios. The baseline's
postguard failure remains preserved.

The [numeric evidence](warm-halfturn-pixels-evidence.json) retains managed
measurements, GC/threading lines, all native samples, guards, identities and
immutable archives. Conservative bounds divide managed confidence upper by
the smaller native confidence lower, using the managed critical value. The
pinned BDN parser reproduces retained official statistics at three/twelve
iterations. Regenerate with `python benchmarks/render-warm-halfturn-pixels.py`;
add `--check` to verify. See the
[worker/replay recipe](../tools/WarmReference/README.md).
No further tuning of this numerically passing raster is planned while larger
gaps remain to be screened.
'''
path=folder/'WARM-HALFTURN-PIXELS.md'
if '--check' in sys.argv:
 assert path.read_text(encoding='utf-8')==text,'Half-turn report differs.'
 print('Half-turn report matches numeric evidence.')
else:path.write_text(text,encoding='utf-8',newline='\n')

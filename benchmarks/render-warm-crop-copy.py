"""Render the fixed original RGBA16 owned-crop observations."""
import json, sys
from pathlib import Path

folder = Path(__file__).resolve().parent
e = json.loads((folder / 'warm-crop-copy-evidence.json').read_text())
assert e['project'] == 'Lokad.Image' and not e['uniformWarmParityComplete']
assert e['independentQuietConfirmationThisPair']
confirmed = e['observations'][-1]
assert all(g['passed'] for g in confirmed['quietness'].values())
assert confirmed['conservativeRatioUpper'] < 2
rows = '\n'.join(
    f"| {o['label']} | {o['managed']['statistics']['Mean']/1000:.2f} | "
    f"{o['worstBracketRatio']:.5f}x | {o['conservativeRatioUpper']:.5f}x | "
    f"{'Pass' if all(g['passed'] for g in o['quietness'].values()) else 'Post-run failed'} |"
    for o in e['observations'])
text = f'''# Warm owned crop: contiguous rows and reusable storage

The fixed RGBA16 crop improves {e['screenedMeanGainPercent']:.2f}% by screened mean,
from about 70x FFmpeg to {confirmed['worstBracketRatio']:.5f}x, with conservative upper
{confirmed['conservativeRatioUpper']:.5f}x. The independent twelve-sample repeat passes both
unchanged quiet guards. This certifies only the existing 513x259 raster cropped
at x=127,y=65 to 256x129; uniform warm parity and other material cases remain open.

| Implementation/run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{rows}

Two primitives are changed and screened separately. First, contiguous row copies
replace generic per-pixel mapping callbacks, coordinate evaluation and repeated
pixel views, reducing the mean 69.6% while retaining ordinary allocation.
Its remaining 21.6x gap justifies a substantial allocation change. Second,
the same complete-overwrite row kernel uses the existing reusable pixel storage,
avoiding a fresh 264,192-byte output array on every warm operation. Released
storage may be reused; each operation still copies every output sample into an
independently owned result and disposes that result. Pixels are never cached as
a substitute for processing. Pool implementation and retention limits are unchanged.

The kernel serves both depths and preserves full channel bytes, alpha, hidden
RGB and source orientation. Admission still counts both complete buffers, with
four logical samples per output pixel. Work and cancellation checks remain
bounded to 1,024 pixels, including partial final chunks. A failed copy disposes
its private result before publishing it, including returning reusable storage.
Centered and clamped crops retain their existing validation and coordinates.

Six independent placement controls cover both depths, long partial chunks,
identity-sized crops and the final source pixel. They check exact memory/work
limits, cancellation, source integrity and two simultaneously live outputs with
mutation isolation. Two additional controls poison released storage, reject a
partially completed copy, then verify every sample of the next complete result.
Focused controls pass on both OSes with intrinsics enabled/disabled: 37 baseline,
43 row-copy and 45 final tests. All 2,145 Release tests per OS pass (533 internal,
716 public API, 896 live differential), as do all three examples through the
same actual dependency-free Release package. Saved timed and fresh production
methods/flags match that package. The only existing production body change is
Crop's dispatch; its two generated closure methods become one row-copy method.
The second primitive changes only that method's allocation call. Other existing
bodies and flags remain equivalent with metadata references resolved.

FFmpeg's crop filter returns a shared view. The original development adapter
therefore allocates an independent frame and copies all cropped pixels with
public frame APIs inside every timed operation, then releases the view/output.
It never compares a shared native view with an owned managed raster. Graph,
source and frame wrappers are reused; padded output rows need no packing copy.
Fourteen protocol/affinity/input controls pass. Full UInt16 pixels, source,
simultaneous owners and pinned CLI output match exactly on the existing original
raster. No image, rectangle, quality or unrelated transform sweep is added.

Runs are sequential Release jobs on the dedicated VM, CPU0, one filter thread,
thread type zero, normal SIMD, tiering off and non-concurrent workstation GC.
Native complete-operation batches have the declared 50 ms calibration target,
with three pre/post samples for screens and twelve for confirmation. Managed
samples retain the 200 ms target. Startup, IPC, I/O, generation, hashing and
validation are excluded. The baseline/ordinary-allocation postguard failures
remain preserved; both reusable-storage runs pass their pre/post guards.
Prepared generated children avoid compilation inside the scored windows.
The confirmation preparation is explicitly validation-only, with a null ratio.

The [numeric evidence](warm-crop-copy-evidence.json) retains raw managed results,
GC/threading lines, all native samples, unchanged guards, identities and immutable
archives. The pinned BDN parser reproduces official report statistics at three
and twelve iterations. Conservative bounds divide managed confidence upper by
the smaller native confidence lower, using the managed critical value.
Regenerate with `python benchmarks/render-warm-crop-copy.py`; add `--check` to verify.
See the [worker/replay recipe](../tools/WarmReference/README.md).
No further tuning of this passing rectangle is planned.
'''
path = folder / 'WARM-CROP-COPY.md'
if '--check' in sys.argv:
    assert path.read_text(encoding='utf-8') == text, 'Crop report differs.'
    print('Crop report matches numeric evidence.')
else:
    path.write_text(text, encoding='utf-8', newline='\n')

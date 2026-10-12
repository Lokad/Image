"""Render the single Paeth RGBA8 baseline and short CPU diagnosis."""
import json,sys
from pathlib import Path
folder=Path(__file__).resolve().parent;e=json.loads((folder/'warm-pngpaeth8-baseline-evidence.json').read_text());o=e['observations'][0]
assert not e['productionChanged'] and not e['independentQuietConfirmationThisPair'] and all(v['passed'] for v in o['quietness'].values())
text=f'''# Warm Paeth RGBA8 baseline

One quiet screen exposes {o['worstBracketRatio']:.3f}x FFmpeg, conservative upper
{o['conservativeRatioUpper']:.3f}x. Managed mean is
{o['managed']['statistics']['Mean']/1000:.2f} microseconds. Both unchanged quiet
guards pass. This fixed pair remains above2x and has no independent confirmation.
Production is unchanged.

One original authoring uses the existing513x259 RGBA8 raster, noninterlaced PNG,
Paeth on every row, BCL Optimal level6 and one IDAT. The345,325-byte input is
identical on both sides. Independent reconstruction verifies every filtered byte
and all531,468 eight-bit samples, including alpha and hidden RGB. Normal/scalar
FFmpeg CLI outputs, source preservation and independent held outputs are exact.
Eight negative native protocol/input/profile/affinity controls pass. Windows
normal/scalar pixels match the retained actual package, with unchanged observed
public Stream allocations2,275,800 bytes/op. Fresh and actual timing libraries
retain all564 production methods and flags from that package. DecodePaeth8 is
explicit; the default remains one Decode8 case. No raster, geometry, filter,
quality or effort sweep is added.

Public resident-input decode includes validation, allocation and disposal.
Native wrappers/context are reused; complete decode and independent output
acquisition/release are timed. RGBA8 output needs no endian conversion or extra
conversion frame. CPU0, one codec/conversion thread, thread type zero, normal SIMD, tiering
off and nonconcurrent workstation GC are fixed. Startup, I/O, IPC, preparation,
hashing and validation checks are excluded. The screen retains three actual
managed iterations and three native samples per bracket. The conservative upper
uses the larger filtered/all-actual confidence bound with the corresponding
managed critical value for native bracket lower bounds.

One six-second CPU profile contains{e['diagnosis']['samples']:,} samples. Paeth
prediction and the generic unfilter loop account for
{e['diagnosis']['paethAndUnfilterFraction']:.1%} of sampled CPU. The next substantial
primitive groups four independent channel bytes in safe128-bit lanes, preserving
Paeth tie order, byte overflow, scalar fallback, tails, cancellation, alias/truncation
behavior and existing work/memory admission. No upstream code or tables are copied.
Diagnostic sample shares do not certify a speedup.

The [numeric evidence](warm-pngpaeth8-baseline-evidence.json) retains every actual
measurement, native bracket, guard, input/source/actual-child/package identity,
Windows controls and immutable timing/analysis/profile archives. Regenerate with
`python benchmarks/render-warm-pngpaeth8-baseline.py`; `--check` verifies this report.
See the [reference recipe](../tools/WarmReference/README.md). Broader required,
unmatched and unconfirmed warm coverage remains open.
'''
p=folder/'WARM-PNG-PAETH8-BASELINE.md'
if '--check' in sys.argv:
 assert p.read_text(encoding='utf-8')==text,'Paeth baseline report differs.'
 print('Paeth baseline report matches numeric evidence.')
else:p.write_text(text,encoding='utf-8',newline='\n')

"""Render accepted per-block mask traversal and its independent confirmation."""
import json,sys
from pathlib import Path
folder=Path(__file__).resolve().parent;e=json.loads((folder/'warm-jpegmask-evidence.json').read_text());a,b,c=e['observations']
assert e['productionChanged'] and e['thisFixedPairClearsConservativeTwo'] and not e['uniformWarmParityComplete']
rows='\n'.join(f"| {o['label']} | {o['managed']['statistics']['Mean']/1000:.2f} | {o['worstBracketRatio']:.5f}x | {o['conservativeRatioUpper']:.5f}x | {'Pass' if all(g['passed'] for g in o['quietness'].values()) else 'FAIL'} |" for o in (a,b,c))
text=f'''# Progressive JPEG: reuse a per-block nonzero mask

Progressive full-resolution decode moves from {a['worstBracketRatio']:.2f}x to
{c['worstBracketRatio']:.3f}x FFmpeg. Independent confirmation has conservative
upper {c['conservativeRatioUpper']:.3f}x including all 12 actual samples, and both
unchanged quiet guards pass. Unfiltered managed time falls
{e['confirmedUnfilteredManagedReduction']*100:.2f}%. This fixed point clears 2x;
broader required, unmatched and unconfirmed warm cases remain open.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{rows}

An isolated diagnostic found 176,124 insertion-group attempts, including 54,930
empty prefixes and 109,632 successful groups with no corrections. The accepted
primitive discovers one 64-bit nonzero-position mask per refinement block and
reuses it across symbols. Corrections preserve nonzero status; insertion alone
updates the mask. BMI2 selects a zero-run boundary without revisiting coefficient
values, and correction fields are admitted in groups of at most 16 logical bits.
Only set correction bits require signed-magnitude updates. This removes repeated
discovery and empty-group traversal without new scratch, tables or allocation.

Entropy order, zero-run/EOB/restart/band semantics, signs and already-refined bits
remain exact. Complete valid input and original work must be admitted together;
otherwise scalar per-bit reads retain partial consumption, coefficient updates,
cancellation and marker/limit/format precedence. An overlong run refines its
existing coefficients before reporting the same format error. Unsupported
hardware retains the preceding traversal. Transform/rounding, quality, memory
admission and independent output ownership remain unchanged.

Only existing ReadBlock changes (738 to 770 IL bytes); two original helpers are
added. Other existing production bodies and implementation flags are equivalent
after resolving metadata references. Both actual generated timing libraries match
the final actual package's complete production methods and flags. Documentation
is updated after freezing timing inputs; the final package retains the identical
production DLL from the tested package.

Both OSes pass six new primitive controls in normal/scalar modes, including
131,072 scalar-reference masks and partial-update/marker/work/cancellable cases,
plus 96 focused public JPEG checks and ten frozen public work/pixel boundaries.
All 2,353 Release tests pass on each OS: 609 internal, 848 public and 896 live
FFmpeg comparisons. The same final actual dependency-free Release package and
all three usage examples pass on both OSes. Normal/scalar Windows public decode
allocation controls retain exactly 3,338,920 bytes per operation with tiering off.
Pixels, source and independent held outputs retain their exact hashes.

The same resident 132,961-byte ten-scan full-resolution JPEG, original 513x259
raster and common sampling factors remain. All 411,840 valid coefficients,
geometry and quantization retain the lossless preparation proof. Normal/scalar
CLI and adapter checks keep the original fixed decoder/alpha bounds. There is no
image, shape, quality or scan-strategy search. Full public decode/admission/
allocation/disposal compares with software native decode/reset/full-range
conversion/independent output acquisition and release, with reused context and
packet/frame wrappers. CPU0, codec/conversion threads one, thread type zero,
normal SIMD, tiering off and nonconcurrent workstation GC remain fixed. Startup,
I/O, IPC, preparation, hashing and validation remain excluded.

The short screen's post-run load guard fails and stays failed; its apparent
reduction motivates confirmation and certifies nothing. The first confirmation
attempt aborts before launching any benchmark because pre-run I/O wait is 1.25%,
above the unchanged 1% limit. That failure is archived. A fresh precheck on the
same prepared child permits the single independent 12-actual confirmation, with
12 native samples per bracket. BenchmarkDotNet selects ten results; all twelve
actual measurements remain included in a second Statistics view without overhead
subtraction. Displayed bounds use the larger of the two conservative ratios.
The passing independent interval establishes certification, without changing
earlier failures or relaxing any guard. All raw/source/actual-child inputs are
frozen before subsequent edits.

The [numeric evidence](warm-jpegmask-evidence.json) retains measurements, both
statistical views, failed/passing guards, diagnostic counts, source/package/
actual-child identities and immutable archives. No upstream code or reference
corpus is tracked. Regenerate with `python benchmarks/render-warm-jpegmask.py`;
use `--check` to verify. The preceding
[direct-boundary rejection](WARM-JPEG-INSERTION-REJECTION.md) remains unchanged.
See the [reference recipe](../tools/WarmReference/README.md). Stop tuning this
passing fixed point; cover one material unmeasured case and rerank.
'''
p=folder/'WARM-JPEG-MASK.md'
if '--check' in sys.argv:
 assert p.read_text(encoding='utf-8')==text,'Mask traversal report differs.'
 print('Mask traversal report matches numeric evidence.')
else:p.write_text(text,encoding='utf-8',newline='\n')

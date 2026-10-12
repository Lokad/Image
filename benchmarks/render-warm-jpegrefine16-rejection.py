"""Render progressive 4:4:4 coverage and the rejected wider group."""
import json,sys
from pathlib import Path
folder=Path(__file__).resolve().parent;e=json.loads((folder/'warm-jpegrefine16-rejection-evidence.json').read_text());a,b=e['observations']
assert not e['productionChanged'] and e['rejectedCandidate'] and not e['uniformWarmParityComplete']
rows='\n'.join(f"| {o['label']} | {o['managed']['statistics']['Mean']/1000:.2f} | {o['worstBracketRatio']:.5f}x | {o['conservativeRatioUpper']:.5f}x | {'Pass' if all(g['passed'] for g in o['quietness'].values()) else 'FAIL'} |" for o in (a,b))
text=f'''# Progressive JPEG 4:4:4: wider correction group rejected

The newly covered progressive full-resolution input screens at
{a['worstBracketRatio']:.2f}x FFmpeg and is the largest measured remaining gap.
Both baseline quiet guards pass. Widening correction groups to 16 AVX-512 lanes
increases screened managed time {e['candidateManagedIncrease']*100:.2f}% and
remains above 2x. The trial is discarded without a longer confirmation.
Production is restored exactly; progressive 4:4:4 coverage remains. Uniform
warm parity and this pair's independent certification remain open.

| Implementation/run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{rows}

One default lossless jpegtran rewrite consumes the existing native qscale5/
RGB0x204060 full-resolution JPEG on the original 513x259 raster. It retains
the common 1x2 sampling factors for all three components, 10 scans and
132,961 bytes. Canonical 824,144-byte dumps prove all 411,840 valid signed
coefficients, quantization and component geometry exact. Dummy blocks outside
valid component dimensions are not claimed as image coefficients. Owned decoded
pixels retain the exact baseline hash. The preceding grayscale and 4:2:0 dump
hashes remain unchanged after extending the original public-API visitor.
The explicit Progressive444 selection adds no source raster, shape, quality or
scan-strategy search; the default remains one Decode8 case.

Normal/scalar CLI agreement is maximum 3/RMSE 0.227168 within unchanged maximum 4/
RMSE 1.5, exactly opaque alpha. The native adapter matches normal CLI pixels
exactly. Source preservation, independent held outputs and 15 negative protocol/
input/sampling/affinity controls pass. Baseline and trial owned pixel hashes and
Windows normal/scalar allocations remain exact. Reference checkouts, fixture
producer and binaries stay ignored under external/ and never become build inputs.
No upstream source, tables, documentation, binary or test corpus is tracked.

A separate restored-input CPU diagnosis places 29.72% of self samples in the
eight-lane correction-group helper, 17.81% in ReadBlock, 14.74% in short Huffman
lookup and 9.26% in ReadBits. Its 19,052 samples and raw maps/recording are retained;
these shares guide work selection and certify no timing ratio. The trial attempts
to reduce repeated group admission/mask/traversal work with 16 lanes, preserving
the 8-lane/scalar fallbacks, signed updates, zero-run boundaries, work and refusal
behavior. Only TryRefineRun changes (450 to 812 IL bytes), with all 560 methods,
other existing bodies and implementation flags equivalent. Focused 33 primitive
and 96 public JPEG checks pass on both OSes across normal, AVX-512-disabled and
scalar modes. The candidate passes 2,349 Windows Release tests and its actual
package's three examples. A full Linux candidate suite and confirmation are not
needed after the slower screen and are not claimed. After exact rollback, both
OSes pass 2,347 Release tests; all production methods/flags and fixture pixels
match the retained accepted actual package.

Both subjects consume identical resident input. Full public decode, allocation
and disposal compare with software MJPEG decode/reset, complete full-range
conversion and independent native output acquisition/release. Context and
packet/frame wrappers are reused. CPU0, one codec/conversion thread, thread type
zero, normal SIMD, tiering off and nonconcurrent workstation GC remain fixed.
Startup, I/O, IPC, preparation and validation remain excluded. Each trial gets
one three-actual screen, native 50ms brackets and managed 200ms iterations. Both
unchanged five-second guards pass; all raw/source/actual-child inputs are frozen
before later edits. Bounds include both BDN result statistics and all actual
measurements without overhead subtraction.

The next action diagnoses the actual correction prefixes and EOB-versus-zero-run
work on an isolated original diagnostic copy, then chooses one substantially
different traversal primitive. No additional width tweaks, quality/image sweep
or micro-tuning of passing points is planned.

The [numeric evidence](warm-jpegrefine16-rejection-evidence.json) retains all
managed/native measurements, uncertainty, guards, provenance, actual child/
package/source identities and immutable archives. Regenerate with
`python benchmarks/render-warm-jpegrefine16-rejection.py`; use `--check` to verify.
See the [reference recipe](../tools/WarmReference/README.md) and preceding
[accepted correction primitive](WARM-JPEG-REFINEMENT.md).
'''
p=folder/'WARM-JPEG-REFINE16-REJECTION.md'
if '--check' in sys.argv:
 assert p.read_text(encoding='utf-8')==text,'Wider group rejection report differs.'
 print('Wider group rejection report matches numeric evidence.')
else:p.write_text(text,encoding='utf-8',newline='\n')

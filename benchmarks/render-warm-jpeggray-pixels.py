"""Render fixed grayscale JPEG decode measurements and correctness evidence."""
import json,sys
from pathlib import Path
folder=Path(__file__).resolve().parent;e=json.loads((folder/'warm-jpeggray-pixels-evidence.json').read_text());assert e['productionChanged'] and not e['uniformWarmParityComplete'];a,b,c=e['observations']
rows='\n'.join(f"| {o['label']} | {o['managed']['statistics']['Mean']/1000:.2f} | {o['worstBracketRatio']:.5f}x | {o['conservativeRatioUpper']:.5f}x | {'Pass' if all(g['passed'] for g in o['quietness'].values()) else 'FAIL'} |" for o in (a,b,c))
status=('Both independent quiet guards pass, certifying this fixed pair only.' if e['independentQuietConfirmationThisPair'] else 'Independent certification remains open; conservative uncertainty and every failed quiet guard remain explicit.')
text=f'''# Warm grayscale JPEG decode: direct byte expansion

The uncovered grayscale baseline screens at {a['worstBracketRatio']:.2f}x FFmpeg.
One direct row-expansion primitive reduces complete decode by
{e['screenedManagedReduction']*100:.2f}% in the short screen, preserving every owned
output byte. The independent twelve-actual repeat is {c['worstBracketRatio']:.5f}x,
conservative upper {c['conservativeRatioUpper']:.5f}x. {status}
Uniform warm parity and other material cases remain open.

| Implementation/run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{rows}

One original RGBA8 513x259 raster supplies a 147,825-byte baseline, one-component
JPEG. Native MJPEG encoding cannot produce an equivalent one-component input;
preparation uses the owned grayscale encoder at fixed quality 85 with alpha
flattened onto RGB 0x204060. Both subjects decode identical resident bytes, so
encode quality matching is unnecessary. FFmpeg remains the pixel oracle. The
explicit OwnedDecodeGray case selects one pair; the default remains one 4:4:4
input. There are two retained original performance rasters, with no new image,
shape, quality enumeration or profile matrix.

A single component has full sampling factors, and its inverse-transform plane
already contains rounded/clipped byte samples. Direct RGB repetition is therefore
exact to the former identity interpolation and three floating-point round calls.
Safe AVX2 widens eight samples, packs RGB by multiplication with 0x010101 and
sets opaque alpha. Direct scalar expansion handles tails and unsupported CPUs.
The new path adds no workspace, pass, method, cache or dependency. Admission,
exact row work charge, cancellation, orientation, ownership and failure disposal
remain unchanged. Only Render changes (1,750 to 2,031 IL bytes); all 558 methods
and flags remain, with every other body equivalent after metadata resolution.
No upstream implementation, table or corpus is copied.

Sixteen new public controls use independently authored unit-quantization DC-only
grids. Their mathematical samples cover every luma byte, baseline/progressive
streams, tiny rows, vector boundaries, tails and independent live owners.
The fixed large input retains its entire owned output hash before/after the
change and on Windows with intrinsics enabled/disabled. All 86 focused public
JPEG tests pass on both OSes in both intrinsic modes. Normal/scalar CLI agreement
retains RGB maximum 1 and RMSE 0.119141 under the declared maximum 4/RMSE 1.5
bounds, with exactly opaque alpha. All 2,306 Release tests per OS pass (572
internal, 838 public API and 896 live differential), along with all three examples
through the same actual dependency-free Release package. Actual timed/fresh
production methods and flags match that package.

The original public-API FFmpeg worker resets the software mjpeg decoder each
operation, then expands owned decoded GRAY8 samples into independent RGBA8.
Full-range conversion and output acquisition/release are timed. Codec/conversion
contexts, resident packet and frame wrappers are reused; padded rows need no
redundant packing copy. The decoder leaves GRAY8 color range untagged; only this
grayscale representation may have an unspecified tag, and the conversion still
explicitly declares full JPEG range. The initial overly strict range validation
failed before any statistical run and remains preserved in the raw evidence.
No pixel tolerance was relaxed. Native output matches the pinned normal CLI
exactly; source and two held output owners remain independent and unchanged.
All four sampling modes pass fifteen negative protocol/input/affinity controls.
The existing 4:4:4 setup also passes a validation-only smoke.

Sequential Release runs use CPU0, one codec/conversion thread, thread type zero,
normal SIMD, tiering off and non-concurrent workstation GC. Native pre/post
batches target 50 ms; managed actual iterations target 200 ms. Prepared generated
children validate outside scored windows with one native operation per bracket
and null ratios. The repeat retains twelve actual and
{c['managed']['statistics']['N']} filtered managed results. Cooldown scheduling
precedes unchanged five-second quiet guards and cannot certify a failed interval.
Startup, IPC, I/O, generation, hashing and validation are excluded.

The [numeric evidence](warm-jpeggray-pixels-evidence.json) retains all managed,
GC/threading and native measurements, guards, provenance, package/source/actual-
child identities and immutable archives. Conservative bounds divide the managed
confidence upper by the smaller native confidence lower using the same critical
value. Regenerate with `python benchmarks/render-warm-jpeggray-pixels.py`; add
`--check` to verify. See the [reference recipe](../tools/WarmReference/README.md).
'''
p=folder/'WARM-JPEGGRAY-PIXELS.md'
if '--check' in sys.argv:
 assert p.read_text(encoding='utf-8')==text,'Grayscale JPEG report differs.'
 print('Grayscale JPEG report matches numeric evidence.')
else:p.write_text(text,encoding='utf-8',newline='\n')

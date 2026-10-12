"""Render fixed JPEG 4:2:2 decode evidence from the retained measurements."""
import json,sys
from pathlib import Path
folder=Path(__file__).resolve().parent;e=json.loads((folder/'warm-jpeg422-rows-evidence.json').read_text());assert e['productionChanged'] and not e['uniformWarmParityComplete'];a,b,c=e['observations']
rows='\n'.join(f"| {o['label']} | {o['managed']['statistics']['Mean']/1000:.2f} | {o['worstBracketRatio']:.5f}x | {o['conservativeRatioUpper']:.5f}x | {'Pass' if all(g['passed'] for g in o['quietness'].values()) else 'FAIL'} |" for o in (a,b,c))
status=('Both independent quiet guards pass, certifying this fixed pair only.' if e['independentQuietConfirmationThisPair'] else 'The numerical target clears; independent quiet confirmation remains open because the post-run load guard fails.')
text=f'''# Warm JPEG 4:2:2 decode: reuse the eight-pixel renderer

The uncovered baseline 4:2:2 case screens at {a['worstBracketRatio']:.2f}x FFmpeg.
Its failed post-run quiet guard remains failed; that screen is ranking evidence.
One render primitive reduces complete decode by {e['screenedManagedReduction']*100:.2f}%
in the short screen, preserving every owned output byte. The independent
twelve-actual repeat is {c['worstBracketRatio']:.5f}x FFmpeg, conservative upper
{c['conservativeRatioUpper']:.5f}x. {status}
Uniform warm parity and other material cases remain open.

| Implementation/run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{rows}

One existing original RGBA8 513x259 raster supplies the fixed baseline JPEG.
Preparation uses native qscale 5, optimal Huffman coding, centered area chroma
reduction and alpha flattening onto RGB 0x204060. The JPEG contains 71,168 bytes.
Both decoders consume identical resident bytes, so encode quality matching is
unnecessary. NativeDecode422 explicitly selects one pair; the default remains
one 4:4:4 input. There are two retained original performance rasters and no new
image, shape, quality enumeration or profile matrix.

The former scalar path repeats interpolation geometry and color conversion.
Full vertical chroma resolution now supplies the same row to both inputs of the
existing eight-pixel renderer. Its exact quarter-weight sum reproduces that row;
horizontal interpolation and separate binary64 YCbCr arithmetic, rounding,
clipping and opaque alpha retain the original bytes. Safe AVX2/AVX512 helpers
are unchanged. First pixels, edges, tails, other sampling and unsupported CPUs
retain the general path. Existing stack scratch, memory admission, exact work
charge, cancellation, orientation, independent ownership and errors remain.
There is no new workspace, pass, cache, method or dependency. Only Render changes
(1,697 to 1,750 IL bytes); all 558 methods and implementation flags remain, with
every other body equivalent after resolving metadata references. No upstream
implementation, table or corpus is copied.

Eight new controls exercise 4,096 reused-row outputs against an independent
horizontal binary64 coordinate recurrence, both quarter-weight choices,
fractional/clipped colors, unaligned spans, unchanged inputs and output/scratch
guards. All 36 focused color checks pass on both OSes with intrinsics enabled
and disabled. The fixed owned output hash is exact to the baseline on both OSes.
Normal/scalar CLI agreement remains RGB maximum 3 and RMSE 0.599884 under the
declared maximum 4/RMSE 1.5 bounds, with exactly opaque alpha. All 2,290 Release
tests per OS pass (572 internal, 822 public API and 896 live differential), along
with all three examples through the same actual dependency-free Release package.
Actual timed and fresh production methods/flags match that package.

The original public-API FFmpeg worker resets the software mjpeg decoder each
operation and expands both ceil-width chroma planes horizontally by two. A crop
view of the original bounds preserves the chroma grid at odd width; vertical
coordinates stay unchanged. Full-range BT.601 conversion, intermediate/independent
RGBA acquisition and release are timed. Codec/conversion contexts, resident
packet and frame wrappers are reused. Padded output rows need no redundant
packing copy. Native pixels match the pinned normal CLI exactly; source and two
held output owners remain independent and unchanged. All three sampling modes
pass fifteen negative protocol/input/affinity controls. Previously supported
4:4:4 and 4:2:0 setups pass validation-only smoke runs.

Sequential Release runs use CPU0, one codec/conversion thread, thread type zero,
normal SIMD, tiering off and non-concurrent workstation GC. Native pre/post
batches target 50 ms; managed actual iterations target 200 ms. Prepared generated
children validate outside scored windows with one native operation per bracket
and null ratios. The repeat retains twelve actual and
{c['managed']['statistics']['N']} filtered managed results. Cooldown scheduling
precedes unchanged five-second quiet guards and cannot certify a failed interval.
Startup, IPC, I/O, generation, hashing and validation are excluded.

The [numeric evidence](warm-jpeg422-rows-evidence.json) retains all managed,
GC/threading and native measurements, guards, provenance, package/source/actual-
child identities and immutable archives. Conservative bounds divide the managed
confidence upper by the smaller native confidence lower using the same critical
value. Regenerate with `python benchmarks/render-warm-jpeg422-rows.py`; add
`--check` to verify. See the [reference recipe](../tools/WarmReference/README.md).
'''
p=folder/'WARM-JPEG422-ROWS.md'
if '--check' in sys.argv:
 assert p.read_text(encoding='utf-8')==text,'JPEG422 row report differs.'
 print('JPEG422 row report matches numeric evidence.')
else:p.write_text(text,encoding='utf-8',newline='\n')

"""Render the progressive scan-layout and grouped-correction evidence."""
import json,sys
from pathlib import Path
folder=Path(__file__).resolve().parent;e=json.loads((folder/'warm-jpegrefinement-evidence.json').read_text())
assert e['productionChanged'] and e['thisFixedPairClearsConservativeTwo'] and not e['uniformWarmParityComplete']
a,layout,screen,confirm=e['observations']
rows='\n'.join(f"| {o['label']} | {o['managed']['statistics']['Mean']/1000:.2f} | {o['worstBracketRatio']:.5f}x | {o['conservativeRatioUpper']:.5f}x | {'Pass' if all(g['passed'] for g in o['quietness'].values()) else 'FAIL'} |" for o in (a,layout,screen,confirm))
text=f'''# Warm progressive JPEG: grouped refinement

Progressive grayscale decode moves from {a['worstBracketRatio']:.2f}x to
{confirm['worstBracketRatio']:.3f}x FFmpeg. The independent 12-iteration
confirmation has conservative upper {confirm['conservativeRatioUpper']:.3f}x,
and both unchanged quiet guards pass. Confirmed managed time falls
{e['confirmedManagedReduction']*100:.2f}% on this fixed case. It clears 2x;
broader required, unmatched and unconfirmed warm cases remain open.

| Implementation/run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{rows}

The first primitive stores progressive coefficients contiguously in spectral
order, mapping each block to natural order once at reconstruction. After EOI
and every component's DC progression have been validated, the already admitted
64-entry approximation array supplies that reconstruction workspace. No extra
allocation, scratch admission or source-derived table is introduced. Layout alone
saves only {e['layoutOnlyManagedReduction']*100:.2f}% and is not accepted as a
standalone optimization. Its screen is frozen before the next primitive.

The next primitive groups correction traversal across zero runs and EOB tails.
AVX2 identifies zero lanes, bounds a group before an insertion zero or through
the final ZRL zero, and updates signed magnitudes only when the refinement bit
is absent. BMI2 deposits oldest-first correction bits into existing nonzero
lanes. Lanes outside the active prefix retain their original values. Complete
valid entropy and the exact original logical work must be admitted together;
markers, truncation, tight budgets and cancellable tokens retain the original
scalar sequence and its partial-consumption/failure precedence. Unsupported
hardware uses scalar traversal. No entropy syntax, quality, transform,
coefficient value, reconstruction arithmetic or admission policy changes.

Only existing ReadBlock (707 to 738 IL bytes) and Render (2031 to 2109 bytes)
change relative to the accepted baseline. Two original methods are added; all
other existing bodies and implementation flags are equivalent after resolving
metadata references. Both actual generated timing libraries match the final
actual package's complete production methods and flags. Subsequent comments and
documentation are separately verified to preserve those methods and fixture
pixels. No upstream source, tables, binaries or corpus is tracked.

There is still one performance pair and one original 513x259 raster. Both subjects
decode the identical resident six-scan, 75,112-byte grayscale JPEG. Its lossless
preparation retains all 137,280 coefficients, quantization and owned decoded
bytes. Normal/scalar CLI bounds, exact alpha, source preservation and independent
held outputs pass. The native boundary retains software decode/reset, full-range
GRAY8-to-RGBA conversion, independent output acquisition and release. Codec and
conversion threads are one, thread type zero, CPU0, normal SIMD, tiering off and
nonconcurrent workstation GC. Startup, I/O, IPC, preparation, hashing and
validation remain excluded. No new image, quality, shape or scan search is added.

Both OSes pass 31 focused reader/group controls and 96 public JPEG controls in
normal/scalar modes. The group controls compare 248,832 admitted prefixes with
an independent scalar reference, covering signs, already-refined values, all
zero masks, bands and insertion/ZRL boundaries. Reader controls cover 32,768
width/prefix/payload examples plus stuffing, padding, work, marker/truncation,
cancellation and nonmutation. Ten public controls retain frozen prior work
minima and pixel hashes. Public decode allocation controls retain exactly
1,569,568 bytes per operation in normal/scalar Windows modes with tiering off.
All 2,347 Release tests pass on each OS: 603 internal, 848 public and 896 live
FFmpeg comparisons. The same final actual dependency-free Release package and
all three usage examples pass on both OSes.

Each primitive gets one short screen. The useful grouped result gets one
independent 12-actual confirmation, with all raw/source/actual-child identities
frozen before later edits. Three-iteration uncertainty alone cannot certify this
case; the longer confirmation does. No further micro-tuning of this passing
fixed pair is planned. Cover one material unmeasured case and rerank.

The [numeric evidence](warm-jpegrefinement-evidence.json) retains all managed and
native samples, GC/threading output, uncertainty, quiet guards, original-input
provenance, actual package/child/source identities and immutable archives.
Conservative bounds divide the managed confidence upper by the smaller native
bracket confidence lower using the same critical value. Regenerate this report
with `python benchmarks/render-warm-jpegrefinement.py`; use `--check` to verify.
The preceding [CPU diagnosis and rejected EOB-only shape](WARM-JPEG-CORRECTION-REJECTION.md)
remain unchanged. See the [reference recipe](../tools/WarmReference/README.md).
'''
p=folder/'WARM-JPEG-REFINEMENT.md'
if '--check' in sys.argv:
 assert p.read_text(encoding='utf-8')==text,'Progressive refinement report differs.'
 print('Progressive refinement report matches numeric evidence.')
else:p.write_text(text,encoding='utf-8',newline='\n')

"""Render the single eight-channel Paeth improvement from frozen evidence."""
import json,sys
from pathlib import Path
folder=Path(__file__).resolve().parent;e=json.loads((folder/'warm-paethvector-evidence.json').read_text());a,failed,b=e['observations'];base=e['baseline']
assert e['productionChanged'] and e['thisFixedPairClearsConservativeTwo'] and not e['uniformWarmParityComplete']
rows='\n'.join(f"| {label} | {o['managed']['statistics']['Mean']/1000:.2f} | {o['worstBracketRatio']:.5f}x | {o['conservativeRatioUpper']:.5f}x | {'Pass' if all(g['passed'] for g in o['quietness'].values()) else 'FAIL'} |" for label,o in [('Quiet baseline screen',base),('Eight-channel screen',a),('Guard-failed confirmation',failed),('Independent quiet confirmation',b)])
text=f'''# Warm eight-channel Paeth reconstruction

Complete Paeth RGBA16 decode improves {e['managedReductionFraction']:.1%} over the
quiet baseline screen, independently confirming at {b['worstBracketRatio']:.3f}x
FFmpeg, conservative upper {b['conservativeRatioUpper']:.3f}x. Both unchanged quiet
guards pass. This fixed pair clears2x; broader required warm coverage remains open.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{rows}

The [baseline diagnosis](WARM-PNG-PAETH-BASELINE.md) attributes about46% of sampled
CPU to Paeth prediction and generic byte-loop reconstruction. One primitive now
reconstructs eight independent channel bytes together in safe128-bit signed-short
lanes. The three predictor distances stay within510; exact ties select left,
then up, then upper-left. Masking sums to255 preserves byte overflow before
narrowing and storing eight bytes through safe MemoryMarshal spans. No upstream
implementation, table or source is copied.

Only Unfilter changes (201 to256 IL bytes), plus the new UnfilterPaeth8 method.
The other562 existing production bodies are equivalent after resolving metadata
references; all old implementation flags are exact. Unsupported hardware, other
pixel widths, overlapping rows and truncated previous rows retain scalar behavior.
Scalar tails preserve odd lengths. The original65536-byte cancellation checkpoints
and empty-row behavior are unchanged. There is no workspace, allocation or
admission change.

Thirteen new internal controls cover all16,777,216 predictor triples with varied
byte deltas, exact tie order/overflow, first rows, tails/chunks/guards, cancellation
before writing, overlapping-row read/write order and truncated-row partial updates
with the original exception. Both OSes also run these controls with intrinsics
disabled. The frozen native and analytical PNG corpus exercises all legal static
profiles, filters, interlace, transparency and hidden samples.

The same772,037-byte all-Paeth/noninterlaced/level6 PNG uses the existing513x259
RGBA16 raster. Owned samples, alpha/hidden RGB, source bytes and independent held
outputs remain exact on both OSes with normal/scalar code. Pinned normal/scalar
FFmpeg CLI outputs agree on the VM. Public Stream allocations remain4,713,416
bytes/op. Exact work minimum15,858,551 and working-memory minimum2,877,305 are
unchanged; one below each fails and the minimum produces exact pixels. No raster,
geometry, filter, quality or effort sweep is added.

Windows and Linux each pass628 internal+848 public+896 live FFmpeg tests
(2,372 total), plus scalar13 primitive/31 public-PNG controls. All three usage
examples pass on both OSes through the same actual dependency-free Release
package. Fresh, packaged and both generated timing libraries match all564
production methods and implementation flags.

One short screen precedes a12-actual confirmation whose post-load0.304688
exceeds the unchanged0.25 guard. It remains failed and is frozen before a bounded
fresh attempt. The fresh process reuses the same four prepared child identities
and waits for a stricter starting load<=0.01; guard limits, input, code, workload
and200ms managed/50ms native targets remain unchanged. Its12 actual managed
iterations and12 native samples in each bracket pass both guards. BenchmarkDotNet
selects all12; the conservative upper takes the larger filtered/all-actual bound:
filtered {b['filteredConservativeRatioUpper']:.5f}x, all-actual
{b['unfilteredActualConservativeRatioUpper']:.5f}x. Each view uses its managed
confidence critical value for native bracket lower bounds. All earlier raw
measurements and failed guards remain unchanged.

Resident-input decode/acquisition/release, CPU0, one codec/conversion thread,
thread type zero, normal SIMD, tiering off and nonconcurrent workstation GC are
unchanged. Startup, I/O, IPC, preparation, hashing and validation checks are excluded.
The [numeric evidence](warm-paethvector-evidence.json) retains every actual/native
measurement, guard, input/source/actual-child/package identity, semantic method
comparison, admission controls and immutable archives. Regenerate with
`python benchmarks/render-warm-paethvector.py`; `--check` verifies this report.
See the [reference recipe](../tools/WarmReference/README.md). Stop tuning this
passing point; cover one Paeth RGBA8 input on the existing eight-bit raster and
rerank the remaining gaps.
'''
p=folder/'WARM-PAETH-VECTOR.md'
if '--check' in sys.argv:
 assert p.read_text(encoding='utf-8')==text,'Paeth vector report differs.'
 print('Paeth vector report matches numeric evidence.')
else:p.write_text(text,encoding='utf-8',newline='\n')

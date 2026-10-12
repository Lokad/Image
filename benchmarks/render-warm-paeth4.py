"""Render the four-channel Paeth round from immutable VM evidence."""
import json
import sys
from pathlib import Path

folder = Path(__file__).resolve().parent
e = json.loads((folder / 'warm-paeth4-evidence.json').read_text())
screen, confirm = e['observations']
assert e['thisFixedPairClearsConservativeTwo'] and not e['uniformWarmParityComplete']
rows = '\n'.join(
    f"| {label} | {o['managed']['statistics']['Mean'] / 1000:.2f} | {o['worstBracketRatio']:.5f}x | {o['conservativeRatioUpper']:.5f}x | {'Pass' if all(g['passed'] for g in o['quietness'].values()) else 'FAIL'} |"
    for label, o in [('Scalar baseline screen', e['baseline']), ('Four-channel screen', screen), ('Independent quiet confirmation', confirm)])
text = f'''# Warm four-channel Paeth reconstruction

Complete Paeth RGBA8 decode improves {e['managedReductionFraction']:.1%} over the
quiet scalar baseline. Independent confirmation reaches
{confirm['worstBracketRatio']:.3f}x FFmpeg, conservative upper
{confirm['conservativeRatioUpper']:.3f}x, with both unchanged quiet guards passing.
This fixed pair clears2x; full required warm coverage remains open.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{rows}

The [baseline diagnosis](WARM-PNG-PAETH8-BASELINE.md) attributes30.03% of sampled
CPU to prediction/reconstruction. The new safe128-bit primitive reconstructs four
independent channel bytes together, using the already owned eight-channel
strategy. Signed-short predictor distances, left/up/upper-left tie precedence,
byte overflow and safe span loads/stores preserve the exact scalar recurrence.
Unfilter dispatch expands from256 to274 IL bytes, and UnfilterPaeth4 is added.
The other563 existing production bodies remain equivalent after resolving metadata
references, including the eight-channel primitive; all existing flags are exact.

Unsupported hardware, overlapping rows and truncated previous rows retain scalar
behavior. Tails, empty rows, partial updates before exceptions and the original
65536-byte cancellation checkpoints are unchanged. No workspace or allocation is
added. Thirteen original controls cover all16,777,216 predictor triples, ties,
overflow, first rows, tails, guarded spans, aliases, errors and cancellation.
These controls and31 public-PNG controls pass with intrinsics disabled on both OSes.

One existing513x259 RGBA8 raster is reused. Its345,325-byte, noninterlaced,
all-Paeth/level6 PNG remains identical. All samples, alpha/hidden RGB, source bytes
and independent outputs are exact on both OSes with normal/scalar code; pinned
normal/scalar FFmpeg CLI outputs agree. Fresh public Stream decode allocates
2,275,800 bytes/op, unchanged from the preceding package. Exact admission minima
are7,709,181 work units and1,488,309 working bytes; one below either fails.
No raster, geometry, filter, quality or effort sweep is added.

The three-actual screen has upper2.392x and remains insufficient by itself.
A confirmation preguard aborts before launching a benchmark because I/O wait
is1.1506%, above the unchanged1% limit. Its immutable receipt stays failed.
One fresh attempt reuses the same prepared child, input and targets, waits for
starting load<=0.01, and passes both guards with12 actual/12 filtered managed
iterations and12 native samples per bracket. The conservative upper retains the
larger filtered/all-actual bound: filtered
{confirm['filteredConservativeRatioUpper']:.5f}x, all-actual
{confirm['unfilteredActualConservativeRatioUpper']:.5f}x. All raw measurements remain.

Windows and Linux each pass641 internal+848 public+896 live FFmpeg tests
(2,385 total). All three usage examples pass on both OSes through the same actual
dependency-free Release package. Fresh, packaged and both actual timing libraries
match all565 production methods and flags. Resident-input decode, admission,
independent owned output acquisition/release and conversion are timed. CPU0,
one codec/conversion thread, normal SIMD, disabled tiering and nonconcurrent
workstation GC are unchanged. Startup, I/O, IPC, preparation and validation are
excluded. No local timing is published.

The [numeric evidence](warm-paeth4-evidence.json) preserves every actual/native
measurement, guard, input/source/child/package identity, method comparison,
admission control and immutable archive digest. Regenerate with
`python benchmarks/render-warm-paeth4.py`; `--check` verifies this report.
See the [current summary](../docs/benchmark.md) and
[reference recipe](../tools/WarmReference/README.md). Stop tuning this passing
point; cover one material gap on the existing rasters and rerank.
'''
output = folder / 'WARM-PAETH-FOUR-CHANNEL.md'
if '--check' in sys.argv:
    assert output.read_text(encoding='utf-8') == text
    print('Four-channel Paeth report matches numeric evidence.')
else:
    output.write_text(text, encoding='utf-8', newline='\n')

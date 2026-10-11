import json
import sys
from pathlib import Path

root = Path(__file__).resolve().parent
evidence = json.loads((root / 'warm-resize-paired-rows-rejected-evidence.json').read_text(encoding='utf-8'))
assert evidence['project'] == 'Lokad.Image' and evidence['verification']['noLongerRepeat']
baseline, screen = evidence['baseline'], evidence['screen']
rows = '\n'.join(
    f"| {label} | {item['managedStatistics']['Mean'] / 1000:.2f} | "
    f"{item['worstBracketRatio']:.5f}x | {item['conservativeRatioUpper']:.5f}x |"
    for label, item in [('Retained packed accumulation baseline', baseline), ('Rejected short screen', screen)])
content = f'''# Warm resize: paired output-row trial rejected

The paired RGBA8 row kernel is {abs(evidence['screenedMeanGainPercent']):.2f}% slower by managed mean in
one short screen. It is discarded without a longer repeat. Production source
is restored exactly; the retained 2.89x result remains unchanged.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: |
{rows}

The trial used eight binary64 lanes for two independent output rows sharing
horizontal geometry. Two bounded sample reads fed packed widening/conversion;
alpha permutations and wide weights supplied independent accumulation lanes.
Each pixel retained separate multiply/add and its original tap order, exact
tiny-alpha handling and rounding. Unequal row supports, odd tails, unsupported
hardware and UInt16 retained the prior path. Paired geometry used 192 bytes of
fixed stack storage, with no new heap workspace.

181 baseline controls per OS and 181 trial controls per OS with intrinsics
enabled/disabled pass. Only PreparedResize.Apply changed among 538 existing
production methods; four named internal methods were added. All existing flags
and other existing bodies remained equivalent. The actual statistical child DLL
matched fresh trial IL/flags. Managed rational-reference pixels were exact;
native alpha was exact and RGB stayed within maximum 1 / RMSE 0.25.

The sole performance pair remains original 513x259 RGBA8 to 256x129 bilinear
resize. Managed timing includes complete public Resize, admission, preparation,
allocation, filtering and disposal. Native timing includes conversion, widened
triangle filtering, encoded-space alpha handling and independent output
acquisition/release through the FFmpeg graph, whose contexts are reused. Native
samples bracket managed samples. Both use one CPU/filter thread and normal SIMD.
Startup, IPC, I/O, generation, hashing and validation are excluded.
No performance image or pair was added.

One JIT inspection of the actual timed DLL accompanies fixed allocation and
owned-pixel controls. Wide coefficient construction performs a 64-byte stack
read and write per active paired tap, plus insertion operations. This is a
generated-code observation, without an instruction-time attribution. Three
fixed batches of 16 Resize/Dispose calls retain exactly the baseline allocation
totals. The next wide-kernel hypothesis removes this materialization and the
alpha permutation/mask work through a different packed-sample formulation.
The unchanged rejected shape will not be repeated.

After restoration, all 2128 Release tests and 181 scalar controls pass per OS;
fresh production IL and flags match the previously verified actual package.
Pre-run quiet checks pass and post-run failures remain. Independent quiet
confirmation and uniform warm parity remain open.

The [numeric evidence](warm-resize-paired-rows-rejected-evidence.json) retains
statistics and identities. The immutable short archive SHA-256 is
`{evidence['immutableEvidence']['shortArchiveSha256']}`;
the rejection/restoration archive SHA-256 is
`{evidence['immutableEvidence']['rejectionArchiveSha256']}`.
Regenerate with `python benchmarks/render-warm-resize-paired-rows-rejected.py`;
use `--check` to verify this report.
'''
target = root / 'WARM-RESIZE-PAIRED-ROWS-REJECTED.md'
if '--check' in sys.argv:
    assert target.read_text(encoding='utf-8') == content
else:
    target.write_text(content, encoding='utf-8', newline='\n')

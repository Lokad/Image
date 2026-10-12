"""Render three retained quiet coverage confirmations; never benchmark."""
import json,math,re,sys
from pathlib import Path
r=Path(__file__).resolve().parents[1];e=json.loads((r/'benchmarks/warm-coverage-quiet-evidence.json').read_text());rows=[]
for c in e['cases'].values():
 o=c['observations'][-1];assert c['independentQuietConfirmationThisPair'] and c['all569TimedMethodsAndFlagsMatchSameActualPackage']
 assert len(o['managed']['actual'])==12 and o['conservativeRatioUpper']<=2 and o['certifiedThisPair']
 assert all(g['passed'] and g['protocol']=='exclusive-vm-idle-v2' for g in o['quietness'].values())
 gc=re.fullmatch(r'// GC:\s+\d+\s+\d+\s+\d+\s+(\d+)\s+(\d+)',o['managed']['rawGc']);allocation=math.ceil(int(gc[1])/int(gc[2]))
 rows.append(f'| {c["label"]} | {o["managed"]["statistics"]["Mean"]/1000:,.2f} | {min(b["mean"] for b in o["nativeBrackets"])/1000:,.2f} | {o["worstBracketRatio"]:.5f} / {o["conservativeRatioUpper"]:.5f} | {allocation:,} | 12 / {o["managed"]["statistics"]["N"]} | Pass |')
text=f'''# Warm coverage: JPEG 4:2:2, nearest RGBA8 and transpose RGBA16

Three existing fixed pairs independently confirm below 2x FFmpeg, including
conservative uncertainty. Both idle guards pass for every confirmation.
Production and benchmark implementations are unchanged. Full required warm
coverage remains open; no averages establish uniform parity.

| Fixed pair | Lokad.Image microseconds | FFmpeg microseconds | Ratio / upper | Managed allocated B/op | Actual / filtered | Quiet guards |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
{chr(10).join(rows)}

All measurements use owned source SHA-256
`7d6af46ce2d59662365395542b2d5b706e7729968ad5b15ae95bd64ac9e97152`,
revision `def1854b5701c449de68bbac1d008a2afd4c88ef`. The same two original
513x259 RGBA8/16 rasters are reused, without new source images, input encoding,
quality calibration, profile or geometry searches. JPEG consumes the existing
frozen 71,168-byte native 4:2:2 input; its decoded managed/native hashes remain
unchanged, with exact opaque alpha and normal/scalar RGB maximum 4 / RMSE 1.5.
Nearest preserves all four samples at exact integer center coordinates in
256x129 RGBA8 output, including hidden RGB. Plain transpose preserves every
UInt16 sample at exact 259x513 placement. Both editing cases pass native CLI,
source integrity and independent owned-output checks.

Each pair retains one three-actual screen and one independent twelve-actual
confirmation, with three/twelve samples per native pre/post bracket at the
declared 50 ms target. Managed iterations target 200 ms. Every actual is retained;
upper is the larger filtered/all-actual conservative bound. All scored screens
and confirmations pass `exclusive-vm-idle-v2`. Transpose's first pre-run guard
failed and launched no benchmark; its immutable archive was frozen before one
bounded fresh attempt. Earlier failed guards remain failed at their own sources.
Historical load is diagnostic only; no historical cooldown or other operator wait.

Timing covers complete public operations, admission, conversion/materialization,
independent output acquisition and disposal. JPEG includes native state reset,
full-range color conversion and chroma expansion/crop each time. Native nearest
includes packing/plane extraction, separate RGB/replicated-alpha point kernels,
merging and output allocation/release. Native transpose allocates and processes
independent pixels. Native contexts/graph/frame/packet wrappers are reused;
no output is cached. Managed transpose may reuse released pixel storage.
Padded native rows remain exposed. Startup, I/O, IPC, preparation, hashing and
validation are excluded. CPU0, one codec/conversion/filter thread, normal SIMD,
tiering off and nonconcurrent workstation GC remain fixed.

Allocations are raw GC allocated bytes / timed operations rounded up, including
harness overhead. Native allocation totals are unmeasured. All 569 production
method bodies and flags in both actual children for each pair match the same
accepted Release package. Its preceding 2,390 tests and 39 scalar Huffman controls
per OS, plus actual-package/all-three-example checks on both OSes, remain
applicable; no production/package checks were repeated without changes.

[Numeric evidence](warm-coverage-quiet-evidence.json) retains screens, all actuals,
brackets, source/input/output/child/package identities, guards and immutable
archive digests. Native compiler prefix paths are normalized for publication;
unaltered original receipts stay bound by their SHA-256 digests and raw archives.
The [earlier JPEG 4:2:2](WARM-JPEG422-ROWS.md),
[nearest RGBA8](WARM-NEAREST-BASELINE.md) and
[transpose RGBA16](WARM-TRANSPOSE-TILES.md) evidence remains historical evidence
at its reported sources, without a newly certified before/after speedup claim.
See the [current three-table summary](../docs/benchmark.md) and
[measurement recipe](../tools/WarmReference/README.md).
Regenerate with `python benchmarks/render-warm-coverage-quiet.py`; `--check`
verifies this report. Stop tuning these passing points and cover a remaining case.
'''
target=r/'benchmarks/WARM-COVERAGE-QUIET.md'
if '--check' in sys.argv:
 assert target.read_text(encoding='utf-8')==text,'Quiet coverage report is stale.'
 print('Quiet coverage report is current.')
else:target.write_text(text,encoding='utf-8',newline='\n')

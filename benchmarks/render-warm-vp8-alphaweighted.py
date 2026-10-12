"""Render retained VP8 alpha-weighted conversion evidence; never run benchmarks."""
import json,math,re,sys
from pathlib import Path
r=Path(__file__).resolve().parent;e=json.loads((r/'warm-vp8-alphaweighted-evidence.json').read_text(encoding='utf-8'));rows=[]
assert e['independentQuietConfirmationThisPair'] and e['thisFixedPairClearsConservativeTwo']
for o in e['observations']:
 s=o['managed']['statistics'];native=min(b['mean'] for b in o['nativeBrackets']);gc=re.fullmatch(r'// GC:\s+\d+\s+\d+\s+\d+\s+(\d+)\s+(\d+)',o['managed']['rawGc']);assert gc
 allocation=math.ceil(int(gc[1])/int(gc[2]));assert all(g['passed'] for g in o['quietness'].values())
 rows.append(f'| {o["label"]} | {s["Mean"]/1e6:.3f} | {native/1e6:.3f} | {o["worstBracketRatio"]:.5f} / {o["conservativeRatioUpper"]:.5f} | {allocation:,} | {len(o["managed"]["actual"])}/{s["N"]} | Pass |')
last=e['observations'][-1]
content=f'''# Warm VP8 encode: alpha-weighted chroma

The fixed lossy VP8 encode independently confirms at
{last['worstBracketRatio']:.3f}x FFmpeg, conservative upper {last['conservativeRatioUpper']:.3f}x,
with both quiet guards passing. Stop tuning this point; full required warm
coverage remains open. Compression efficiency parity is not established.

| Run | Lokad.Image ms | FFmpeg ms | Ratio / upper | Managed allocated B/op | Actual/filtered | Quiet guards |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
{chr(10).join(rows)}

One existing original 513x259 RGBA8 raster is encoded, with its hidden RGB and
alpha unchanged. Native quality 75 and managed quality 100, both effort 4, are
fixed before measurement. Their source PSNRs are 16.701663 and 16.698017 dB:
gap 0.003646 dB clears the unchanged 0.25 dB band. Alpha is exact. Registered
normal/scalar native decoding agrees exactly; managed/native decoded RGB agrees
within maximum 1 / RMSE 0.136842. No additional raster, quality enumeration,
profile sweep or relaxed acceptance bound is used. The fixed-anchor metadata
gate recomputes the match and rejects fifteen invalid controls before native work.

Lokad.Image emits **471,436 bytes** versus FFmpeg's **98,590 bytes**. The managed
encoder uses fixed entropy models and raw ALPH; the native encoder uses adaptive
coding. This is a quality-matched complete-operation timing comparison; encoded
size efficiency remains a limitation. Native allocations are unmeasured.

One production primitive changes chroma preparation: each 2x2 cell averages
encoded RGB using normalized alpha weights before the existing limited-range
BT.601 matrix and rounding. Hidden colors therefore do not dilute visible
chroma. Fully transparent cells keep the original average; opaque and uniform
opacity keep their original result. Luma, exact normalized alpha, 16-to-8
rounding, odd/padded edge extension, admission, work charges and cancellation
remain unchanged. No transfer table, new workspace or upstream code is added.
The earlier linear-sRGB candidate remains a rejected historical experiment.

Twelve original analytical controls cover both depths, visible/hidden chroma,
unequal and uniform opacity, zero alpha and odd/padded edges. The existing 42
managed corpus recipes/options are replayed: 15 alpha bitstreams and their
independent native decoded goldens change; all opaque bitstreams and native
fixtures stay identical. The first QA replay failure is retained before that
selective refresh. Regenerate with the registered original oracle through
`tools/Fixtures ... --refresh-managed-vp8`; no new fixture inputs are introduced.

Timing includes the complete public encode, new managed MemoryStream creation/
growth, independent bytes and disposal. Native RGBA-to-BGRA repacking, internal
RGB-to-YUV conversion and complete encode run every operation; codec/scratch/
frame/packet wrappers are reused. No output is cached. Startup, I/O, IPC,
generation, validation and quality matching are excluded. CPU0, one codec thread,
thread type zero, normal SIMD, tiering off and nonconcurrent workstation GC
remain fixed. Managed iterations target 200ms; native brackets target 50ms.
All twelve actuals, including two filtered outliers, are retained; upper takes
the larger filtered/all-actual conservative bound. `exclusive-vm-idle-v2`
checks live overlap plus five-second aggregate/CPU0 busy <=5%, I/O wait <=1%,
steal <=1%. Both screen and confirmation pass both guards without retries.

Measured source is `{last['source']['sourceSha256']}`
at recorded revision `{last['source']['sourceRevision']}`. Raw receipts, four
actual child files, source and package are archived before mutation. Fixture
refresh uses a separately bound subsequent source with identical production
method bodies and flags. Both OSes pass 2,410 Release tests each (660 internal,
854 public API, 896 live comparisons), 19 scalar chroma/admission controls each
and all three examples through the same actual dependency-free Release package.
All 571 fresh/timed/package production methods and flags match.

[Numeric evidence](warm-vp8-alphaweighted-evidence.json) retains all actuals,
brackets, quality/size figures, guards, input/output/source/child/package
identities, prototype, selective refresh and immutable archive digests. See the
[current three-table summary](../docs/benchmark.md) and
[measurement recipe](../tools/WarmReference/README.md).
Regenerate with `python benchmarks/render-warm-vp8-alphaweighted.py`;
`--check` verifies this report. Unmatched and uncovered scope stays open.
'''
p=r/'WARM-VP8-ALPHAWEIGHTED.md'
if '--check' in sys.argv:
 assert p.read_text(encoding='utf-8')==content,'VP8 alpha-weighted report is stale.'
 print('VP8 alpha-weighted report is current.')
else:p.write_text(content,encoding='utf-8',newline='\n')

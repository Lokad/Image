"""Render retained warm native-input VP8L evidence; never benchmark."""
import json,math,re,sys
from pathlib import Path
r=Path(__file__).resolve().parents[1];e=json.loads((r/'benchmarks/warm-vp8l-canonical-evidence.json').read_text());last=e['observations'][-1]
assert e['independentQuietConfirmationThisPair'] and e['allTimedMethodsAndFlagsMatchSameActualPackage']
assert len(last['managed']['actual'])==12 and last['conservativeRatioUpper']<=2
assert all(g['passed'] and g['protocol']=='exclusive-vm-idle-v2' for g in last['quietness'].values())
gc=re.fullmatch(r'// GC:\s+\d+\s+\d+\s+\d+\s+(\d+)\s+(\d+)',last['managed']['rawGc']);allocation=math.ceil(int(gc[1])/int(gc[2]))
rows=[]
for o in e['observations']:
 quiet='Pass' if all(g['passed'] for g in o['quietness'].values()) else 'Failed / screening only'
 rows.append(f'| {o["label"]} | {o["managed"]["statistics"]["Mean"]/1000:,.2f} | {o["worstBracketRatio"]:.5f}x | {o["conservativeRatioUpper"]:.5f}x | {quiet} |')
text=f'''# Warm native-input VP8L decode: canonical lookup

Complete public VP8L decode independently confirms at
{last['worstBracketRatio']:.3f}x FFmpeg, conservative upper
{last['conservativeRatioUpper']:.3f}x. Both idle guards pass. This native-input
case clears the 2x timing target; full required warm coverage remains open.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{chr(10).join(rows)}

The same original 513x259 RGBA8 raster is reused. Its fixed libwebp
lossless=1/method=4/quality=75/exact=1 input is frozen once at 183,812 bytes,
SHA-256 `08ddac115f1770d778e6dbc76373440224b32e6f93b616605e234c63206eebde`.
This exercises native adaptive coding beyond the earlier literal/run case.
Both decoders consume identical encoded bytes and reproduce every original RGBA
sample, including alpha and hidden RGB. Managed ownership/source checks and
normal/scalar public FFmpeg decoding pass. No new performance raster or source,
quality, effort or strategy sweep is introduced. FFmpeg's public software decoder
remains the timed baseline; the encoder-backend exception is only for preparation.

One six-second CPU profile places 33.8% in Huffman lookup and 21.0% in bit reads.
One primitive resolves longer canonical symbols using local bit-reader state
and the existing prefix tables, replacing repeated per-bit reader dispatch and
work checks with one exact logical-bit charge. No new table, storage or admitted
construction work is added. It loads no byte beyond the symbol. Tight budgets,
truncation and cancellation retain the original per-bit fallback, consumed
prefixes and limit-before-truncation precedence. Short lookups and zero-bit
single leaves remain available. This is independent code derived from RFC 9649;
FFmpeg's lookup strategy was inspected only for insight, with no code/table copy.

The original screen observes 6.61 ms and the candidate confirmation 5.70 ms.
These are declared measurements at different sources, not a separately certified
before/after speedup: the baseline has only three actuals, and the candidate
short screen fails its post-run idle guard. The independent final confirmation
retains all 12 actual / 10 filtered measurements and 12 samples per native
bracket. Upper is the larger filtered/all-actual bound. The baseline and candidate
confirmation each had a failed pre-run idle check, launching no benchmark; both
archives are retained before one bounded fresh attempt. Passing confirmation
uses `exclusive-vm-idle-v2`, with historical load diagnostic only.

Timing includes complete public Decode, admission, entropy and transform
reconstruction, independent owned output acquisition and disposal. Native
software WebP decode resets state, converts ARGB to RGBA and acquires/releases
owned output. Codec/conversion contexts and packet/frame wrappers are reused;
every pixel is decoded/converted. Padded native rows remain exposed. Startup,
I/O, IPC, input encoding/preparation, hashing and validation are excluded.
CPU0, one codec/conversion thread, thread type zero, normal SIMD, tiering off
and nonconcurrent workstation GC remain fixed.

Managed allocation is unchanged at **{allocation:,} B/op**, raw GC bytes /
operation count rounded up, including harness overhead. Native allocations
are unmeasured. Both OSes pass **2,390 tests** (646 internal, 848 public, 896
live FFmpeg), **39 scalar Huffman controls**, and the **same actual Release
package/all three usage examples**. Fresh and both actual timing children match
every production method body and flag in that package. New controls cover
uniform 10/15-bit trees in every byte phase alongside existing mixed lengths,
partial budgets, truncation, cancellation, exact storage/work admission and
unchanged single leaves. Source pixels, independent owners and errors stay intact.

[Numeric evidence](warm-vp8l-canonical-evidence.json) retains every actual,
brackets, source/input/output/actual-child/package identities, guards and immutable
archives, including failures and the CPU profile. The
[earlier literal/run confirmation](WARM-VP8L-DECODE-QUIET.md) remains historical
evidence at its own source. See the [current three-table summary](../docs/benchmark.md)
and [measurement recipe](../tools/WarmReference/README.md).
Regenerate with `python benchmarks/render-warm-vp8l-canonical.py`; `--check`
verifies this report. Stop tuning the passing point and cover one remaining case.
'''
target=r/'benchmarks/WARM-VP8L-CANONICAL.md'
if '--check' in sys.argv:
 assert target.read_text(encoding='utf-8')==text,'VP8L canonical report is stale.'
 print('VP8L canonical report is current.')
else:target.write_text(text,encoding='utf-8',newline='\n')

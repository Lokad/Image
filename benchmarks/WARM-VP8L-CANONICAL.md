# Warm native-input VP8L decode: canonical lookup

Historical timing checkpoint: [borrowed VP8L lookup](WARM-VP8L-BORROWED-GREEN.md)
records the current qualified short measurement. Its selected decode timing is superseded; original packets and boundaries remain.

Complete public VP8L decode independently confirms at
1.887x FFmpeg, conservative upper
1.904x. Both idle guards pass. This native-input
case clears the 2x timing target; full required warm coverage remains open.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Baseline coverage screen | 6,606.16 | 2.18219x | 2.79897x | Pass |
| Canonical candidate screen | 5,728.95 | 1.89385x | 2.81723x | Failed / screening only |
| Independent quiet coverage confirmation | 5,703.55 | 1.88670x | 1.90400x | Pass |

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

Managed allocation is unchanged at **1,216,180 B/op**, raw GC bytes /
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

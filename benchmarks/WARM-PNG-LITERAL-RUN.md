# Warm PNG decode: consecutive Deflate literals

Complete public RGBA16 PNG decode independently confirms at
1.958x FFmpeg, conservative upper
1.968x. Both current idle guards pass.
This fixed pair clears 2x; full required warm coverage remains open.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Baseline screen (original guard) | 6,543.63 | 2.06600x | 2.65384x | Pass |
| Baseline confirmation (original guard failed) | 5,958.48 | 2.03251x | 2.04666x | FAIL |
| Literal-run screen | 6,222.94 | 1.98273x | 2.20619x | Pass |
| Independent literal-run confirmation | 5,740.60 | 1.95836x | 1.96783x | Pass |

One existing original 513x259 RGBA16 raster and its identical level-6,
filter-None, noninterlaced PNG are reused. All sixteen-bit samples, alpha,
hidden RGB, source bytes and independently owned outputs match exactly.
Normal/scalar native CLI checks now cover this decode profile too. Native
complete decode produces RGBA64BE, then allocates and converts a second
independent RGBA64LE output inside each operation; padded rows remain exposed.
Managed public decode includes framing, checksums, inflation, sample conversion,
admission, allocation and disposal. Startup, I/O, IPC and validation are excluded.
No source-raster, shape, filter or quality enumeration.

One six-second baseline profile samples 48.0% in `ZlibFraming.Validate` and
42.8% in BCL decompression. The independently implemented RFC 1951 framing walk
now counts consecutive root-table literals in one loop with local bit-cache
state, avoiding repeated length/distance dispatch. It charges exactly the
consumed bits. Long/unassigned prefixes, end codes, matches, tight work limits,
excess output and short tails retain the existing dispatcher. Cancelable
operations retain per-symbol cancellation checks. Memory admission is unchanged;
no new allocation or production dependency.

The owner confirmed exclusive VM use. Protocol `exclusive-vm-idle-v2` checks
live jobs and five-second aggregate/CPU0 samples: busy <=5%, I/O wait <=1%,
steal <=1%. Load average is diagnostic only; our previous work needs no cooldown.
The earlier confirmation fails its original load guard and remains failed.
No certified before/after speedup is claimed across the protocol change.
The candidate's first preparatory guard fails aggregate I/O wait (1.951%) and
launches no benchmark. One fresh idle check passes, followed by the screen and
independent repeat without a load-average scheduler. All failures remain retained.

The repeat retains 12 actual / 11 filtered
measurements and 12 native samples per bracket. The upper uses the larger
filtered/all-actual confidence bound. Managed allocation is **2,032,864 B/op**
from raw GC bytes / operations, rounded up and including harness overhead.
Native allocation totals are unmeasured. CPU0, one codec/conversion thread,
thread type zero, normal SIMD, disabled tiering and nonconcurrent workstation GC
are fixed on the dedicated VM.

Both OSes pass **2,387 tests** each (643 internal, 848 public, 896 live FFmpeg),
plus 90 scalar framing tests each. New controls check every tight budget across
a literal run, output bounds and reserved-code failure precedence against the
cancelable per-symbol path. The same actual Release package and all three usage
examples pass on both OSes. Fresh builds, both actual timing children and that
package match all 566 production method bodies/flags. Support and contract are unchanged.

[Numeric evidence](warm-pngliteralrun-evidence.json) retains all measurements,
guards, receipts, input/output/source/child/package identities, correctness
proofs and immutable archive digests. Regenerate with
`python benchmarks/render-warm-pngliteralrun.py`; `--check` verifies this report.
See the [current summary](../docs/benchmark.md) and
[reference recipe](../tools/WarmReference/README.md). Stop tuning this passing
pair and prioritize the largest remaining gap.

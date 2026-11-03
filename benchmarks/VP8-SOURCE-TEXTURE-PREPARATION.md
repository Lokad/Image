# VP8 source texture preparation

Prepare each source block's weighted Hadamard energy once per macroblock, then
reuse it across luma16/luma4 candidate decisions. Candidate energy stays fresh.
The source and weights are already snapshotted before result writes; the new
borrowed view adds 64 stack bytes within the existing admitted 16 KiB scratch.
No heap allocation, pool, cache, reservation or retained owner is added.

A current-source count audit finds 54,038 source-energy calculations for 8,976
distinct values on the frozen photograph: 45,062 calculations are redundant.
These counts motivate preparation; they are not a timing or allocation claim.
Preserve each block's absolute difference and division by32 before summing,
every low bit, mode order/ties, pruning, trellis and texture-zero bypass.

| Complete warm encode / FFmpeg | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Opaque photograph | 13,670.07 | 9,706.88 | 1.408 / 1.418 | 809,072 | 17,688 |
| Text/diagram, effort4 | 8,091.60 | 6,264.06 | 1.292 / 1.319 | 741,137 | 7,100 |
| Text/diagram, effort6 | 13,518.03 | 9,816.75 | 1.377 / 1.394 | 754,344 | 6,818 |
| Transparent RGBA8 | 67,390.45 | 58,098.07 | 1.160 / 1.200 | 10,749,072 | 98,590 |
| Opaque RGBA8 | 21,160.69 | 16,885.52 | 1.253 / 1.265 | 1,201,552 | 72,380 |
| Opaque RGBA16 | 20,367.41 | 15,593.58 | 1.306 / 1.331 | 1,201,552 | 72,380 |
| Transparent RGBA16 | 70,818.32 | 57,929.11 | 1.222 / 1.236 | 10,749,072 | 98,590 |

The paired opaque photograph falls from **13,883.77 to 13,670.07 µs**:
**213.70 µs / 1.54% raw**, **1.29% reference-normalized**. Filtered/all-actual raw
confidence and conservative normalized intervals separate. The independent
three-actual screen suggested 2.23%, with wider confidence; it is not substituted
for confirmation. Allocation stays 809,072 B/op. The gain is narrowly scoped;
all seven comparisons retain CPU gaps.

The other twelve-actual confirmations refresh affected current costs. They are
not paired incremental-gain claims. Transparent RGBA8's direct paired screen is
flat (62,968.42 → 62,971.39 µs). Its current confirmed raw cost is higher than the
previous 65,037.57 µs observation, alongside a slower native clock; no causal
regression or full normalized point-gain claim. Unaffected lossless direct points
fluctuate, including an adverse diagram point; all actuals are retained without
attribution to this stage. No sampling solely to obtain favorable results.

Direct allocations and admitted peaks stay identical: photograph10,166,464 B,
diagram4,802,292 B, transparency3,847,888 B, small authored control312,716 B.
Scratch retention is zero. These admitted capacities differ from total allocation;
physical heap peaks and native allocation remain unmeasured.

Windows/Linux each pass **5,489 full/live checks**, **726 internal and194 public**
affected checks per normal/scalar mode. Frozen 102,656-block energy/magnitude,
low-bit, stride/overlap, metric/mode and complete-packet checks pass. Two new
checks cover repeated prepared candidates, independent per-block rounding and
invalid/overlapping storage. Existing cancellation, refusal cleanup and shared
ownership pass. The **same actual Release package** and all three usage examples
pass both OSes, including authored/mixed-alpha sync/async surviving outputs.

One production/internal build per OS serves every check and measurement. Reuse
existing harnesses/selectors/reference workers; one private count-projection
build, no public/benchmark/selector/native rebuild. All seven current confirmations
retain twelve actuals, before/after native brackets and quiet guards. Keep both
filtered and all-actual confidence views. Two routing errors happened before any
actual iteration for those launches; only unstarted cases resumed with the proper
prepared runtime. Completed measurements were reused unchanged.

Complete normalization, preparation, selection and independent packet ownership
remain timed. These are FFmpeg-backed VP8 measurements. Lossless WebP's libwebp
exact=1 and other approved reference boundaries remain unchanged. Older affected
method/quality/async costs and whole-profile fractions are historical. Lossless
encoding itself, low-effort distortion-only decisions, decoders, PNG/JPEG,
resizing, drawing and text retain applicable unchanged-path evidence.

[Evidence](vp8-source-texture-preparation-evidence.json) pins sources, producers,
all observations, adverse points and package proof. Raw
vp8-source-texture-preparation.tar.xz: **25,764,968 B /623 members**,
SHA2568139c385c9f9ef841b7ca11d3dc798a2e19ef77bde0b317c09fd29193e73604a,
mirrored/member-verified. Prior production and attribution dependencies remain
pinned. This is development qualification; full282-path parity stays open and
publication is postponed.

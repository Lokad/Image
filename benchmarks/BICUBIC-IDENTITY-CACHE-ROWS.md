# Direct identity-axis cache rows

Identity-axis gains and costs remain source-valid. Six affected nonidentity
cached comparisons now belong to [wide overlap execution](BICUBIC-WIDE-OVERLAP.md);
its current whole-build qualification supersedes this stage's package proof.

ICR614 converts source rows directly into the active bicubic cache slot when
the horizontal axis is unchanged. It removes a separate copy, its cancellation
loop and 16,416B of decoded-row storage on the existing 513-pixel-wide controls.
One conversion body serves two private polling modes selected before the loop.
Live tokens retain two-pixel/tail checkpoints; uncancelable tokens need no polls.
The unsigned-product arithmetic, row schedule, vertical certificates/repair and
independent outputs are unchanged. No API, owner, pool or session is added.

Independent complete-operation comparisons establish:

- **RGBA8:** 636.49 → 530.97µs, saving **105.52µs / 16.58% raw / 16.50% normalized**.
  Native moves −0.09%; conservative parity upper **0.908**.
- **RGBA16:** 666.59 → 511.76µs, saving **154.84µs / 23.23% raw / 21.26% normalized**.
  Native moves −2.50%; conservative parity upper **0.904**.

Filtered/all-actual raw and normalized confidence separates for both. This removes
the preceding [overlapping-window stage's](BICUBIC-OVERLAPPING-HORIZONTAL.md)
vertical-word tradeoff. Other refreshed points establish current costs, not causal
gains. All frozen inputs, settings, exact managed pixels and native float bounds
remain. Managed one-shot preparation, independent output and disposal stay timed;
the labeled FFmpeg zscale zimg backend retains graph, input planes and scratch.
Its packing, halo, filtering and fresh output ownership remain timed.

| Existing control | Lokad µs | zimg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Byte reduction | 485.10 | 416.46 | 1.165 / 1.169 | 199,980 | Gap |
| Word reduction | 494.80 | 570.19 | 0.868 / 0.871 | 199,953 | Pass |
| Byte horizontal reduction | 479.18 | 574.84 | 0.834 / 0.837 | 199,980 | Pass |
| Byte vertical reduction | 530.97 | 588.97 | 0.902 / 0.908 | 367,675 | Pass |
| Byte width down / height up | 816.23 | 840.52 | 0.971 / 0.979 | 199,943 | Pass |
| Word horizontal reduction | 583.75 | 581.31 | 1.004 / 1.010 | 199,930 | Uncertain |
| Word vertical reduction | 511.76 | 580.26 | 0.882 / 0.904 | 367,595 | Pass |
| Word width down / height up | 790.79 | 820.20 | 0.964 / 0.983 | 199,891 | Pass |
| Byte 32×16 reduction | 356.88 | 375.09 | 0.951 / 0.956 | 197,300 | Pass |

Nine costs refresh; seven pass and stop. Byte reduction remains a gap and horizontal
word reduction is uncertain. Four displayed rows update, with no new workload or
settings grid. Four cached word controls plus eleven unchanged paths now give
nine passes, five gaps and one uncertainty in the fifteen-word inventory.

Identity-axis workspace admission falls **383,852 → 367,436B**. Total allocation
falls **384,090 → 367,675B/op** for byte and **384,013 → 367,595B/op** for word.
Scratch retention is zero; independent results retain 264,708/529,416B until
disposal. The existing output pool is unchanged; governed outputs bypass it.
Exact-alpha repair still admits its overlapping 4KiB separately. Six new small
controls verify actual peaks, refusal, cancellation and recovery, including a
23,756 + 4,096 = 27,852B transparent-enlargement peak. Workspace admission, total
allocation, result retention and physical peak are distinct; physical/native
peaks are unmeasured.

The first direct-cache implementation kept conversion in the large driver with
an identity/poll check inside each iteration. Its sole word-vertical screen was
adverse, 1,011.78µs, despite correct pixels and lower allocation. It was not
confirmed. The selected version changes the execution layout, retaining one
conversion source body and selecting polling before the loop. These results do
not isolate the original regression's cause. Both epochs and all samples remain.

Both OSes pass **5,938 full/live checks**, 188 internal/703 public checks per
normal/scalar mode and the affected checks with AVX2/AVX512 disabled. Linux normal
mode exercises both polling paths and full-depth arithmetic; Windows exercises
portable fallbacks. The **same actual 0.2.1 package**, `80349672`, passes both OS
consumers and all three examples, including exact byte/word identity rows, terminal
shared refusal and independent survivors. Published releases remain untouched.

One selected main/embedded build per OS is reused throughout. The rejected epoch
has one main and three embedded versions: two new-test setup repairs accounted
for exact-rounding storage in the oracle and alongside the cache. Production
modules stayed unchanged during those repairs. Only consumer source changes
after timing; the exact measurement manifest reconstructs. No favorable sampling
repeat, new fixture/harness/native build or numerical relaxation was used.

[Evidence](bicubic-identity-cache-rows-evidence.json) retains source proof, bounds,
all-actual comparisons and qualification. Mirrored/member-verified raw
`bicubic-identity-cache-rows-qualified.tar.xz`, SHA256
`38182d3e21a2f068072676b45213be99288187cdb3fd68a6fd8cd838951c449e`,
10,891,128B/308 members, preserves both epochs, failed test setups, actual package
and reproduction scripts. Full 283-path parity and publication remain open.

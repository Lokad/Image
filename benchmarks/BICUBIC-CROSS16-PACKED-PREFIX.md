# Packed horizontal weights for tight RGBA16 bicubic

The output-backed row prefix packs horizontal coefficients once for each
proven contiguous eight-tap pair. Each cached source row then reads two
packed weight vectors per pair and runs the existing binary64 filter with
the same multiply/add order. On the original TightCross16 geometry, 124
pairs use the layout over 238 source-row fills. The shape model gives
472,192 original per-weight broadcasts and 59,024 candidate packed loads.
These are counts of operations in the two layouts.

The prefix reuses admitted weight storage and decoded scratch. Exact repair
restores each lane's original coefficients in scratch after horizontal readers
retire. Clipped or noncontiguous support keeps its ordered weights and the
strip tail retains its executor. A three-pixel-source regression covers the
short scratch guard. The existing 4 KiB working allowance and independently
owned result cover the same lifetimes.

## Complete operation results

| Original control on the guarded Release DLL | Lokad.Image µs | Prepared zimg µs | Conservative ratio interval | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| TightCross16, 513×259 → 256×519 | 810.34 | 820.55 | 0.982875–0.991723 | 113 |
| TightHorizontal16, 513×259 → 256×259 | 446.91 | 579.65 | 0.762764–0.778780 | 117 |

The first private build confirms **887.08 → 878.27 µs** over twelve
independent managed pairs, saving **8.81 µs** at **112 B/op** on both
sides. That build also passes both standalone cases. The guarded build above
uses the same original generated delegates, both native brackets and all
twelve managed actuals. Exact pixels, approved native bounds, source,
independent result ownership and idle-v2 guards pass. The reference retains
its prepared zimg graph and scratch.

The original 61-case inventory has two controls that use the changed
executor. The default versions choose the cache first. The two tight cases
are refreshed above and now stop. TightWide8 is the remaining uncertain
direct bicubic case.

## Qualification

Windows and Linux each pass 6,855 full tests including live FFmpeg. The
68 focused checks pass in each normal, AVX-512-disabled and scalar mode.
The new controls cover low alpha, odd tails, the tiny prefix and caller-root
ownership. Both consumers and all three usage examples pass through the
same actual development package, which contains the unchanged timed Linux
DLL. The initial Linux stage lacked its original compression fixtures.
Qualification uses the resolved fixture paths, with their original bytes.

[Evidence](bicubic-cross16-packed-prefix-evidence.json) retains both source
revisions, packages, input and output identities, raw timings and checks.

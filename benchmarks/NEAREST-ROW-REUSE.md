# Reusing completed nearest-resize rows

NRR1164 saves **110.756 µs / 10.35%** in independently controlled complete RGBA16
enlargement pairs on the final DLL: **1,070.582 → 959.826 µs**, ratio interval
**0.893792–0.899305**, unchanged **4,264,364 B/op**. The final standalone
comparison passes at **949.453 / FFmpeg 981.396 µs**, upper **0.981828**.

All four original direct-nearest controls and both affected complete PNG
composition controls pass and stop. Original inputs, reference settings, exact
pixels, ownership and operation boundaries are unchanged; no matrix was added.

| Complete original workload | Managed / reference µs | Conservative upper | Managed B/op |
| --- | ---: | ---: | ---: |
| Nearest enlargement RGBA16 | 949.453 / 981.396 | .981828 | 4,264,364 |
| Nearest enlargement RGBA8 | 198.808 / 994.775 | .201861 | 112 |
| Nearest reduction RGBA8 | 20.889 / 252.806 | .124180 | 112 |
| Nearest reduction RGBA16 | 19.351 / 170.561 | .121848 | 112 |
| PNG16 decode/orient/crop/fit | 2,772.041 / 2,937.903 | .951202 | 8,888.16 |
| PNG8 decode/orient/crop/fit | 1,185.369 / 1,475.389 | .807373 | 4,792.08 |

Direct cases retain the original 513×259 source, 1027×519 enlargement or 256×129
reduction, at the stated sample depth. Complete public calls, admission,
materialization and independent output disposal remain timed. The reference is
FFmpeg n8.1's original retained graph/source/geometry wrappers; native packing,
filtering and owned output acquisition/release remain timed. PNG pipelines retain
their documented native contexts while managed stream delivery, decode,
orientation, crop, fit and every intermediate/result/disposal stay included.

## Implementation and memory

The existing generic executor fills a destination row using the unchanged exact
integer center mapping. When the next row selects the same source Y, it copies
the preceding completely written output row instead of repeating horizontal
quotient/remainder calculations and gathers. Monotone source selection makes this
a normal enlargement strategy; no fixture value or dimension is special-cased.

Copies preserve complete uint/ulong pixels, including hidden RGB and every
16-bit sample. Both fill and copy retain a cancellation checkpoint before each
1024-pixel chunk. The original result owner is established before allocation;
failure abandons/disposes private output. There is no new array, scratch, plan,
cache, budget abstraction, pool-cap change, public API or numerical policy.

Total allocations and admitted/retained capacity are separate. Original Up16's
4,264,104-byte result remains above the unchanged 4 MiB idle cap and is not cached.
Its source/result payload is 5,327,040 bytes; this is not a physical-process peak.
Existing pooled byte/smaller outputs retain their original ownership policy.
No added owned or retained capacity follows from this change. Physical/native/JIT
peaks are unmeasured. Four new public tests cover irregular enlargement, mixed
axes and widths crossing multiple copy checkpoints at both depths, exact rational
pixels, aggregate refusal, retained independent outputs, cancellation cleanup and
successful reuse of a borrowed root.

## Measurement corrections

The original default-loader Up16 capture lacked a passing I/O guard. IGD1160–1162
then isolate untimed temporary-file writeback: the original setup/cleanup under
syscall/process/block-I/O observation reproduces post-run wait, while bounded
tmpfs temporary image files eliminate it with all original checks intact.
NCR1163 uses that explicit tooling adjustment and classifies the current path as
a gap: **1,053.504 / 982.643 µs**, interval **1.056779–1.081972**. Neither earlier
guard-rejected capture is upgraded or discarded. Thresholds and single-CPU
boundaries remain unchanged; file I/O was already excluded on both sides.

A preliminary two-AssemblyLoadContext screen has different resident state and
absolute costs. Its larger effect remains diagnostic, outside the production
gain claim. The subsequent ALC confirmation stops before any sample on a failed
preflight guard. The revised comparison uses one default-loader process per DLL,
one original source/native graph per process, and sequential CPU0 execution.
That screen saves 9.82%; independent twelve-pair confirmation saves 10.09%, before
the final DLL's separate confirmation above. Default-process costs and allocations
agree with the standalone boundary. All original actuals, aborted preflight and
both comparison implementations remain; no filtering, forced GC or favorable
replay is used. This resolves the working-state mismatch before promotion.

Cleanup removes 585,933,344 bytes of reconstructable private NuGet caches from
411 completed generated consumers on the nearly full VM disk. Each archive is
byte-verified against its retained feed; sources/configuration, feeds, newest two
consumers, release packages and cited raw evidence remain. Core/package hashes
are unchanged by cleanup. Free space rises from 455 MB to 1.068 GB.

## Qualification

Final Windows/Linux focused checks pass **534 internal / 204 public** per normal
and scalar mode, explicitly including PackedColumnStorageTests,
EnlargingStripBoundaryTests and EnlargementVerticalStorageTests. Windows/Linux
each pass **6,776 full/live checks** (3,316 internal, 2,512 public, 948 live).
The SAME actual Release package passes its consumer and all three usage examples
on both platforms. Published releases remain unchanged.

Windows module: `09b7165b60db3305be6330cdc439e556228e660b1ea2d4f9548cd9397722b5af`.
Timed Linux module: `e3f81889640ef8cc8bb7302f9e1805186bab9b86034f5170e8f73b695fbed0e9`.
Private 0.2.1 package:
`3167deb580ba05ecf4121a062515140e333964039576c30b73b89ba708b6aa38`
(469,162 bytes); symbols
`2a754a7e4c567d79eb2dfe054125c9372650c198e40204c60e3ed01a3170d77d`.
One final core/embedded build per OS; the newly built public test assembly is
reused on both platforms. Existing harness/reference/fixture builds are unchanged.
The final timed module is immutable. Encoder packets, other decoder/editing
contracts and unaffected font/drawing/resize paths retain their source-valid
evidence. This six-workload round does not finish the 289-path objective or the
later complete immutable-package refresh. Publication remains postponed.

[Evidence](nearest-row-reuse-evidence.json) pins current source, producers,
all costs, memory and qualification. Mirrored/member-verified raw
`nearest-row-reuse-qualified.tar.xz`, SHA-256
`a4cdf889d0c1ecda054e283e9436e9a452cebbb5181923638fcf97d1d6c25496`,
5,278,740 bytes / 275 members, retains actual package/both modules, source overlay,
prototype, default/ALC workers and every actual, guard, diagnostic and executable
recipe. Linked archives retain unchanged inputs, references and dependencies.
The summary also selects both already recorded shared-storage flip uncertainties,
without remeasuring them or using superseded costs as current passing evidence.

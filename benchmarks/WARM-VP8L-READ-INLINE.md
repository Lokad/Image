# Warm VP8L short-lookup inlining

One inlining hint reduces the retained complete warm VP8L decode mean by
12.6%, from 2.00x to
1.74x FFmpeg. The longer repeat's conservative
upper is 1.76x. Independent quiet
confirmation and other required warm cases remain open.

The [numeric evidence](warm-vp8l-read-inline-evidence.json) preserves raw samples,
uncertainty, source/output/tool identities, allocations and immutable archive
hashes. Regenerate with `python benchmarks/render-warm-vp8l-read-inline.py`.

| Stage | Managed mean, Âµs | Native before / after, Âµs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
| Current 12-sample baseline | 4312.04 | 2179.19 / 2154.22 | 2.0017x | 2.0662x |
| Inlining screen | 3763.12 | 2154.67 / 2159.96 | 1.7465x | 2.2821x |
| Inlining 12-sample repeat | 3767.14 | 2162.64 / 2158.83 | 1.7450x | 1.7604x |

Only `WebpBitReader.TryReadHuffman` gains `AggressiveInlining`. All 535 production
method bodies are byte-identical; its implementation flag is the only flag
change. Reader fields, logical bit work, cancellation, canonical lookup,
long-code/short-input/tight-budget fallbacks and partial failure state retain
their algorithm. No new allocation, table, cache or API is introduced.

One optimized-JIT inspection of the actual current baseline statistical DLL
shows a separate short-lookup call inside `WebpHuffman.Read`. The candidate's
actual statistical DLL removes that call, with Huffman Read code growing from
339 to 497 bytes; ReadImage remains 2,438 bytes. Both diagnostic pre/post quiet
checks pass. These inspections establish code shape, not CPU attribution or
parity timing. No CPU profile is taken.

All 37 existing Huffman controls pass before the edit on both OSes. They cover
uniform and mixed code lengths, cached single leaves, required byte consumption,
all starting bit phases, incomplete input, partial work budgets, long canonical
fallbacks and cancellation. All 54 focused VP8L controls pass with normal and
disabled intrinsics on both OSes. All 2,113 Release tests pass per OS: 533 internal,
686 public API and 894 live FFmpeg comparisons. The same actual dependency-free
Release package passes all three usage examples on both OSes. Actual short/long
statistical DLLs and fresh Linux production methods/flags match the package;
fresh Windows DLL bytes match it. Three fixed repeats of sixteen complete public
decodes/disposals allocate exactly the same bytes through the old/new actual
packages. Thirty existing original JPEG outputs also remain exact as package
correctness controls; they are not additional benchmark images. The initial
package evidence helper fails on a receipt-schema assumption after all three
examples pass. Corrected comparisons run alone; the failed job remains archived.
No timing or example repetition is needed.

The baseline is a new twelve-sample check of the existing case, at 2.0017x,
conservative upper 2.0662x. It is too close to the boundary to certify. The
candidate receives one short screen and one twelve-sample repeat. This uses one
original 513x259 RGBA8 raster and the same frozen 533,100-byte owned VP8L input,
with literal/run coding, no transforms, cache or spatial entropy groups. All
RGBA bytes, alpha and zero-alpha hidden RGB match the original source and the
pinned FFmpeg CLI. Independently owned outputs and source integrity remain
checked. No image generation, encoder-quality search or native-worker change
occurs.

Both subjects perform complete resident-input decode and owned output acquisition/
release. Native timing includes state reset, software WebP decode, ARGB-to-RGBA
conversion and output allocation/release. Codec/conversion contexts and packet/
frame wrappers are reused; every call decodes and converts every pixel. Padded
native rows are exposed directly. Startup, I/O, IPC, generation, encoding,
hashing and validation are excluded. Sequential Release measurement uses CPU0,
one codec/conversion thread, normal SIMD, disabled tiering and non-concurrent
workstation GC on the dedicated EPYC 9V45 VM, .NET 10.0.8 / SDK 10.0.204.

Statistical prequiet checks pass and postquiet checks fail under unchanged
thresholds. Observations and later cooldowns remain archived; independent quiet
passing confirmation is still required. This literal/run profile does not
establish uniform parity or performance on other VP8L strategies.

# Warm JPEG local entropy work accounting

Exact block-local work accounting lowers complete native-input JPEG decode screens to 1.9180x / 1.9008x
FFmpeg, from the retained 2.0550x screen. The owned-input affected screen is 1.7706x.
Native-input managed time falls about 7%. These narrow screens guide retention;
uniform warm parity remains open. No new benchmark image or quality search.

The [numeric evidence](warm-jpeg-local-work-evidence.json) retains actual samples,
confidence bounds, allocations, source/input/output identities, quietness failures
and immutable raw/source archive hashes. Regenerate this report with
`python benchmarks/render-warm-jpeg-local-work.py`.

| Stage | Case | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | --- | ---: | ---: | ---: | ---: |
| Work baseline | NativeDecode8 | 3539.85 | 1755.60 / 1722.52 | 2.0550x | 2.7364x |
| Block-local work | NativeDecode8 | 3297.62 | 1726.73 / 1719.27 | 1.9180x | 2.1872x |
| Block-local work | NativeDecode8 | 3272.95 | 1721.91 / 1726.16 | 1.9008x | 2.2459x |
| Historical owned | Decode8 | 3826.16 | 1899.11 / 1910.50 | 2.0147x | 2.5775x |
| Owned affected | Decode8 | 3371.71 | 1904.42 / 1904.25 | 1.7706x | 1.8467x |

One current native-input JPEG EventPipe profile selects the coefficient/entropy
loop: 46.4% inclusive, with a 26.9% visible baseline-block leaf. ReadScan accounts
for 63.5% inclusive, inverse transformation 19.4%, bit reads 12.1%, short Huffman
reads 6.5%. Inclusive methods overlap and cannot be added. Workload loops include
warmup/calibration; visible leaves may contain inlined work. This is CPU attribution,
not parity timing. Its actual profiler child DLL and both correctness receipts
match the accepted package in all production method bodies and implementation flags.

After the original 64-unit baseline block charge, a noncancelable reader snapshots
remaining context work. Each entropy read/Huffman admission checks and subtracts
its original exact atomic cost locally. A finally block publishes precisely the
successful charges on every exit, before any further parent-context work. No work
is charged ahead. Failed whole-code admissions retain the original per-bit fallback
and partial consumption. No other parent-context charge occurs inside that scope.
Cancellable readers retain their original per-call checks. Progressive traversal
and restart handling keep their original context work. Two local stack integers
replace repeated shared tally writes; no successful-path heap allocation, buffer
or cache is added. Huffman tables, bit refill, markers, cursor, predictor, partial
coefficient writes, memory/output admission, pixels and quality remain unchanged.

The three new cases pass against both original and candidate code. They start
after eleven units of parent work, then truncate a category-five amplitude after
four payload bits. They verify the admitted read charge before failing refill,
tight-budget limit/format precedence, exact pending-work publication and reader
reuse after failure. Both default and cancellable tokens are exercised. All 41
focused entropy/Huffman/baseline controls pass on Windows and Linux.

Both frozen JPEGs come from the same existing 513x259 RGBA8 raster. The native
input uses 1x2 sampling factors for all components, still full 4:4:4; the owned
input uses 1x1. All encoded input and decoded output hashes stay exact, including
opaque alpha and the existing decoder maximum/RMSE agreement. Native output also
matches the pinned public CLI. The owned historical baseline predates Work inlining;
do not attribute its whole reduction to this new primitive.

Complete public span decode, output acquisition and owned disposal are timed.
Native state reset, full-range BT.601 conversion and independent owned output
allocation/release are included. Reused native context/conversion/packet/frame
wrappers are declared; every call decodes and converts all pixels. Startup, IPC,
I/O, generation, hashing, validation and tight-row transport writes are excluded.
Sequential Release jobs use CPU 0, one codec/conversion thread, normal SIMD,
.NET 10.0.8 / SDK 10.0.204, disabled tiering and non-concurrent workstation GC
on the dedicated AMD EPYC 9V45 VM. All statistical prechecks pass; postchecks fail
and remain recorded, together with cooldown failures. These screens do not
certify <=2x passing status.

BDN allocations and all actual samples remain in the numeric evidence. A separate
fixed-count control uses complete public Stream decode with fresh caller streams
and owned disposal through both actual packages. All sixteen per-operation counts
and aggregate counts match exactly, and output hashes remain equal. Caller-stream
ownership is checked; reflection argument storage is reused equally. This control
makes no timing claim and is not a span-specific allocation comparison.

All 2,047 tests pass on Windows and Linux: 467 internal, 686 public and 894 live
FFmpeg comparisons. The same actual dependency-free Release package passes all
three usage examples on both platforms. All actual statistical libraries match
that package in every production method body and implementation flag. SHA256:
`ae100c7958f2d27369289fae67bbca9cc17f68feec796efb0dc212da3d901455`.

The historical VP8L decode gap near 2.05x needs a current recheck before reranking.
PNG8 remains near 2.037x, JPEG encode 2.021x, PNG16 near 2x. No unrelated codec
gain is inferred. Unknown, unmatched and uncovered required directions remain open.

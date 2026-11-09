# Shared byte/word bicubic columns

The subsequent [shared byte repair](BICUBIC-SHARED-BYTE-REPAIR.md) owns the current
small-byte cost. The unchanged small-word path retains this report's evidence;
earlier byte observations and adverse results remain history.

CWS659 replaces two small-support column drivers with one preparation, ownership
and row scheduler. Constant pixel-depth/strip specializations keep typed kernels;
word pixels gain the existing16-column result-backed prefix and retain their
8/6-column suffix. Production source falls493→395 lines. The duplicate word driver
is deleted. No forwarding wrapper, public API, pool, session or owner is added.

The complete frozen513×259→256×129 RGBA16 resize saves **98.59 µs /14.30% raw /
14.50% normalized**, with separated filtered/all-actual raw/normalized confidence.
Both standalone costs use Windows-produced artifacts and the identical original
input/settings/native boundary. The contemporary original is689.43 µs; the older
751.02 µs point would overstate this gain. Word parity remains an18.03 µs gap.

| Complete input+output+4 KiB control | Managed µs | zimg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA16 original31bc1b42 | 689.43 | 571.48 | 1.206 /1.218 | 113 |
| RGBA16 selected795f3a0f | 590.84 | 572.81 | 1.031 /1.033 | 113 |
| RGBA8 original31bc1b42 | 541.82 | 417.96 | 1.296 /1.301 | 113 |
| RGBA8 selected795f3a0f | 574.56 | 422.95 | 1.358 /1.410 | 113 |

The byte confirmation is **32.74 µs /6.04% slower in raw time**. Its native brackets
move422.95→445.90 µs; normalized intervals1.268–1.410 overlap the original1.289–1.301.
The incremental byte effect remains inconclusive. Its535.70 µs short is retained,
and the adverse confirmation supplies the current summary. There is no byte
gain/no-regression claim or uniform improvement claim. Acceptance retains the
verified word gain and consolidation, with byte attribution explicitly next.
Both cases remain parity gaps; uncertain incremental effects are not retuned by
repeatedly sampling unchanged code for favorable clocks.

Each original/selected confirmation has twelve actuals; original word/byte retain
10/11 filtered results and their all-actual views. Both selected confirmations
retain all twelve. Native brackets and idle-v2 guards pass. There is exactly one
three-actual screen and one independent selected confirmation per control, plus
one matched contemporary original confirmation to resolve artifact/clock effects.
No completed job repeats. The word short is590.66 µs.

Word unsigned products, each half's original tap Multiply/Add order, normalizers,
pair publication certificate and original repair remain exact. Byte horizontal
arithmetic, reciprocal certificate and repair remain unchanged. Six added word
layout controls exercise the frozen/second shape, exact2.25 boundary, smaller
prefix, unaligned stride and short output. Word refusal, cancellation, poisoned
output, tiny-alpha unwind/replay and aggregate surviving-result checks are added.

The16-column prefix borrows4608 B of cache in unpublished result-row tails;
129 geometry footers borrow12384 B at disjoint locations. Only one3232/4080 B
array payload is live under one4096 B reservation. Prefix frames retire before
suffix publication and exact replay. Total allocation stays113 B/op; new owned
capacity/retained scratch are zero. Retained word/byte outputs own264192/132096 B
until disposal. Aggregate test ceilings are532480/268288 B for two results plus
scratch, with the source borrowed separately. These are admitted-capacity bounds,
not physical/native peaks or measurements of existing idle pixel-pool capacity.

Timing includes complete public Resize, preparation/normalization/workspace,
independently owned output and disposal. Explicitly labeled zimg, FFmpeg's scaler
backend, retains graph/scratch before timing; halo/deinterleaving, processing,
quantization and independent output remain timed. This is not fresh graph setup
parity. Exact managed/frozen/rational pixels and native max1/RMSE≤0.25 stay intact.

Windows795f3a0f/Linux70c78791 each pass **5994 full/live checks**,215 internal/
703 public per normal/scalar/disabled-AVX512/disabled-AVX2 mode. The SAME actual
private Release0.2.1 package0161df80 (436726 B) passes both OS consumers and all
three examples, including frozen byte/word column admission, refusal, cancellation
and independent survivors. One main/embedded build per OS is reused throughout;
no public/harness/native/selector builds or new timing fixtures. Publication stays
postponed, and the full283-path objective remains open.

[Structured evidence](bicubic-shared-byte-word-columns-evidence.json) pins complete
costs, adverse observations, source proof, qualification and mirrored/member-verified
raw bicubic-shared-byte-word-columns-qualified.tar.xz (13045064 B/297 members,
SHA256 c9e6d9e7e956f9e88403f5c16892980bfa4e1dcf310d895b69910402c9013eff).
The retained [byte](BICUBIC-STRIDED-COLUMN-CACHE.md) and
[word](BICUBIC-WORD-COLUMN-PREPARATION.md) stages are history, not current costs.
CWA658 source/model/profile evidence supports this design; its work counts are
not stage timings or predicted gains. Wider/packed/deep/default-cache, Lanczos,
codec/text/compositing paths reuse unchanged-path evidence.

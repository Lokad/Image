# Bounded lossless WebP color-cache encoding

The fixed photograph shrinks **383,154→295,936 bytes (22.8%)** and text/diagram
**15,120→7,636 bytes (49.5%)**, preserving every sample. Both still exceed their
size ceilings,160,032/7,234 B. All six affected complete operations independently
pass warm single-CPU parity, including the conservative all-actual confidence view.

| Complete encode | Lokad / reference µs | Ratio / conservative upper | Lokad allocated B/op | Encoded B, Lokad / reference |
| --- | ---: | ---: | ---: | ---: |
| Original RGBA8 | 14,359.09 / 121,634.08 | 0.118 / 0.119 | 625,240 | 252,476 / 183,812 |
| Original RGBA16 →8-bit | 17,347.00 / 121,489.79 | 0.143 / 0.144 | 625,240 | 252,476 / 183,812 |
| Original opaque RGBA8 | 12,490.98 / 70,362.52 | 0.178 / 0.179 | 614,408 | 170,156 / 121,896 |
| Photograph RGBA8 | 15,930.72 / 82,818.25 | 0.192 / 0.195 | 1,140,030 | 295,936 / 160,032 |
| Text/diagram RGBA8 | 6,652.38 / 9,501.86 | 0.700 / 0.715 | 107,568 | 7,636 / 7,234 |
| Original RGBA8, async nonseekable output | 14,798.74 / 121,007.07 | 0.122 / 0.123 | 691,224 | 252,476 / 183,812 |

The reference remains **libwebp exact=1, FFmpeg's encoder backend**, method4,
lossless quality75. Source rasters, opacity projection,16→8 normalization, source/
alpha/hidden-RGB equality and settings are unchanged. The two content controls
remain the only photograph/diagram inputs. Native allocations are unmeasured.
All times include public admission, source processing, fresh independently owned
complete output and release. Async adds staging and immediate nonseekable delivery
on both sides. Startup, IPC, preparation and validation stay outside both timings.

Compression costs more CPU: photograph/diagram means are15.93/6.65ms versus retained
9.28/2.71ms. Their allocations also rise, to1,140,030/107,568B per operation.
Ordinary RGBA8/16 output instead falls533,100→252,476B, opaque400,262→170,156B;
smaller stream growth cuts their total allocations. These are explicit size/time/
allocation tradeoffs within the existing parity requirement, not speedup claims.

The independent RFC9649 implementation adds green decorrelation, a1,024-entry
color cache, frequency-derived complete canonical trees and code-length repeats.
It retains existing repeated-color/previous-row references; no general hashchain,
spatial prediction, entropy groups or frame copy. Effort>=3 and>=16,384pixels use
this path only when the configured LOCAL allowance has64KiB remaining after
borrowed source storage. Other cases retain the8KiB path; effort0 stays literal-only.
Selection precedes shared admission. Refusal never retries or selects another path.
Actual retained tree/cache capacity and temporary histogram/construction/header
capacity are reserved before allocation through the same borrowed context.
Temporary charges end promptly; cache/trees remain charged through actual use.
Three resident-source passes gather frequencies, count and write, with bounded
work/cancellation and complete RIFF/padding preflight before caller output.

Six three-actual screens and six independent12-actual confirmations retain all90
actuals, both native brackets and both official idle-v2 guards per run. One frozen
production build per OS is reused throughout; one necessary harness update binds
async packet equality to the independently verified prepared packet instead of an
old compression hash. All selectors/native workers are reused. Async's completed
short is recovered after a receipt-field postprocessing error, with no repeated job.
No image/effort/quality grid, unchanged-case retuning or new benchmark boundary.

All **4,509tests/OS** (1,678internal,1,899public,932live FFmpeg),18prefix+105public
focused checks normally and with intrinsics off perOS, and the **same actual private
0.2.0 package** consumer/all three examples pass on Windows/Linux. Checks cover
canonical completeness/lengths, sparse/skewed trees and code-length repeats,
cache zero-init/collisions/overwrite/copies across rows and overlap,8/16hiddenRGBA,
independent owners, low-local-memory selection, aggregate root refusal without
fallback, cancellation/work/output cleanup and subsequent success. Two test-only
assumptions were corrected before qualification; failure logs remain retained.
No unsafe code, native binding, production dependency or imported upstream table.

Precisely five synchronous lossless rows and the existing async lossless row are
replaced. Frozen VP8L decode inputs stay unchanged. All other codec/edit/inspection/
drawing/text paths retain identified evidence; VP8 packets reproduce their frozen
source-quality anchors exactly without retuning. The finite254-case scope remains;
45current/193historical,72passes/165mean gaps/1confidence-open,125independent
observations/113qualified shorts. Published0.1.0 is immutable; size parity, uniform
warm parity and replacement readiness remain open.

Next: spatial prediction on the same photograph, whose remaining size ratio1.849
is larger than the diagram's1.056. Preserve exact samples and admission, then use
the existing short/independent cadence. Naive hashchain and unchanged cost-parse
trials stay stopped. Claims here apply only to the named content/depth/settings.

[Evidence](warm-vp8l-color-cache-evidence.json) records all actuals, allocations,
identities and mirrored raw hashes. Raw:`c34-measurements.tar.gz`,
`c34-qualified-windows.tar.gz`,`c34-qualified-linux.tar.gz`. The actual package
SHA256 is`b2b0bd5d2663827e6a3de791fa709955c05732ce595fbea5fb2f116119770a75`.

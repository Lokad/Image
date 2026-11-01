# Exact JPEG vertical chroma rows

Caller-RGB420 independently uses **11.80% less elapsed time** (11.77% all-actual,
**11.95% native-normalized**), with separated raw and normalized confidence.
It remains a gap at **1.047× / upper 1.061**. Its allocation is **388,220 B/op**;
the 2 B/op difference from the original is GC reporting granularity. Complete
packets remain **52,091 / 52,093 bytes**, differing only by approved COM attribution.

| Existing complete 513×259 encode | Lokad µs | Reference µs | Ratio / upper | Managed B/op | Packet B, Lokad/native | Status |
|---|---:|---:|---:|---:|---:|---|
| Caller RGB24, resident420 | 1,655.50 | 1,580.59 | 1.047 / 1.061 | 388,220 | 52,091 / 52,093 | gap |
| RGBA8,422 | 2,103.93 | 2,348.75 | 0.896 / 0.903 | 529,872 | 71,166 / 71,168 | pass-stop |
| RGBA8,420/restart33 | 2,089.39 | 1,615.45 | 1.293 / 1.314 | 151,179 | 58,010 / 58,012 | gap |
| RGBA8,forward420 | 1,638.05 | 1,764.17 | 0.929 / 0.936 | 388,246 | 52,091 / 52,093 | pass-stop |
| RGBA16,422 | 2,161.28 | 2,426.45 | 0.891 / 0.910 | 529,872 | 71,208 / 71,210 | pass-stop |
| RGBA16,forward420 | 1,712.53 | 1,839.06 | 0.931 / 0.936 | 388,246 | 52,115 / 52,117 | pass-stop |
| RGBA16,420/live token | 1,727.97 | 1,867.85 | 0.925 / 0.951 | 388,214 | 52,115 / 52,117 | pass-stop |
| RGBA8,420 | 1,642.34 | 1,766.38 | 0.930 / 0.941 | 388,214 | 52,091 / 52,093 | pass-stop |
| RGBA16,420 | 1,714.83 | 1,845.18 | 0.929 / 0.946 | 388,214 | 52,115 / 52,117 | pass-stop |
| RGBA16,420/sharedroot | 1,876.82 | 1,835.59 | 1.022 / 1.038 | 819,488 | 52,115 / 52,117 | confidence-open |
| CallerRGB24,async nonseekable420 | 1,654.09 | 1,635.82 | 1.011 / 1.026 | 309,524 | 52,091 / 52,093 | confidence-open |

These are eleven existing required boundaries, with frozen inputs, Q85/native
qscale5, sampling, flattening and exact coded content. Complete operations include
normalization/conversion, encoding, fresh independent output acquisition and
release; async additionally includes staging/delivery and actual users. One CPU,
normal native SIMD, idle-v2 guards and all 12 actuals per confirmation are retained.
Native allocations are unmeasured. Equal numerical quality values do not establish
equivalence. Only the caller primary has a paired speedup claim. Passing cases stop.

One eight-pair AVX2 vertical filter replaces eight scalar calls. Signed widening,
ordered Q12 products, bias, arithmetic shift and final saturation preserve every
output byte. The original scalar arithmetic remains the fallback. Source loads
and output stores borrow existing MCU storage; no buffer, cache, owner, charge or
cancellation boundary changes. Q12 weights sum to 4096; full signed-short products
fit Int32. An independent Int64 oracle covers phases, signed extrema, clipping,
unaligned/guarded spans and zero allocation. All original frozen packets remain.

Fresh **5,354 checks per OS** (2,172 internal, 2,234 public, 948 live), focused
4 internal/160 public checks normal/scalar per OS and the **same actual Release
package/three examples on Windows/Linux** pass. One core/internal build per OS;
zero public/differential/harness/native/selector builds. One primary 3/12 and one
12 per existing companion;
no new raster/settings grid, repeated pair or unrelated qualification.

The reused [original caller confirmation](WARM-JPEG-COLUMN-PLANS.md) remains
applicable: its raw-sample subsampled encoder path is unchanged by subsequent
word normalization, 444 encoding, decoding and editing changes. Other affected older
costs are superseded by this table. Gray/444 encoders, all decoders and unrelated
codecs/editing/text retain applicable unchanged evidence. The full 268-path warm
parity objective remains open; these controls do not establish broader parity.

[Evidence](jpeg-vertical-chroma-row-evidence.json). Mirrored/member-verified
jpeg-vertical-chroma-row-qualified.tar.xz, 329 members, SHA256 5f18050615ddf30758e6af225b77ade98b54a5a9f4efa3af0dd98adb5e482b10.
Raw artifacts retain the two-second actual-IP/JIT diagnostic (instrumented times
discarded), original/candidate DLLs, selectors, workers, inputs, all actuals/guards,
both-platform tests and actual packages. Dependencies are pinned in the evidence.

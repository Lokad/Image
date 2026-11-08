# Prepared JPEG Huffman replay

JHB622 prepares one bounded family/symbol lookup during entropy replay. The
complete RGBA8→grayscale control saves **151.45µs: 9.43% raw and 10.09%
reference-normalized**, with separated filtered/all-actual confidence intervals.
The contemporary original is1,606.75µs; current1,455.31µs. The labeled SIMD
libjpeg-turbo point moves+0.73%. Exact58,910B packets and155,436B/op remain.
One short precedes the independent original/candidate confirmations. Historical
native clocks differ materially, so the incremental claim uses that current pair.

The source has one real replay body. It prepares a1,024-entry uint bank from the
private generated Huffman tables, then reads code/length together with one masked
family/symbol index. Candidate preparation stays timed. Record generation, order,
optimal tables, the writer, validation, cancellation, stuffing, flushing and output
ownership are unchanged. No test-only prior production body or compatibility
wrapper remains. Nonempty internal replay requires four table slots and full
code/length arrays; unused grayscale null slots remain valid. A selected missing
code fails as image data; empty replay remains a no-op.

Bank payload is4KiB on the stack under the existing16KiB table owner. Retained
table payload<=4,160B, conservative metadata1,600B and bank total9,856B; separate
builder graph scratch has ended. No heap capacity, owner, pool, cache or escaping
state is added. The governed RGBA16 peak stays1,704,856B. Existing coefficient
pool retention is unchanged:4MiB ceiling, three normal slots/four after full-CMYK
use; shared-budget operations bypass it. Bank retention is zero. GC allocation,
admitted capacity and retained storage are separate; physical/native peaks are
unmeasured. Small observed GC rounding differences are not causal allocation gains.

All17 existing affected boundaries have twelve actuals and native samples per
bracket, exact frozen inputs/settings/packets and quiet idle-v2/process guards.
Color references are FFmpeg n8.1; the two grayscale rows use explicitly labeled
SIMD-enabled standalone libjpeg-turbo3.1.4.1, tooling only. Color packets differ
solely by the declared strict2B attribution exception; grayscale is raw-exact.
Quality numbers are not the equivalence proof. Complete normalization, preparation,
admission, encoding, independent output acquisition/growth/staging and release
remain timed. Native contexts/private scratch retain their declared boundaries.

| Existing complete513×259 control | Managed / reference µs | Conservative upper | B/op | Packet B, managed / reference | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| RGBA8 → grayscale / SIMD libjpeg-turbo | 1,455.31 / 1,409.83 | 1.042 | 155,436 | 58,910 / 58,910 | Gap |
| Caller GRAY8 / SIMD libjpeg-turbo | 1,061.94 / 1,076.74 | 0.994 | 155,438 | 58,910 / 58,910 | Pass |
| Caller RGB24 → 420 | 1,584.61 / 1,578.98 | 1.009 | 388,220 | 52,091 / 52,093 | Uncertain |
| RGBA8 → 420 | 1,722.54 / 1,764.37 | 0.982 | 388,214 | 52,091 / 52,093 | Pass |
| RGBA16 → 420 | 1,628.28 / 1,836.50 | 0.892 | 388,214 | 52,115 / 52,117 | Pass |
| RGBA8 → 422 | 2,001.60 / 2,347.23 | 0.858 | 529,871 | 71,166 / 71,168 | Pass |
| RGBA16 → 422 | 2,073.46 / 2,422.37 | 0.859 | 529,872 | 71,208 / 71,210 | Pass |
| 420, forward output8 | 1,574.51 / 1,767.75 | 0.895 | 388,246 | 52,091 / 52,093 | Pass |
| 420, forward output16 | 1,647.15 / 1,842.72 | 0.899 | 388,246 | 52,115 / 52,117 | Pass |
| 420, live-token16 | 1,667.79 / 1,841.78 | 0.911 | 388,214 | 52,115 / 52,117 | Pass |
| 420, shared admission16 | 1,706.53 / 1,836.80 | 0.934 | 819,475 | 52,115 / 52,117 | Pass |
| Caller RGB24, async nonseekable420 | 1,581.70 / 1,629.27 | 0.979 | 309,524 | 52,091 / 52,093 | Pass |
| RGBA8 → 444 | 3,520.11 / 3,766.85 | 0.941 | 820,088 | 133,039 / 133,041 | Pass |
| RGBA16 → 444 | 3,604.85 / 3,837.48 | 0.946 | 820,088 | 133,041 / 133,043 | Pass |
| 444, live-token8 | 3,517.80 / 3,775.61 | 0.939 | 820,088 | 133,039 / 133,041 | Pass |
| 444, forward output8 | 3,492.70 / 3,776.95 | 0.931 | 820,120 | 133,039 / 133,041 | Pass |
| RGBA8, async nonseekable444 | 3,517.84 / 3,856.40 | 0.918 | 820,531 | 133,039 / 133,041 | Pass |

**15pass/1gap/1uncertain.** RGBA→gray remains about45µs behind its reference;
callerRGB420 is uncertain, not a demonstrated optimization gap. Stop passing
cases. Seven displayed costs refresh;119 reuse unchanged providers. Other points
are current standalone evidence, without separate paired causal gain claims.
Restart/fixed-table and tight-memory recomputation never call replay and reuse
source-valid evidence. Decoder/PNG/WebP/resize/text paths are unchanged.

Each OS passes **5,943 full/live checks**, plus **424internal/375public JPEG checks
per normal/scalar and disabledAVX2/AVX512 mode**. New actual-C# bit packing checks
114,688 records across families/widths/MCU edges; the frozen28,672-block corpus
retains946,249 records and its digest. Empty/disposed/canceled input, missing
grayscale entries, published failure prefixes and owned cleanup pass. The SAME
actual Release package and all three examples pass Windows/Linux. One core perOS,
two Windows embedded versions and one Linux embedded version serve the round:
one test constructor was canceled too early, corrected without rebuilding core.

Completed async jobs expose different receipt schemas. Missing optional affinity/
GC fields and a scalar quality field are handled using pinned execution settings
and original gates. The initial recovery helper needed the existing environment
sourced. Completed logs/guards/fixture restoration are recovered without rerunning
either measurement or qualification. All adverse/setup results remain in raw.

[Evidence](jpeg-prepared-huffman-bank-evidence.json) pins current costs, source
proof, all actuals/guards, reused workers/fixtures, package and qualification.
Raw `jpeg-prepared-huffman-bank-qualified.tar.xz`, SHA256
`14f0e8ed1f88d9714ef10b919fceff40e64c41395834bae39f3fda144f836776`,
15,360,056B/457members, is mirrored/member-verified. It retains original/candidate
modules, corrected/failed tests, scripts and every unique measured-runtime input.
Historical stages remain linked dependencies; the current renderer replaces six
superseded JPEG encoder dependencies with this owner. Publication, full283-path
parity and the full126-row actual-package performance refresh remain open.

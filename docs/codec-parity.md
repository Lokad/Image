# Codec byte compatibility

The qualification target is byte-exact encoder output, followed by exact decoder
samples, against pinned FFmpeg builds and explicit settings. CPU/allocation tuning
follows correctness. JPEG uses one managed baseline encoder in0.2.0 development.
The superseded Original/CompactColor branches, option switches and experimental
forward-transform machinery are removed; historical size/quality results remain
producer-specific evidence, not compatibility obligations.

Only identified encoder-attribution metadata may differ. For the current JPEG
reference, its single `Lavc62.28.100\0` COM payload becomes `Lokad.Image\0`,
with its length updated. Every other byte must match. JFIF, ICC, EXIF, orientation,
color information, quantization, entropy and extra comments are not exempt.
Other codecs need their own explicit attribution inventory.

The managed JPEG encoder reproduces the frozen
**513×259 RGBA8/16** controls at **4:4:4, 4:2:2 and 4:2:0** completely. Encoding receives source samples and options;
it reads no oracle packet or table. It generates color
samples, integer transforms, quantization, optimal Huffman tables, headers and
the complete scan, including all **195 vertical padding blocks per 4:4:4 control**.

The pinned oracle is FFmpeg n8.1, revision
`9047fa1b084f76b1b4d065af2d743df1b40dfb56`: single-thread MJPEG, qscale 5,
optimal Huffman, full-range BT.601 4:4:4, baseline/no restart, 0x204060 flattening.
The test-only prototype with native attribution reproduces its exact
**133,041/133,043 B** hashes. Production Lokad.Image
attribution produces **133,039/133,041 B**, with only the approved COM difference.
All these checks pass on Windows/Linux, preserving inputs and surviving outputs.
The Huffman generator additionally matches **32 original bounded histograms**,
including ties, sparse alphabets and length limiting; changed tables are rejected.

The quantization generator uses normative format data from
[H.262 §6.3.11](https://www.itu.int/rec/dologin_pub.asp?id=T-REC-H.262-200002-S%21%21PDF-E&lang=s&type=items).
Transform constants are generated mathematically. No upstream implementation,
implementation table or corpus is incorporated.
[Numeric evidence](../benchmarks/jpeg-byte-parity-evidence.json) identifies the
sources, binaries, earlier exact stages and mirrored raw results.

Production also reproduces the four existing **4:2:2/4:2:0, RGBA8/16** controls
on Windows/Linux: **71,168/71,210 B** and **52,093/52,117 B**
with native attribution, two bytes less with Lokad.Image attribution. All **931,360**
submitted color samples per OS match exactly. Six small functional controls also
qualify even-width RGB pairing, tiny one/two/three-cell filters and partial MCUs.
These reuse the original procedural recipe and do not broaden content claims.

The existing **4:2:0 RGBA8 row-restart33** control also matches: **58,012 B** with
native attribution, **58,010 B** with the approved COM replacement, including all
16 sequenced restart markers. The native reference uses two **serial** slice
contexts on one codec thread; these force fixed tables despite its requested
optimal setting. Lokad.Image independently generates those optional format
examples from [T.81 K.3.3](https://www.w3.org/Graphics/JPEG/itu-t81.pdf#page=162).
No oracle packet or coding table is an encoder input.
At18×19, non-row intervals2/3 have functional checks for444/422 from RGBA8 and
420 from RGBA16, plus native-decoder interoperability checks for all three from RGBA8.
Those checks establish no additional native-packet or exact-decoder claim.

The production **grayscale Reference profile** also reproduces its existing
**58,910 B** packet exactly, using the approved SIMD libjpeg-turbo3.1.4.1 reference,
quality69 and the existing owned quantizers. Its **137,280 raw transforms** and
**137,280 quantized coefficients** match; the native scalar/SIMD transforms agree.
The independent [T.81 K.2](https://www.w3.org/Graphics/JPEG/itu-t81.pdf#page=148)
table prototype matches **34 original histograms**, including length limiting;
production tests also freeze its complete packet and uniform-alphabet table.
No metadata exception is needed: the grayscale reference has no attribution COM.
The existing RGBA16 control also matches its complete **58,942 B** packet after
independent16-bit normalization. The RGBA8 grayscale control with explicit MCU
restart interval3 matches **61,152 B**, including714 sequenced markers and
optimized tables. These checks use the same backend, quality and quantizers.

The production encoder uses a 512-byte MCU and a bounded18×18 packed tile, reused
from normalized RGB through signed horizontal chroma. Negative guard samples
survive until vertical rounding; two original yellow/cyan controls check this
boundary against complete native packets. There is no full-frame YUV or coefficient
copy. Plans and scratch share the existing8-KiB owner. SSE2/AVX2 forward transforms
and quantization retain exact scalar results and fallbacks. Optimal tables use two passes;
color restarts use fixed tables in one pass. Grayscale uses a single 64-byte
block. RGBA8/16 images and caller Gray8/RGB8 spans with stride support sync,
forward-only and async delivery. Color quality maps to native qscale
`1 + (100 - Quality) * 30 / 99`; default85 maps to the frozen qscale5 reference.
Grayscale uses its existing owned quantization formula and the same quality as its
pinned backend, with69 frozen above. Caller Gray8 overrides Sampling and needs no
RGBA intermediate. Grayscale packet claims cover the two declared source depths
without restarts, plus the8-bit interval3 control. Other intervals have functional
coverage; quality endpoints do not imply a broader native-packet claim.

Shared admission reserves fixed/table capacity and coarse table-building peaks
before allocation. Temporary storage is released before async delivery; encoded
chunks remain charged until the actual writer finishes, including cancellation
and failure. Inputs and the shared root are borrowed. Existing local/work limits
remain independent; refusal has no fallback. Other settings/scopes need separate
qualification. These procedural controls establish no general compression claim.
Current JPEG timing/allocation scope and older producer-specific observations are
identified in the [benchmark state](benchmark.md); full CPU parity remains open.

All four PNG compression policies use original managed streaming Deflate:
stored level0, greedy level1, lazy level6 and longer lazy level9. The existing
four color types at8/16bits qualify Optimal; RGBA8/16 also qualify each other
policy. All14 **513×259** controls match complete frozen FFmpeg packets raw-exact,
including pHYs,4096-byte IDAT chunks, checksums and entropy. No metadata exemption
is used. Required async, forward, live-token and ImageBudget boundaries retain
those packets; no quality search or new reference setting is involved.

One bounded history/dictionary/token-block workspace is reserved before allocation
through the borrowed operation context. Stored blocks keep scanlines whole at the
flush threshold. Refusal, cancellation and writer failure release workspace without
fallback or cleanup writes. Async output retains its separate owner until actual
writes finish. [Numeric evidence](../benchmarks/png-byte-parity-evidence.json)
records exact tested scope; current CPU/allocation qualification remains open.
The initial RGBA8 level6 prototype retains111,511token,21tree and1549code-length
run comparisons through independent RFC1950/1951 observers.

In `tools/WarmReference`, `verify-jpeg-scan-stage.ps1` checks the independent
complete encoder and its supplied-table stage; `verify-jpeg-huffman-stage.ps1`
checks the table generator. The color/forward-stage verifiers retain intermediate
checks. `compare-jpeg-packets.py ACTUAL REFERENCE --allow-encoder-attribution`
enables only the exception above, reporting raw equality separately. Its boundary
checks run with `python -B tools/WarmReference/test-jpeg-packet-comparison.py`.
`verify-jpeg-subsampling-stage.ps1` checks the source-only 4:2:2/4:2:0 prototype.
`verify-jpeg-gray-stage.ps1` checks the grayscale stages, tables and complete packet.

The managed VP8L encoder independently selects transforms, palette ordering and
packing, bounded matches, cache/references, spatial groups and canonical trees.
The pinned libwebp1.6.0 backend reference uses quality75, matching method, exact=1
and one CPU. The frozen inventory contains19 selected packet identities across25 existing
input/settings checks, plus two additional forced spatial/box candidate identities.
The total is21 distinct complete packets. No WebP metadata
exception is used. Source pixels and options are encoder inputs; reference bytes
and observed tables are verification data only.

The scope includes the513×259 photograph/diagram at160,032/7,234 bytes, asymmetric
RGBA8 across efforts0/1/4/5, adaptive photograph effort5, direct noise, hidden RGB,
packed palettes, single-axis16383 controls and all existing two-color effort settings. Complete public sync,
async and ImageBudget boundaries retain the asymmetric packet at8/16bits, with
one normalization and independent owners. The16384-axis companions remain functional
format extensions; the pinned native encoder accepts at most16383, so those axes
make no encoder-byte claim. Exact-decoder and new single-CPU/allocation
qualification remain open. [Numeric evidence](../benchmarks/vp8l-byte-parity-evidence.json)
records source/settings and qualification scope.

The production VP8 encoder now uses the qualified managed input/analysis/RDO,
alpha and arithmetic-partition pipeline. Twenty frozen513×259 identities span22
public rows, including all seven efforts, quality1/75/85/100 named policies,
photograph/diagram and default-method RGBA8/16 normalization. Complete reference
photograph/diagram packets are17,688/7,100 bytes; the method6 alpha control is
85,796 bytes with an11,667-byte ALPH payload. The separate5,632-square opaque
header case has one retry and an exact16,787,132-byte packet. No metadata exemption
is used. These claims identify tested scope, without an exhaustive option grid.

The superseded encoder and its internal experimental profiles are removed.
Admitted planes, frame state and old/new arithmetic capacities share the borrowed
root. Source/frame storage releases before delivery; alpha/partitions remain owned
until actual sync/async users finish. Lossy async streams need no extra encoded
staging copy. Refusal, cancellation and actual work exhaustion terminate without
another strategy. Small frozen header fields provide a fast regression for the
large case. [VP8 evidence](../benchmarks/vp8-byte-parity-evidence.json) preserves
primitive, complete-packet, producer and resource scope. The high-effort positive
alpha proof uses an explicit64B work allowance; default2B and public32M failures
remain separate safeguards. Work counters are not timing/allocation measurements.

Lossless WebP retains the approved libwebp `exact=1` reference; grayscale JPEG
retains the explicitly labeled SIMD-enabled libjpeg-turbo reference. New encoding
checks compare exact packets, retaining original decoder corpora and minimal
interoperability/alpha sanity tests. Historical quality/timing evidence keeps its
recorded scope; exact decoding and CPU/allocation qualification remain open.

JPEG decoding uses the independently generated fixed-point inverse for all
blocks. All30 existing frozen grayscale/4:4:4/4:2:2/4:2:0 controls have exact decoded
samples, including odd edges, progressive refinement, EOB/split scans and restarts.
The three31x23 baseline anchors also have exact array/sync/async stream output,
caller gray/RGB, padding and opt-in orientation. Existing live controls require
exact samples; the three anchors and signed refinement pair also check normal
native execution. Named CMYK/YCCK constant, varying, progressive, saturated and
AC controls also require exact samples. The two65x31 varying controls cover
RGB/grayscale, streams, caller padding and orientation. CMYK output preserves its
explicit canonical ink contract. Four-component half-grid/black/color precision
is shared across owned and caller outputs. Named three-component4:1:1/4:1:0
controls also require exact RGB/Y through dimension-aware filtering and retained
fractional chroma, including progressive, padded/oriented caller and near-integral
filter cases. The same frozen513x259 companion packets match the existing live
oracle's decode options. Historical timing workers apply additional centered
normalization; their unchanged references and prior-production timings do not
establish current workload equivalence or a new performance pass.
[Decoder evidence](../benchmarks/decoder-exactness-evidence.json) states the scope;
[rounding](jpeg-inverse-rounding.md) describes the integer boundaries. No old
inverse variant or compatibility mode remains. Earlier JPEG decode CPU/allocation
rows describe the replaced producer; current measurements remain open.

The two original513x259 direct-RGB422/420 packets retain their approved centered/
component normalization and exact RGBA/RGB hashes. Caller grayscale now converts
those normalized8-bit components with the shared rational BT.601 Q15/Q14 luma
precision. The fixture manifest records provenance, input/settings and normalized
reference hashes. Caller RGB/gray orientation, padding, admission and stream/
retained-result boundaries have exact gates; no new performance claim is implied.

VP8 decoding now retains the approved saturated16-bit/Q15/Q13 normalization using
integer scalars. All164 existing frozen RGBA controls and live comparisons require
exact samples. Reconstruction, deblocking, centered chroma and alpha were already
exact; their algorithms are unchanged. The previous floating color renderer is
removed. Existing VP8 decode timing/allocation rows describe the replaced producer;
current CPU/allocation qualification remains open.

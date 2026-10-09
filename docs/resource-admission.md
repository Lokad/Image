# Resource admission

`ImageLimits` describes one operation's admission policy. It counts supplied
pixel/encoded buffers, newly allocated results and algorithm workspaces, with
fixed allowances for parser state and BCL compression. It does not bound total
process RSS, GC retention, runtime/JIT state, or additional buffers retained by
the caller. Releasing admission ends an algorithm's use of storage; it does not
force the .NET collector to reclaim that storage.

Horizontal flips may reuse one released exact-size pixel array, with a global
idle-storage ceiling of 4 MiB. It retains only released storage, never a larger
array or a source/result owner. Every returned image is fully written, and
simultaneously live buffers have separate storage. Results keep their exact
byte admission charge; pooled capacity is never rounded up. Failed private
results release their lease. Idle cache storage is outside per-operation
admission, as is GC retention; the fixed ceiling bounds the cache separately.

`MaxDimension` and `MaxPixels` constrain newly created buffers and codec input
geometry. A transform can use an existing larger buffer to produce a smaller
result under lower geometry limits; the supplied buffer still counts in
`MaxWorkingBytes`. `MaxInputBytes` constrains encoded input and
`MaxOutputBytes` constrains emitted encoded bytes. Work units are deterministic
algorithm charges, separate from time, instructions or allocations.

## Reviewed buffer, transform and codec paths

| Path | Admission and lifetime | Work and cancellation | Boundary evidence |
| --- | --- | --- | --- |
| Buffer creation | Positive dimensions/depth; dimension/pixel/array length checks precede result allocation | Cancellation checked before creation | `TransformTests`, `PixelTests` |
| Clone and identity resize | Supplied bytes plus independent result bytes | Four units per pixel; copy checkpoints every 16 KiB | `TransformAdmissionTests` at both depths, including partial chunks |
| Crop, flips, quarter turns, transposes, auto-orientation and nearest resize | Supplied bytes plus exact output bytes; crop/enum/geometry validation before mapping | Four units per output pixel; map checkpoints every 1024 pixels within each row | `TransformAdmissionTests`; independent pixel grids and live native controls |
| Bilinear/bicubic resize | Supplied/output bytes, support-work preflight, additional 4096-byte allowance only for exact low-alpha rounding | Row work plus kernel-row cancellation; low-alpha rational work checked per tap | `ResizeReferenceTests`, including scratch/work boundaries and exact rational controls |
| Encoded span input | Entire supplied encoded input admitted before container/codec processing | Container and codec charges share the operation context | Public codec limit and malformed-input tests |
| Encoded stream input | 8192-byte read scratch; old/new capacity checked during bounded growth; capacity/final-copy peak checked before parsing | Work/cancellation checked while buffering; caller stream remains open | `PngTests`, codec stream/limit tests |
| Container/EXIF inspection | Dimensions and encoded item/metadata totals bounded; no decoded pixel plane; PNG palette/transparency copies admitted; valid IFD entry count exhaustion raises `ImageLimitException` | Container traversal, CRC and bounded IFD entry visits; unrelated entries/links never cause recursive parsing or allocation | Container/header, PNG framing and EXIF profile/admission tests at both TIFF byte orders |
| PNG decode | Encoded input, concatenated IDAT, result and palette/transparency; fixed 4096-byte framing scratch and 256 KiB inflater allowance; current/previous pass rows admitted together | Exact decompressed length checked before inflation; bounded copy, checksum, unfilter/sample and framing checkpoints | `PngFramingTests` exact peak and one-byte-below checks; original format and native controls |
| PNG encode | Supplied bytes, one packed row, 64 KiB IDAT buffer and 1 MiB BCL compressor allowance, admitted before the signature | Per-row charges, pixel checkpoints, bounded encoded writes, final cancellation check | All 32 color/depth/compression profiles in `PngEncoderAdmissionTests` |
| JPEG decode | Encoded input; 1024-byte parser allowance; quantization/frozen component tables; 64-entry approximation arrays; padded MCU coefficient and sample planes; final RGBA8 result | Marker/segment and table visits; one unit per MCU, 64 per entropy block, one per bit-read call, 4096 per IDCT block and 32 per output pixel | All sampling modes, baseline/progressive, tiny/padded geometry and combined orientation in `JpegAdmissionTests`; exact peak and one-byte-below checks |
| JPEG table redefinition | Each new quantization table admits 128 bytes before releasing its predecessor; every Huffman definition keeps its charge for the operation (204 bytes plus symbols), including replaced/unused tables | Every table's 16 lengths visited with cancellation/work checks; encoded segment/item limits remain active | Original repeated DQT/DHT controls, with unchanged pixels and exact admission boundaries |
| JPEG encode | Supplied bytes plus 8192-byte fixed table/block workspace admitted before the first output byte; no full-frame intermediate plane | MCU/block sampling, 4096 per DCT block and entropy writes checked; emitted bytes bounded, final cancellation before EOI | Both source depths and every sampling mode in `JpegAdmissionTests`; memory/output limits and unchanged input |
| JPEG decode plus orientation | Additional full result admitted in the same decode context; parser, tables, approximation and sample-plane charges conservatively remain through orientation | Four additional units per pixel, plus the transform's independent checks | Exact shared-operation boundaries above, including cases where orientation determines the peak |
| VP8L auxiliary state | Four distinct transforms at most; transform rasters, palette and optional entropy atlas each admitted; auxiliary images cannot introduce another spatial atlas | Predictor mode and entropy group scans charge one unit per entry, with checkpoints every 1024 entries | `WebpLosslessAdmissionTests` cover complete/partial chunks, exact work and cancellation; original transform/atlas raster controls |
| VP8L entropy data | Cache admits `4*size+32` bytes; group references admit `40*groups+32`; temporary length vectors and prefix trees admitted before allocation; pixels admit `4*count+32` | Per token/bit read, upfront tree construction work and every backward-copy sample; overlapping runs bounded by raster and length code | Exact cache sizes 1..11 bits, sparse group IDs up to 255, complete/single-leaf trees and run controls; live native agreement |
| VP8L inverse transforms and output | Palette expansion admits old/new rasters together; each transform releases its auxiliary raster after use; full UInt32 raster and RGBA8 result coexist during conversion | Packed palette expansion charges four units per output pixel; other inverse transforms 32; final RGBA conversion four | Every transform, palette packing boundaries, hidden RGB and exact memory boundaries in `WebpLosslessAdmissionTests` |
| VP8L decode plus orientation | Cache/tables/auxiliary rasters and 512-byte parser allowance released before return; common decode context then admits the second RGBA result | Four additional units per oriented pixel, plus independent transform checks | All 34 original decode profiles, raw and oriented, pass exact peak and reject one byte below |
| VP8L encode | Supplied pixels plus 8192-byte fixed table/workspace allowance; counted payload then streaming output, with no full encoded-frame staging | Both passes charge every visited sample and entropy write; runs bounded to 4096; complete output size checked before RIFF | Both depths and all seven efforts at exact memory/output boundaries; nonseekable output, long overlapping runs and unchanged source |
| VP8 decode | 8192-byte header/model/transform allowance; macroblock modes and contexts; padded Y/U/V planes admitted together; parser/metadata released before alpha | Arithmetic bit/tree reads; 2048 units per reconstructed macroblock; nonzero frame filter pass checks every block, including adjusted-zero levels; bounded edge filtering | Authored partitions/segments/filters, mutations and exact reconstruction admission in `Vp8AdmissionTests` |
| WebP ALPH | Alpha bytes plus 32; raw payload length checked; compressed form additionally admits headerless VP8L workspace and full RGBA intermediate until green extraction | Raw copying checks every 64 KiB, preserving one unit per byte; row-wise extraction and all inverse filters charged | Raw/compressed forms and all four filters, independent alpha samples and live FFmpeg controls; chunk work/cancellation tests |
| Lossy WebP output/orientation | YUV/alpha coexist with first RGBA result, then are released; optional orientation admits a second RGBA result in the decode context | 64 units per rendered pixel, four more for orientation | 54 exact shared-operation memory boundaries across opaque/raw/compressed alpha, padded geometry and orientation |
| VP8 encode | Supplied pixels plus 4096-byte fixed workspace; source/reconstruction planes, optional normalized alpha, mode/context arrays; arithmetic partitions retain capacity | Per-row conversion, bounded mode search and block transforms, bit/carry work and 64 KiB output writes; finalization explicitly checks cancellation | 48 depth/alpha/quality/effort/geometry cases at exact memory and encoded-size boundaries; nonseekable output and unchanged source |
| VP8 arithmetic growth | Initial capacity 64, then doubling/clipping; old and new buffers plus 32 bytes each admitted together before copying; disposal releases retained capacity | Growth copy charges existing bytes; output maximum enforced per appended byte | Exact 64-to-128 and clipped 64-to-96 peaks, one-byte-below failures, disposal/reuse and independent BigInteger interval controls |

PNG framing uses a 512-entry, nine-bit literal prefix lookup and a 32-entry,
five-bit distance lookup generated from the validated canonical trees. They and
the existing arrays total 4056 bytes within the admitted 4 KiB scratch; there is
no heap table or static cache. Every fixed/dynamic block charges 8192 additional
bounded literal construction units and 512 distance construction units.
Stored blocks build neither lookup. Actual prefix bits keep their exact work charge,
including long-code and unassigned-prefix fallback; cancellation is checked
for each decoded symbol. Alignment returns complete unread lookahead bytes
before stored payloads or the checksum.

PNG Adler accumulation uses chunks of at most 4096 bytes. Normalized initial
sums bound the largest intermediate `b` to 2408052720, below `uint.MaxValue`,
so modulo reduction at chunk endpoints is exact. Safe AVX2 blocks use the
RFC 1950 recurrence weights `32-i`; adjacent signed sums cannot saturate.
Scalar fallback and tails retain the same recurrence. Cancellation is checked
at every chunk; existing row work and working-memory charges remain unchanged.
There is no additional array or heap buffer.

PNG CRC uses an 8 KiB shared polynomial table generated once from the format's
reflected polynomial. Eight independent byte contributions combine into one
word update, with a byte tail. The table is read-only after initialization and
replaces the former 1 KiB table; it is fixed shared format data, not an image
cache or per-operation buffer. Per-image admission and work remain unchanged;
cancellation is checked every 4096 bytes as before.

Filter-None PNG rows already contain their original bytes. This branch checks
cancellation and returns without reading neighbour rows or rewriting pixels.
Row work and memory admission remain unchanged; other filters retain prediction.

The larger encoder allowance is deliberate. The documented default zlib
compressor state is about 268 KiB before the managed IO buffer, whereas the
inflater is substantially smaller. The earlier uniform 256 KiB charge was
insufficient as a conservative encoder allowance. See the primary
[zlib memory description](https://zlib.net/zlib_tech.html#memory) and
[.NET 10.0.8 DeflateStream](https://github.com/dotnet/runtime/blob/v10.0.8/src/libraries/System.IO.Compression/src/System/IO/Compression/DeflateZLib/DeflateStream.cs).
The 1 MiB charge includes margin for runtime implementation details; it is an
admission constant rather than a measurement of runtime or process memory.

Nearest-neighbor centers use integer rational arithmetic across the valid axis
range. Original literal controls immediately below a cell boundary show why a
binary64 division can select the adjacent cell on large, explicitly admitted
axes. The coordinate tests exercise these cases without allocating GiB-sized
images; ordinary public/native raster tests check the complete transform.

JPEG's coefficient planes contain signed Int32 values at padded 8-by-8 block
positions. A sample plane is admitted before converting its component, then that
component's coefficient charge is released; the sample planes remain charged
through color conversion and optional orientation. For a supported layout with
total sample-plane length `S` and first-component length `L`, coefficient rendering
peaks at `4*S + L` bytes. Result/orientation peaks at `S + R` or `S + 2*R`, where
`R` is the RGBA8 result length. Add encoded input, parser/table charges and
384 bytes per component for approximation and frozen quantization. The boundary
tests use independently constructed inputs and these declared charges, without
calling production admission helpers.

Progressive JPEG does not allocate a new coefficient plane for each scan. Scan
progression is tracked per coefficient, approximation shifts are limited to
0 through 13, and DC predictors are checked before shifting. Spectral runs stay
within their 64-coefficient bands; EOB runs must end at the scan/restart boundary.
Even an empty EOB block incurs the MCU/block work checks. Refinement visits at
most one band per block and bit reads remain charged. Direct tests cover table
construction's memory/work boundaries and full block transforms' work exhaustion
and cancellation before changing their destination.

Lossless WebP auxiliary syntax and the 16-bit entropy group identifiers follow
[RFC 9649 sections 3.7.2.2 and 3.8.3](https://www.rfc-editor.org/rfc/rfc9649.html#section-3.7.2.2).
The implementation reads each transform's auxiliary image separately; only the
main raster can contain a spatial atlas, whose own image has no further atlas
or transform list. This bounds image-parser nesting to two image-data calls.
The maximum identifier determines how many groups are read, including unused
IDs between populated tiles; admitted memory and work bound all of their trees.
Backreferences cannot extend outside the raster or refer before its first pixel.

VP8L's fixed parser allowance is 512 bytes. Arrays of pixel/cache words admit
their payload plus 32 bytes. A single-leaf prefix table admits 64 bytes; a complete
table admits 320 plus two bytes per leaf. Its input length vector is admitted
separately while constructing it, as is the temporary code-length tree for a
complex definition. Tables and the color cache are released when their image
is decoded. Palette/transform data survive until their inverse transform ends.
The public boundary tests derive original stream peaks from these charges,
without production allocator instrumentation, across tree construction, main
raster allocation and result-conversion peaks. Native controls cover all legal
cache sizes, sparse groups, palette/predictor composition and maximum-width
constant input. Compression efficiency remains a separate pending measurement.

VP8 uses bounded arithmetic partitions and static format trees derived from
[RFC 6386](https://www.rfc-editor.org/rfc/rfc6386.html). Coded dimensions fit their
14-bit fields. Segment/quantizer indices are clamped before table lookup;
coefficient categories, sign and inverse transforms use bounded integer values.
Short dequantized/intermediate stores use explicit 16-bit truncation, covered
by authored coefficient controls and exact native YUV comparisons. Reconstruction and filtering access complete padded
macroblocks; public output crops to the coded canvas. Probability/tree contexts
come only from bounded modes and coefficient states.

Let `C`/`M` be the padded macroblock columns/count, `P=384*M+128` the three-plane
charge, `N` the coded pixel count and `R=4*N` the RGBA8 bytes. Decoder metadata
admits `20*M+13*C+256`. Its reconstruction phase peaks at `8192+metadata+P`;
color output at `P+alpha+R`; orientation at `2*R`. Alpha is zero for opaque
input or `N+32` for an ALPH chunk. Compressed alpha additionally peaks at
`P+alpha+VP8L-workspace` before extracting/releasing its RGBA intermediate.
All phases include the supplied encoded input. The exact public tests exercise
cases where different phases determine the peak, with all raw/compressed filters.

VP8 encoder metadata admits `17*M+12*C+128`. Source and reconstruction planes
are released after finishing the token partition and before building the header
partition. The alpha array and token capacity remain admitted through output.
At capacity `K` above 64, a doubled arithmetic buffer grows through `K+K/2+64`
admitted bytes; retained capacity is `K+32`. Tests derive partition capacities
from actual encoded lengths and these documented rules, without reading
production allocator state. Complete container size is checked before any
caller-visible bytes, and all streamed chunks remain bounded. Lower work or
memory limits may reject before that size check, also before output.

## Platform and measurement evidence

The local path review covers the current buffer, transform, inspection and
PNG/JPEG/WebP implementations. It establishes their declared admission policy
and bounded checkpoints; it does not prove exhaustive validation of every
possible input or complete the cross-platform/package/performance gates.
Changes to codec algorithms or their workspaces require updating these charges
and boundary evidence.

Current local boundary tests establish the declared admission charges. Linux
package-consumer and kernel child-memory validation remain separate gates.
Actual process-memory budgets and performance claims require controlled
measurements on the dedicated benchmark VM.

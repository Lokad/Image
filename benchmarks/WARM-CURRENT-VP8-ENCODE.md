# Current alpha-bearing VP8 encode evidence

The two named RGBA8/16 alpha-bearing VP8 encode cases independently confirm on
current production paths. Their earlier independent proofs preceded the Boolean
output-writer change; latest short controls did not independently confirm that
path. These measurements resolve the uncertainty. Stop both fixed pairs; no
production optimization is needed. Unchanged opaque cases remain stopped.

| Fixed alpha-bearing source | Lokad.Image / FFmpeg µs | Ratio / upper | Managed B/op | WebP bytes Lokad / FFmpeg | Actual / selected |
| --- | ---: | ---: | ---: | ---: | ---: |
| Encode16 | 30,755.98 / 53,436.98 | 0.576 / 0.581 | 2,615,472 | 471,436 / 98,590 | 12 / 12 |
| Encode8 | 30,390.89 / 53,447.22 | 0.569 / 0.573 | 2,615,939 | 471,436 / 98,590 | 12 / 11 |

This is the existing fixed quality-matched pair: managed quality100 versus
FFmpeg/libwebp quality75, both effort4 and VP8 8-bit 4:2:0 output. The same
original 513x259 RGBA byte/word rasters and frozen packets are reused. Both source
depths normalize to the same reference. Alpha-weighted RGB source error is
measured before composite rounding, with the unchanged <=0.25dB band. Managed
source PSNR is16.6980165dB versus native16.7016630dB, gap0.00364647dB. Exact
normalized alpha and unweighted public/native decoder agreement, including hidden
RGB, remain required: RGB maximum4/RMSE1.5. Actual decoder maximum is1.

The managed fixed-entropy stream is471,436 bytes versus98,590 native adaptive-
entropy bytes. These timings establish this source-quality comparison, not
compression-efficiency parity. No setting, quality metric, tolerance or source
image is changed, and no new quality sweep is performed. Existing preparation
records/managed/native packets remain byte-for-byte identical throughout.

Each operation produces a complete independently owned WebP packet. Managed
MemoryStream creation/growth, public EncodeWebp and disposal are timed, without
redundant ToArray. Native codec/scratch/frame/packet wrappers are warm, while
RGBA-to-BGRA repacking, complete encode, independent packet acquisition and release
execute every call. Word inputs include full per-channel normalization
(sample+128)/257 and repacking on both subjects. Source preservation, held
independent outputs, deterministic bitstreams, geometry, codec/sampling, alpha,
source quality and decoder agreement pass before/after timing. No output cache.
Startup, IPC, file/network I/O, generation, hashing and validation are excluded.

Three-actual screens precede independent twelve-actual confirmations on the same
immutable production/harness/native build. All twelve actuals are retained;
conservative uppers include selected/all-actual intervals against the faster
native bracket's lower confidence bound. Both versioned quiet guards pass every
scored run. Sequential Release uses CPU0, one codec/conversion thread, normal
SIMD, tiering disabled and workstation nonconcurrent GC. Managed allocations
include harness costs; native allocation totals are unmeasured.

The only code change is benchmark-only parsing of one-to-three distinct VP8
selectors, outside preparation validation, Setup and the measured operation.
Their unchanged source prefix/suffix is verified. One harness build and two
shared two-case drivers serve all four jobs; every cached child including WarmDry
receives verified assemblies. Production/native builds are zero; the prepared
worker, fixtures, settings and quality checks are reused without recalibration.
Existing production/package qualification remains valid: 3,483 Release tests per
OS including898 live comparisons, and the consumer/all three examples through
the same actual Release package on both OS. No repeated qualification, profile,
whole-library method audit, extra image or quality/shape/depth/effort grid.

[Numeric evidence](warm-current-vp8-encode-evidence.json) retains samples,
allocations, packet sizes, exact quality data, preparation/packet hashes, binary
identities, guards and qualification reuse. This closes named B09 fixed-input
scope; full required warm coverage remains open. Next: existing literal/run VP8L
decode, retaining newer native/indexed/cancelable/Stream confirmations.

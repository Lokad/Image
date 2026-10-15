# RGBA16 PNG Stream, orientation, crop and fit

The baseline below is superseded by [fused framing/inflation](WARM-PNG-FUSED-INFLATION.md),
which confirms 1.216× / upper 1.231× with the same workload and exact pixels.

At the baseline source this complete warm public composition remains **open**: the independent repeat
measures **1.993× FFmpeg / conservative upper 2.022×**, at **5,879.77 µs** and
**4,468,718 B/op**. All twelve actuals and both native brackets remain represented;
every versioned quiet guard passes. A mean below two does not close the case.

Each operation creates a fresh non-exposed read-only seekable MemoryStream,
decodes the existing original 513×259 RGBA16 PNG, calls `AutoOrient` with Normal
orientation, `CropCentered(256,129)` and `ResizeToFit(128,64,false,Nearest)`, and
releases every owner. All managed intermediates are materialized. The output is
127×64; every selected word matches independent integer center-sample coordinates
in the original raster. Normal/scalar native CLI output, source preservation,
caller lifetime/EOF, orientation and two-live-output independence pass. The crop
contains 32,393 words that are not replicated bytes and no zero-alpha output
pixels; qualified individual codecs retain full hidden-RGB coverage. No new
original image, shape grid or tolerance adjustment is introduced.

The original native adapter times fresh public AVIO acquisition and owned packet
copy, complete PNG decoding, a public filter graph and all releases. PNG's owned
RGBA64BE frame stays big-endian through crop; an explicit packed RGBA64LE format
conversion then feeds exact point sampling of all four 16-bit planes and final
independently owned RGBA64LE output. Intermediate views are allowed, as in the
[byte composition](WARM-PNG-COMPOSITION8.md); no artificial full-frame endian copy
is required. Contexts and wrappers are reused, while each input is reacquired
and decoded. One CPU/codec/filter/conversion thread, normal SIMD, tiering off and
workstation nonconcurrent GC are used. Startup, preparation, I/O, IPC, hashing and
validation are excluded. Native allocations are unmeasured.

The first graph crashes in FFmpeg's format negotiation, in both the original
worker and CLI, before timing. Explicit packed conversion after crop resolves
negotiation with exact words. The original refusal, binary and diagnostic are
retained unscored.

A six-second profile places 43.1% of samples in BCL inflation and 37.7% in the
managed literal-prefix framing walk. A four-literal batching experiment passes
111 focused framing checks and exact public pixels but has only an approximately
0.5% observed complete-operation mean difference, without a convincing material
gain. It is rejected without a candidate confirmation, full suite or package
proof. Its actual core, source, tests and three-actual receipt are retained;
production and tests are restored exactly to the qualified build. Do not retry
this unchanged experiment. Prioritize fusing framing and inflation to avoid the
second Deflate walk while preserving work, failure order and constrained fallbacks.

Only four benchmark/native-adapter inputs change. Existing **3,318 Release tests
per OS**, including **896 live FFmpeg comparisons**, and consumer plus three
examples through the same actual package are reused after restoration. No full
suite, package proof or whole-library method audit is repeated.
[Numeric evidence](warm-png-composition16-evidence.json) retains all samples,
allocations, native brackets, source/build identities, profile, rejected trial,
restoration proof and immutable archive identities. This pair and the overall
material/current-source warm target remain open.

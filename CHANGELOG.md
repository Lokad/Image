# Changelog

## Unreleased

- Reduce JPEG transform addressing and repeated loads while preserving the exact
  mathematical basis, arithmetic order, frozen bitstreams and resource charges.
  Record the initial dedicated-VM invocation baseline, paired FFmpeg results,
  quality exclusions, child memory and a flagged variable native control.
- Verify an existing NuGet artifact without repacking it, so the same release
  package can be exercised on Windows and Linux.

- Package the usage guide and verify all three runnable examples through the
  actual dependency-free NuGet package.
- Bound default quality calibration to twelve profiles from six original inputs,
  sampling at most eight qualities per profile while retaining matching bounds
  and explicit exclusions. Extended matrices remain optional.
- Establish the safe .NET 10 library, ownership and resource-limit contracts.
- Implement original PNG codecs, exact 8/16-bit edits and alpha-aware resizing.
- Add the full authored PNG color/depth/filter/interlace matrix, bounded Deflate
  framing validation, correct trailing-IDAT and future-chunk handling, and exact
  decode/orientation memory admission checks. Establish exact packed-gray alpha
  through an explicit native color-key reference and retain unfiltered evidence.
- Add container inspection, xUnit validation and FFmpeg development tooling.
- Report valid EXIF directory entry limit exhaustion as `ImageLimitException`;
  cover bounded traversal across codecs, both byte orders and stream/span inputs.
- Cover all eight EXIF orientations and both TIFF byte orders across the codec
  profiles with original public/live controls; add decode/orient and coded-header
  inspection benchmark workloads with explicit native boundaries and provenance.
- Add baseline JPEG encoding/decoding and progressive JPEG decoding, sampling,
  restart markers, explicit alpha flattening and original FFmpeg fixtures.
- Review JPEG coefficient/table/plane lifetimes and progressive work; add exact
  decode/orientation, redefined-table and encode admission boundaries.
- Record 320 original JPEG quality/size cases, freeze quantization endpoints and
  validate native chroma placement and once-only 8/16-bit alpha flattening.
- Add JPEG raw-file decode/encode and opaque resize/transcode invocation benches,
  larger authored progressive grayscale controls and untimed validation reports.
- Freeze the original mathematical JPEG basis for consistent Windows/Linux
  coefficient and pixel rounding; preserve the existing original corpus hashes.
- Add pinned Linux SDK/PowerShell and source-built FFmpeg/libwebp/zlib tooling
  with archive/source checks and recorded compiler, configuration and binary hashes.
- Preserve native benchmark wildcard filters on Unix and fail empty or unsuccessful
  benchmark runs explicitly.
- Add 32-image decode batches for PNG 8/16-bit, baseline/progressive JPEG and
  lossy/lossless WebP, with complete per-frame validation and input provenance.
- Add 32-image raw-file encode batches for PNG/JPEG/WebP at both source depths,
  with per-bitstream interoperability, exact public workflows and quality/size
  evidence; record the native hidden-RGB comparison restriction explicitly.
- Record Linux codec-child peak RSS/page faults in separate untimed executions,
  apart from BenchmarkDotNet's managed allocation scope.
- Add exact 8/16-bit raw-file crop/flip/quarter-turn/transpose benchmarks for
  single images and 32-image batches, with independent coordinate validation.
- Group benchmark artifacts and validation reports by a unique execution ID.
- Validate null sources consistently in centered/clamped crop, auto-orientation
  and aspect-fit helpers.
- Clear filtered RGB whenever rounded alpha is zero, including fractional-support
  residue and tiny positive alpha; add independent exact rational resize tests.
- Resolve low filtered-alpha rounding with exact integer weights so mathematical
  half-alpha ties retain visible color; admit scratch and charge bounded work.
- Add exact nearest-neighbor raw-file resizing checks and invocation/batch benches
  at both depths and scale directions, with explicit native scalar-plane transport.
- Select nearest-neighbor cells with exact integer centers on large admitted axes;
  keep clone/mapping checkpoints bounded within long rows and preserve work charges.
- Admit a 1 MiB PNG compression workspace before writing output; add complete
  color/depth/compression memory boundaries and resource-admission review notes.
- Add lossless WebP decoding/encoding, bounded VP8L entropy/transform handling,
  exact hidden-RGB preservation and original fixtures validated with FFmpeg.
- Charge and check cancellation during VP8L predictor/entropy atlas scans;
  verify all legal cache sizes, sparse prefix groups and lossless codec memory
  boundaries, including orientation and two-pass nonseekable encoding.
- Add VP8 still-image decoding, integer reconstruction/filtering, raw/compressed
  ALPH, original frozen YUV expectations and explicit RGB conversion checks.
- Bound raw alpha copy checkpoints, check arithmetic finalization cancellation,
  and skip disabled filter passes while checking adjusted-zero macroblocks.
  Establish exact lossy decode/orientation and encoder staging memory boundaries.
- Add independently implemented VP8 encoding, quality/effort controls, exact
  normalized alpha and FFmpeg interoperability checks for 8/16-bit sources.
- Add authored VP8 partition, segment, coefficient and filter-boundary fixtures;
  honor zero frame-level filter strength and validate saturated reference colors.
- Further compression tuning and broader performance measurements remain pending.

# Warm reference tooling

These original development adapters use public FFmpeg/libwebp/libjpeg-turbo/zimg
APIs. They are not production build inputs, package assets or native bindings.
Reference sources, binaries and generated fixtures belong under ignored `external/`
or experiment directories. Never copy upstream implementations or tables into
tracked files.

[Current results](../../docs/benchmark.md), [finite coverage](../../benchmarks/WARM-COVERAGE.md)
and [codec parity](../../docs/codec-parity.md) define current qualification.
The [previous recipe inventory](https://github.com/lokad/image/blob/0bd4a507cd87dcf85bfec4f9ac0a38c49174b86b/tools/WarmReference/README.md)
retains detailed historical commands and experiment reports. Its timings, removed
APIs, matched-dB searches and IDAT-only gates do not qualify current production.
Cited raw archives, failed runs and published packages remain retained.

## Complete-operation contract

Measure one CPU per operation on the exclusive quiet VM, sequentially in Release.
Pin both subjects to the declared CPU; use normal SIMD, processor count one,
fully optimized JIT without tiering, workstation GC without concurrent collection,
and one native codec/filter thread. Startup, fixture generation, transport, IPC,
hashing and validation are outside timing. Never publish local workstation timings.

Include equivalent normalization, input delivery, admission, processing, fresh
independent output ownership and release. Stream cases include fresh wrapper,
reads/copies/EOF and disposal. Native frame padding or negative strides need no
copy solely for cosmetic agreement; materialize independent pixels when the public
operation requires them. Context/wrapper and released-storage reuse is allowed;
completed outputs cannot be cached to replace processing. Do not time ToArray
unless that copy is part of the declared public boundary.

Before and after each run, validate frozen inputs/settings, complete encoder
packets, decoded/editing samples, source preservation and simultaneous independent
outputs. Encoder acceptance requires complete-packet equality; only approved JPEG
attribution COM may differ. Lossless pixels and managed editing pixels stay exact.
Equal numeric qualities, matched dB alone, a timing pass or 5% IDAT tolerance cannot
establish compression parity. Current limits cover input/output, dimensions,
containers and memory; cancellation is cooperative, without work quotas.

Native workers report ready/affinity evidence, internally timed `sample COUNT`,
untimed `check` and `quit`. Use reported elapsed/count; timing IPC measures another
operation. Invalid affinity, counts, protocol tokens and output checks must fail.

## Prepare once, screen, confirm

Reuse verified immutable production/harness/native/fixture/selector artifacts while
the relevant inputs remain unchanged. Internal tests compile linked sources;
explicitly build the production project when it changes. Build/prepare outside
scored intervals. Capture owned sources with `capture-source.ps1` and retain actual
DLLs, runtime configuration, generated dispatch, native identity and raw receipts.

A new fixed selection can be prepared with the existing script, for example:

```powershell
./bench.ps1 -DedicatedVm -Warm -WarmFamily VerticalFlip -WarmCases Whole16 -Cpu 0 -NoBuild -PrepareWarm
./bench.ps1 -DedicatedVm -Warm -WarmFamily VerticalFlip -WarmCases Whole16 -Cpu 0 -NoBuild -WarmConfirm -PrepareWarm
```

Preparation is validation-only and establishes no performance ratio. A retained
compatible child can be replayed directly after verifying its method/case/job and
current assemblies. Do not generate a new child for unchanged dispatch.

Run `capture-vm-environment.py .` immediately before and after scoring. Its
`exclusive-vm-idle-v2` guard rejects overlapping builds, codecs or profilers and
requires a five-second CPU0/aggregate idle sample: busy ≤5%, I/O wait ≤1%, steal ≤1%.
Load averages are diagnostics; do not wait for our historical load to decay.
Older failed guards remain failed under their recorded protocol.

Set a unique `LOKAD_IMAGE_BENCHMARK_RUN_ID`, `LOKAD_IMAGE_WARM_CASES`,
`LOKAD_IMAGE_WARM_CPU`, and `LOKAD_IMAGE_WARM_MODE=measure` for the prepared
three-iteration WarmShort job, or `confirm` for the twelve-iteration WarmConfirm
job. Set `DOTNET_PROCESSOR_COUNT=1`, `DOTNET_TieredCompilation=0`,
`DOTNET_GCServer=0`, `DOTNET_gcConcurrent=0`. Native brackets use three/twelve
samples respectively. `LOKAD_IMAGE_WARM_NATIVE_SAMPLE_MS` accepts 50 or 200;
record the chosen duration and all actual elapsed/count values.

Recover standalone child measurements with the existing parser:

```powershell
dotnet tools/WarmReference/ParseBenchmarkLog/bin/Release/net10.0/ParseBenchmarkLog.dll confirm.log confirm-statistics.json
```

Add `3` for a short job; default is twelve. The parser retains actual and adjusted
rows and refuses evidence overwrite. Independent parity requires conservative
upper ≤1× and both guards. Shorts identify candidates; they do not certify parity.
Keep failed attempts unscored. Profiling is a separate bounded diagnostic interval;
discard instrumented timings and never run a profiler while scoring.

Use focused correctness checks and one short screen, followed by independent
confirmation if promising. Batch Windows/Linux Release/live/affected scalar and
the same actual Release-package consumer/usage-example qualification for accepted
production changes. Do not repeat unrelated audits, synchronization, package proofs
or full suites for unchanged builds or rejected experiments.

## Reference families and exceptions

Build only affected workers with their `build-*-linux.sh` scripts after provisioning
the pinned ignored Linux oracle. Use the corresponding `verify-*-linux.py` and the
linked case report to preserve exact input, packing and ownership boundaries.

- PNG/JPEG/VP8 codecs use the original `ffmpeg-*-warm.c` adapters. RGBA16 encoding
  includes required per-operation normalization/endian conversion. Separate
  caller-sample, restart, retained-result, stream and async workers preserve their
  named delivery/ownership boundaries. Consult each report rather than substituting
  a convenient CLI graph or reusing historical output correction blindly.
- Single-component grayscale JPEG encoding explicitly uses SIMD-enabled standalone
  libjpeg-turbo 3.1.4.1 (`build-libjpeg-gray-linux.sh`), the approved tooling-only
  exception. Include flattening, BT.601 normalization, encoding and fresh packet
  ownership. The declared controls now require complete raw packet equality.
- Lossless WebP explicitly uses libwebp `exact=1`, FFmpeg's encoder backend
  (`build-libwebp-lossless-linux.sh`). FFmpeg's public wrapper drops hidden RGB;
  its diagnostic adapter is not an exact-lossless timing baseline. Normalize all
  RGBA16 channels with `(sample + 128) / 257` inside each operation.
- Bilinear/bicubic/Lanczos3 use zimg, FFmpeg's zscale backend, with the declared
  clamped halo/active region, packing, premultiplication, filtering,
  unpremultiplication, quantization, zero-alpha handling and owned outputs timed.
  Reuse independent rational/precision references and existing case geometry.
  Native RGB/alpha max1 and RMSE ≤0.25 apply only where approved: bilinear Down8
  keeps exact alpha; explicit RGBA16 and Up8 allow native alpha rounding1. Bicubic
  RGBA16 513×259→513×519 alone permits RGB≤2 at reference alpha≤16, ≤1 elsewhere,
  retaining alpha≤1 and both RMSE≤0.25. Managed samples stay exact.
- Exact crop/clone/orientations and nearest resize retain original pixel placement,
  full sample depth, hidden RGB and independent owners. Build/verify scripts use
  the named operation; the [fixed inventory](../../docs/benchmark-pre-cleanup.md)
  retains their exact source and precision boundaries.
- Metadata references use PNG parser/CRC, MJPEG discard-all with canonical SOF2
  projection, or WebP parser, plus original complete structural/EXIF validation
  and fresh descriptors. Stream references include AVIO/scratch/input/EOF and
  release. No pixel work substitutes for Identify. Historical six-field evidence
  alone does not qualify the expanded typed source metadata: reconcile required
  fields and validation before current timing claims. InspectHeaders remains a
  separate bounded-workspace public boundary.

Source-only `*Parity.cs` models and stage verifiers are retained for the cited
byte-replication history and diagnosis. Production packet/decoder tests qualify
actual code; an old model pass does not establish integration, ownership or speed.
[PNG](../../benchmarks/PNG-MANAGED-POLICIES.md),
[JPEG](../../docs/codec-parity.md), [VP8L](../../benchmarks/VP8L-COMPLETE-PACKET.md)
and [VP8](../../benchmarks/VP8-INPUT-STAGE.md) record the current tested scope.

After an accepted round, update selected evidence in
`benchmarks/render-benchmark.py`, regenerate `docs/benchmark.md`, and run the
renderer with `--check`. Keep at most three summary tables, allocations and
verification status. Link superseded experiments instead of extending a timeline
here. Reuse unchanged qualification; preserve reproducibility without new
bookkeeping or quota machinery.

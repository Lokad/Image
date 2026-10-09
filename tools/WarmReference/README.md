# Warm FFmpeg reference

`ffmpeg-warm.c` is an original development-only adapter using public FFmpeg
filter APIs. No implementation code or tables are copied from upstream. It is
not a library build input, native binding or NuGet asset. Build the pinned Linux
oracle first, then compile this worker against its ignored installed libraries:

```bash
bash tools/WarmReference/build-linux.sh
python3 tools/WarmReference/verify-linux.py --cpu 0
```

The executable and build identity stay under ignored `external/warm-reference/`.
The initial operation is horizontal flip at RGBA8/RGBA16LE. The worker loads
one original raster before timing and keeps a single-threaded hflip graph and
source/frame wrappers. Each operation produces fresh non-aliasing result pixels
and releases them. Native allocator pools may reuse released storage; no source
pixels or edited results are cached to replace processing.

Both subjects expose owned interleaved RGBA rows. FFmpeg's allocation alignment
can add row padding; input/output strides are recorded. The library has tight
rows. No extra copy is added solely to remove native padding. File transport and
canonical byte packing happen only in untimed validation, and every channel,
including zero-alpha hidden RGB, must match independent pixel placement.

The bounded line protocol reports ready/thread/affinity evidence, internally
timed `sample COUNT` operations, untimed `check` output/source verification and
`quit`. A worker refuses to run without affinity to exactly one logical CPU.
Native monotonic samples include submitting the source, acquiring/materializing
the owned result and releasing it. Process launch, IPC, hashing and file I/O are
outside the scored interval. The controller must use the reported elapsed
time/count; timing a protocol request with BenchmarkDotNet would measure a
different boundary.

Run `./tools/WarmReference/capture-source.ps1` from PowerShell before execution;
it inventories the owned tracked/unignored files under ignored `tmp/`.
`./bench.ps1 -Smoke -Warm -Cpu 0 -NoBuild` validates execution/output on Linux.
`./bench.ps1 -DedicatedVm -Warm -Cpu 0 -NoBuild` measures the two precision cases
on the quiet VM. Managed
workers use processor count one, workstation GC and no background GC, with
a fixed fully optimized JIT policy and tiered compilation disabled, so
optimization cannot be deferred into measurements on the single busy CPU. Both
subjects are pinned to the chosen allowed logical CPU. Native pre/post samples
bracket the managed BenchmarkDotNet samples. Statistics, identities, exact checks
and raw native elapsed/count values remain under ignored benchmark artifacts.
Screened ratios need independent repeats before parity is certified.
If short-run uncertainty overlaps the target, add `-WarmConfirm` to the dedicated
VM command. It keeps the same two inputs and boundary, extending both managed
and native measurements from three to twelve samples per case.
The verification command reuses exactly the two declared inputs, validates
sample counters and every pixel, and requires rejection of multi-CPU affinity,
zero/excessive counts, trailing protocol tokens and unknown commands.

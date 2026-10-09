# Warm single-CPU PNG: framing reader

Batching Huffman bit reads and work admission reduces PNG RGBA16 decode from
about 37–39 ms to 22 ms on the dedicated VM. Both measured decode paths remain
above the 2× FFmpeg target. PNG encode is below it in the first screen, with
confirmation still pending. Other PNG profiles and warm codec/editing families
remain open; these results do not establish uniform parity.

The [numeric snapshot](results/warm-png-framing-2026-10-09.json) retains all actual
managed measurement counts/times, native counters/times, uncertainty, identities,
exact checks and quietness observations. Only two original 513x259 rasters are
used: RGBA8 and RGBA16, including zero-alpha hidden RGB.

| Case | Before managed ms | After managed ms | Before worst mean ratio | After worst mean ratio |
| --- | ---: | ---: | ---: | ---: |
| Encode8 | 15.42 | Not repeated | 0.87× | Pending |
| Decode8 | 11.53 | 9.35 | 8.06× | 6.59× |
| Decode16 | 36.68–38.58 | 22.09–22.13 | 13.22× | 7.58× |

Each launch requests three managed iterations and three native samples before
and after them, at a 200 ms sample target. RGBA16 has two independent baseline
launches and two candidate launches; RGBA8 has one before/after screen. Ratios
divide managed mean by the faster native bracket mean, then retain the worst
launch. The snapshot includes conservative 99.9% mean interval estimates, using
the managed Student-t critical value for native intervals with no fewer samples.
These short screens guide priority; they do not certify the passing encode row.

## Changed primitive

A separate VM diagnostic on the same RGBA16 input measured about 25.8 ms of
strict Deflate framing work within a 38.1 ms full decode. Filter-None processing
was about 1.3 ms. Framing was therefore the first optimization target.

The reader now buffers bounded prefix lookahead and charges actual consumed
bits once per decoded symbol. It retains canonical prefix validation, exact
output/history/window checks, computational work limits and cancellation at
each prefix. Byte alignment returns complete unread lookahead bytes before
stored blocks or the Adler checksum. Fixed framing scratch stays at 4 KiB;
there is no new lookup table, heap allocation or production dependency.
Other PNG primitives and output storage are unchanged.

New analytical RFC 1951 tests cover all eight bit alignments before stored
blocks, checksum positions with unused trailing bytes, every truncation of
those streams, exact work admission and the unchanged scratch-memory boundary.
The existing authored dynamic/fixed trees, maximum-depth paths, repeats,
overlapping matches and malformed inputs remain covered.

## Workload boundary

Both decoders consume identical resident PNG bytes and materialize independent
owned RGBA output, including acquisition, complete decoding and release.
Native RGBA16 includes a second owned output and big-endian to little-endian
sample conversion. Padded native rows are exposed without a packing copy.
Managed decode includes strict container, framing and checksum validation.
Native codec/input/frame wrappers are reused; no decoded raster is cached.

Encode uses level 6, filter None and noninterlaced RGBA on both subjects. All
inflated rows must match; prepared IDAT payload sizes differ by less than 5%.
Managed encode includes a fresh MemoryStream, growth, encoding and disposal;
native encode includes packet acquisition, encoding and release. Every native
frame is forced intra and every input marked key to exercise independent
still images instead of sequence delta frames. Output independence is checked
with two simultaneously live results before timing.

Process launch, IPC, file transport, fixture preparation, hashes and correctness
checks are outside both scored intervals. FFmpeg uses one effective codec thread;
both subjects and runtime threads are pinned to CPU 0. Managed execution uses
processor count one, workstation GC, no background GC and tiering disabled.
The VM runs Ubuntu 24.04.4 x64, AMD EPYC 9V45, SDK 10.0.204/runtime 10.0.8,
BenchmarkDotNet 0.15.6 and pinned FFmpeg n8.1.

Pre-run quiet checks passed. Failed immediate post-run rolling-load observations
and later cooldown observations are retained. Statistical runs, builds and
profiling were sequential. No local timing numbers are used.

## Validation and reproduction

Windows and Linux each pass 1,688 Release tests with live FFmpeg: 111 internal,
683 public API and 894 differential. All three original warm PNG smoke cases,
native full-pixel/counter/ownership checks and twenty protocol/affinity negative
controls pass. The same actual dependency-free Release package passes on both
platforms, including all three usage-guide examples. Package SHA256:
`9dfa0273cd25a78ecc18cc4158aa4fe797f519b712354cf4f786eeacbcf371f7`.

After provisioning the pinned ignored native tools on the dedicated Linux VM:

```bash
source external/linux-x64/environment.sh
bash tools/WarmReference/build-png-linux.sh
python3 tools/WarmReference/verify-png-linux.py --cpu 0
```

In PowerShell:

```powershell
dotnet build benchmarks/Lokad.Image.Benchmarks -c Release --tl:off
./tools/WarmReference/capture-source.ps1
./bench.ps1 -Smoke -Warm -WarmFamily Png -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases Decode16 -Cpu 0 -NoBuild
```

Capture/review quietness around statistical runs and repeat independently.
Use only the affected cases during a primitive iteration. Regenerate this report
with `python benchmarks/render-warm-png-framing.py`. Original native adapters are
development tools; no upstream code, native assets or dependencies enter the
production package.

# Warm fresh-Stream PNG identification

Independent quiet confirmation measures **1.445x / upper1.459x**, at
**22.94 us /184 B/op** against **15.87 us**. All twelve actuals are retained
(ten selected); the bound includes selected and all-actual intervals against
the faster native bracket's lower confidence bound. Both five-second guards pass.
The original valid screen measured7.863x/124.88us and743,840 B/op. Managed time
falls81.6%; allocation falls99.98%. No longer job was run for that above-two
screen. The candidate screen passed before independent confirmation. Stop it.

One existing218,775-byte PNG from the original513x259 byte raster is used,
identical to resident C01. Each operation creates a fresh seekable non-exposed
input wrapper, acquires and reads all encoded bytes through EOF, validates full
PNG structure/CRC/metadata, constructs an independently owned six-field descriptor,
and releases input/wrapper/output ownership. Identify leaves the caller open;
the operation then disposes its wrapper. Neither subject inflates pixels.

The labeled reference is **FFmpeg fresh custom AVIO plus public PNG parser/CRC
and an original full structural/metadata adapter**. Fresh AVIO/scratch acquisition,
length query, owned input allocation/copy/reads/EOF, metadata/CRC inspection,
AVCodecParameters descriptor and all releases are timed. Source/parser/context
are reused. Managed times the complete public Identify(Stream) and disposal;
its private acquired prefix is fully overwritten and never borrowed from the
caller. Startup, I/O, IPC, preparation/hashing and validation are excluded.
One CPU, normal SIMD, nonconcurrent workstation GC; no new image or workload grid.

The demonstrated cost was repeated large-array buffering:302 Gen2 collections
in1,616 baseline units. One private input-acquisition primitive uses bounded
BCL pooling for exact power-of-two capacities, stack scratch and the acquired
prefix without a redundant final copy. Oversized pool offers use exact new
storage instead. Original read order, growth/final-copy admission allowance,
work, cancellation, failure precedence and EOF/caller lifetime remain unchanged.
One idle array per bucket up to4MiB bounds retention below8MiB; simultaneous
operations never share a live lease. No descriptor or decoded output is cached.

Eleven focused tests compare bytes, read/consumed counts, remaining work/memory,
errors and cancellation tokens against the unchanged decoder input reader,
including small budgets, odd chunks, overlapping leases and dirty prefixes.
All21 same-packet structural/metadata controls and direct fresh-Stream exact
input/working/work, CRC, invalid Deflate, owner and cancellation gates pass.
The candidate reuses the native worker, fixture, harness and original short
driver. A premeasurement link-library identity correction changed metadata only;
an untimed exact-capacity guard precedes the final immutable measured build.

Fresh **3,546 Release tests per OS** (1,262 internal,1,386 public,898 live FFmpeg)
and the consumer/all three examples through the **same actual Release package
on Windows/Linux** pass. Production remains safe managed dependency-free C#/.NET10.
Only Identify(Stream)'s acquisition and the new private helper change. The
original decoder ReadInput, resident Identify bodies, pixel codecs, edits and
resizes remain unchanged; all B01-B15/C01-C03 proofs reuse their executed paths.

[Numeric evidence](warm-png-stream-identify-evidence.json) records raw actuals,
guards, exact controls, qualification, source/binary hashes and the precise
invalidation. This closes C04 and the finite [material checklist](WARM-COVERAGE.md).
The claim covers its named fixed workloads on the original two rasters and
documented projections, preserving approved native-only baseline/quality policies;
it does not extrapolate to arbitrary images/settings/hardware or concurrency.

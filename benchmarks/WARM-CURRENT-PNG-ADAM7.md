# Current Adam7 decode evidence

The two named RGBA16/packed-gray1 uncertainties independently confirm on current
production paths. Their older complete-operation timings preceded selection of
the nonmaterializing Deflate CountBlock loop. No optimization is needed; stop
these cases. Later palette4/8 Adam7 proofs remain valid and are reused.

| Fixed Adam7 case | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| DecodeAdam7_16 | 5,393.13 | 3,917.00 | 1.377 / 1.381 | 23,600 | Current confirmation |
| DecodeGray1Adam7 | 1,512.17 | 1,150.81 | 1.314 / 1.320 | 1,424 | Current confirmation |
| WarmPng/DecodePalette4Adam7 | 1,480.35 | 1,135.35 | 1.304 / 1.387 | 2,592 | Reused unchanged path |
| WarmPng/DecodePalette8Adam7 | 1,710.03 | 1,465.40 | 1.167 / 1.177 | 4,960 | Reused unchanged path |

Every operation consumes the identical original resident 513x259 PNG, validates
the container/checksums/framing, reconstructs all seven passes and materializes
an independently owned RGBA output, including coded-color/depth expansion and
word endian conversion where needed. Native contexts/wrappers are warm; decode,
required conversion, output acquisition and release remain timed. Startup, I/O,
IPC, generation/hashing/validation are excluded. No decoded-output cache.

Exact RGBA, precision, alpha/hidden RGB, normal/scalar CLI pixels, frozen packet/
projected/original raster hashes, source preservation and simultaneous independent
owners pass before/after timing. Only the two retained original byte/word rasters
and the existing packed projection are used. No new geometry/depth/filter grid.

The three-actual screens precede independent twelve-actual confirmations on the
same immutable production/harness/native build. All twelve actuals are retained;
conservative uppers cover selected/all-actual intervals against the faster native
bracket's lower confidence bound. Both versioned quiet guards pass every scored
run. Sequential Release uses CPU0, one codec/conversion thread, normal SIMD,
tiering disabled and workstation nonconcurrent GC. Native allocation totals are
unmeasured; managed allocations include harness costs.

One shared two-case driver per sampling length replaces four separate driver
builds. Production/harness/native rebuilds are zero; all cached children,
including WarmDry, receive the verified assemblies. Existing full qualification
is reused: 3,483 Release tests per OS, including 898 live comparisons, and the
consumer/all three examples through the same actual Release package on both OS.
Production and package inputs are unchanged.

The RGBA16 screen is retained after correcting a private postcheck: its historical
anchor predates native conversionThreads metadata. The current worker explicitly
uses zero conversion threads for direct RGBA and times word endian conversion.
Original refusal/prepared records, frozen packet/coded layout, actual binaries,
raw timings, exact pixels/owners and both guards revalidate unchanged. No timing
rerun or relaxed correctness/confidence gate is used.

Palette4/8 source witnesses include CountBlock selection, both guards, all twelve
actuals and exact pixel/ownership gates. Their IDAT sizes (21,665/101,390 bytes)
stay below the previous one-MiB retention ceiling; later PNG input-cache and
other-codec changes do not affect these selected sequential paths. The numeric
evidence records exact witness file/source/archive hashes and qualification reuse.

[Numeric evidence](warm-current-png-adam7-evidence.json) retains all samples,
allocations, guards and identities. This closes the named B06 fixed-input scope;
full required warm coverage remains open. Next: named PNG Stream/composition
boundaries, retaining their complete input/output ownership costs.

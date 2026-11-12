# Stored PNG operation boundaries

Historical stage: [current IDAT publication](PNG-IDAT-FORWARDING.md) supersedes
these affected encoder costs and transport ownership. Its deflater algorithms and
the reference boundaries qualified here remain; raw evidence is retained.

The two existing 513x259 stored controls now have equivalent complete one-shot
comparisons. Managed stream creation/growth, codec storage, packing, encoding and
disposal remain timed. Native codec/options/zlib and packet-wrapper creation,
encoding and disposal are timed; RGBA16 also includes fresh conversion-frame
allocation, complete endian conversion and disposal. Both borrow resident inputs.

| Lifecycle and sampling | Lokad / FFmpeg us | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: |
| Fresh codec/state on both sides, RGBA8 | 101.03 / 589.32 | 0.171 / 0.187 | 613,844 |
| Fresh codec/state on both sides, RGBA16 | 545.62 / 1513.66 | 0.360 / 0.374 | 1,151,588 |
| Retained native codec/frame, RGBA16, post-GC4 | 501.70 / 405.21 | 1.238 / 1.255 | 1,151,996 |
| Retained native codec/frame, sameRGBA16, natural-GC64 | 350.36 / 405.10 | 0.865 / 0.904 | 1,151,529.875 direct |

Both fresh-state controls pass on the current immutable package. Production is
unchanged; this qualifies an additional reference-lifetime view. The original
retained-context word post-GC result remains a gap, and its natural-GC result
passes only that scope. Those earlier captures use their original source-valid
modules. Neither their conditioning difference nor native preparation time is
measured by subtracting these historical means. No overall warm or production
speedup follows from this round.

All complete packets match the frozen references: 533,444 B RGBA8 and 1,066,552 B
RGBA16, including all samples, hidden RGB, metadata, stored-block boundaries and
checksums. Inputs, compression0/filterNone/color6, CPU0, one thread, warm JIT,
nonconcurrent workstation GC, four managed operations per actual and idle-v2
guards stay fixed. Native adaptive batches retain the 50 ms target; every actual
and both brackets are retained. An initial three-actual word screen precedes one
independent twelve-actual confirmation per depth.

Managed total allocation includes independently owned caller output. Stored
deflater storage remains 69,632 B payload / 73,728 B admission, with zero scratch
retained after the operation. Native allocations and physical peaks are
unmeasured. Both new workers guard against codec state surviving into the next
operation; the word worker also checks its conversion frame has been released.
Existing retained workers, generated sources, identities and captures are intact.

The one-shot encoder scope is now seventeen existing controls: fifteen passes,
two uncertainties. The finite 289-path inventory and unresolved retained-native
comparisons remain required. No settings/shape/quality grid is added. The
[discarded streaming design](PNG-STORED-DIRECT-STREAM.md) remains recorded; its
allocation saving did not demonstrate a CPU gain.

One benchmark-only build and two native adapters suffice. Production, embedded
tests and the package were not rebuilt. Reuse the qualified
[6,497 checks per OS and actual-package proof](STREAMING-BYTE-VECTOR-WEIGHTS.md).
The timed library is the exact current package DLL. New source and binary hashes,
all old/new settings and packet checks, allocation statistics and raw dependencies
are indexed in [the evidence](png-stored-one-shot-boundaries-evidence.json).

Build the additional adapters on the pinned quiet Linux environment with
`bash tools/WarmReference/build-png-one-shot-stored-linux.sh`, then select the two
existing `EncodeNoCompression8/16` cases with `LOKAD_IMAGE_PNG_ONE_SHOT=1`.
The raw commands retain the exact selectors, flags and sampling boundaries.

Raw: png-stored-one-shot-boundaries.tar.xz (662,396 B, 53 verified members), SHA256
26162a101ce263daacb657b6d3036c726523fdd4f071b4063a60f04e649964fd.

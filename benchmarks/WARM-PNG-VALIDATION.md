# PNG16 pixel-validation cost

The existing 513×259 RGBA16 control independently measures **5,090.51 µs** versus
FFmpeg's **2,956.51 µs**: **1.722× / upper1.727**, 17,008 managed B/op. All twelve
actuals, exact descriptors, equivalent complete boundaries, native brackets and
both quiet guards pass. This confirms a gap; the full ≤1× objective remains open.
[Evidence](png-validation-current-evidence.json) retains inputs and samples.

Skipping inverse row filtering was tested and removed: its short measures
5,097.24 µs with unchanged allocation. Both frozen PNG8/16 packets use filter 0
on all 259 rows, so inverse filtering was already a no-op. No independent timing
confirmation, full-suite/package repeat, filter grid or further variant follows.
The original source and exact qualified production binaries are restored.

A six-second CPU profile of the warmed qualified producer yields 5,985 samples:
native BCL inflation accounts for about 45.8%, `ReadLiteralRun` 39.3%, `CountBlock`
5.2%, CRC 3.2% and Adler 0.6%. This is approximate self attribution, without a
new timing claim. The two Deflate walks and literal counting are material;
inverse filtering is not a useful optimization on this control.

The existing actual-package/Windows/Linux qualification remains applicable:
5,040 checks per OS, 98 focused normal/scalar checks and the SAME private package
plus three usage examples. The rejected candidate passed 87 focused checks per
OS and all twelve reference preparations. Production, packet/pixel oracles,
admission/ownership and unrelated valid confirmations are unchanged.

The mirrored, member-verified raw archives named in the evidence retain the
baseline, rejected source/binaries/screen, filter inspection and CPU profile,
including its exact runtime, source, perf map and samples. The next opportunity
is a bounded literal-count scanning strategy, preserving exact cursor, prefix,
failure and cancellation behavior without new ownership or full-frame storage.

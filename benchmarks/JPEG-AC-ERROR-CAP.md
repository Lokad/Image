# JPEG source-relative AC error cap

C129 extends the existing Original AC keep/zero pass to full-resolution4:4:4,
with cumulative additional block DCT error capped at1/32 of its initial
nearest-quantized error. DC and DQT remain unchanged. The cap is source-derived;
the existing measured RGB-quality gates remain decisive. No reference-derived
quantizer, quality-number matching, new frame buffer or heap scratch is introduced.

| Existing513×259 control | Previous B | Current / reference packet B | Source-PSNR gap |
| --- | ---: | ---: | ---: |
| Original4:4:4, RGBA8 | 158,812 | 155,732 / 133,041 | 0.186133 dB |
| Original4:4:4, RGBA16 | 158,827 | 155,795 / 133,043 | 0.187018 dB |

Savings are3,080/3,032 B. Reconstructions change, while both gaps remain within
the unchanged0.25 dB tolerance, alpha is exact, and native/public decoder difference
is at most3 with RMSE0.227. Actual production packets equal the independently
derived C128 prototypes exactly, including normal/scalar public goldens across
RGBA, caller samples, fixed/adaptive, live-cancellable and async boundaries.
Frozen source/reference inputs, native settings and normalization remain unchanged.
These two procedural controls do not establish photographic/diagram compression
claims. Their complete-packet gaps remain about17.1%.

The [19-row ledger](JPEG-COMPLETE-PACKET-AUDIT.md) still has five default size/quality
passes and thirteen open cases. Optional CompactColor remains separate. Passing
4:2:2, grayscale, restart4:2:0 and all declared WebP controls stay stopped;
unaffected packet/quality and qualification evidence is reused.

The cap scan charges64 work units before evaluation; maximum extra AC work is4160
per4:4:4 block. Adaptive estimates include it before event allocation. Async caller
staging also includes existing coefficient-decision work before choosing contiguous
storage. Shared refusal remains terminal. Fifteen focused cases cover cumulative
error, exact-source/tie behavior, DC preservation, work/cancellation before mutation,
the maximum work bound and tight async admission. Existing stream/root/ownership
failure tests pass with the new decisions; independent work/memory limits remain.

Both platforms pass4,907 Release tests (1,998 internal,1,973 public,936 live FFmpeg),
plus87 internal and53 public scalar checks. The same actual private0.2.0 package
and all three examples pass on Windows/Linux. Each platform uses one immutable
production build; unchanged differential, benchmark, native and driver assemblies
are reused. Historical quality-report inputs/DQT/sampling stay fixed; grayscale
and quality100 retain their frozen pixels. New matched-quality evidence qualifies
the changed color coefficients separately.

One quiet-VM three-actual forward-only4:4:4 screen records **38,556.568 µs /
reference3,772.167 µs**, ratio10.221/upper10.976, **563,746 B/op**. Both idle-v2 guards
pass; native allocations are unmeasured. This substantial CPU cost is accepted for
the verified compression gain under the current ordering. No CPU-parity or
independent-confirmation claim is made. Normalization, full DCT/decision work,
admitted events/tables, fresh wrapper/output ownership and release are timed on
one CPU. The reused harness retains historical `fixed-geometric-v1` selector text;
the recorded producer uses adaptive Huffman and capped AC decisions.

[Numeric evidence](jpeg-ac-error-cap-evidence.json) records exact producers,
statistics, allocations, quality reuse and affected historical costs. Raw
qualification is mirrored as `c129-qualified-linux.tar.gz` (2,742,185 B; SHA256
`39b5a873be70b2d65f95b77ed5f50309440a09e7af8c3f21a0dbadac6b559b4c`) and
`c129-qualified-windows.tar.gz` (2,730,631 B; SHA256
`4bb1d4e30f601568282cc5dfbf03185f9c97740ddf6248df33248e1ad27e9714`).

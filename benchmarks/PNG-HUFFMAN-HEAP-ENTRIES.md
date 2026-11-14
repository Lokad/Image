# PNG Huffman heap entries

Gray8's [longer-integration current-code confirmation](PNG-GRAY-CURRENT-PATH.md)
passes/stops and supersedes its displayed standalone cost below. All17equivalent
fresh-state controls now pass. This stage's incremental and other valid family
evidence remains retained.

Compressed PNG tree construction now keeps each exact weight/depth priority beside
its heap symbol. This removes dependent lookups through separate weight/depth
arrays. Signed64 priorities preserve signed32 weight ordering, then subtree depth;
the original right-child tie, removal order, signed addition, overflow handling,
zero-weight skipping, codes and independently owned result arrays remain exact.
Explicit zero-weight flags retain the information needed after tree construction.

The independent Gray8 comparison improves **2048.63→1986.46 µs**:
**62.18 µs / 3.035% filtered raw gain**, with separated confidence. All12actuals
give2061.72→1999.33µs (62.39µs/3.026%); that confidence overlaps. Normalized3.041%
also overlaps; native moves+.006%. No all-actual, normalized or uniform encoder
gain is claimed. Gray8 parity remains **uncertain**, range.968155–1.038242.

| Complete encode, selected controls | Lokad / FFmpeg µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Gray8, Optimal6, fresh states | 1,986.46 / 1,993.40 | .997 / 1.038 | 546,628 | uncertain |
| RGBA8, Fastest1, fresh states | 4,111.01 / 4,361.68 | .943 / .963 | 816,180 | pass |
| Predictable RGBA16, original retained-native boundary | 259,795.57 / 388,008.23 | .670 / .685 | 2,491,252 | pass |

The historical Predictable16 point254.31ms is lower than the new259.80ms result.
One contemporary qualified-DLL capture gives259.32ms, with overlapping raw,
all-actual and normalized confidence against the candidate. No Predictable16
regression or gain is claimed; no successful candidate capture is repeated.

All16affected compressed controls are measured once on the same immutable module,
retaining12actuals, both12sample native brackets and outliers. Every complete
packet, original input/setting, full-depth/hiddenRGB/alpha pixel, output owner and
idle-v2 gate passes. The17fresh-state controls have16passes/zero gaps/one Gray8
uncertainty; passing Fastest8 and other passing controls stop. This is incremental
qualification, not a release-wide refresh or a broader compression-size claim.

Gray8 retains the original513×259 input, Optimal6/filter None/color0 and exact
72,586Bpacket. CPU0, optimized warm JIT and nonconcurrent workstation GC remain.
Managed timing includes the complete public encode, fresh MemoryStream
creation/growth and output disposal, without ToArray. Fresh-native controls include
codec/zlib/frame/conversion/output setup and disposal. Predictable16 retains its
explicitly labeled original preparation boundary. Startup/IPC/hashing are excluded.

Node[286] plus bool[572] replace the two int[572] metadata arrays and int[286]
index heap. Element capacity drops572B; measured total allocation drops600B/op
on every affected control, including array headers and the smaller workspace
object. The existing384KiB admission still precedes allocation. There is no new
owner, shared allowance, session or retained operation scratch. Logical capacity,
total allocation and retained state remain distinct; physical/native/JIT peaks
are unmeasured. Cancellation, refusal and actual async storage lifetimes remain.

A two-second current-package profile yields1035sample locations, including
631ProcessLazy,199WriteTokens and52heap-comparison locations. Samples include
inlining/skid and do not measure phases or predict savings. The first indexed-key
layout removes the105Bcomparison helper but has an inconclusive three-actual
screen2081.71/native1979.43µs. The direct-entry revision's short screen2048.05/
1982.57µs precedes its independent confirmation. Both sources/modules, all raw
actuals, guards, profile data and compiler output remain retained.

The original frozen trees/codes cover empty/tied/sparse/overflow alphabets,
reversed/repeated reuse, cancellation and independent results. The poison test
now covers typed heap entries and zero flags. Signed priority algebra and2304
ordering/extraction checks establish the representation; no new benchmark matrix
or duplicated production implementation is introduced. Codes/Runs source remains
verbatim; compressed callers' changed workspace field offsets are measured,
not described as identical generated bodies.

PngDeflater's582Bconstructor matches normalized instructions/labels/code size;
stored mode never constructs changed tree workspace. Reuse the four source-valid
[stored scopes and natural-GC64 view](PNG-STORED-WORD-NORMALIZATION.md),51PNG
reads/shared-root and unrelated codec/editing/text evidence. Reused costs retain
their measured producer and package; they are not relabeled as new measurements.

Final code passes **6,522 full/live checks per OS**,713internal/247public PNG checks
in normal/scalar/AVX512-off/AVX2-off, then the **SAME actual Release-package consumer
and all three examples** on Windows/Linux. The existing packaged compressed PNG
consumer checks the exact210,973Bpacket,384KiB admission, dictionary slides and
sync/async ownership; full-word color controls also run. Two core/embedded builds
perOS cover both layouts, zero public/harness/native/fixture builds, one final
full/package milestone. Published packages remain immutable; publication is postponed.

[Evidence](png-huffman-heap-entries-evidence.json) pins actual package/module hashes,
current and reused producers and every control. Raw
`png-huffman-heap-entries-qualified.tar.xz`, SHA256
`049ade8e4321908a29fd827787aabdc5403f4001861ff5ca5da0179dfb2467d1`,
3,613,108B/369members, is mirrored/member-verified. Actual0.2.1 development
package457,843B has SHA256
`e6e287a9834252fdf9418422fcfa93729ab16b412008f412939ca2467841e2ad`;
timed/Windows DLL`82d606417326387112a50443981abfeaf32ccf2c7d1c69964af4b665aa96f2a5`,
Linux qualification DLL`061fc234d91b642dd90d2491ea33dde69f1abb8bb8994fb519cb86527a02e8bc`.
The full289-path objective remains open; uncertainties are not presumed gaps.

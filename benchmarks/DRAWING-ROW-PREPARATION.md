# Prepared rectangle-copy rows

CopyRegion prepares first-row offsets and signed strides once, then advances
between valid rows. The same grouped/BCL kernels, logical chunk sequence,
4,096-pixel cancellation checkpoints, clipping, alias direction and public
validation/admission order remain. No buffer, cache, owner, API or fixture branch.
Used cursors equal the former row formula; no unused final-row cursor is calculated.

| Existing complete 513×259 boundary | Lokad / FFmpeg µs | Conservative upper / status | Allocated B/op |
| --- | ---: | ---: | ---: |
| DrawingCopy8 | 1.969 / 1.919 | 1.031682 / gap | 0 |
| DrawingCopy16 | 3.783 / 3.780 | 1.004027 / uncertain | 0 |
| DrawingCopyOverlap8 | 3.669 / 3.778 | 0.979836 / pass | 0 |

Independent complete DLL comparison saves **2.87% / 0.058 µs** for separate byte
copying, interval **.967565–.975058**, with zero allocation. The first cheap screen
and independent confirmation each reuse twelve alternating 8,192-call pairs;
the existing jobs are already short, so no three-pair helper is built. All actuals,
full source/canvas checks and idle-v2 guards remain. This is a scoped incremental
gain, not FFmpeg parity or a percentage added to earlier stages.

Byte copying remains a gap; overlap passes/stops. Word copying is uncertain.
Its managed point is lower than the prior passing 3.797 µs, while the native
reference also moves faster (3.818→3.780 µs). No causal regression or passing
current word result is established. Keep this uncertainty open without assuming
it needs optimization or repeating identical measurements for favorable bounds.

Actual code moves four multiplications into preparation and leaves two in the
chunk path instead of five. Caller size is 1,417→1,357 B; instrumented times are
discarded. A second variant reuses three first-chunk scalars but grows code to
1,499 B without demonstrating added benefit. It is not retained, with all samples
and exact source/module/test snapshots preserved. This is no permanent family ban.

Windows/Linux each pass **6,721 full/live checks** and **25 internal/190 public**
focused normal/scalar checks. The SAME actual private package and all three examples
pass both platforms, including wide multi-chunk aliases, clipped byte/word pixels,
cancellation, shared-budget ownership and recovery. One accepted build per OS plus
one Linux experimental build; no helper/harness/native/fixture/public build. Selected
Linux modules are restored byte-exactly and never rebuilt after timing. One broad/
package milestone only. A preserved-snapshot preflight fails before testing; no
completed job is repeated. Reference preparation starts after the paired result
is written; file birth/mtime evidence and both passing idle guards are retained.

All borrowed drawing remains **0 B/op**, with zero added admission or retained
workspace. Source/canvas payload stays 1,062,936/2,125,872 B for separate byte/word
controls, 531,468 B for overlap. Physical/native/JIT peaks are unmeasured. Other
editing, codec, resize and font/text evidence remains source-valid. Published
packages are unchanged; publication and full 289-path completion remain open.

[Evidence](drawing-row-preparation-evidence.json) retains current costs, original
reference boundaries, producer/package identities, restoration and qualification.
Raw `drawing-row-preparation-qualified.tar.xz`: 6,647,212 B/204 members, SHA256
`d2b513ae62a98351ad517b316129c15f900bbc8d360498a072bd56244a1fa4d5`, mirrored/member-verified with the preceding copy-stage dependency.

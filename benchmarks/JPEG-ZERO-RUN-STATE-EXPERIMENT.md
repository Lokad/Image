# JPEG zero-run state experiment

Neither implementation is selected. JRO669 reuses the selected zero ordinal
instead of recounting its prefix after correction traversal. JRT670 strengthens
that result to one deferred missing-target flag, retiring both the prefix mask
and remaining-run state. A78,336-case independent linear model proves the
malformed-band predicate, including insertion, ZRL, exhaustion and bit63.

| Complete live progressive444 decode, three-actual screen | Managed µs | FFmpeg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Source-valid qualified original¹ | 3,553.06 | 3,351.49 | 1.060 / 1.066 | 169,664 |
| Ordinal reuse | 3,584.77 | 3,354.04 | 1.069 / 1.261 | 169,664 |
| Deferred target validity | 3,551.46 | 3,357.13 | 1.058 / 1.119 | 169,664 |

¹ Retained twelve-actual confirmation. Both candidate intervals are wide and
inconclusive; there is no useful point gain or allocation benefit. No favorable
repetition, independent confirmation, broad production qualification or package
rebuild follows these implementations. The results do not close revised designs.

The frozen132,961-byte packet still produces the exact531,468-byte independent
frame. Timing includes public parse/preparation, entropy, inverse/color, output
ownership and disposal. Native contexts and the declared cancellation asymmetry
remain unchanged. All source/native/ownership and idle-v2 single-CPU gates pass.
No new raster, profile, harness or native build. Four relevant entropy/orchestration
sources match the retained JPR608 trace; it supplies leads, not phase timings.

Both variants pass427 internal/375 public focused checks per normal/scalar mode
on Windows/Linux. Polls, input delivery, canonical reader publication, overflow,
immediate coefficient stores and deferred error order remain exact. The new
terminal-zero-ordinal tests cover found/missing targets and partial error tails
against the independent bitwise reader. Those three useful production-entry tests
remain, with tracked source in this commit. They pass against the restored
qualified decoder on both OSes, normal/scalar, after one necessary embedded-test
rebuild per OS.

Production sources/main modules and public bindings are restored byte for byte
to [qualified shared repair](BICUBIC-SHARED-BYTE-REPAIR.md). Its6,006 full/live,
ISA and SAME actual-package/all-three-example qualification remains valid; the
new tests supplement that proof. The package and displayed costs are unchanged.
There is no renderer dependency or inferred all-path parity.

[Evidence](jpeg-zero-run-state-experiment-evidence.json) pins both variants,
models, actuals, gates, original artifacts and restoration. Mirrored,
member-verified raw `jpeg-zero-run-state-unselected.tar.xz` has15,112,624 bytes /
242 members, SHA256
`1afd61509f51f7ad64f6093a2dd9b920e41012558712fbaef8b05ce919c51a27`.
Continue independent text preparation/blending work; exact compression and pixels
remain ahead of CPU gains.

# Lanczos storage on the current package

All eight displayed comparisons affected by the shared pool change pass on the current
0.2.1 package. The reference uses the original zimg 3.0.6 adapters with the
declared paired operation boundaries. Package DLL `7b59eabe`, package `f4c5b5db`.

| Complete operation | Lokad.Image µs | zimg µs | Conservative upper ratio | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| Prepared RGBA8 enlargement | 3,333.69 | 4,090.78 | 0.830922 | 148 |
| Prepared RGBA16 reduction | 497.44 | 512.29 | 0.983850 | 141 |
| Prepared RGBA8 reduction | 459.28 | 488.52 | 0.946866 | 138 |
| One-shot RGBA8 enlargement | 3,779.99 | 5,260.25 | 0.722898 | 319,224 |
| One-shot RGBA16 reduction | 632.28 | 1,823.31 | 0.354899 | 155,078 |
| One-shot RGBA8 reduction | 591.66 | 1,574.50 | 0.390058 | 138,650 |
| One-shot RGBA16 horizontal | 611.14 | 2,088.67 | 0.301072 | 44,712–44,720 |
| One-shot RGBA16 vertical | 734.50 | 2,054.59 | 0.363035 | 254,672–254,679 |

The six two-axis captures use one runtime with 33 frozen non-core files and
the original generated workload delegates. All twelve actuals and both native
brackets are kept. The one-axis captures reuse the original adjacent driver
with 32 complete calls per actual and equal native brackets. One-shot includes
fresh preparation on both sides. Prepared cases retain caller-owned geometry
and workspace outside the clocks and acquire and dispose each independent output
inside. Natural GC, one pinned CPU and idle-v2 guards apply throughout.

The exact managed frames, original native bounds, frozen inputs and
source/owner checks pass. The prepared plan sizes and one-axis governed
admission probes agree with the previous measurements. GC allocation measures
managed bytes allocated per operation. It is separate from admitted capacity,
retained plans, native scratch and unmeasured physical peaks.

The original one-shot and prepared RGBA16 enlargement results each own
4,264,104 B, above the unchanged 4 MiB retention cap. Both versions take the
same direct allocation and release branches. Their source-valid passing
evidence remains applicable. [Original Lanczos evidence](LANCZOS-IDENTITY-AXIS.md)

This round changes no production code. The existing package qualification is
reused. The ten displayed Lanczos cases all have passing evidence. The original
asymmetric fresh-managed and retained-native timings remain available as setup
diagnostics. [Original setup diagnostics](WARM-CURRENT-JPEG-ROOT-LANCZOS.md)

The mirrored archives retain validation records, all samples, quietness
guards and executable recipes. Their members were hash-verified on both ends.

- `lanczos-storage-current-package-confirmations.tar.xz` has 39 members and 37,736 B. SHA-256 `5f411755fb2b42bab47502d8ce7450da009f5a1346a2a9fa5c2fbebc48b38048`.
- `lanczos-one-axis-current-package-confirmations.tar.xz` has 16 members and 15,448 B. SHA-256 `c024b9037d32e6254c1fa717bc938d21cff639b3e5fda798642c98548747b721`.

[Current evidence](lanczos-current-package-evidence.json)

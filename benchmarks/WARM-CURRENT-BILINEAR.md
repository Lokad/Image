# Current bilinear confirmations

The current goal is independent upper≤1x. The [word row-reuse round](WARM-BILINEAR-WORD-CACHE.md)
supersedes Down16; [exact byte factorization](WARM-BILINEAR-BYTE-FACTORIZATION.md)
supersedes Up8. Both are qualified short gaps. Down8/Up16 retain their paths and
evidence. Historical closed/stopped labels below refer to the completed ≤2x phase.

All four existing bilinear workloads independently pass the 2x warm single-CPU
target on the same qualified production DLL. This resolves uncertainty from
edits to the shared public Resize dispatcher. No production optimization is
needed. Stop these fixed pairs; full required warm coverage remains open.

| Existing case | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op | Actual / selected |
| --- | ---: | ---: | ---: | ---: | ---: |
| Down8 | 656.29 | 386.57 | 1.698 / 1.704 | 157,957 | 12 / 11 |
| Down16 | 783.89 | 416.64 | 1.881 / 1.892 | 290,118 | 12 / 12 |
| Up8 | 3,774.00 | 2,150.06 | 1.755 / 1.762 | 2,235,599 | 12 / 11 |
| Up16 | 2,563.23 | 1,869.40 | 1.371 / 1.382 | 4,498,974 | 12 / 12 |

Each operation starts with the existing original 513x259 RGBA8/16 raster and
returns a fresh independently owned 256x129 downsample or 1027x519 enlargement.
Managed timing covers public admission, geometry, complete triangle filtering,
allocation and disposal. The existing FFmpeg/zscale public graph times all
packed/planar conversions, encoded-space premultiplication/filtering/
unpremultiplication and independent output acquisition/release. Native contexts
and wrappers are warm; each call processes every source sample. The approved
Up8 path includes writable output acquisition and rounded-zero-alpha RGB
clearing. Startup, I/O, IPC, preparation and validation are excluded.

Managed samples match the exact rational reference, including alpha and hidden
RGB policy. Native RGB maximum 1 / RMSE 0.25 remains unchanged; Down8 alpha is exact,
and the previously approved Down16/Up8/Up16 native-only alpha maximum 1 / RMSE 0.25
remains scoped to those cases. Zero-alpha RGB, unchanged input/output hashes,
normal/scalar FFmpeg CLI pixels, source preservation and independent owners
pass before/after timing. No new raster, shape, filter, depth or quality grid.

Each passing three-actual screen precedes an independent twelve-actual repeat
on the same immutable production, benchmark and native worker. Every actual is
retained; the upper includes selected/all-actual uncertainty against the faster
native bracket's lower confidence bound. Both versioned quiet guards pass on
every run. Sequential Release uses CPU0, normal SIMD, one filter thread,
disabled tiering and workstation nonconcurrent GC. Native allocations are
unmeasured; managed allocations include harness costs.

Production, harness and native rebuild counts are zero. Only necessary cached
selection drivers change; every cached child, including WarmDry, receives the
verified assemblies. Production/package inputs remain identical to the existing
[qualification](WARM-JPEG-AC-PLANNING.md): 3,483 Release tests per OS, including
live comparisons, and the same actual package's consumer/all three examples
on Windows/Linux. No whole-library method audit, full-suite/package repeat or
profile is introduced. Older measurements stay historical at their sources.

The [three-case numeric evidence](warm-current-bilinear-evidence.json) retains
all six runs, raw archives, allocations, exact identities, gates and reused
qualification. [Down8 evidence](warm-bilinear8-quiet-evidence.json) separately
records the preceding current-code confirmation. Continue the finite named
coverage checklist with affected adaptive JPEG encodes, one case at a time.

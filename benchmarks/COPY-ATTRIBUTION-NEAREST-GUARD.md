# Remaining copy attribution and nearest uncertainty

CPA1158/NCR1159 change no production code, reference, fixture or required path.
The current immutable package and all qualified passing controls remain stopped.

The original complete RGBA16 forward/reverse copy reference was profiled once on
CPU0, reusing its exact FFmpeg n8.1 worker and frozen input. All instrumented
clocks are discarded. Of **1,998 leaf samples**, 1,340 resolve into libc and 658
into the native worker, including 482 in FFmpeg's image-copy wrapper and 161 in
the original adapter. There are no lost samples. These counts locate execution;
they are not phase costs or a forecast of removable time.

The actual worker reaches the same libc implementation identified by the earlier
standalone IFUNC probe. Mapped instruction addresses confirm AVX-512 aligned bulk
stores with directional copying and unaligned edge handling. This closes the
earlier attribution uncertainty; it does not justify copying upstream code,
fixture-specific alignment assumptions, a managed numerical change or automatic
promotion. The rejected addressing, eight-vector and fixed-tail variants are not
replayed. Full-word snapshots, unchanged input, worker exit and both idle-v2 guards
pass. An initial parser counted callchain entries; `perf script -G` corrects the
leaf-only summary offline without repeating the profile. Original output remains.

Only the original nearest RGBA16 enlargement was then measured, because its prior
capture lacked a passing post-run I/O guard. Down8/Down16/Up8 were not repeated.
The existing worker, original 513×259 input → 1027×519 output, native preparation
boundary, twelve alternating 512-operation pairs and all pixel/ownership gates
remain unchanged. The actual current package DLL was reused without rebuilding.

All actuals and correctness checks completed, but aggregate post-run I/O wait was
**1.4007%**, above the unchanged **1%** limit. CPU0 I/O wait was zero and no
overlapping jobs were found; this does not waive the aggregate gate. The result
remains **unqualified**, with no accepted performance comparison or assumption
that optimization is required. Both guard-rejected captures remain. Diagnose
concrete I/O activity before another confirmation, while independent work proceeds.
An initial historical-receipt path typo failed before preparation or samples;
that preflight and the corrected execution are retained. Result recovery after
the guard failure repeated no operation.

Zero production/harness/reference/package builds or broad qualification repeats.
Reuse HBC1157's 6,772 full/live checks per OS, normal/AVX2-disabled/scalar focused
checks and SAME actual package/all three examples. Production commit `b365f26b`
is pushed; [CI 43960385597](https://github.com/Lokad/Image/actions/runs/43960385597)
passes all three jobs. Compression, ownership and cancellation are unchanged.

[Evidence](copy-attribution-nearest-guard-evidence.json) identifies all producers,
source, captures and qualification reuse. Mirrored/member-verified
`copy-attribution-nearest-guard.tar.xz`: SHA-256
`42f0019b3fceed6bc4e1359b6beed4438761c04dec0fc41831134e9012709984`,
1,468,484 bytes / 48 members. It retains original/corrected parsing, native trace,
mapped libc and worker, input/output, all nearest actuals/validation, failed guards
and executable recipes; linked archives retain unchanged runtimes and the package.
This diagnostic adds no current-cost renderer dependency. All 289 material paths
remain required and publication remains postponed.

# Current bicubic byte attribution

RPA610 audits the existing 513×259 → 256×129 RGBA8 bicubic operation on qualified
Linux `4fa55390`, with no builds, production changes or statistical measurements.
The source-valid cost remains 549.84µs versus 418.61µs, allocating 199,981B.
Managed timing includes geometry, workspace, exact certification and independent
output ownership. The zimg backend retains its graph, input planes and scratch;
per-operation packing, halo, fresh output and disposal remain timed. This is
not an equivalent fresh-graph comparison.

The driver admits 199,840B of workspace: 131,072B row cache, 32,768B weights,
2,048B normalizers, 16,416B decoded row, 17,408B geometry and 128B stack metadata.
Zeroed padding is used by the paired-tap loop. These capacities are not CPU costs.
The current capture locates 965 samples in horizontal filtering, 690 in byte-row
writing and 242 in the driver; its hottest driver blocks convert source rows.
Kernel evaluation has ten samples. Geometry setup has not been timed separately.

**The completed capture failed its post-run I/O guard:** aggregate I/O wait was
1.10%, CPU0 4.4%, above the 1% gate. The pre-guard passed; there were no overlapping
jobs, CPU activity or steal. Trace flushing is only a possible cause. Original
failure and completed logs are retained without rerunning. Locations are diagnostic
leads only, not phase times, passing quiet evidence or a projected speedup.

The next model shares overlapping source windows between neighboring eight-tap
outputs with two-pixel source stride. Prepare eligibility once in unused admitted
geometry capacity; retain ordered multiply/add, certified rounding and all edge,
unequal-tap and portable paths. No public plan, pool or cache is justified yet.
Earlier wide-vector/view regressions remain in the
[shared-driver report](BICUBIC-SHARED-CACHED-DRIVER.md); a revised implementation
needs exact lane/geometry checks and a complete-operation screen.

[Evidence](bicubic-byte-current-attribution-evidence.json) pins sources, JIT,
boundaries, samples, output checks and the failed guard. Its mirrored,
member-verified raw archive retains the trace and reproduction scripts. Current
costs and Windows/Linux/package qualification are unchanged.

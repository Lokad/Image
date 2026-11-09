# Current bilinear enlargement execution

This pre-change capture led to [packed row storage](BILINEAR-PACKED-ROW-STORAGE.md).
That report owns current execution costs; this capture remains attribution.

BDA686 captures the complete frozen Up8 workload once for two seconds, using the
qualified actual-package DLL13016616 and existing worker/fixtures/reference.
All exact managed/native/source/ownership checks and idle-v2 guards pass. CPU0
runs the operation; CPU1 samples. Instrumented elapsed is discarded. Older
upsampling captures predate the current dedicated, factored and row-reuse kernel.

Of1,996 locations,1,258 are in the main byte upsampler,673 in horizontal row
preparation and44 in scalar repair. Maps/JIT are retained. These counts and
sampling skid are not phase times or speedup forecasts.

The preparation loop gathers and premultiplies both input taps for each four-output
batch. At513→1027, that repeats2,048 input-pixel preparations per source row for
only513 distinct pixels. Existing caching prepares259source rows; logical vector
preparations total530,432. A bounded next design prepares132,867 distinct source
pixels into exact Int32 premultiplied rows, then reuses them for interpolation.
These are work counts, not measured CPU savings.

One decoded row needs8,208 B, fitting in the final two output rows'8,216 B of
already owned storage. At the frozen height, all source rows are prepared by
output row516; the protected tail starts at517. A general implementation must
retire the view before that tail, using existing raw preparation afterward and
for tiny/insufficient outputs. Keep original horizontal caching, integer sums,
vertical arithmetic, division, certificates and scalar repair. No new array,
owner, pool, session, unchecked access or numerical policy is proposed.

Extra memory traffic and gather/register pressure may offset reuse. Prove
disjoint lifetimes, full overwrite, exact products, fallback and cleanup, then
measure complete operations before acceptance. Do not infer gains from work or
allocation counts, or enlarge the fixture/settings matrix.

No production, harness, native or fixture build occurs. Existing full Windows/
Linux/scalar and actual-package qualification is reused. Capture postprocessing
initially used the wrong report family; the completed trace and after-guard were
recovered without recapture or another timing run. The confirmed229.19µs gap and
full283-path objective remain; publication is postponed.

[Evidence](bilinear-current-execution-attribution-evidence.json) retains source,
lifetime/count model and reproduction. Mirrored/member-verified raw
`bilinear-current-execution-attribution.tar.xz` has102,824 B /24members, SHA256
`076f1d9c32c664e0b39727ace19d9c5948c9aba3883a48180ccd7752eca189b0`.
Runtime inputs reuse the verified confidence archive. No renderer dependency is added.

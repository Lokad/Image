# Current one-shot RGB16 PNG cost

The existing513×259 RGB16 Optimal6 control now has current evidence with equivalent
one-shot preparation. No production change or new performance case.

| Complete encode | Lokad µs | FFmpeg µs | Ratio / upper | Managed allocated B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGB16 Optimal6, one-shot | 18,413.80 | 17,724.93 | 1.039 / 1.048 | 2,431,484 | 739,472 |

All twelve actuals, including two filtered observations, are retained. Filtered
and all-actual raw/normalized uncertainty remains; conservative lower1.030 confirms
a remaining gap. Native brackets17,747.45/17,724.93µs. This current cost replaces
the historical20.24/15.33ms comparison; it establishes no incremental CPU or
allocation improvement. No unchanged repeat or independent gain confirmation.

Both sides borrow the frozen resident RGBA64LE input and own complete independent
packets. Managed fresh MemoryStream/public EncodePng/output disposal stays timed.
The separate native RGB16 adapter includes fresh codec/options/zlib, packet wrapper,
RGB48BE working frame and identical single-thread SWS_POINT|SWS_BITEXACT conversion
context/options/init/disposal. Existing native workers/builders are unchanged. The prior
reference retained conversion/codec state; relabeling its costs would be incorrect.

The execution smoke and current-cost job preserve the exact739,472B packet
(`4b8a2c3c…`), level6/filterNone/color2, input, normal/scalar CLI samples/packets,
independent output, caller stream and quiet-VM CPU0 gates. Native allocations and
physical/admitted peaks are unmeasured. B/op is complete managed allocation at
four-operation batching, not a claimed incremental saving or retention reduction.

One successful isolated-output Windows harness build and one successful Linux
worker build; each has a corrected compile attempt retained. One smoke driver
build; reuse the frozen existing RGB16 confirmation selector, fixtures and workers.
Zero production/test/package/measurement builds. Correct a smoke-versus-confirm
postprocessing check without repeating completed actuals. Unchanged WCP2815,628
full/live checks perOS, applicable scalar and SAMEactualpackage/all-three-example
proof remain valid. No broad qualification repeat; published packages immutable.

[Evidence](png-rgb16-one-shot-evidence.json) pins sources, producers, boundaries,
gates and raw replay. Full282-path objective remains open; publication postponed.

Mirrored/member-verified `png-rgb16-one-shot-current.tar.xz`: 2,846,728B /204members,
SHA256 `cb30c7736ca063f03c6dedade27905d0d19cd4e69f618fbb3b67ae5d4f0c58d9`. Current source/modules, native
projection/worker/identity, failed/corrected logs and all actuals/brackets/guards
are retained with qualified dependencies.

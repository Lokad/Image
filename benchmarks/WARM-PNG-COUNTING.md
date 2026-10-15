# Shared PNG Deflate counting

RGBA8 PNG decoding at its exact memory limit now independently confirms
**1.821× FFmpeg / upper 1.950×**, at unchanged **4,800 managed B/op**. The previous
confirmation's mean was below two, but its upper was 2.083× and remained open.
Managed time improves 6.8%. Both RGBA16 cancellation controls also independently
clear two on the same accepted build. Every pixel and resource contract stays exact.

| Independent twelve-actual result | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Previous RGBA8 exact limit, upper open | 2,796.44 | 1,428.31 | 1.958 / 2.083 | 4,800 |
| Accepted RGBA8 exact limit | 2,605.93 | 1,430.84 | 1.821 / 1.950 | 4,800 |
| RGBA16 exact limit, live cancellation | 5,265.78 | 2,871.10 | 1.834 / 1.852 | 8,896 |
| RGBA16 default budget, live cancellation | 4,518.51 | 2,873.34 | 1.573 / 1.583 | 8,552 |

The existing separate counting loop now handles every count-only compressed
block, including noncancelable calls. This keeps materialized-output dispatch
out of that loop. The existing reader retains token validation, cancellation
polls, exact atomic bit charges, history/window bounds and failure precedence.
Output materialization, BCL inflation, optional-buffer admission and heap
workspace remain unchanged. No unsafe code, dependency, binding or output cache
is added. Existing exhaustive bit-budget, truncation and materialization controls
cover both token policies; no new test machinery is needed.

The two original 513×259 RGBA8/16 rasters and frozen None/Optimal packets are
reused. Exact logical limits remain 1,235,173 / 3,254,193 bytes, including the
original inflater allowance and two rows. Both reject one byte less. Exact
normal/scalar native pixels, alpha/hidden RGB, source preservation, live-token
identity and two simultaneous output owners pass outside timing. The tokens
are never signaled during measurement.

Both subjects time complete identical-input decoding and independent output
acquisition/release. Native codec/input packet/frame wrappers are reused. Full
owned BE-to-LE conversion is included at word depth; byte depth needs no conversion.
Native has no corresponding caller admission limit. Startup, I/O, IPC, fixture
creation, validation and immutable-option creation are excluded. Native allocations
are **unmeasured**; managed allocations include harness overhead and storage reuse.

Sequential Release runs use one pinned CPU, one codec/conversion thread, normal
SIMD, disabled tiering and workstation nonconcurrent GC. Both quiet guards pass
for every retained run. All twelve actuals are retained for each final pair;
the word pairs select eleven. The upper includes selected and all-actual intervals
against the faster native bracket's lower bound. Production, harness and tests
stay frozen across final screens, regressions, confirmations and qualification.

Only three existing workload types are used. The prior byte screen was reused
because its production/harness matched, then its independent upper failed;
the production change precedes the next confirmation. The prior word confirmation
and default-budget screen remain retained. Full-solution qualification had refreshed
the root harness; its verified measured copy was restored from a prepared child.
The harness and native worker need no production-trial rebuild.

All regression means pass before full qualification. **3,374 Release tests per
OS** pass, including **898 live FFmpeg comparisons**. Focused 113 framing plus
127 public PNG checks pass normal/scalar on both OSes, with two live checks each.
The same actual Release package's consumer and all three examples pass on
Windows/Linux. Measured and qualified owned inputs are identical; publication
documentation is updated afterward.

[Numeric evidence](warm-pngcountall-evidence.json) retains every sample, allocation,
guard, prior result, source/assembly/job/native identity and package qualification.
Stop these three confirmed pairs; remaining material and unmatched coverage stays open.

# PNG decode boundary confirmations

Two existing public boundaries independently clear the warm single-CPU target
on the accepted production build. Full samples, alpha and hidden RGB remain exact.
Production is unchanged; its existing package qualification is reused.

| Independent twelve-actual confirmation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA8 default budget, live cancellation | 1,929.15 | 1,370.88 | 1.407 / 1.469 | 4,456 |
| RGBA16 exact working limit, noncancelable | 5,214.71 | 2,870.48 | 1.817 / 1.821 | 8,896 |

The byte case adds the missing benchmark selector for default-budget public
decoding with a live token, which is never signaled during timing. The word case
reuses its existing selector and exact 3,254,193-byte logical limit, including
the original 256 KiB inflater allowance and two rows. One byte less is rejected.
The original 513×259 RGBA8/16 rasters and frozen None/Optimal PNG packets are
reused. No image, shape, filter, quality or effort enumeration is introduced.

Both subjects time complete identical-input decoding and independent output
acquisition/release. Managed admission, framing and all applicable inflation,
materialization and row processing are included. Native codec/input packet/frame
wrappers are reused, with complete decoding/owned output/release each operation;
word decoding includes full owned BE-to-LE conversion. Native has no corresponding
caller memory-limit option. No output is cached. Startup, I/O, IPC, fixture creation,
validation, token and immutable-option creation are excluded. Native allocations
are **unmeasured**; managed figures include harness overhead and released storage reuse.

Exact native normal/scalar output, geometry/orientation, source preservation,
two simultaneously live output owners, pre-canceled rejection/token identity and
the applicable admission boundary pass outside timing. Sequential Release runs
use one pinned CPU, one codec/conversion thread, normal SIMD, disabled tiering
and workstation nonconcurrent GC. Both quiet guards pass on every run. All twelve
confirmation actuals are selected and retained for both pairs. The upper includes
selected and all-actual intervals against the faster native bracket's lower bound.

Only one benchmark input changes. The same verified immutable production/harness
serves the screens and confirmations; the native worker is unchanged. Existing
**3,374 Release tests per OS**, including **898 live comparisons**, and the same
actual package's consumer plus all three examples on Windows/Linux are reused
from the [counting round](WARM-PNG-COUNTING.md). No production rebuild, full suite,
package proof or method audit is repeated.

[Numeric evidence](warm-png-boundaries-current-evidence.json) retains all actuals,
allocations, guards, source/assembly/job/native identities and qualification reuse.
Stop these confirmed pairs; remaining material and unmatched coverage stays open.

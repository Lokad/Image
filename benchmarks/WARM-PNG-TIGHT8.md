# RGBA8 PNG decoding at the exact memory limit

Complete RGBA8 PNG decoding at its exact working-memory limit independently
confirms **1.964x FFmpeg / conservative upper 1.972x**, at **4,800 managed B/op**.
Every RGB/alpha sample, including hidden RGB, remains exact. Production is unchanged.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 2,694.81 | 1,370.58 | 1.966 / 2.059 | 4,800 |
| Independent twelve-actual confirmation | 2,691.69 | 1,370.50 | 1.964 / 1.972 | 4,800 |

The original 513x259 RGBA8 raster and frozen None/Optimal PNG are reused. The
managed logical limit is exactly **1,235,173 bytes**: encoded input+IDAT+output,
262,144 bytes of original inflater allowance and4,104 bytes for two rows. Optional
whole-filter and8KiB read buffers cannot fit. One byte less is rejected. These
are admission figures; measured allocations include harness overhead and reuse
of released pixel storage. No new image, setting, filter or geometry is introduced.

Both subjects time complete identical-input decoding and independent output
acquisition/release. Managed admission, framing, BCL inflation and row work are
included; immutable options are prepared outside timing. Native codec/input packet/
frame wrappers are reused, with complete decoding/owned output/release every
operation, no conversion and no corresponding caller admission limit. No output
is cached. Exact normal/scalar native CLI samples, geometry/orientation, source
preservation, two live owners and the admission boundary pass outside timing.
Startup, I/O, IPC, fixture creation and validation are excluded. Native allocations
are **unmeasured**.

Sequential Release runs use one pinned CPU, one codec/conversion thread, normal
SIMD, disabled tiering and workstation nonconcurrent GC. Both versioned quiet
guards pass on both runs. All twelve confirmation actuals are retained;
BenchmarkDotNet selects eleven. The upper includes selected and all-actual
intervals against the faster native bracket's lower bound. Both runs use the
same immutable production DLL and harness; the native worker is unchanged.

Only one benchmark input changes. Existing **3,373 Release tests per OS**, including
**898 live comparisons**, and the same actual package's consumer plus all three
examples on Windows/Linux are reused from the [streaming round](WARM-PNG-FAST2.md).
No production rebuild, profile, full suite, package proof or method audit is repeated.

[Numeric evidence](warm-pngtight8-evidence.json) retains all actuals, allocations,
guards, source/assembly/job identities and qualification reuse. Stop this confirmed
pair; cancellation and other material boundaries remain open.

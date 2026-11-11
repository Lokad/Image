# VP8 coefficient-position preparation

VPP788 expands the encoded band/context probability rows to coefficient-position
rows once per frame. Ordinary windows, live cancellation, scalar tails and coding
validation share the same prepared model. Per-coefficient band lookup and plane/band
addressing leave the repeated block loop. Arithmetic, header decisions, quantization,
successful cursors and every cancellation poll remain unchanged.

| Complete decode, frozen predecessor → candidate | Before µs | Current µs | Raw / normalized reduction | Managed B/op, before → current |
| --- | ---: | ---: | ---: | ---: |
| Ordinary opaque VP8 | 5215.68 | 4732.18 | 9.27% / 2.02% | 229,344 → 230,880 |
| Live-token opaque VP8 | 5283.81 | 4803.55 | 9.09% / 3.54% | 229,344 → 230,880 |

Ordinary decoding separates filtered/all-actual raw and normalized intervals;
its raw saving is 483.50 µs. Reference clocks also move materially, so
the 2.02% normalized reduction provides the scoped incremental comparison. Live
raw intervals separate, but its normalized intervals overlap: no causal live gain
or regression is established. Both follow three-actual screens with independent
twelve-actual predecessor/candidate confirmation. All actuals remain; no favorable
repeat follows. The ordinary short raw point was adverse while its native clock
also slowed; that observation remains alongside the independent confirmations.

## Current complete operations

All six affected existing controls refresh on the same immutable candidate.
No fixture, image, setting or required-path grid is added. CPU 0, idle-v2 guards,
frozen source/reference identities, exact managed output, native pixel/alpha
checks and independent source/output lifetimes pass. Each row has 12 actuals
and 12 native samples per bracket.

| Existing control | Lokad.Image µs | Reference µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Native8 | 5,812.70 | 5,963.21 | 0.975 / 0.981 | 240,400 | Pass |
| StreamNative8 | 5,740.96 | 6,025.29 | 0.953 / 1.001 | 240,496 | Uncertain |
| Opaque8 | 4,732.18 | 4,789.11 | 0.988 / 0.995 | 230,880 | Pass |
| OpaqueCancelable8 | 4,803.55 | 4,786.85 | 1.003 / 1.009 | 230,880 | Uncertain |
| SimpleOpaque8 | 4,989.31 | 5,248.00 | 0.951 / 0.995 | 230,880 | Pass |
| ValidateVp8Alpha | 4,605.58 | 5,742.91 | 0.802 / 0.809 | 15,600 | Pass |

Four pass / two uncertain. No prior passing control becomes a demonstrated gap.
Stop the passing controls; live/stream uncertainty does not justify automatic tuning.

The native worker retains its decoder, three conversion contexts and packet/frame
wrappers. Every operation still resets/decodes the full image, allocates chroma,
RGBA16 scratch and independent RGBA output, converts/normalizes all pixels, merges
alpha when present and releases actual storage. Managed frame preparation, output
and disposal stay timed; active cancellation remains an explicit managed boundary.
These retained-context observations remain separate from complete one-shot setup
comparisons. No setup cost is silently removed and no reference worker changes.

## Storage, exactness and qualification

One model array grows from 1,536 to 3,072 payload bytes; all six allocation totals
grow by 1,536 B/op. Descending planes expand in place; a 384 B stack copy protects
each plane because the format band sequence is not monotone. There is no second
model array, pool, cache, session or retained operation state. Adding both the
new capacity and temporary stack to the existing conservative fixed-storage bound
gives 7,028 B, within the existing 8,192 B admission before allocation. Admitted
peaks stay unchanged; physical/native/JIT-stack peaks are unmeasured. Returned
images keep independent charges/lifetimes. Refusal and async cleanup remain terminal.

The encoded 96-row probability syntax and update order remain exact. Only after
the header completes are 192 position/context rows prepared. Source reverse proof
reconstructs all three production files by reverting only preparation/layout and
addressing. Original scalar probability/token oracles retain encoded bands; random
blocks compare every coefficient, cursor and following read. A new test checks all
192 rows, padding and outer guards, including in-place overlap hazards. Existing
memory, cancellation, malformed-input and independent-owner tests remain active.

The earlier source-valid two-second profile is reused: 1,441 of 1,997 locations
fall in the live reader. It is sampled attribution, not removable phase cost.
Inspection of the external reference informs early probability-layout preparation;
no upstream code, constants or test corpus becomes a build input. The unsuccessful
macroblock-window implementations remain preserved and are not replayed here.

Windows/Linux each pass **6,247 full/live checks** (2,938 internal, 2,361 public,
948 differential), plus **1,041 internal / 433 public** in normal/scalar modes.
The focused filter is `Vp8|Webp|Memory`; it includes shared memory checks. Unchanged
numerical ISA paths and JPEG/resize/text qualification are reused. The SAME actual
Release package and all three usage examples pass both OSes. One production and
embedded-test build per OS is reused; no public, harness, fixture or native builds.
All affected timing reconciliation precedes broad/package qualification.

Timed/Windows DLL `72d7e81f0c3c8078e6f52158c3d8b17789a7412481d733d2bdaad9e345949815`;
Linux DLL `0fe25c1038a2c7da5855c96be9d32ff7395257e4dfb668049ddba804987842fe`;
private 0.2.1 package `616a651525e5949473701b8e5a72ea9b61337f57dd2e78f36d4ac8c8452403c6`.
Published packages remain unchanged; publication is postponed.

A non-build CHANGELOG entry left by the preceding package copy was reconciled in
the source manifest before timing. A validation-driver dependency-path startup
failure also precedes all timed actuals. Both failures and restoration evidence
are retained. Completed captures are recovered rather than repeated.

[Evidence](vp8-position-model-preparation-evidence.json) pins complete boundaries,
module/source/input identities, all actuals, scalar correctness and actual-package
proof. Mirrored/member-verified raw `vp8-position-model-preparation.tar.xz`: 19,826,504 B / 863 members,
SHA256 `b44c7d3cb3dba4b615fa28a2fda3cf7b0311f55069fdb1c0bc6f003637e95a87`. Dependencies retain [prior attribution](VP8-LIVE-CURRENT-ATTRIBUTION.md),
[window experiments](VP8-LIVE-MACROBLOCK-WINDOW-EXPERIMENT.md) and the
[qualified predecessor](JPEG-SHARED-BLOCK-SUPPORT.md). Prior model/alpha decode CPU
costs are superseded only on these six paths; encoder packets and unrelated
production evidence remain source-valid. All 289 required paths remain in scope.

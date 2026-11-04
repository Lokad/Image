# Shared bicubic streaming blocks

Two full/packed drivers replace four duplicated schedulers, sharing bounded row
geometry and keeping four depth-specific filtering/publication/replay kernels.
Depth dispatch is per filtered source row or completed output block. The twelve
numeric kernel bodies remain unchanged; there is no pixel interface or new owner,
pool, cache or numerical policy. Production loses 94 lines net.

Capacity follows the existing 4 KiB footprint: full coefficients fit 18 rows rather
than 16 (4,048 B fixed arrays); packed coefficients fit 13 rather than 8 (4,096 B).
The 8×17 full path filters 259 source rows per output column rather than 297.
Every output keeps its original tap order and separate clamped-edge contributions.
These logical counts explain removed work; they are not timings or physical peaks.

Independent before/after twelve-actual confirmations save **66.62 µs / 11.24%** on
RGBA8 and **51.17 µs / 9.95%** on RGBA16, both at 8×17. Filtered/all-actual raw and
conservative bracket-normalized confidence separate. Normalized gains are 11.10% /
9.78%; native clocks are 0.16% /0.19% faster, disclosed. Allocation is unchanged.
The two development stages' six short screens remain; only the completed design
has independent gain proof. No claim assigns the gain between block capacity and
driver consolidation. Other controls below refresh current costs only.

Primary means use established BDN filtering: byte baseline11/12 and candidate12/12;
word baseline12/12 and candidate9/12. All actuals remain. The unfiltered word gain
is9.58%, also with separated confidence. No filtering rule or sampling repeat changes.

| Existing complete resize, input+output+4 KiB | Managed µs | zimg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| 8×17 RGBA8 | 526.05 | 476.90 | 1.103 / 1.127 | 113 | gap |
| 8×17 RGBA16 | 463.39 | 536.59 | 0.864 / 0.877 | 113 | pass |
| 8×4 RGBA8 | 477.05 | 720.24 | 0.662 / 0.696 | 113 | pass |
| 8×4 RGBA16 | 482.45 | 753.46 | 0.640 / 0.648 | 113 | pass |
| 4×2 RGBA8 | 832.68 | 1,235.65 | 0.674 / 0.703 | 114 | pass |
| 4×2 RGBA16 | 685.97 | 1,332.89 | 0.515 / 0.519 | 113 | pass |
| 4×9 RGBA8 | 815.17 | 684.23 | 1.191 / 1.269 | 113 | gap |
| 4×9 RGBA16 | 702.00 | 856.71 | 0.819 / 0.827 | 113 | pass |

All eight existing costs are current on one immutable final producer. Six pass
and stop; RGBA8 8×17/4×9 retain gaps. Every source/settings/ownership/quiet and
original exact-pixel/native-normal/scalar gate passes. No new performance image
or shape/filter grid is added. Claims cover these controls, not all resizing.

Reference is explicitly **zimg 3.0.6, FFmpeg's zscale backend**, with graph/input/
scratch retained before timing. Timed 128/256-pixel clamped halos, deinterleaving,
premultiplied float filtering, independent output allocation/repacking and release
remain. Managed public Resize includes geometry/working preparation, admission,
independent output ownership and disposal. This preserves the declared repeated-
use comparison; no one-shot setup-parity claim follows.

The unchanged ungoverned pixel-storage pool may retain idle backing arrays within
its 4 MiB bound. Rent, complete materialization and return remain timed; results
never share a live lease. Governed operations bypass that pool. B/op measures
new allocations, not output payload, admitted/physical peak or idle retention.
The operation workspace retains nothing after return; complete physical/admitted
peaks and native allocations are unmeasured. Local admission stays 4 KiB, with no
refusal fallback. Exact rounding/repair, tiny-alpha unwind and portable paths remain.

Windows/Linux each pass **5,587 full/live checks**, plus 71 internal / 114 public
affected checks per normal/scalar mode. Twelve new cases cover 13/14 and 18/19
block boundaries/tails and tiny alpha; six corrected full-block cases use 135-wide
inputs and an independent rational oracle. Initial wider inputs selected the
packed path, so their test-only correction gets a public-module rebuild without
a production or benchmark repeat. Existing 17/33/65 controls remain.

The **same actual Release package/all three examples** pass both OSes. Four
packaged full/packed byte/word controls verify frozen pixels, shared admission
refusal, cancellation, surviving ownership and subsequent recovery. Production
uses two immutable builds per OS across the coherent stages; public tests use two
versions, internal/harness/selectors none. Two stale native identities stop before
actuals and receive necessary current-source builds; six other workers are reused.
Their compiler/flags/static zimg/settings and selected halo boundaries remain.
Published packages stay immutable; publication is postponed.

[Evidence](bicubic-shared-streaming-evidence.json) pins current costs, source/
producers, all actuals/confidence decisions, qualification and package identities.
Mirrored/member-verified raw `bicubic-shared-streaming-blocks-qualified.tar.xz`:
7,092,088 B / 757 members, SHA256
`3b26c6b86ff3e3fd65a6bd312e76ec0be7f5f9d3a09b2a2d1594680ffe20d39a`.
Both source/binary stages, originals, stopped preflight, test correction, selectors,
workers, guards and reproduction remain, with dependencies pinned. Other families
reuse valid unchanged-path proof; the complete 282-path objective stays open.

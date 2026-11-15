# Font header validation and complete text workloads

Owned font loading uses an immutable ten-byte glyph-header view and inlines its
existing validator. The complete 757,076-byte copy, eager directory/mapping/glyph
checks, error order, every glyph cancellation checkpoint and ownership remain.
Independent loading saves **4.01 µs / 12.65% raw** and **15.90% reference-normalized**;
filtered/all-actual raw and conservative normalized intervals separate. The reference
clock moves +3.86%. This is a scoped loading gain, not an overall text speedup.

| Frozen public operation / explicitly labeled reference | Lokad µs | Reference µs | Ratio / upper | Managed GC B/op |
| --- | ---: | ---: | ---: | ---: |
| DejaVu Sans Regular load/dispose / FreeType 2.13.2 | 27.71 | 13.64 | 2.031 / 2.046; gap | 176 |
| Complete one-shot text / ImageSharp public integration | 1,200.65 | 6,162.85 | 0.195 / 0.221; pass | 283,276 |
| Complete preparation + eight draws / ImageSharp public integration | 1,882.43 | 6,820.83 | 0.276 / 0.305; pass | 284,628 |

One-shot includes load, all 30 fitting trials and winner layout, ordinary drawing,
fresh canvas/explicit clear, independent ownership and cleanup. Repeated use prepares
once and disposes font/layout before drawing eight independent fresh cleared canvases,
then disposes preparation. Direct batch/8 costs **235.30 / 852.60 µs per draw**.
Preparation alone is **1,066.09 / 6,001.76 µs**, with prepared-owner disposal outside
that diagnostic. Never add stage timings to infer complete costs. Earlier complete
costs are superseded; no causal pipeline gain or regression is established.

FreeType 2.13.2 copy/open/retain/dispose validates lazily while managed loading eagerly
checks mapping/glyph headers; this does not establish general parser equivalence.
Text uses ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 / ImageSharp 3.1.12, including every
public layout/fitting call. Retained raster state differs. The prepared-draw diagnostic
remains 96.99/73.79 µs, upper1.326: a scoped gap, not overall text slowdown. Full16-bit
samples, exact glyph/cluster/line/fit/bounds checks, original frame hashes and approved
visual gates pass. No new face, text, raster or settings grid is introduced.

Allocations, admitted capacity and retention stay separate. Active font bytes remain
757,076 plus the 256-byte descriptor allowance; caller input is borrowed. Default idle
storage stays within two arrays/4 MiB and caller-root loads bypass it. Prepared owned
capacity is 39,904 B, with no retained font/layout; governed peak is 807,708 B. Untimed
heap deltas remain approximate, excluding idle pools; physical/native/JIT peaks are
unmeasured. No allocation or retention reduction is claimed.

The original 384-byte helper call disappears from the actual load constructor;
constructor size grows 2,894→3,325 B. Generated code motivates and checks the design;
it is not speed evidence. One immutable core/embedded build per OS serves the stage;
existing public/harness/reference/font modules are reused. Quiet CPU0, natural GC,
all twelve actuals/reference brackets and idle-v2 guards remain. All **6,712 full/live
checks per OS**, 258 internal/159 public focused checks per normal/scalar mode and
the **same actual Release package/three examples on Windows/Linux** pass. The package
contains the timed Linux DLL, without a post-timing production rebuild. Unchanged
codec/resize/edit/raster/layout/fitting paths reuse evidence. Published releases and
publication postponement remain unchanged; the full289-path objective is open.

[Evidence](font-header-validation-evidence.json) retains identities, memory boundaries,
source reconstruction, confirmations and qualification. Raw
font-header-validation-qualified.tar.xz, 3,413,356 B /166members, SHA256
0babb5ecf37b8d7d392adfae62d8cd0e228b2b60debc1ef2750da7579bfc5880,
is mirrored/member-verified. [Prior storage](FONT-OWNED-STORAGE.md) and
[prior complete pipelines](RELEASED-IMAGE-STORAGE.md) retain history and dependencies.

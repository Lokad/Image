# Shared bounded-band bicubic execution

RBS566/RBC567 uses one source loop for 4/8/16-row mixed-word bands. With local
8 KiB headroom, sixteen rows reduce repeated column-cache setup and overlapping
source conversion. Entry-selected value types make capacities and cache strides
compile-time constants; pixel loops have no interface or dictionary dispatch.
The 4 KiB mode retains eight/four-row scheduling. No heap workspace, pool, session
or public API is added.

Independent complete operations establish these scoped gains against the recent,
unchanged qualified-original confirmations:

- Default Mixed16: **1,076.61 → 917.38 µs**, saving **159.23 µs / 14.79% raw /
  15.40% reference-normalized**.
- TightMixed16: **1,073.70 → 987.47 µs**, saving **86.23 µs / 8.03% raw / 8.71%
  normalized**.

Filtered/all-actual raw and normalized confidence separates for both. Native
points move +0.72%/+0.74%. This is not a blanket resizing gain or parity claim.
Earlier 1,016.78/1,017.38 µs historical points are retained, without substituting
them for contemporary original/candidate evidence. The vertical refresh below
establishes current cost only, without a causal historical gain/regression claim.

| Existing RGBA16 case / output | Lokad µs | zimg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Mixed16 / 1027×129 | 917.38 | 851.87 | 1.077 / 1.079 | 114 |
| TightMixed16 / 1027×129 | 987.47 | 851.08 | 1.160 / 1.164 | 114 |
| TightVertical16 / 513×129 | 730.32 | 591.71 | 1.234 / 1.240 | 113 |

All three remain gaps. Twelve other word controls reuse unchanged filtering and
publication evidence: the fifteen-case inventory remains six passes/nine gaps.
The original 513×259 RGBA16 input, output dimensions, settings, exact managed
rational/certified pixels and scoped native float bounds remain. Complete public
resize, admission, geometry, filtering, independently owned output and disposal
stay timed. The explicitly labeled reference is zimg, FFmpeg zscale's backend,
with graph/input/scratch prepared outside timing. This is not one-shot setup
parity or an FFmpeg CLI comparison.

The first dynamic-capacity implementation saves 130.22 µs on default Mixed16 but
regresses TightMixed16 **22.79 µs / 2.12% raw / 1.67% normalized**, with separated
intervals. The revised entry/core design resolves that demonstrated regression.
Both stages' screens and confirmations remain; they are not pooled or repeated
to obtain favorable results. Original confirmations are reused with their exact
module and complete-operation identities. No shape/filter/quality grid is added.

On the frozen default control, modeled cache resets fall 17→9, horizontal driver
columns 17,459→9,243 and converted source pixels 182,115→157,491. These counts
explain removed work; they are not measured phase times or predicted speedups.
The qualified-original diagnostic agrees with the already valid
[execution attribution](BICUBIC-MIXED-CURRENT-EXECUTION-ATTRIBUTION.md); reuse it.
It does not establish CPU shares for the revised implementation.
The earlier [column-helper split](BICUBIC-COLUMN-PHASE-EXPERIMENT.md) remains
unselected and is not replayed.

Explicit scratch arrays are **7,596 B**, admitted as **8,192 B**, for sixteen-row
execution; bounded mode remains **3,992 / 4,096 B**. Select the strategy from the
local ceiling before shared admission. Refusal is terminal, with no smaller-band
retry. Scratch retained after the operation is zero; the mixed result retains
1,059,864 B until disposal. Existing default pixel-pool retention remains separate
and unchanged. Physical/JIT-stack and native peaks are unmeasured. No allocation
reduction is inferred from scheduling; measured GC remains 114/113 B/op.

Source projections reconstruct the original outside band selection, capacity,
cache stride and admission charge. Sample decoding, tap multiply/add order,
quotients, certificates, rounding and exact repair remain. Cooperative checks
remain at geometry, column and repair boundaries; one column handles at most
sixteen rows. No numerical-policy change or pixel tolerance is introduced.

Windows/Linux each pass **5,888 full/live checks**, plus **66 internal / 688 public**
per normal/scalar mode. The same actual Release package and all three examples
pass both OSes, as recorded in the
[evidence](bicubic-bounded-band-execution-evidence.json). Package consumers exercise
both 4/8 KiB limits against the frozen frame, shared peak/refusal/cancellation,
recovery and independently surviving outputs. Published packages are immutable;
publication remains postponed. Raw identities and reproducibility are pinned in
the evidence; no public/benchmark/native/selector/fixture rebuild is required.
Linux executes SIMD bands and checks exact charged peaks; Windows uses its
portable path within the same envelopes. An initial consumer incorrectly required
SIMD scratch on every platform. Its failure and diagnostic are retained; correcting
that check reuses the same package without repacking or production rebuilds.

Raw `bicubic-bounded-band-execution-qualified.tar.xz`, SHA256
`44d5a1a39b9baad657db64cb42bca0e993158844ed74865cdf3c2873208ec089`,
19,758,948 B / 435 members, is mirrored and member-verified. It retains both
stages, original/candidate identities, complete actuals/guards, source projections,
qualification, initial consumer failures and the actual package. Source-specific
providers are pinned separately. Publication remains postponed.

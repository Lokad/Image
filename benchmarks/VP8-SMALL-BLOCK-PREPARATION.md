# Per-block VP8 candidate preparation

The distortion search now prepares its thirteen border/DC values and packed
source once per block, reusing them across visited modes. It calls the existing
exact geometry and squared-error kernels. Header pruning, strict ties, candidate
order, selected reconstruction and every per-mode cancellation checkpoint remain.
Unvisited modes are not generated. Single-mode RDO consumers retain their actual
entry point through the same preparation; no obsolete compatibility wrapper.

Complete effort2 encoding measures **2,799.65→2,745.35 µs**, saving **54.30 µs/1.94%
raw elapsed** in independent twelve-actual original/candidate confirmations. Both
raw confidence views separate. Native time changes −0.84%; normalized point
reduction is 1.11%. Its filtered interval separates, but the all-actual normalized
interval reaches 1.000652: **relative gain is inconclusive across all views**.
This is a useful elapsed/preparation stage, not proof of normalized gain, equivalence
or parity. No unchanged favorable resampling follows. GC stays **406,705 B/op**.

## Current complete warm costs

Same original deterministic 513×259 inputs/settings, quiet VM, CPU0, Release;
twelve actuals and both native brackets per launch. Normalization, preparation,
admission, fresh partitions, independent packet ownership and disposal stay timed.
The labeled FFmpeg/libwebp reference retains its codec/wrapper/private scratch
contexts as before. Complete packets, decoded pixels/alpha, quality, source and
ownership checks stay exact. Only effort2 has an incremental elapsed claim.

| Existing control | Managed µs | Reference µs | Ratio / upper | Managed GC B/op | Exact packet B, both | State |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| Opaque RGBA8 / effort4 | 13,891.65 | 15,503.56 | 0.896 / 0.898 | 1,201,560 | 72,380 | pass |
| Opaque RGBA16 / effort4 | 13,986.62 | 15,595.93 | 0.897 / 0.899 | 1,201,560 | 72,380 | pass |
| Photograph / effort4 | 9,882.98 | 9,712.50 | 1.018 / 1.022 | 809,233 | 17,688 | gap |
| Diagram / effort4 | 5,494.05 | 5,798.04 | 0.948 / 0.953 | 741,196 | 7,100 | pass |
| Diagram / effort5 | 6,538.42 | 6,440.95 | 1.015 / 1.019 | 741,073 | 7,026 | gap |
| Diagram / effort6 | 9,452.61 | 9,810.83 | 0.963 / 0.993 | 754,505 | 6,818 | pass |
| Diagram / effort2 | 2,745.35 | 2,501.11 | 1.098 / 1.104 | 406,705 | 7,330 | gap |
| RGBA8 + alpha / effort4 | 53,682.97 | 53,470.52 | 1.004 / 1.012 | 7,034,400 | 98,590 | uncertain |
| RGBA16 + alpha / effort4 | 53,286.54 | 53,566.32 | 0.995 / 0.999 | 7,033,584 | 98,590 | pass |

Five pass, three gaps, one uncertain. Effort6 uses the predeclared four-current-
launch confidence envelope, 0.922873–0.993439; its table point is the first launch.
This preserves the established treatment of launch variation. The three additional
launches are fixed in advance, with no builds or favorable retries. Alpha16's pass
is scoped to its current single independent confirmation; Alpha8 remains uncertain.
No broader stability or causal gains on the other eight controls are claimed.

## Ownership and qualification

No owned heap, cache, pool or session is added. The private 52-byte border view and
16-byte packed source live only through the current block's search, within the
existing 16 KiB decision admission. Previously the child prediction call prepared
the same 52 bytes repeatedly. Workspace cannot alias borrowed borders; all required
reads precede trial writes. Scalar execution uses the original rectangular metric.

Both OSs remeasure four existing public method2 controls, normal/live-token:
admitted peaks remain **397,205 B diagram, 930,669 B dense opaque, 54,821 B hidden
RGB/alpha, 51,200 B singleton/alpha**. Packets match the preceding qualified source,
borrowed source/root survive and retained library charges are zero. Source/output
are caller-owned in this diagnostic. These coarse peaks, total GC, retained capacity
and physical/native memory are distinct; physical/native peaks are unmeasured.

Each OS passes **5,825 full/live checks** (2,560 internal/2,317 public/948 differential),
plus 87 internal/239 public per normal/scalar mode. Tests retain native prediction
hashes on fifty contexts for both entries, method0/1/2 statistics/final decision
hashes, thirteen frame controls and independent geometry, snapshots, invalid/aliased
workspace and output canaries. The SAME actual private Release package/all three
examples passes both OSs, including exact async method2 packets, shared admission,
cancellation/recovery and independent survivors. Published releases stay immutable.

One immutable core/embedded build per OS; no public-test, harness, selector, native,
fixture or diagnostic build. Current source audit and one two-second profile choose
this preparation lead; instrumented elapsed is discarded. Summary postprocessing
corrects the packet-field projection across cached harness versions without replaying
timing, tests, builds or packaging. The initial script/archive is retained; final
raw includes the correction. Eight displayed costs refresh, 117 numbers reuse
unchanged paths. This is not a full actual-package performance refresh, a full
283-path pass or a publication step.

[Evidence](vp8-small-block-preparation-evidence.json) retains all confidence views,
identities, current nine controls, memory scope and package proof. Raw `vp8-small-block-preparation-qualified-v2.tar.xz`,
2,145,980 B / 213 members, SHA256
`289597c32d451e82526b2e7b6e7742f2db3b8d680aab3dfde8d8a2a512fbc6c3`, is mirrored/member-verified.
[Compact residual programs](VP8-COEFFICIENT-PROGRAM.md) retain the earlier storage
gain/dense tradeoff and [distortion audit](VP8-DISTORTION-PREPARATION-AUDIT.md)
retains its preceding source insight. Select larger independent codec work next;
stop passing cases, resolve uncertain cases before tuning and preserve this
round's normalized uncertainty.

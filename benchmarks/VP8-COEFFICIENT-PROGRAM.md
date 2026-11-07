# Compact VP8 residual programs

CEP461/462, 2026-11-07. Accepted architectural stage; publication postponed.
[Evidence](vp8-coefficient-program-evidence.json) owns the nine affected existing
complete encode controls. Full parity across283 named paths remains open.

Method2 now retains a private residual program and one400-short block workspace,
instead of a full-frame coefficient raster. A28-bit instruction carries adaptive
base, category/remainder, sign and EOB scheduling. Statistics are recorded in that
same construction walk; final probability selection stays unchanged. Sparse data
replays instructions. A dense residual stores its16levels and context when nine
words are smaller, then calls the actual unchanged coefficient emitter.

This representation choice uses physical word counts before admission. It never
consults remaining shared capacity or retries refusal. An operation-owned linked
sequence admits each actual chunk and metadata before allocation, without replacement
copies. All reconstruction/statistics users finish before block scratch is reused;
instruction users finish before frame disposal. Only independent owned partitions
survive into asynchronous output. No pool, general cache, session or quota is added.
Method2 alpha preparation no longer retains an unused coefficient-storage offer.

The complete frozen effort2 diagram encode independently improves
**2,887.72→2,825.91µs**, saving **61.81µs/2.14%raw and2.15%native-normalized**.
Filtered and all-actual confidence views separate; native movement is+0.012%.
Its exact7,330-byte packet is unchanged. Allocation falls741,386→406,705B/op.
Current reference2,510.76µs/upper1.132 remains a gap. No further resampling follows.

## Current complete warm costs

Dedicated quiet VM, Release, oneCPU0;12managed actuals and12samples per native
bracket with idle-v2 guards. Frozen inputs/settings, exact complete packets,
reconstruction/alpha/quality/source/owner checks remain. Normalization, preparation,
admission, fresh output growth, observation and disposal stay timed. FFmpeg/libwebp
retains the documented codec/wrapper/private scratch contexts. These are current
development-module costs, not a125-row actual-package release refresh.

| Existing control | Managed µs | FFmpeg/libwebp µs | Ratio / upper | Managed GC B/op | Exact packet B, both | State |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| Opaque RGBA8, effort4 | 14,299.50 | 15,518.45 | 0.921 / 0.928 | 1,201,677 | 72,380 | pass |
| Opaque RGBA16, effort4 | 14,150.86 | 15,516.49 | 0.912 / 0.914 | 1,201,560 | 72,380 | pass |
| Photograph, effort4 | 9,933.83 | 9,715.88 | 1.022 / 1.026 | 809,233 | 17,688 | gap |
| Diagram, effort4 | 5,685.47 | 5,820.19 | 0.977 / 0.987 | 741,230 | 7,100 | pass |
| Diagram, effort5 | 6,598.78 | 6,452.60 | 1.023 / 1.026 | 741,073 | 7,026 | gap |
| Diagram, effort6 | 9,478.04 | 9,796.36 | 0.968 / 0.974 | 754,505 | 6,818 | pass |
| Diagram, effort2 | 2,825.91 | 2,510.76 | 1.126 / 1.132 | 406,705 | 7,330 | gap |
| RGBA8 + alpha, effort4 | 53,422.93 | 53,399.30 | 1.000 / 1.009 | 7,034,400 | 98,590 | uncertain |
| RGBA16 + alpha, effort4 | 53,333.30 | 53,367.52 | 0.999 / 1.003 | 7,033,584 | 98,590 | uncertain |

Four pass, three gaps and two uncertain. All nine costs refresh because the shared
frame/statistics/writer dispatch changed; only method2 uses the new representation.
The eight displayed rows refresh;117 numerical rows reuse unchanged paths. Effort5
is retained in this family report. Other cases establish current costs and status,
without incremental CPU claims. Effort6's current module confirmation passes; the
older [four-mode report](VP8-FOUR-MODE-TRELLIS.md) retains its launch variability.
There is no broader cross-launch-stability claim or repetition to seek favorable
points. Decoder/lossless/PNG/JPEG/editing/font paths are unchanged.

## Storage and transition evidence

| Existing public method2 input | Admitted peak before → selected | Initial instruction-only stage | Retained after return |
| --- | ---: | ---: | ---: |
| Diagram | 732,597 → 397,205 B | 413,653 B | 0 B |
| Dense opaque | 886,733 → 930,669 B | 1,259,629 B | 0 B |
| Hidden RGB / alpha | 56,229 → 54,821 B | 54,821 B | 0 B |
| Singleton / alpha | 51,808 → 51,200 B | 51,200 B | 0 B |

Both OSs measure these coarse peaks through complete public encoding, with fresh
roots for normal/live-token operations. Caller-owned source/output storage is outside
these charges. Packets are identical to the qualified original, input remains
unchanged, roots stay borrowed and all library charges end. TotalGC, admitted live
capacity, retained storage and physical/native memory are distinct. Physical peaks and native
allocations are unmeasured. Dense storage still grows43,936B: no blanket
memory reduction is claimed.

CEP461's instruction-only end state independently saved1.55%raw, with all-actual
normalized confidence overlapping, and318,233B/op. The generalization check exposed
its372,896B dense peak increase. CEP462's size-selected literal form is a revised
design, not repeated sampling of that implementation. Preserve both stages and
their adverse/uncertain results. Earlier [two-byte event streams](VP8-SHARED-EVENT-STREAM-INCONCLUSIVE.md)
and [instruction projection](VP8-DISTORTION-PREPARATION-AUDIT.md) remain separate history.

Direct node9/node10 statistics, category/sign order, strict decisions, skip/Y2
contexts and the zero-header-cost policy remain exact. Source projections prove
all other text of the three changed existing files equals the qualified original;
seven decision/transform/statistics kernels are unchanged. Tests compare the actual
production writer/counters over every signed level, positions, categories, contexts,
tails and counter renormalization. They also exercise every observed method2 growth
refusal/cancellation boundary, cleanup/recovery, and independent retained frames and
surviving partitions. No tolerance, format, quality or pixel contract changes.

Windows/Linux each pass **5,790 full/live checks** (2,525internal/2,317public/
948differential), plus73internal/260public pernormal/scalarmode. Unchanged vector
kernels reuse their disabled-ISA evidence. The SAME actual private0.2.1Release
package and all3examples pass both OSs, including exact method2async encoding within
a512KiBsharedroot, cancellation, recovery and independently surviving output.

PackageSHA256`f686130b802260b2bfca18396e65ec096d2882c675f616c6c394fe59d066decd`,
423,187B; symbols`5f1f5f04be30643d7f1bd74b7cb765bbb466738d3d5f4ffc903d750caf6f978b`.
Final Windowscorea68f25c7…/embedded84c1aaff… and Linuxcored1495aa2…/embedded878c969e…
remain immutable throughout qualification. Two production builds per OS cover the
coherent transition; an extra Windows internal-test build adds ownership/domain
coverage. Public-test/benchmark/native/selector/fixture builds are zero. Published releases
remain immutable; publication and full actual-package performance refresh stay open.

Raw`vp8-coefficient-program-qualified.tar.xz`,4,138,584B/333members,
SHA256`e0a0ac3b24ca3221be6b4fa57950efb476d19b5e544569ee20c13b0c79b49bf1`,
is mirrored and member-verified. It retains both source/module stages, qualified
originals, commands, actuals/brackets/guards, the adverse dense audit, consumer and
functional logs, and identity-to-prior-archive maps for unchanged harness/reference
inputs. A first shared-control receipt's overbroad method2SourceNote is clarified
by its actual caseId/effort/commands and the corrected retained script; no data was
replayed or hidden. Current rendering uses one owner for this family; superseded
generation overlays no longer become summary dependencies.

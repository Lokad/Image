# Default bicubic enlargement uses owned output workspace

Eligible RGBA8 bicubic enlargement now selects the existing output-backed execution
before preparing a separate heap cache. Default and bounded operations share its
geometry, prefix/tail scheduling, filtering, publication and exact repair. Remove
the duplicate late dispatch. No new kernel, owner, pool, cache, session or numerical
policy is introduced; word and portable arithmetic remain unchanged.

Independent complete default 513×259→1027×519 enlargement improves
**3,597.17→3,383.47 µs: 213.70 µs / 5.94% raw and 5.53% reference-normalized**.
Both filtered/all-actual confidence views separate. Native time changes −0.43%.
Total allocation falls **751,975→116 B/op**, preserving the exact frozen frame.
The short screen's 8.14% raw point is retained separately.

| Default working limit, original RGBA8 source / zimg | Managed µs | Reference µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Full enlargement, 1027×519 | 3,383.47 | 3,316.54 | 1.020 / 1.032 | 116 |
| Width enlargement, 1027×259 | 1,687.11 | 1,645.00 | 1.026 / 1.036 | 114 |
| Height enlargement, 513×519 | 1,919.70 | 1,890.29 | 1.016 / 1.026 | 115 |

All three remain confirmed gaps. Axial rows refresh affected current costs; they
are not additional causal speedup claims. Every run retains twelve actuals, both
native brackets, exact managed frames/source/independent owners, existing native
pixel bounds and passing idle/process guards. Reuse unchanged tight, word, other
resize and codec evidence; one displayed default row changes, 124 reuse valid paths.

Reference is explicitly zimg 3.0.6, FFmpeg zscale's backend, with graph/input/scratch
prepared before timing. Halo, conversion, filtering, independent output, quantization
and release stay timed. Managed public Resize, geometry/work preparation, admission,
independent output and disposal stay timed. These are repeated-reference comparisons;
they do not establish equivalent one-shot setup or FFmpeg CLI performance.

The preceding bounded audit checks all 22 bicubic implementation files and shared
Lanczos preparation/execution. Existing axis/workspace/row schedulers and geometry
helpers already share much of the machinery. Source duplication alone does not
establish repeated runtime work. Its unchanged-build default/tight comparison finds
a 257 µs filtered point and overlapping all-actual confidence, alongside 751,859 B/op
less allocation. That attribution leads to the dispatch change; the independent
default-boundary confirmation above establishes the accepted gain.

For full enlargement, additional coarse working admission falls 751,876→4,096 B.
The source+output+working bound falls 3,415,396→2,667,616 B. Already admitted output
temporarily holds 254,680 B through 457 prefix rows; its 62 tail rows then retire
those views. Existing 80 B prefix and 4,000 B tail stack frames do not overlap.
These are algebraic capacity bounds, separate from GC totals; whole-operation
physical peak and native allocations are not newly measured. Retained operation
scratch is zero; independent outputs remain charged until disposal. The existing
ungoverned pixel pool's 4 MiB idle bound and governed bypass remain unchanged.

One immutable main/internal/public build per OS passes **5,737 full/live checks**
(2,488 internal / 2,301 public / 948 differential), plus 91 internal/57 public checks
per normal/scalar/AVX512-disabled mode. Two existing opaque/tiny tall controls now
also cover default working limits across prefix/tail retirement, exact rational
pixels, refusal/cancellation cleanup, independent survivors and successful recovery.
The SAME actual private 0.2.1 Release package and all three examples pass Windows/
Linux; its existing full-frame consumer now verifies default output ownership too.
Published packages remain unchanged. No additional production/harness/native/selector
or fixture rebuilds, method audits or repeated confirmations are needed.

The private package is `b97fad2af320b4c10f3f2141f95adbc482632ea7fad4fb8db3cd94b6e95761b2`.
Development costs measure the qualified Linux producer; this is not a 125-row
actual-package release refresh or full 282-path parity. Publication is postponed.
Next audit word enlargement's preparation/storage before selecting another design.

[Evidence](bicubic-default-output-workspace-evidence.json) retains all observations,
uncertainty, source identities, capacity derivation and qualification. Raw
`bicubic-default-output-workspace-qualified.tar.xz`: 2,932,304 B / 175 members,
SHA256 `a4ff4eb24fc61dce4599aae3328bffa0b07a56e991d6b636b6cbc39c53e0317b`,
mirrored/member-verified with pinned reused inputs and reference tooling.

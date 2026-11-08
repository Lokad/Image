# PNG lazy view preparation

Current encoding costs are superseded by [streaming token frequencies](PNG-STREAMING-TOKEN-FREQUENCIES.md); this report retains the lazy stage and its evidence.

Retain one acquired-state design for level6/9 lazy compression. Complete RGBA16
Optimal encoding independently saves **390.08µs/1.83%**; RGBA8 SmallestSize saves
**375.08µs/2.16%**, with exact frozen packets and unchanged paired allocation.
No new workspace, owner, cache, pool, session, option or fixture is introduced.

## Design and boundaries

The sweep borrows byte/head/link/token storage and cancellation once, prepares the
level-dependent search policy, and uses bounded window coordinates. The deferred
match retains its64-bit logical origin across slides. Rolling insertion, chain
order/depth, prefix/tail comparisons, extension, lazy ties, block order and
cancellation checkpoints are unchanged. Publish before slides/blocks and in
finally, preserving writer failures and deferred state. A source projection
verifies all other deflater bodies unchanged; original, deterministic exact-packet
and failure tests establish behavior rather than a new implementation-mirroring test.

One bounded profile of the qualified preceding module supplies1177samples,
1106managed/867in ProcessLazy, whose actual code is2192B. It identifies repeated
state access as a lead. Instrumented elapsed is discarded; samples/skid/static
code do not measure phase costs or predict the eventual gain. A copied template's
stage-name typo was corrected while retaining both receipts; numerical module,
source, guard, packet and trace identities never changed.

Reuse the existing513×259 inputs/settings/workers and full-packet gates. Complete
managed operations include packing, fresh history/dictionaries/tokens, growing
independent output and disposal; resident input/JIT/validation are outside timing.
The standalone reference includes fresh FFmpeg codec/zlib/packet preparation,
required conversion, independent output and disposal each operation. Native state
is not retained. Older retained-context comparisons remain historical.

## Independent effect

After one3-pair screen per control, an independent12-pair confirmation alternates
immutable original/candidate order with16complete operations per subject per pair.
All points, short screens and confidence views remain in evidence. Both intervals
are below1. These paired gains do not establish native parity.

| Complete control | Original µs | Candidate µs | Saved µs / percent | Candidate/original interval | Allocated B/op |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA16 Optimal6 | 21,366.85 | 20,976.77 | 390.08 / 1.83% | 0.97798–0.98553 | 2,437,752–2,437,767.5, both |
| RGBA8 Smallest9 | 17,341.67 | 16,966.59 | 375.08 / 2.16% | 0.97513–0.98162 | 820,358–820,373.5, both |

Both subjects retain the small second-position allocation variation, counterbalanced
by alternating order; no causal allocation explanation is asserted. Ten fresh
standalone allocation figures equal their preceding values. Grayscale8 measures
551,772 versus551,364B/op (+408B); no cause is established from these totals.
Total allocations,
peak live capacity and retained storage are distinct: same384KiB coarse deflater
admission, no new arrays/retained capacity, scratch released on disposal. Complete
physical/admitted peaks and native allocation are not newly measured.

## Current complete comparisons

Refresh all11 existing level6/9 costs using the actual immutable Linux candidate;
four Fastest costs reuse unchanged source bodies and their earlier qualified
module. Each comparison retains12actuals, both native brackets, filtered/all-actual
confidence, quiet guards, exact complete packet/pixel/alpha/source checks and
independent owners. No workload grid is added. Nine pass/stop, two remain gaps,
four are uncertain. Preserve adverse points; no favorable repeat or equivalence
claim. Old/new standalone clock changes are not causal production gains.
The async packet matches the retained normal/scalar/native/CLI packet proof;
its unchanged native worker and fresh output boundary are verified again.

| Existing case | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op | Status / evidence |
| --- | ---: | ---: | ---: | ---: | --- |
| EncodeSmallest8 | 16,972.34 | 17,224.29 | 0.985 / 0.993 | 820,452 | Pass/stop; Fresh actual build |
| EncodeSmallest16 | 20,800.66 | 20,840.32 | 0.998 / 1.004 | 2,437,660 | Uncertain; Fresh actual build |
| EncodeFast8 | 4,543.94 | 4,356.87 | 1.043 / 1.059 | 820,916 | Gap; Reused Fastest body |
| EncodeFast16 | 18,449.19 | 18,610.40 | 0.991 / 1.019 | 2,437,660 | Uncertain; Reused Fastest body |
| EncodeFastCancelable16 | 18,727.28 | 18,564.11 | 1.009 / 1.016 | 2,437,660 | Uncertain; Reused Fastest body |
| EncodeFastForward16 | 18,557.49 | 18,594.14 | 0.998 / 1.004 | 2,437,692 | Uncertain; Reused Fastest body |
| EncodeGray8 | 2,053.51 | 1,974.20 | 1.040 / 1.075 | 551,772 | Gap; Fresh actual build |
| EncodeRgb8 | 9,471.66 | 9,769.77 | 0.969 / 0.984 | 819,884 | Pass/stop; Fresh actual build |
| EncodeGrayAlpha8 | 4,276.85 | 4,484.44 | 0.954 / 0.971 | 552,332 | Pass/stop; Fresh actual build |
| Encode8 | 16,932.50 | 17,259.94 | 0.981 / 0.987 | 820,452 | Pass/stop; Fresh actual build |
| Encode16 | 20,534.92 | 20,745.47 | 0.990 / 0.995 | 2,437,660 | Pass/stop; Fresh actual build |
| EncodeRgb16 | 15,576.65 | 16,093.65 | 0.968 / 0.975 | 2,431,492 | Pass/stop; Fresh actual build |
| EncodeGray16 | 4,773.10 | 5,394.24 | 0.885 / 0.902 | 822,740 | Pass/stop; Fresh actual build |
| EncodeGrayAlpha16 | 10,050.55 | 10,479.84 | 0.959 / 0.967 | 1,360,900 | Pass/stop; Fresh actual build |
| Png8AsyncNonSeekable | 16,996.93 | 17,633.09 | 0.964 / 0.968 | 882,212 | Pass/stop; Fresh actual build |

## Qualification and reproducibility

Windows/Linux each pass **5,722 full/live checks** (2483internal/2291public/
948differential),581internal/211public focused checks each normal/scalar, plus
affected AVX512-disabled checks. Frozen generic-write noise/repetition packets
cover origins0/2^40, chunks1/262/65521, slides, overlaps and exact output prefixes;
admission, cancellation, failure cleanup and subsequent root reuse remain covered.
No new test exists solely to mirror this refactoring.

One immutable core and embedded-source test build perOS are reused throughout.
The SAME actual unpublished0.2.1 Release package and all3usage examples pass on bothOS;
the existing generated consumer covers frozen Optimal/Smallest byte/word packets,
source color layouts, shared admission, awaited async output and surviving owners.
The existing consumer is reused without adding fixtures. No extra qualification
is run for unchanged production. Published releases are
unchanged. Publication remains postponed.

All11 affected displayed costs refresh;114others reuse relevant unchanged paths.
This is not a125-row actual-package release refresh or282-path parity completion.
The [evidence](png-lazy-view-preparation-evidence.json) identifies every producer,
input, packet, raw point, source projection, current cost and qualification result.

Main Windows `cef13f75ea7d16f5db273c8b9f04ed5e3a05099a0ba0029e90a5d428849f5f4e`;
Linux `c36eb6d9cbbdd148fa7672826309f2ee08503d63bb6aa46bd060df1ccbb29149`.
Actual package `648629fa74d6de4f182916c117c2de6646132965d2226f368e904ec0a5a5363d` (416,723B).
Raw `png-lazy-view-preparation-qualified.tar.xz`, `94a5284c79cf9feb85774f20b3a9e922b2fc530eb74959c5f08f94e19bcaf635`
(2,698,216B/284members), mirrored/member-verified with its
pinned dependencies; equal payloads are materialized by hash.


The [greedy stage](PNG-GREEDY-VIEW-PREPARATION.md) retains its independent
269/836µs Fastest gains. Earlier experiments remain linked there. Stop this lazy
round and confirmed passing cases; resolve uncertain costs before assuming
optimization, and choose larger independent work from current attribution.

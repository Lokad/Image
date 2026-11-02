# WebP attribution after workspace changes

Two one-second captures reuse the verified trace-storage-stage encoder runtimes,
existing workers, original inputs and packet gates. Their WebP paths are unchanged
by the subsequent bicubic consolidation. No build or production change.

| Existing complete encode | Samples | Selected inclusive attribution |
| --- | ---: | --- |
| Lossless photograph | 793 | Color search 32.2%; prediction 26.0%; clustering 20.7% |
| Transparent lossy, lossless alpha | 779 | ALPH 68.9%; main parsing 27.3%; matcher 15.5%; trace 10.1% |

Inclusive fractions overlap and must not be added. These are sample attribution,
not measured method times, savings or elapsed-time bounds. Instrumented job timings
are discarded. Both quiet guards and exact 160,032 / 98,590-byte packets pass.
The alpha receipt's output-hash field differs from the photo schema; validation
resumed on the completed trace after correcting that field, without recollection
or a changed correctness gate.

Photo color-histogram construction alone receives 123/793 exclusive samples;
color-cost evaluation receives 87/793. The next bounded audit examines what
candidate-independent tile/channel preparation remains repeated across adaptive
coefficient searches. Existing trial-result caching and prepared history costs
already avoid some work. Preserve search order, ties, exact integer costs,
packets, cancellation and admission. This is a lead, not optimization approval.
Alpha matching/parsing remains another independent material opportunity.

Current elapsed costs and qualification stay unchanged; reuse valid evidence
instead of another full/package cycle. Publication remains postponed.
[Evidence](post-workspace-webp-attribution-evidence.json) retains compact selected
attribution and producer/output pins. Raw post-workspace-webp-attribution.tar.xz:
286,680 B / 37 members, SHA256
c42042d699b3ba2ebebcf2ad3e51b7a7c3a7369ef971aac9ef8043b13a7ba182,
mirrored/member-verified. Both original traces, complete analyses, guards and scripts
remain; immutable runtime producers depend on the retained trace-storage archive.

# VP8 luma execution trials

**Discarded and restored.** A per-block streaming selector is slower. A whole-search
specialization gives contradictory independent paired results, so it supplies no
reproducible gain. Keep both directions; the cause is unproven. No favorable rerun,
current-cost substitution, equivalence, overall parity or family-ban claim follows.

| Complete photograph pair | Original / candidate µs | Candidate/original log interval |
| --- | ---: | ---: |
| Streaming per-block selector | 9,693.49 / 10,148.25 | 1.04492–1.04891 |
| Whole-search mode screen | 9,587.53 / 9,498.53 | 0.98804–0.99340 |
| Whole-search independent confirmation | 9,581.32 / 9,712.52 | 1.01236–1.01503 |

The3031Bstreaming helper retains quantizer vectors through quantization/rate scoring,
but reconstruction calls still require spills. It adds a712Bframe, per-block setup
and argument/call work; its parent grows6630→7185B. These observations do not isolate
the cause of the regression. The second design keeps one shared scoring source,
selects a value-only plain/uncancelable SIMD mode once per macroblock, and removes
those per-block additions. Its6406Bcode still does not establish a consistent gain.

All12alternating pairs of32operations, naturalGC,CPU0,idle-v2 and every adverse
sample remain. Full public encoding,source processing,fresh MemoryStream growth,
independent outputs and disposal are timed. The frozen513×259source,Q75/effort4,
exact17688Bpacket and ownership gates pass. All allocate809080B/op; no new heap,
admitted payload or retained state. Physical/native/JIT peaks are unmeasured.

Each version passes1035internal/402public checks per normal/scalar mode, plus166
actual public codegen cases. Two Linux core/embedded builds; zero Windows/public/
harness/native/fixture/helper builds. Original source,main,embedded tests,bindings
and manifest restore byte-exactly without a build. Reuse6685full/live checks perOS
and the SAME actualc39ddab1package/all3examples; no broad/package repeats.

Next inspect a direct work reduction: the plain batched search transforms12modes
but consumes10. A two-candidate tail could eliminate two unused transforms while
preserving exact arithmetic and the existing trellis padding. A16.7%transform-count
reduction is not an elapsed-time claim. No scoring/grid/reference changes.

[Evidence](vp8-luma-execution-trials-evidence.json) pins all source variants, modules,
captures, codegen, guards and restoration. Mirrored/member-verified raw:
vp8-luma-execution-trials.tar.xz, **2484396B/89members**, SHA256
46a362063fc8857ada94ae9f68886b80fddba38fffcb356d0fbb90b976c50c92. Its dependency retains the accepted package, existing helpers,
fixtures and reference sources. This history stays outside renderer dependencies;
current production costs are unchanged. Publication remains postponed.

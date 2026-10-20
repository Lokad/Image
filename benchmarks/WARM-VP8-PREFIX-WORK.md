# WebP prefix statistics under a work limit

The [single-pass alpha successor](WARM-WEBP-ALPHA-SINGLE-PASS.md) supersedes this
work32M packet/cost observation; the prefix-admission and historical proofs remain.

The existing32M-work control shrinks **452,932 → 268,028 B (40.8%)**, with exact
old decoded RGBA, alpha and source quality. It finishes at **31,818,464 work units**
inside the unchanged32M cap, compared with31,961,577 before. Complete-packet size
remains open against the frozen **98,590 B** FFmpeg/libwebp reference. Managed100/
native75, effort4, input, source-quality tolerance and decoder gates are unchanged.

| Existing control | Managed / reference packet B | Managed / reference µs | Managed allocated B/op | Short CPU ratio / upper |
| --- | ---: | ---: | ---: | ---: |
| EncodeBudget8 | 268,028 /98,590 | 21,258.31 / 53,690.92 | 2,174,726 | 0.396 / 0.429 |

The previous whole-frame estimate was a prerequisite for even the small prefix
statistics cache. Limited-work q0 calls can now admit that bounded prefix after
accounting for the worst prediction/transform/header work, prefix work and minimum
token work. Variable arithmetic carries and buffer growth are charged as performed.
This is **eligibility, not a promise the entire operation fits**: every cumulative
work check remains authoritative, and exhaustion stops without a fixed-model retry.
Full-frame/mixed-mode work eligibility and the conservative memory gate are unchanged.
No work units are discounted, quality formula changed or shared allowance created.

The same borrowed root admits existing source/reconstruction storage, the24,064 B
prefix cache and arithmetic growth. Refusal, cancellation and late work exhaustion
refund temporary storage while retained caller images/results keep their charges.
The new packet is exactly the already qualified C58 prefix packet, so its normal/
scalar native decoder proof is reused by packet hash. All four unrestricted current
controls retain identical packets, work and quality. Passing primary content/ALPH
and unrelated codec/edit/font/inspection paths are not retuned.

Both OSes pass **4,635 tests** (1,735 internal,1,966 public,934 live FFmpeg),
163 internal+8 public+2 differential scalar checks, and the same actual private
0.2.0 package/all three examples. Seven new cases cover prefix eligibility,
independent8/16-bit surviving outputs, terminal shared refusal, cancellation,
late actual-work exhaustion and successful subsequent operations after cleanup.
One core/internal/public build perOS; existing differential/reference/harness/
native/selector binaries are reused. No post-freeze producer rebuild or whole-suite
repeat. The single short cost screen retains all three actuals and both idle-v2
guards. Native allocations are unmeasured; CPU parity/repeats remain deferred.

The reused immutable harness retains historical “fixed coefficients” descriptive
fields for this case. The exact packet/header evidence identifies the actual prefix
model; those fields are explicitly superseded in the evidence. Source wording is
corrected for future builds, with invocation, limits, complete operation, gates and
harness binary unchanged. No actual benchmark behavior is patched or relabeled.

The preceding q0 reconstructed/rate-luma screen saved only190–192 B (<0.13%) and
is retained without adoption or timing/full/package qualification; the much larger
remaining gaps take priority. All five procedural packet gates stay open. Only
one existing performance row changes, with the256-case checklist unchanged.

[Structured evidence](vp8-prefix-work-evidence.json) retains original/current
quality, exact hashes/work, cost/allocation data, actual-policy annotation,
qualification and immutable raw-archive identities.

# Lossless WebP references with seeded copy costs

Photograph refinement is superseded by the
[bounded second pass](WARM-VP8L-SECOND-REFINEMENT.md). The other five costs remain
selected after confirming their unchanged paths.

The existing opaque control falls **123,602 → 41,088 bytes**, saving **82,514 B
(66.8%)**, below its **121,896-byte** reference. The transparent RGBA8/16 and async
controls fall **145,648 → 60,904 B**, saving **84,744 B (58.2%)**, below **183,812 B**.
All normalized RGBA, including hidden RGB, remain exact. These controls pass and
stop; this establishes no arbitrary-image compression claim.

Photograph **161,410/ref160,032 B** remains **1,378 B open**. Diagram **7,160/7,234 B**
passes unchanged. All seven lossy gates stay stopped with identical primary VP8
packets/quality anchors and unchanged separate ALPH helpers. Default JPEG and
two PNG complete-packet gaps also remain open.

Cached LEFT residuals seed green/distance coding probabilities from the original
tokens plus one longest/nearest valid three-pixel-key copy per original start.
Strict positive modeled savings rewrite spans at original endpoints; one existing
group/table refit and complete-packet comparison choose the result. The search
bounds history at262144 pixels, length1024,16 matching keys and256 chain visits.
Transforms, cache size, source/reference/settings and original cache-prefix semantics
stay fixed. The actual packet improves on the fixed prototype's41,390 B without
recipe retuning; prototype bytes were never reported as production results.

Search indices, a bounded residual history/lookahead ring, original/new token
capacities, prefix costs and new models are admitted before allocation through the
same borrowed root. Seed models borrow original red/blue/alpha trees and release
only their new green/distance owners. Temporary scratch ends before groupfit;
selected token capacity remains charged through final output. Local guards precede
shared attempts; refusal, cancellation and work exhaustion stop with cleanup.

The six existing complete operations retain original513×259 inputs/settings,
effort4, **libwebp exact=1, FFmpeg's encoder backend**, quality75/method4, frozen
reference hashes,16-to-8 normalization, hidden RGB and independent output ownership.
Normal/scalar managed and FFmpeg pixels are exact. These are **three-actual quiet-VM
cost screens**, retaining selected/all-actual bounds, without independent CPU
confirmation. Native allocations are unmeasured.

| Existing operation | Packet B managed/reference | Managed/reference µs | Ratio/upper | Managed allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| EncodeOpaque8 | 41,088/121,896 | 481,885.87/69,570.25 | 6.927/8.486 | 53,104,872 |
| DiagramLossless8 | 7,160/7,234 | 21,878.12/9,549.20 | 2.291/3.344 | 1,721,182 |
| PhotographLossless8 | 161,410/160,032 | 397,121.83/83,717.78 | 4.744/5.304 | 44,090,320 |
| Encode8 | 60,904/183,812 | 445,941.10/121,336.20 | 3.675/5.157 | 55,238,624 |
| Encode16 | 60,904/183,812 | 464,921.62/121,865.49 | 3.815/4.757 | 55,238,624 |
| WebpLossless8AsyncNonSeekable | 60,904/183,812 | 441,626.63/121,213.42 | 3.643/4.686 | 55,170,368 |

Opaque allocation rises from36,142,008 to53,104,872 B/op; RGBA8 alpha rises from
37,808,768 to55,238,624 B/op. The extra CPU/allocation is accepted for the verified
compression gain. Costs from different rounds are not a controlled speed-change
estimate. No independent timing repeat or speed-only optimization was performed.

Both OSes pass **4,828 tests each**, including936 live FFmpeg comparisons, plus
**93 internal/117 public scalar checks**. Thirteen new cases cover8/16-bit packets,
exact output limits, independent outputs, retained capacity/replacement peaks,
borrowed-tree ownership, local guards, early/late refusal/cancellation/work cleanup
and recovery, variable-alpha hidden RGB, and history/lookahead wrap/rewind.

One immutable production build per OS is reused. Internal tests required one Linux
build and two Windows builds after correcting a test-only group assumption. Public/
differential tests, generated drivers, native workers and selectors were reused.
One Linux harness build adapts untimed sink-failure validation to a packet delivered
in one64KiB chunk; the timed operation is unchanged. The same actual private0.2.0
package and all three usage examples pass on both OSes; published0.1.0 is immutable.
Other relevant qualification remains valid. CPU parity stays deferred.

[Numeric evidence](vp8l-seeded-reference-evidence.json) retains every actual,
allocation, guard, producer identity, quality result and qualification. Mirrored raw:

- `c110-qualified-windows.tar.gz`: `d87501721bdeab97fc48f1875d87676c627c59812d482a6d5f5db45f6f0513bd`
- `c110-qualified-linux.tar.gz`: `1752a1d87449c410e8f927bff5d38b80defc8c3d59f594761077c2f5106ef999`

Actual private package SHA256: `6d15de121b47a0fe5dd5eaf4ecd6a5bb80941e55f5ac79d16baa9fa2559358e2`.

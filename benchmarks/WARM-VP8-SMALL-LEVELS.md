# Constrained WebP small-coefficient decisions

The existing 32M-work packet shrinks **100,544 → 99,448 B**, saving **1,096 B**.
The fixed **98,590 B** ceiling remains open by **858 B**. Six of seven declared
lossy controls pass complete-packet size; this is an incremental gain on the
remaining control, not whole-codec parity.

| Existing control | Managed / reference packet B | Managed / reference µs | Managed allocated B/op | Short ratio / upper |
| --- | ---: | ---: | ---: | ---: |
| EncodeBudget8 | 99,448 / 98,590 | 19,482.25 / 53,662.00 | 2,004,836 | 0.363 / 0.407 |

The fixed local zero/one decision extends to nearest magnitudes 1..4 versus
their immediately smaller levels. Existing segment steps and rate scale remain;
the explicit token saving must offset coefficient error. Common EOB bits cancel,
as do nonzero signs for 2..4. Later coefficient rates remain unmodeled. One DCT
replaces ordinary quantization; source allowance, base map, clustering and the
128-block entropy sample stay fixed. No threshold/lambda/quality/effort grid.

Lossy RGB changes within the unchanged source-quality and native/public decoder
gates. The source-PSNR gap is **0.025713 dB**, within **0.25 dB**. Alpha is exact;
ALPH stays **19,246 B**. Managed 100/native 75, effort 4, frozen inputs, reference
packets/settings and tolerances are unchanged. Matching follows reconstruction
quality, never numerical quality equality. Claims cover these controls only.

The 6,144 B threshold table, 1,028 B costs and block scratch fit an **8 KiB**
stack envelope. An additional **4 KiB** is checked against the independent local
limit, then reserved through the same borrowed root before use. Cancellation is
checked before that reservation; refusal remains terminal. Successful and failed
operations release the charge without GC. No new heap owner, frame, cache or copy.
Two table preparations charge 12,416 units each, initialization 257, and each
performed small-level decision one unit. Work rises **31,696,404 → 31,782,253**
within 32M. No work units or quality constraints are discounted.

Both OSes pass **4,749 tests**, 271 internal + 14 public + 4 differential scalar
checks, and the same actual private 0.2.0 package/all three examples. Added cases
cover signed half ties, independent complete codeword scores, local scratch
admission, shared refusal/cancellation cleanup and subsequent operations. Prefix
cancellation proves no later scratch growth. Existing exact alpha, matched quality,
work exhaustion and independent surviving outputs remain tested. Normal/scalar
packets and managed pixels agree across OSes. One core/internal/public build per
OS; no full-suite repeat or reference/harness/native/selector rebuild.

One guarded three-actual job records the higher CPU cost (about 3.6% above the prior
short mean), accepted for compression. No independent timing repeat or CPU-parity
claim; native allocations are unmeasured. Four unrestricted full records are exact
C85/C83 and primary positive-base paths exclude this decision. Their six costs keep
C83 identities. Unrelated codec/edit/font/inspection and frozen decoder evidence
retains its scope. [Structured evidence](vp8-small-levels-evidence.json) includes
immutable raw, the actual package, quality/work and governed scratch proofs.

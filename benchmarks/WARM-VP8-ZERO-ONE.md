# Constrained WebP zero/one coefficient decisions

The existing 32M-work control shrinks **100,980 → 100,544 B**, saving **436 B**
with the unchanged reconstruction-quality gate and exact alpha. The complete
**98,590 B** reference ceiling remains open by **1,954 B**. Six of seven declared
lossy controls pass; this gain does not establish whole-codec compression parity.

| Existing control | Managed / reference packet B | Managed / reference µs | Managed allocated B/op | Short ratio / upper |
| --- | ---: | ---: | ---: | ---: |
| EncodeBudget8 | 100,544 / 98,590 | 18,808.72 / 53,464.53 | 2,004,965 | 0.352 / 0.406 |

The earlier owned zero/one candidate is reconsidered because current prediction
pruning and admission leave enough work for it without displacing stronger alpha
compression. Its original threshold, segment rate scale and steps are unchanged.
Only magnitude-one nearest seeds can become zero when the explicit token-rate
benefit outweighs additional coefficient error. The shared EOB prefix cancels;
later coefficient rates are not modeled. One DCT replaces ordinary quantization.
Lossy RGB may change; the source-PSNR gap is **0.041936 dB**, within the unchanged
0.25 dB gate. Native/public decoder bounds pass and ALPH stays exactly **19,246 B**.
Managed100/native75, effort4, frozen source/reference/settings and all gates remain
fixed; numerical quality equality is not the matching criterion.

The same 128-block cache remains charged before allocation. Scalar threshold and
cost tables fit the existing 4 KiB stack allowance; no new owner, frame, cache or
input pass. Two table preparations charge 3,200 units each, cost initialization
257, and each performed magnitude-one decision one unit. Local eligibility includes
the worst decision/table envelope. Actual work rises **31,632,851 → 31,696,404**
within 32M. Refusal, exhaustion and cancellation stay terminal, without retries.

Both OSes pass **4,733 tests**, 257 internal + 12 public + 4 differential scalar
checks, and the same actual private 0.2.0 package/all three examples. Ten new cases
cover signed half ties, independent codeword scores, common-prefix cancellation,
work/cancellation before mutation, local admission, shared refusal and subsequent
success. Public 8/16-bit lifetime checks retain the same reconstruction-quality
tolerance, exact source alpha, unchanged inputs/root and independent surviving
outputs across local policies. Normal/scalar packets and managed pixels agree
across OSes. One producer/internal/public build per OS; no full-suite repeat.

One guarded three-actual job records the higher CPU cost, accepted for compression.
CPU parity and independent repeats remain deferred; native allocation is unmeasured.
Four unrestricted full records are exactly C83 and primary positive-base paths are
excluded. Their six cost observations retain C83 identities. All unrelated codec,
editing, font, inspection and frozen decoder proofs retain their scoped validity.
[Structured evidence](vp8-zero-one-evidence.json) includes the old rejected candidate,
changed work prerequisite, immutable raw, actual package and current size/quality costs.

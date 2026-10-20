# Selected-prediction coefficient refinement

Four existing unrestricted WebP controls now pass **complete-packet size and
matched reconstruction quality**. Opaque8/16 shrink **73,740 → 71,154 B** against
**72,380 B**; alpha8/16 shrink **98,760 → 96,708 B** against **98,590 B**.
Source-PSNR gaps are **0.003192/0.022172 dB**, within the unchanged0.25dB gate;
native/public decoder bounds and exact alpha pass. This establishes these fixed
controls only. The work32M packet remains102,006/ref98,590 B, open by3,416 B.

| Existing control | Managed / reference packet B | Managed / reference µs | Managed allocated B/op | Short ratio / upper |
| --- | ---: | ---: | ---: | ---: |
| EncodeOpaque8 | 71,154 / 72,380 | 75,237.69 / 15,555.78 | 1,006,328 | 4.837 / 5.092 |
| EncodeOpaque16 | 71,154 / 72,380 | 76,128.24 / 15,591.97 | 1,006,328 | 4.883 / 5.126 |
| Encode8 | 96,708 / 98,590 | 83,500.36 / 53,596.60 | 1,537,304 | 1.558 / 1.651 |
| Encode16 | 96,708 / 98,590 | 84,953.17 / 53,637.57 | 1,537,304 | 1.584 / 1.761 |

The retained independently authored adjacent-level/EOB selector is reconsidered
under current segment quantizers. It compares coefficient error and the existing
context-dependent rate scale after choosing each4x4 prediction. Final reconstruction
feeds subsequent blocks. This changes lossy pixels; nearest seeds, base quality
mapping, source-conversion policy and quantizers remain unchanged. No quality,
lambda or effort grid is used. Fixed managed100/native75 and effort4 are preserved;
matching is demonstrated by reconstruction quality, never numerical quality equality.
ALPH remains byte-identical18,579 B and passing complete primary controls stay stopped.

One bounded block of state/backpointers fits existing4KiB stack admission, without
another frame/cache/input copy. Added work is included in local eligibility before
shared admission; lower-work paths retain the original search. Each performed
refinement charges4,736 units before mutation, plus selected reconstruction work.
Shared refusal, actual exhaustion and cancellation terminate without retry; owners
and unused reservations end promptly. Actual unrestricted work is199,970,831/
205,502,553 units; the constrained full record remains exactly31,947,691 within32M.

Both OSes pass **4,699 tests**,223internal+12public+4differential scalar checks and
the same actual private0.2.0 package/all three examples. Seven new cases cover
nearest ties, EOB, work/cancellation before mutation, local selection, unchanged
cache ownership, shared refusal and subsequent success. Existing100/78 public
ownership/every-growth/native decode cases run unchanged. Depthpairs and normal/
scalar packets/pixels agree acrossOS. One frozen core/internal-test build perOS;
unchanged public/differential/harness/native/selectors are reused.

Four guarded three-actual jobs record higher CPU costs, accepted for the byte
gains. CPU parity and independent timing repeats remain deferred. Native allocations
are unmeasured. Only these four cost/quality records change; work32M and primary
observations retain their C74/C70 producer identities. Decoder inputs and unrelated
codec/edit/text/inspection proofs remain unchanged. No new workload, calibration,
reference packet/settings or quality tolerance is introduced. Six of the seven
declared lossy controls pass; the constrained packet must still reach its ceiling.

[Structured evidence](vp8-coefficient-refinement-evidence.json) retains exact
source/producer/package, quality/work/ownership, CPU/allocation and raw identities.
The earlier q0-only refinement screen and current coefficient audit remain available;
their old distributions are not substituted for current qualification.

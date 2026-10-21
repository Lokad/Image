# Token histograms close the declared lossy WebP size gates

All **seven declared controls** now pass complete-packet size and unchanged
reconstruction-quality gates. The final 32M-work packet shrinks **99,448 → 98,490 B**,
**100 B below** its fixed **98,590 B** reference. This establishes these frozen
inputs/settings only; broader compression and full CPU parity remain unproven.

| Existing control | Managed / reference packet B | Managed / reference µs | Managed allocated B/op | Short ratio / upper |
| --- | ---: | ---: | ---: | ---: |
| EncodeBudget8 | 98,490 / 98,590 | 19,502.47 / 53,435.71 | 2,036,564 | 0.365 / 0.433 |

The [seven-control ledger](WARM-VP8-SIZE-COVERAGE.md) retains the other passing
packets: photograph 17,366/ref17,688 B, diagram 6,722/ref7,100 B, opaque 8/16
70,298/ref72,380 B and alpha8/16 96,016/ref98,590 B. Reference inputs/settings
stay frozen. Procedural managed 100/native 75 and primary 77/75 versus native 75,
effort 4, are matched by reconstruction quality, not numerical equality.

One token-class counter replaces repeated tree-branch updates. A separate flag
preserves the omitted EOB after explicit zero. Bounded per-context reduction
recovers branch statistics exactly. Cheaper recording supports one **384-block**
cache, derived from the old/new record-work ratio. The original **128-block** fit
still sets coefficient thresholds; the later fit changes entropy alone. Exact
arithmetic replay of both packets confirms all **13,464 signed coefficient blocks**
match. Native/public decoded RGBA, source quality and compressed ALPH also match
exactly. The source-PSNR gap stays **0.025713 dB**, within the unchanged 0.25 dB gate.
There is no quality/prefix/capacity tuning grid or extra source pass.

The retained owner grows **31,744 → 63,232 B**, including 768 B for histogram
capacity, and is reserved before all arrays through the same borrowed root.
Independent local limits can retain the original 128/64 caches before shared
admission; selected shared refusal is terminal. No replacement overlap or new
frame. One context uses 22 stack counters within the existing 8 KiB allowance.
Recording charges 80 rather than 256 units because only one counter is updated
per token; each reduction/fit charges 44,544. Actual work rises
**31,782,253 → 31,802,951**, within 32M. All writer/alpha work remains charged.

Both OSes pass **4,759 tests**, 281 internal+14 public+4 differential scalar
checks, and the same actual private 0.2.0 package/all three examples. Ten new cases
cover token/context equivalence, EOB flags, fit failure before mutation, local
selection, selected-owner refusal/idempotence/recovery and exact cache replay.
Public injections target the actual larger owner without weakening cleanup,
cancellation, source/root or surviving-output assertions. Normal/scalar packets
and managed pixels agree across OSes. One producer/internal/public build per OS;
no full-suite repeat or reference/harness/worker/selector rebuild.

One guarded three-actual cost screen records costs; no independent timing repeat
or CPU-parity claim. Native allocation is unmeasured. Four unrestricted full records
remain exact and primary positive-base paths exclude histogram selection. Six
historical C83 costs retain their producer identities, without a new current-code
confirmation. Unchanged VP8L/JPEG/PNG/edit/font/inspection and frozen decoder proofs
retain scope. Lossless WebP compression is next; default JPEG and PNG complete-
packet gaps and the full finite warm-performance objective remain open.

[Structured evidence](vp8-histogram384-evidence.json) retains exact coefficient/
alpha proofs, immutable raw, actual package, source quality, work and allocations.

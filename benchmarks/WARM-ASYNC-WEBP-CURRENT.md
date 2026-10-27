# Current nonseekable async WebP encoding

The cost screen below is superseded by the [current independent confirmation](WARM-ASYNC-WEBP-CONFIRMATION.md). Validator cleanup and its recorded qualification remain useful evidence.

The existing 513×259 RGBA8 lossless-WebP async control measures 201.47 ms,
1.663× the reference (upper 1.889×), allocating 47,675,312 B/op. This is a guarded
short with all three actual/filtered samples, not independent parity or a speedup
claim. Both complete packets are exactly 183,812 bytes; all RGBA, alpha and hidden
RGB samples match normal/scalar decoding. Input and libwebp exact=1/quality75/
effort4 settings are frozen.

Both complete operations include normalization/repacking, fresh output acquisition,
staging, actual nonseekable delivery/growth and release. Managed WriteAsync is
awaited; neither subject forces scheduling or performs real I/O. Caller storage
remains independent and open. Hashing, validation and final snapshot copies are
untimed. Both idle-v2 guards pass on one CPU.

Preparation exposed ten obsolete benchmark refusal cases left as empty limit
clones after fuel removal. Those cases are deleted; meaningful input/output/local
memory refusal and cancellation checks remain. Six expected metadata records now
include the typed source descriptors. The frozen unmarked three-component JPEG
correctly retains Unknown source color. JPEG sink-failure validation now selects
the first write for packets below 64 KiB. Async VP8L explicitly checks complete
packet equality before timing.

Existing actual validation methods pass nine groups on Windows and eleven on
Linux. The additional two JPEG groups use their pinned Linux-oracle preparation;
Windows is not relabeled to that oracle. Source/packet corruption, limits,
cancellation before/during delivery, sink failure/recovery and retained-output
ownership remain covered. No production code changed. The qualified production
cores, full/scalar regression and same-actual-package proof are reused from
[current qualification](WARM-VP8-TRELLIS-SCAN-DATA.md); no full/package repeat.

Only a binary-reference benchmark harness was rebuilt. Native workers and existing
selectors were reused; earlier measured runtime artifacts stay untouched. An initial
source preflight exited before timing because eight documentation files had not
been synchronized; that failed invocation is retained, separately from this valid
short. [Evidence](warm-async-webp-current-evidence.json) retains controls, identities,
samples and the mirrored, member-verified raw archive. Remaining preparation
receipts require live refresh before use; this is no broader async/settings claim.

# Counterbalanced lossless-photograph comparison

One bounded revised measurement reduces clock separation between managed and
native operations. The existing 513×259 photograph remains **uncertain**, with
conservative ratio interval **0.98970–1.02007**. This proves neither
parity nor an optimization gap. No production change or additional control.

| Complete encode | Lokad µs | libwebp exact=1 µs | Ratio / upper | Managed B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Lossless photograph, effort4 | 83,428.22 | 82,763.93 | 1.008 / 1.020 | 6,516,744 | 160,032 |

Twelve adjacent managed/native pairs alternate order, with four complete operations
per subject; four warmup pairs precede measurement. Twelve native batches before
and twelve after preserve the original bracket policy. All actuals remain; no
pair is discarded. Native before/paired/after means are 82,780.03/83,136.20/
82,763.93 µs. The paired log-ratio interval alone is 0.99231–1.01485. The reported
bound takes the worse of that and all-actual/upper-outlier-diagnostic managed
confidence against the faster before/paired/after native confidence bracket.
Both diagnostic views retain all twelve observations here. No weaker confidence
criterion is introduced to force a pass.

The original public benchmark method includes source processing/admission,
fresh MemoryStream creation/growth, complete encode, ownership and disposal.
Delegate dispatch is included. Native normalization/repacking, pixel import,
complete encoding and independent packet acquisition/release remain timed inside
the worker. IPC, fixture decoding, setup/JIT, hashes and correctness checks are
excluded. Native documented wrappers/BGRA workspace are retained; every pixel is
reprocessed. One quiet pinned CPU, tiering off, nonconcurrent workstation GC.
The reference is explicitly **libwebp exact=1, FFmpeg's encoder backend**.

Managed quality 85/native 75, effort4, original source and complete 160,032-byte
packet stay frozen. Exact samples/alpha/hidden RGB, normal/scalar CLI roundtrips,
unchanged input, independent outputs, worker counters and quiet guards all pass
before/after. Equal quality numbers are not the matching criterion. Managed
allocation remains 6,516,744 B/op in every pair. Native allocations and physical/
admitted peaks are unmeasured; production storage/ownership is unchanged.

One private driver build reuses the existing harness, worker and fixture. Zero
production, test, harness, native, selector or package builds; one measurement job,
no completed-job repeat. Unchanged production reuses the qualified 5,636 checks
per OS, scalar paths and SAME actual Release-package/all-three-example proof.

**Next:** retain the uncertain result without unchanged resampling, and continue
independent demonstrated gaps. The earlier [matcher report](WEBP-MATCHER-CANCELLATION-PREPARATION.md)
retains its historical costs/clock observations. This schedule reconciliation is
not a CPU/allocation gain, regression or broader compression claim. Publication
remains postponed; all 282 material paths remain required.

[Evidence](webp-photograph-counterbalanced-evidence.json) pins scope, source,
producer, confidence and qualified dependencies. Reproduce with the retained
`wpc299-*.py` scripts and private driver on the quiet VM. Raw
`webp-photograph-counterbalanced.tar.xz`: 1,020,904 B / 42 members,
SHA256 `7f1031ae298c2119ebc112adda0e1472a492cd91a200dee6f93c5a00c5716520`. Driver source/DLL, selected
runtime, every pair/bracket, original correctness receipt and guards are retained
and member-verified locally and on the VM, with unchanged source/package/framework
dependencies. No new qualification or timing is inferred from those dependencies.

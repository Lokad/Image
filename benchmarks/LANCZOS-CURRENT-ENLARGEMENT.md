# Current Lanczos enlargement boundaries

The four already named 513×259 → 1027×519 byte/word boundaries now compare
equivalent complete one-shot or prepared execution on both sides. The selected
qualified production DLL, existing worker, harness, selector, inputs and exact
oracle are reused without builds. This resolves old affected costs and preparation
boundaries; it is not a production speedup or regression claim.

| Existing complete operation / zimg | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Plan retained B | Status |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| RGBA8 one-shot | 4,219.49 | 5,228.71 | 0.807 / 0.812 | 319,229 | 0 | Confirmed pass |
| RGBA8 prepared | 3,953.90 | 4,059.53 | 0.974 / 0.978 | 148 | 325,156 | Confirmed pass |
| RGBA16 one-shot | 5,130.98 | 5,107.53 | 1.005 / 1.014 | 4,592,777 | 0 | Confidence open |
| RGBA16 prepared | 4,807.95 | 3,903.64 | 1.232 / 1.239 | 4,265,302 | 333,396 | Confirmed gap |

Byte one-shot and prepared pass and stop. Word prepared has a demonstrated gap;
word one-shot's interval crosses parity. Keep that uncertainty rather than infer
an optimization requirement or repeat sampling merely for a favorable result.
The confirmed prepared word gap is the next attribution target. No enlargement
gain is extrapolated from the prior word-reduction improvement.

One-shot includes geometry, working storage, fresh source processing, independent
output and disposal on both sides. Prepared execution excludes explicit plan/
workspace preparation equally, while processing current source pixels and owning
independent output each call. Managed Q30/rational/certified results remain exact;
the native float path keeps maximum RGB/alpha error ≤1 and RMSE ≤0.25. All frozen
pixel/source/ownership/native checks and both versioned quiet guards pass.

Each independent confirmation retains all twelve actuals and native brackets.
Conservative intervals consider filtered and all-actual statistics. Sequential
Release execution uses CPU0 and the established runtime settings. No short-stage
gain claim, new control grid, changed reference setting or favorable retiming.

Allocations, retained capacity and peaks are separate. Prepared plans retain
325,156 B for RGBA8 and 333,396 B for RGBA16; one-shot retains no useful execution
plan between calls. The harness separately retains validation state outside the
operations. Existing unbudgeted output storage reuse remains unchanged; governed
calls do not use that pool. Native ready receipts report scratch capacity, but
graph/input-plane retention, native allocation and physical peaks are unmeasured.

Unchanged resize-source pins reuse RSP186's eight frozen normal/scalar boundaries.
The selected build reuses its 5,543 full/live checks per OS and same actual Release
package/all three examples on Windows/Linux. No unrelated rebuild, broad suite or
package regeneration. All 282 required paths remain in scope; the displayed
summary gains two rows by replacing two mixed-boundary rows with four existing
equivalent boundaries. This is not a full release refresh; publication postponed.

[Evidence](lanczos-current-enlargement-evidence.json) pins costs, full actuals,
pixels, quality, source/producer identities and unchanged qualification. Raw
`lanczos-current-enlargement.tar.xz`: 6,563,936 B / 60 members, SHA256
`9c14c01d2967fe0713604116b7805e5f9c8df5daa03df35040459bb52cdd45e8`,
mirrored/member-verified. It retains logs, guard receipts, actuals, selected runtime
binaries, current resize sources and reproduction; qualified-core and existing
reference/runtime archives are pinned dependencies.

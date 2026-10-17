# Cancelable progressive JPEG refinement

Updated 2026-10-17. The existing live-token ProgressiveCancelable444 case improves
10.6% on the short screen. **Parity remains open:** these are three quiet actuals,
not an independent ≤1x confirmation. The two original rasters and fixed matrix
remain unchanged.

| Existing case | Prior / current Lokad µs | FFmpeg µs | Ratio / upper | Lokad B/op |
| --- | ---: | ---: | ---: | ---: |
| ProgressiveCancelable444 | 6,477.56 / 5,791.94 | 3,354.98 | 1.726 / 1.798 | 2,682,364 |

The unchanged 132,961-byte progressive packet decodes to independently owned
513×259 RGBA8. Both sides include complete resident decode, normalization/output
materialization and release. One pinned CPU, normal SIMD and the existing FFmpeg
decoder/converter worker are retained. Exact frozen managed pixels, exact alpha,
RGB max4/RMSE1.5, native/scalar CLI checks, deterministic hashes, source preservation
and live independent owners pass. Every actual and both versioned quiet guards
are retained. Native allocations remain unmeasured; managed allocation is unchanged.

The new guarded reader keeps a whole AC refinement block's cursor/cache/work
local. Short Huffman codes retain their original per-code cancellation check;
sign/EOB fields and **every correction bit** retain their original checks and
immediate coefficient updates. Canonical codes and marker tails call the existing
reader after publishing prior work. Every exit publishes exact reader/work state
without another cancellation check replacing the original exception.

Eligibility requires AVX2/BMI2, a live cancelable token, no active local-work
scope and at least 2,048 remaining work units; a block costs less than 2,048.
Tight budgets and unsupported hardware keep the original traversal. There is no
new heap storage, deferred update group or changed cancellation cadence.

One six-second current profile and three existing-case screens bounded the round:

| Candidate | Lokad µs | Disposition |
| --- | ---: | --- |
| Fuse individual correction groups | 6,973.29 | Rejected regression |
| Fuse whole block; immediate per-bit updates | 5,791.94 | Accepted |
| Whole block with speculative grouped updates | 5,742.99 | Rejected: no material gain for added complexity |

Only this live-token timing replaces its old independent result. The four
ordinary progressive cases return through the unchanged preceding fused reader;
baseline JPEG uses its separate block reader. All previous reader/refinement
method bodies and fields are unchanged; one dispatch and new reader/helper/wrapper
are added. The other 206 comparisons, including all 51 passing parity proofs,
remain applicable. Screening reuses the native worker, harness, selector and
fixtures. Full qualification regenerates harness metadata; the original measured
harness is restored afterward, with the screened production DLL byte-identical.

All **3,594 Release tests per OS** pass (1,306 internal, 1,390 public, 898 live
FFmpeg), plus 454 scalar JPEG controls and ten scalar public work/pixel controls.
Fourteen added cases cover live-token original-traversal equivalence, pending
bits, malformed/truncated data, partial updates/work, tight budgets, overflow and
concurrent cancellation's original logical prefix. The same actual Release
package passes its consumer and all three examples on Windows/Linux; packed
symbols match its qualified DLL. Released 0.1.0 packages and tag are unchanged.

[Numeric evidence](warm-jpeg-cancelable-refinement-evidence.json) retains all
statistics, trial/qualification identities and raw archive hashes. The qualified
archive is `parity-p10-qualified-evidence.tar.gz`, SHA256
`78760abc13514dc012838f94ecd444fb092a29c334b626f395e838b265ea4c62`.

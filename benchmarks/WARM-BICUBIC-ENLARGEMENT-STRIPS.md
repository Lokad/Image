# Wider bicubic enlargement strips

The [current TightUp8 confirmation](CURRENT-TIGHT-BYTE-BICUBIC-ENLARGEMENT-COST.md)
supersedes that case's cost below. Other measurements retain their identified
producer and scope; no new causal comparison is established.

Updated 2026-10-18. Fourteen-column strips reduce the seven existing enlargement
times **8.3–17.8%**, with exact pixels and unchanged4-KiB admission. All results
remain **qualified three-actual parity gaps**; every actual and both quiet guards
are retained. No twelve-actual confirmation is run for these demonstrated gaps.

| Existing enlargement | Managed µs | Reference µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA16 / width / default | 1,965.91 | 1,460.21 | 1.346 / 1.467 | 2,129,333 |
| RGBA16 / width / tight | 1,920.59 | 1,453.02 | 1.322 / 1.373 | 2,129,333 |
| RGBA8 / width / tight | 2,125.43 | 1,509.12 | 1.408 / 1.529 | 1,178,425 |
| RGBA16 / both axes / tight | 4,188.86 | 2,870.82 | 1.459 / 1.637 | 4,264,616 |
| RGBA8 / both axes / tight | 4,766.98 | 3,042.21 | 1.567 / 1.693 | 2,493,057 |
| RGBA16 / height / tight | 2,286.62 | 1,672.40 | 1.367 / 1.438 | 2,133,103 |
| RGBA8 / height / tight | 2,614.86 | 1,744.07 | 1.499 / 1.722 | 1,280,402 |

The two original513×259 RGBA8/16 rasters become1027×519 (both axes),1027×259
(width) or513×519 (height). Tight cases admit input+owned output+4096 bytes.
The same complete public admission, geometry/filtering, allocation and disposal
are timed. The explicitly labeled reference is **zimg, FFmpeg's zscale backend**,
with the existing four-pixel clamped halo, source deinterleaving, premultiplied
float B=0/C=.5 filtering, independent planar/packed output allocation,
unrounded-alpha unpremultiplication/quantization and release. Context/private
storage may be warm; every output is freshly computed and independently owned.
Startup, IPC, disk I/O, preparation and validation are excluded. Native allocation
is unmeasured; managed B/op includes harness amortization. No image/settings grid.

One primitive widens the existing8-column strip to14, with21 decoded source
slots instead of15. Its fixed arrays occupy **4000 bytes**, previously2416,
inside the same4096-byte charge. Fewer tiles repeat vertical geometry/source
preparation. Five row slots, per-column tap order, every multiply/add/divide,
quotient certification, quantization, original-order replay and deferred exact
tiny-alpha handling stay unchanged. There is no new heap, persistent state,
reservation or work charge. Reversing only the capacity/comment substitutions
recovers both entire predecessor files. No new profile is needed.

Exactly seven older independent timings are superseded. Default horizontal word
enlargement also executes this strip because the earlier full-row cache rejects
that shape. Other default enlargement rows retain the full cache. All **200 other
comparisons and58 passing proofs** remain applicable; stopped cases are not retuned.
The original managed/native hashes, source/owners and precision gates pass.
Native RGB/alpha max1 and bothRMSE≤.25 remain; only the existing vertical-word
exception allows RGBmax2 at referencealpha≤16, max1 elsewhere. Managed pixels
remain exact, including the already certified rational-half cases.

The two historical tight-horizontal references cite older binaries. Their
existing source reconciliation proves only unselected mode11, an equal HALO_Y
alias and untimed ready changes. The current approved default horizontal workers
perform identical geometry and complete operations; they are reused without
building. All compiler/flags/static archives stay fixed. A metadata preflight
refusal launches no timing. No completed measurement is repeated, baseline
policy changed or tolerance relaxed.

One immutable production/harness/reference/fixture build serves all seven short
screens. The first mean is read before controls in batches of at most three.
The retained dynamic selector is reused with zero harness/native/selector builds.
All **3656 Release tests/OS** pass (1348 internal,1410 public,898 live FFmpeg),
plus163 scalar enlargement/reference controls/OS. Two authored correctness facts
were checked on the predecessor package first: both depths, strip-wrap/odd tails,
identity/enlarged height, direct-order reference, source and independent owners.
These are correctness inputs, not new performance representatives. The same
actual private package passes its consumer and all three examples on Windows
and Linux; symbols match. Timed Linux production bytes remain identical through
qualification. Published0.1.0 package/tag and unaffected scalar proofs remain.

[Numeric evidence](warm-bicubic-enlargement-strips-evidence.json) retains complete
statistics, identities, receipts and qualification. Raw archive
`parity-p36-qualified-evidence.tar.gz`, SHA256
`02b4e1fdaada6d878fa61f3731e62b99d499b3bf9ce44f581f3039a111c6b974`.

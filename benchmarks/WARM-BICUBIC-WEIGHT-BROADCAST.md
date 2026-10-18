# Packed bicubic weight broadcasts

Updated 2026-10-18. Existing tight RGBA8 64×32 bicubic reduction is **12.5%
faster**, with identical pixels, arithmetic order, workspace and allocation.
It remains a **qualified three-actual warm parity gap**, rather than an
independent parity confirmation.

| Existing case | Lokad.Image µs | Reference µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA8,64×32 /4KiB (changed) | 560.51 | 367.05 | 1.527 / 1.797 | 8,313 |
| RGBA8,128×64 /4KiB (unchanged control) | 588.80 | 347.50 | 1.694 / 1.898 | 32,889 |
| RGBA16,64×32 /4KiB (unchanged control) | 620.76 | 391.04 | 1.587 / 1.802 | 16,505 |

Both subjects resize the original **513×259 RGBA8 raster to64×32**. Complete
public Resize includes admission, filtering, fresh owned output and disposal
under input+output+4,096 working bytes. The existing approved **zimg (FFmpeg
zscale backend)** includes a clamped halo16 for this scale, deinterleaving,
encoded-space premultiplied B0/C0.5 FLOAT filtering, unpremultiplication,
quantization and independent output acquisition/release. The128×64 control
retains halo8. Startup, IPC, I/O, setup, hashing and validation are excluded.
Native allocation is unmeasured; managed B/op includes harness amortization.

One current six-second profile records5,981 samples:92.7% in the packed-column
core,53.7% overall in its horizontal loop. It rules out whole-operation fallback
as the material cost. The eight-scalar vector constructor emits a long shuffle
chain for repeating two weights. The accepted **one-expression change** broadcasts
the first weight to eight lanes and uses an explicit AVX512F insert for the upper
four-lane broadcast: two broadcasts and one insert. Every lane bit pattern,
multiply/add, accumulation order, quotient guard, fallback, work charge,
cancellation poll, output boundary and3,992-byte fixed workspace stays identical.
No fields, tables, heap workspace or precision change is introduced.

The first generic join attempt regressed78.7%: its JIT output creates a
loop-carried512-bit stack load/store on each tap. Its source, binaries, short
result and assembly are retained as rejected. That concrete diagnosis justifies
the direct-intrinsic followup. Two zero-timed-iteration JIT checks verify code
generation/pixels without additional CPU profiles. No controls, confirmation,
full suite or package qualification was run for the reject.

Only **Tight64x32_8** replaces selected evidence. Its >20/≤36tap byte packed
body is separate from narrower/deeper supports, default full-row caches and word
kernels. Replacing one expression restores the entire predecessor file.
Unchanged wider-byte/word control means differ by−0.006%/+1.515%; their selected
proofs are reused, as are **206 comparisons and all58 passing proofs**. No new
performance raster, selector, settings grid, quality tolerance or baseline is added.

Frozen managed rational/original pixels, source and independent-owner checks
pass. Native normal/scalar RGB and alpha retain maximum1 and RMSE≤0.25, original
hashes, exact geometry-specific worker/source/build/flags/static-archive pins and
complete boundaries. No worker is rebuilt. An initial preflight used the generic
worker identity; it launched no build/timing and was corrected to the exact
already-prepared64×32 worker.

All **3,680 Release cases per OS** pass (1,359 internal,1,423 public,898 live
FFmpeg), plus138 scalar packed-byte/word/reference checks. Existing138 focused
checks pass for each candidate on bothOS; no duplicate test is added for a vector
constructor. The same actual private package passes its consumer and all three
examples on Windows/Linux, with matching DLL/symbols/documents. Accepted screened
Linux bytes survive qualification unchanged. Released0.1.0 artifacts/tag remain.

Each candidate reuses its immutable producer/harness/selector/reference/fixtures;
only changed production is built once perOS. One private profile driver is built,
without rebuilding production/harness. Four sequential three-actual200ms screens
cover three existing workload types, with50ms native pre/post brackets. All
actuals and both5s `exclusive-vm-idle-v2` guards remain. Means and controls are
read before one accepted-change full qualification/package proof perOS. A1.527×
mean gap does not warrant a routine twelve-actual confirmation. Unaffected prior
qualification is reused. The full207-case parity objective remains OPEN.

[Numeric evidence](warm-bicubic-weight-broadcast-evidence.json) retains all
statistics, allocation, identities, source scope and qualification. Accepted raw
`parity-p45-qualified-evidence.tar.gz`, SHA256
`649874c5ba18681cff1ed43c3eebaaedb533287c820880d910f987fa86d83686`.
Pre-change profile `parity-p45-current-packed-profile.tar.gz`, SHA256
`f99e2033897f20e3083854268f05cef8719b716548952bf9b218e095ab0a3336`.
Rejected generic join `parity-p45-rejected-generic-join.tar.gz`, SHA256
`6698a12c8e3a778f82d6f0110671127fb731afb3df89bd9dd8e93c6cfaa33334`.

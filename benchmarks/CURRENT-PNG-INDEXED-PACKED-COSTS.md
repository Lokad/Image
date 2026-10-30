# Current indexed and packed PNG decode costs

Seven existing controls independently pass warm one-CPU parity and stop. Adam7
Gray1 now also passes through [packed sparse expansion](WARM-PNG-SPARSE-GRAY-EXPANSION.md);
its baseline below is superseded. These are current costs, without a historical
speedup claim.

| Complete 513×259 decode | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Palette8 | 323.424 | 0.879 / 0.886 | 2,472 | Pass; stop |
| Palette4 | 91.103 | 0.676 / 0.682 | 1,000 | Pass; stop |
| Palette2 | 59.686 | 0.590 / 0.596 | 704 | Pass; stop |
| Palette1 | 50.103 | 0.576 / 0.586 | 568 | Pass; stop |
| Gray1 | 57.343 | 0.178 / 0.185 | 504 | Pass; stop |
| Gray2 | 68.301 | 0.601 / 0.619 | 632 | Pass; stop |
| Gray4 | 102.794 | 0.563 / 0.598 | 888 | Pass; stop |
| Adam7 Gray1 | 1,335.074 | 1.083 / 1.094 | 1,440 | Prior; superseded |

All12actuals/control, native brackets, filtered/unfiltered bounds and idle-v2
guards are retained. Both subjects include complete decode, normalization,
independent owned output and release. Frozen packets/projections/settings,
exactRGBA/alpha/hiddenRGB and source/output lifetimes pass. Managed allocations
are B/op; native allocations unmeasured.

Zero builds, newimages/cases/settings or production edits. Reuse the immutable
PAI1core, generic selector, harness/native workers and full5094tests/OS plus
SAMEactualpackage/three examples. No repeated qualification or package proof.
Resolve remaining Paeth costs before assuming which primitive needs work.

25existing costs remain unrefreshed; the full268pathobjective stays active.
[Evidence](current-png-indexed-packed-costs-evidence.json). Mirrored/member-verified
archive current-png-indexed-packed-costs.tar.xz, 85members,
SHA256 2f3c5752d7aaa811e37545f9a027269f0dcbb3beef38db15de246d5957bac33b. Qualified-build dependency pinned by hash.

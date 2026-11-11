# Shared cached-narrow bicubic publication

For current cached costs and package proof, see the
[family status](WARM-COVERAGE.md#current-family-status). Measurements below retain
this stage's design, incremental effects and raw evidence as history.

CNP789 replaces the cached narrow byte row’s private division, certificate and
packing block with the existing shared certified byte publisher. It removes
15 net production lines. Geometry, ordered filtering, zero-weight elision, original
repair, ownership and workspace stay unchanged. No public API or storage is added.

| Complete operation, predecessor → candidate | Before µs | Current µs | Saved µs | Raw / normalized reduction |
| --- | ---: | ---: | ---: | ---: |
| Prepared narrow RGBA8 | 438.33 | 413.36 | 24.97 | 5.70% / 5.97% |
| One-shot narrow RGBA8 | 494.29 | 470.66 | 23.62 | 4.78% / 4.95% |

Both filtered/all-actual raw and normalized confidence intervals separate.
Three-actual screens precede independent twelve-actual predecessor/candidate
confirmations on the same immutable builds. This is a scoped incremental gain;
prepared parity remains uncertain, with conservative upper 1.011. No unchanged
candidate is resampled for a favorable bound.

## Current affected comparisons

| Existing complete boundary | Lokad.Image µs | zimg µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Repeated8 | 413.36 | 418.98 | 0.987 / 1.011 | 137 | Uncertain |
| OneShot8 | 470.66 | 1,042.28 | 0.452 / 0.454 | 199,990 | Pass |
| Down8 | 438.33 | 419.09 | 1.046 / 1.064 | 199,983 | Gap |
| Horizontal8 | 429.99 | 573.30 | 0.750 / 0.758 | 199,979 | Pass |
| Vertical8 | 457.73 | 588.43 | 0.778 / 0.783 | 367,676 | Pass |

Three passes / one gap / one uncertain across five refreshed existing byte costs.
All use the existing 513×259 RGBA8 fixture: narrow outputs are256×129,
horizontal256×259 and vertical513×129.
One-shot includes all geometry/workspace or native graph/input/scratch preparation
and independent output/disposal. Prepared execution retains explicitly owned state
on both sides but processes every source/output sample. Default Down8/Horizontal8/
Vertical8 retain the separately labeled managed-fresh/native-prepared boundary.
The latter’s remaining Down8 gap is not closed by its equivalent one-shot pass.

The reference remains **zimg 3.0.6, FFmpeg zscale’s backend**, with the frozen
integer output, halo, sampling/settings, normal/scalar frames and approved native
float bounds. Existing full managed hashes, exact rational/qualified pixels,
source immutability and independent-owner checks pass; the three Down8 views
produce identical managed bytes. No fixture, filter, shape or quality grid is added.

## Exactness and storage

Only nonwide, nonmixed RGBA8 cached rows reach this publisher; horizontal/vertical
support is at most 16. The existing wider 400/2^40 accumulation bound covers their
unchanged ordered double sums. With normalizers ≥.5 and alpha sums ≥16, the
existing reciprocal14/Newton relative error <2^-27 and critical quotient bound 257
give RGB error <7.852e-6 and alpha error <2.660e-6, below the 2^-16 margin. This
reuses the [shared certificate](BICUBIC-SHARED-BYTE-PUBLICATION.md); it changes no
managed numerical policy. Native float tolerances remain distinct from exact
managed pixels.

The helper refuses without writing on low alpha or rounding ambiguity. The
unchanged original-order evaluation and tiny-alpha rational path retain cleanup
and terminal admission behavior. More low-alpha replay was an explicit risk;
complete measurements include it. The enclosing public operation owns the private
result and releases it on failure; returned images keep independent lifetimes.

No new array, owner, root, pool, cache, reservation or retained capacity is added.
Prepared state remains **199,840 B**; its controlled governed peak remains
**331,936 B**, excluding borrowed input. Existing repair scratch remains available.
Measured total GC is **137–138 B/op prepared** and **199,989–199,990 B/op one-shot**
across the paired confirmations; the one-byte variation is not an allocation-saving
claim. Physical/native/JIT-stack peaks remain unmeasured.

A line-normalized source reverse proof restores the entire original file by
replacing only the publication block. Word, wide, mixed, bounded, geometry and
preparation sources stay unchanged. Two unchanged narrow word boundary costs and
unaffected cached/codec/editing/text evidence are reused within their source scope.

## Qualification and reproducibility

Windows/Linux each pass **6,248 full/live checks** (2,939 internal, 2,361 public,
948 live differential), plus **363 internal / 776 public** resize checks per
normal/scalar/disabled-AVX2/disabled-AVX512 mode. All three named storage suites
are included. Hardware kernels execute where supported. Existing certificate,
exact half, alpha, odd-tail, refusal/recovery and independent-owner tests remain.
A new actual-row check covers low alpha, half-neighbors, poisoned unused rows,
unchanged borrowed storage, zero allocation, pre-cancelled publication and recovery.

The initial Linux test cancelled before constructing an operation context that
correctly rejects cancelled tokens. Its setup is corrected before timing. Only
embedded tests rebuild again; production remains one immutable build per OS.
Both original and corrected test sources/assemblies/logs remain. No harness,
native, selector, fixture or profiler rebuild/capture occurs. Affected performance
reconciliation precedes broad qualification. The SAME actual Release package and
all three usage examples pass Windows/Linux; published releases remain unchanged.

Timed/Windows DLL `9a1498d806db8e60808c6b90f2b58fb2f936262ca72c19e73059c00eae2b12bc`;
Linux DLL `eb2ad53013df4701da0a7b036e51ee89c1380d477a21a8711054f5e14be96dfa`;
private 0.2.1 package `565088e0b39ef05cb13b9d0ed7eed204fb0b1ee7050ea017f33ff56ec01ccccd`.
Publication remains postponed.

[Evidence](cached-narrow-shared-publication-evidence.json) pins producer/source/
input identities, all actuals, guards, capacity, correction and actual-package proof.
Mirrored/member-verified raw `cached-narrow-shared-publication.tar.xz`: 14,873,700 B / 613 members,
SHA256 `39ca3f401345b7d61d66e170df8963a9ac3ce86f4c10f0e27f8c2d81cce750dd`. Pinned dependencies retain the
[qualified predecessor](VP8-POSITION-MODEL-PREPARATION.md) and original
[equivalent narrow boundaries](BICUBIC-NARROW-BOUNDARIES.md). These supersede only
the five affected byte costs; historical and adverse evidence remains reproducible.
All 289 required paths remain in scope.

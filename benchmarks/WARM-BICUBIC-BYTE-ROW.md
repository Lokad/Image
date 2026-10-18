# Bicubic horizontal byte-row output

Updated 2026-10-18. The existing default-budget RGBA8 horizontal enlargement is
**16.4% faster**, with unchanged exact pixels and allocation. All thirteen current
results remain **qualified three-actual warm parity gaps**. Every actual and both
quiet guards are retained; no routine twelve-actual confirmation is run.

| Existing selector | Lokad.Image µs | Reference µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Cross16 | 1,055.90 | 820.33 | 1.287 / 1.393 | 1,263,255 |
| Cross8 | 996.57 | 832.44 | 1.197 / 1.282 | 731,753 |
| Down16 | 744.46 | 573.19 | 1.299 / 1.383 | 464,342 |
| Down8 | 718.10 | 418.49 | 1.716 / 1.840 | 332,177 |
| Horizontal16 | 900.09 | 581.43 | 1.548 / 1.644 | 730,690 |
| Horizontal8 | 852.95 | 574.75 | 1.484 / 1.577 | 465,375 |
| HorizontalUp8 | 2,208.23 | 1,522.50 | 1.450 / 1.533 | 1,930,617 |
| Up16 | 4,354.03 | 2,874.40 | 1.515 / 1.647 | 5,016,578 |
| Up8 | 4,762.86 | 3,043.84 | 1.565 / 1.787 | 3,244,622 |
| Vertical16 | 808.97 | 590.12 | 1.371 / 1.917 | 913,729 |
| Vertical8 | 788.30 | 589.44 | 1.337 / 1.662 | 648,990 |
| VerticalUp16 | 2,136.14 | 1,668.48 | 1.280 / 1.547 | 2,516,605 |
| VerticalUp8 | 2,214.82 | 1,742.24 | 1.271 / 1.449 | 1,664,240 |

The two original 513×259 RGBA8/16 rasters become 256×129 (Down), 1027×519 (Up),
256×519 (Cross), 256×259 (Horizontal), 513×129 (Vertical), 1027×259
(HorizontalUp) or 513×519 (VerticalUp). These are existing default-workspace
selectors, with no new raster, shape or settings grid. Complete public Resize
includes admission, geometry, filtering, fresh independently owned output and
disposal. The approved **zimg, FFmpeg zscale backend** reference includes source
conversion, timed four-pixel clamped halo, premultiplied float B=0/C=.5 filtering,
owned float-planar/packed output, unrounded-alpha division/quantization and
release. Private contexts may be warm; each output is freshly computed. Startup,
IPC, I/O, preparation, hashing and validation are excluded. Native allocation is
unmeasured; managed B/op includes harness amortization.

One primitive outlines the horizontal byte-row writer from the shared cache
method. Byte-specific constants and packing stay local to that emitter. Original
vertical tap order, sums, divisions, byte margins 1e-4/1e-6, range/normalizer and
tiny-alpha guards, original-order replay, clipping/rounding/packing and odd-tail
handling stay unchanged. No identity-tap omission, reassociation, approximation,
workspace, allocation, admission or work change is introduced. Removing one
dispatch and one helper recovers the entire predecessor source text.

A six-second pre-change profile records 5,790 samples: 90.8% in the shared cache
method; 43.1% in JIT blocks containing output normalization, guards and packing,
and 12.7% in vertical accumulation/bounds. These identify sampled code regions,
not individual instruction costs. Exactly thirteen existing selectors executing
the shared heavy cache body are remeasured, including word/cross controls. All
**194 other comparisons and 58 passing proofs** retain their applicable paths.
Original hashes, exact managed pixels, source/owners and approved normal/scalar
native RGB/alpha max1 and RMSE≤0.25 pass. The existing vertical-word enlargement
exception remains RGBmax2 only at reference alpha≤16, max1 elsewhere, alpha max1
and both RMSE≤0.25; managed pixels remain exact.

Controls mostly differ by a few percent from older means. Vertical8 initially
looks 4.14% slower. One additional short check on the frozen predecessor, with
the identical current reference, gives a **0.94%** candidate/predecessor mean
difference. Both short jobs and their uncertainty are retained; no unchanged case
is repeatedly sampled until it passes. Verified snapshots are swapped outside
guards and restored exactly, with zero rebuilds. Two historical vertical worker
identities reuse the existing source reconciliation: an unselected one-pixel
mode, an equal halo alias and untimed ready metadata. Current approved tight-
vertical workers perform the same native operations. No worker rebuild, policy,
tolerance or completed measurement changes.

One immutable candidate/harness/driver/fixture/reference build serves all thirteen
screens, in batches of at most three. All means are read before one accepted-
change qualification. All **3,663 Release tests per OS** pass (1,352 internal,
1,413 public, 898 live FFmpeg), plus 209 scalar cache/reference checks per OS.
One new public check, also passed on the predecessor, covers odd row offsets,
overshoot, zero/tiny alpha and held independent owners. The same actual private
package passes its consumer and all three examples on Windows and Linux, with
matching symbols. Screened Linux bytes survive qualification unchanged; published
0.1.0 packages and tag remain intact.

[Numeric evidence](warm-bicubic-byte-row-evidence.json) retains complete statistics,
identities, scope, the predecessor check and qualification. Raw archive
`parity-p39-qualified-evidence.tar.gz`, SHA256
`841f3b97faeca6057cf52ab09a58e762e06b8aaf2498951bdef22c904842cfac`. Pre-change profile archive
`parity-p39-horizontal-byte-profile.tar.gz`, SHA256
`8555c09c4e22a4a39492734a3a9e166ccb09e54d7a21a321932b05320f736ccf`.

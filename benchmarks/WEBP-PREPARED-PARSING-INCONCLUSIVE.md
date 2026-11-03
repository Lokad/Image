# WebP prepared parsing investigation

One coherent stage prepares exact cache-hit/zero-plane literal prices and splits
long pending-ring updates into at most two contiguous ranges. SIMD strict-minimum
updates preserve sources on ties and retain scalar execution. A second stage uses
retired histogram capacity for guarded coupled literal costs, without input-color
assumptions or another allocation. Both preserve model rounding, token order,
packets, ownership, cancellation and admission.

| Independent complete encode, twelve interleaved blocks | Before ratio / 99.9% interval | Candidate ratio / interval | Raw / normalized point gain |
| --- | ---: | ---: | ---: |
| Transparent RGBA8 / FFmpeg | 1.2276 / 1.2085–1.2467 | 1.2177 / 1.1989–1.2366 | +0.88% / +0.80% |
| Lossless photograph / libwebp exact=1 | 1.0975 / 1.0849–1.1100 | 1.1080 / 1.0957–1.1203 | +0.22% / −0.96% |

Neither raw nor normalized confidence separates. Allocation is unchanged. Native
clock drift is −0.08% / −1.17%; points establish no useful gain or causal regression.
All twelve ratios use the pinned interval without filtering, two complete managed
operations per block and the fastest of six adjacent native samples. Existing
workers, full preparation/normalization, independent output/disposal and original
packet/quality/source/quiet gates are unchanged. Packets remain exactly 98,590 and
160,032 B. Paired ratios are not substituted for global-bracket timings.

Initial three-actual screens lead by 1.48% normalized on alpha and are flat on the
photograph; the apparent 7.9% alpha raw lead largely follows a faster native clock.
Histogram reuse adds no useful lead (alpha1.35%; photograph−0.83%). Select the
simpler first stage from its verified snapshots without rebuilding, then use the
independent comparison above. All adverse points remain; no further sampling.

Each candidate passes 804 internal / 194 public WebP checks per normal/scalar mode
on Windows/Linux. Four helper cases cover signed costs, overflow, strict ties,
sources, unaligned/tail/guard ranges, zero allocation and cancellation-before-writes.
Frozen complete traces/packets and wrap/ownership/refusal/recovery controls pass.
Both candidates retain the original admitted arrays; stage2 reuses 2,048 B after
histogram construction. Physical/live peaks and retained capacity are not newly
measured, and allocation equality implies no CPU gain.

The complete qualified source, core/PDB/XML and internal assemblies are restored
on both OSes, without builds or repeated checks. The obsolete helper and its tests
are removed. The unchanged actual package reuses RSP186's 5,529 full/live checks per
OS, affected normal/scalar and same-package/three-example qualification. Published
packages and current benchmark costs stay unchanged.

[Evidence](webp-prepared-parsing-inconclusive-evidence.json) records both stages,
selection, all paired actuals/intervals and exact restoration. Raw
`webp-prepared-parsing-inconclusive.tar.xz`: 2,681,732 B / 258 members,
SHA256 `e68a859bc2271e259a28709153a5269cda96db5f8218df049130a584d7fdfb8c`,
mirrored/member-verified, with pinned baseline, worker and attribution dependencies.
This closes these implementations; materially different designs remain eligible.

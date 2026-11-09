# Completed JPEG output storage

[Initial spectral AC](JPEG-SPECTRAL-AC-WINDOW.md) supersedes six progressive
costs below and carries the current29-read status; allocation/ownership evidence
and the other23 costs remain source-valid.

JGO607 uses the existing complete-overwrite allocator in the generic completed-plane
renderer. All grayscale, subsampled RGB/YCbCr and quarter-width writers fill every
visible RGBA sample. Parsing, coefficients, numerical kernels, row cancellation,
orientation and private-result failure cleanup are unchanged. Default results reuse
existing exact-size storage after disposal; governed calls reserve the retained
owner before fresh uninitialized storage and bypass the pool. No new pool or owner.

Contemporaneous grayscale confirmations save **98.02µs/5.17%** raw and **5.11%**
native-normalized. Filtered/all-actual raw/normalized intervals separate; all-actual
saving is 97.13µs/5.12%. Allocation falls **683,176→151,432B/op**. The native point
moves −0.06%. This is scoped causal evidence; other historical point differences
are not causal gain claims.

| Complete 513×259 decode | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Baseline420 | 829.94 | 1,009.76 | .822 / .829 | 258,450 | Pass |
| Baseline422 | 1,107.72 | 1,349.36 | .821 / .831 | 330,260 | Pass |
| Owned grayscale | 602.47 | 681.27 | .884 / .891 | 140,455 | Pass |
| Progressive grayscale | 1,797.74 | 1,711.60 | 1.050 / 1.054 | 151,432 | Gap |
| Progressive420 | 1,600.87 | 1,949.71 | .821 / .824 | 243,664 | Pass |
| Progressive422 | 2,089.26 | 2,248.06 | .929 / .934 | 315,496 | Pass |
| Baseline420 restart | 985.88 | 1,012.80 | .973 / .982 | 262,824 | Pass |
| Baseline420 restart, fresh seekable Stream | 984.90 | 990.07 | .995 / 1.002 | 262,920 | Uncertain |
| Raw RGB422 | 1,133.12 | 1,094.19 | 1.036 / 1.041 | 330,260 | Gap |
| Raw RGB420 | 752.07 | 743.25 | 1.012 / 1.020 | 258,452 | Gap |
| 4:1:1 | 493.78 | 448.75 | 1.100 / 1.110 | 219,548 | Gap |
| 4:1:0 | 339.51 | 505.41 | .672 / .678 | 189,090 | Pass |

Each row retains twelve actuals, native brackets, frozen inputs/pixels/settings,
independent outputs and idle-v2 guards. Complete public decode includes parsing,
preparation, conversion, output acquisition and disposal. Native retains its declared
codec/frame/input/conversion contexts. Quiet pinned CPU0, sequential Release,
unchanged selectors/workers. Grayscale/420/411 shorts and the original confirmation
remain; no completed timing repeats. Seven passes stop. Seventeen unchanged
controls reuse evidence: current 29 reads are **16 pass / 12 gap / 1 uncertain**.
Live progressive444 remains an independent gap.

Four frozen controls have unchanged governed peaks on both OSes: gray 699,834B,
baseline420 756,276B, 411 752,177B and 410 721,969B. Each retains 531,468B until
disposal, then zero charge; borrowed input is excluded. Sequential default idle
pixels rise 0→531,468B within the existing two-slot/4MiB ceiling. Gray idle
coefficients stay 549,120B; the other three stay zero. Governed calls neither use
nor populate either pool. GC, admission and idle retention are separate;
physical/native peaks and governed CPU gains are unmeasured.

Windows/Linux each pass **5,924 full/live checks**, 420 internal/375 public per
normal/scalar mode, and 20 storage checks with AVX2/AVX512 disabled separately.
Thirteen new controls cover dirty output, tails, caller samples, independent
survivors, terminal refusal and cancellation/async cleanup. The SAME actual private
Release package and all three examples pass both OSes, including gray/420/422
completed-plane consumers. One main build per OS; Windows embedded tests rebuild
to fix a test assumption about cancelable admission counts. A stale expectation
caused one setup failure with zero actuals; independent frozen-pixel proof and the
existing prepared projection resolve it.

[Evidence](jpeg-completed-output-evidence.json) records routing, costs, ownership
and qualification. Mirrored/member-verified raw `jpeg-completed-output-qualified.tar.xz`,
SHA256 `3ca6d81b0c0ba52ce3d6e6740c978751510f4b9402d54393bb4bcdbfe98948ae`,
6,434,100B/290 members, preserves failures, samples, modules, inputs and package
proof. [Paired AC](JPEG-PAIRED-AC-WINDOW.md) retains prior gains and seventeen
unchanged costs. Six displayed comparisons refresh; the full 283-path objective
remains open. Publication stays postponed.

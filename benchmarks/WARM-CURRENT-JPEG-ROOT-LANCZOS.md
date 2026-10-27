# Current JPEG shared-root and Lanczos costs

Five existing complete operations are measured on the unchanged qualified build,
sequentially on one quiet CPU. All are short screens with three actual/filtered
samples and both native brackets retained. They establish current gaps; none is
an independent parity confirmation or a speedup claim against an earlier build.

| Existing control | Lokad µs | Ratio / upper | Allocated B/op | Output |
| --- | ---: | ---: | ---: | --- |
| JPEG RGBA16 4:2:0, caller IMemoryBudget | 2,683.55 | 1.462 / 1.654 | 804,637 | 52,115 / 52,117 B |
| Lanczos3 reduction RGBA8 | 723.75 | 1.478 / 1.818 | 138,455 | Exact managed pixels |
| Lanczos3 reduction RGBA16 | 730.37 | 1.438 / 1.769 | 138,435 | Exact managed pixels |
| Lanczos3 enlargement RGBA8 | 4,221.10 | 1.039 / 1.101 | 319,038 | Exact managed pixels |
| Lanczos3 enlargement RGBA16 | 5,131.20 | 1.310 / 1.533 | 4,583,983 | Exact managed pixels |

JPEG includes normalization, flattening, conversion, encoding, fresh output
ownership and shared-root admission/refunds. Its observed root peak is
1,704,856 B, including the separately charged resident source. Borrowing,
terminal refusal, cancellation, recovery and surviving outputs pass. Complete
packets differ only by the approved attribution COM; settings and inputs stay frozen.

Lanczos uses zimg, FFmpeg's zscale backend, with the existing full halo,
normalization, filtering, alpha division, quantization and owned output boundary.
Managed frames remain exact; native RGB/alpha maximum 1 and RMSE 0.25 bounds
remain unchanged. Allocation counts are distinct from retained image capacity;
the existing ungoverned pixel-storage reuse remains inside the timed public API.

The four existing managed frames and independent surviving outputs are checked
against their retained exact bytes on the actual producer. Unchanged native
worker/source/input/output identities permit reuse of its quality proof. The
cached selector is recovered byte-for-byte from its retained archive. No new
image, settings grid, producer/native/benchmark/selector build or profile.

[Evidence](warm-current-jpeg-root-lanczos-evidence.json) records samples, guards,
frame refresh, identities and the mirrored, member-verified raw master.
[Windows/Linux and actual-package qualification](WARM-VP8-SIGNED-DOMAIN-GUARD.md)
is reused for unchanged code. Other valid costs remain; full 255-path warm upper
≤1× and replacement readiness remain open.

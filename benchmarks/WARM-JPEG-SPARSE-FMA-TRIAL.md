# Rejected sparse JPEG FP32-FMA trial

Complete CMYK sparse-2D decode **regressed 7.2%**. The qualified
implementation and actual DLL bytes are restored. No selected timing row changes.

| Complete resident decode | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Qualified baseline | 805.26 | 440.28 | 1.829 / 1.902 | 1,085,680 |
| Rejected FP32-FMA candidate | 863.54 | 437.27 | 1.975 / 2.056 | 1,085,682 |

The candidate processes two rows per SIMD vector. A strict rounding interval
certifies bytes against the original double evaluation; uncertain bytes use
exact fallback. Eligibility is at most eight frequencies with dequantized absolute
mass ≤65536. It adds no heap storage, cache, owner or work/cancellation boundary.
Seven focused controls pass normally, scalar and AVX512F-off on both OS; the Linux
run exercises actual SIMD. These cover existing signed/quantizer/guard cases plus
exact half-bin masks, float thresholds, mass rejection before writes and extremes.

All analytical managed pixels remain exact. Native normal/scalar maximum1,
RMSE0.7091 and alpha255 pass; source and two-live-output independence pass.
The same JPEG22,708B and RGBA531,468B, native worker, harness, selector and complete
operation are reused. Both idle-v2 guards pass; three actuals and both three-sample
native brackets are retained. No long confirmation, profile, grid expansion or
full/package proof for rejected production.

Five controls freshly compiled against restored source pass normally/scalar per
OS. The accepted 4,457-test/OS and actual private-package qualification remains
valid. Full parity and release readiness remain open; the finite checklist is
unchanged. Next: audit complete-overwrite output ownership and its existing bounded
pool before considering another arithmetic variant.

[Evidence](warm-jpeg-sparse-fma-trial-evidence.json) records the samples,
allocation counters, certificates, producer and four mirrored raw-archive hashes.
Do not retune this or the rejected prefix/axis-cross variants without materially
different evidence.

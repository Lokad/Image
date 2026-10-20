# JPEG output and component-storage trials

Lower allocation alone did not close the elapsed-time gap. Output reuse is
neutral in elapsed time; paired materialization is **5.1%
slower** than the qualified complete CMYK sparse-2D baseline. Both production
changes are removed; the selected benchmark rows stay unchanged.

| Complete resident decode | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Qualified baseline | 805.26 | 440.28 | 1.829 / 1.902 | 1,085,680 |
| Output reuse, parked | 809.58 | 437.02 | 1.852 / 1.963 | 553,891 |
| Paired materialization, rejected | 846.20 | 438.23 | 1.931 / 2.019 | 141,853 |

The first candidate acquires fully overwritten output from the existing bounded
pool only when ungoverned; admitted images keep their separate reservation and
bypass the pool. It cuts allocation49.0%.
The second keeps one padded plane of L bytes, releases its4L coefficient storage,
then acquires a result≤4L and streams the other three components through stack
batches. Its live peak stays within the old first-plane peak, and allocation falls
86.9%; the original transform, normalization and total logical work remain.

Exact managed pixels, normal/scalar native maximum1/RMSE0.7091, opaque alpha,
unchanged source and independent surviving outputs pass. Initial27 and paired29
public controls pass normally/scalar on both OS, including poisoned reuse, every
growth refusal, cancellation at output admission, subsequent success and the
unchanged caller-plane peak bound. Original JPEG22,708B/RGBA531,468B, workers,
selectors and native gates are reused. Only receipt wording needs one harness build.

All completed shorts retain three actuals, native pre/post brackets and passing
idle-v2 guards. Two additional output-reuse screens are retained in the evidence;
their historical baselines do not establish current-code relative gains. A later
general-case preguard fails CPU0 I/O-wait2.0%; it launches no benchmark. Remaining
interim screens are superseded by the decision to target more substantial work.
No historical-load wait, new profile/grid, long repeat or full/package proof.

Qualified production source/DLL bytes are restored. Twenty-nine public checks
freshly pass normally/scalar per OS against the restored producer. Four new
admission/ownership cases and stronger survivor/overwrite assertions are retained;
the full4,457-test/OS and actual private-package proof are reused for unchanged
production. All six raw archives are mirrored; identities and all samples are in
the [evidence](warm-jpeg-storage-trials-evidence.json). Full parity/release remain open.

Next is an original Q16 integer sparse-IDCT certificate, a distinct arithmetic
strategy. Mathematical feasibility is not a production or speed claim. No selected
passing case or workload matrix is expanded.

# CMYK JPEG context boundaries

JCB785 adds an equivalent one-shot reference view for the existing horizontal-AC
Adobe CMYK control. Production, its package and exact pixels are unchanged.

| Complete RGBA8 decode | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Fresh operation state on both sides | 427.75 | 750.55 | 0.570 / 0.573 | 20,944 |
| Existing managed-fresh / FFmpeg-retained view | 438.61 | 392.08 | 1.119 / 1.160 | 20,944 |

The first row passes an independent twelve-actual confirmation following its
three-actual screen. The second retains the previous source-valid confirmation;
these different boundaries and clocks do **not** establish a production speedup
or close that retained-reference gap. The full performance objective remains open.

The existing FFmpeg worker retains decoder, empty frame wrapper and swscale
packing context. `LOKAD_IMAGE_JPEG_FRESH=1` now creates/opens/disposes all three
inside every operation. Both modes include complete decode and device-color
conversion, opaque alpha, independently owned zeroed RGBA output, packing and
decoded-plane/output release. The same padded resident AVPacket is borrowed input
delivery. Startup, JIT, I/O, IPC and checks remain excluded. One CPU/thread applies
throughout. The intended Windows package DLL supplies managed timing on Linux.

A short native-only attribution gives391.52µs for the original retained worker,
385.65µs for the revised retained worker and749.46µs for fresh contexts. This
identifies material setup/disposal, not a removable managed phase percentage.
It is diagnostic evidence, not a separate parity confirmation or a claim that
the revised retained adapter is faster. Reference allocations/physical peaks and
the individual decoder-versus-packing setup costs are unmeasured.

The unchanged513×259 packet is17,346B; all531,468 output bytes match the frozen
managed/reference hash. Existing normal/scalar CLI, geometry, opaque-alpha,
input-immutability and independent-survivor checks pass, with zero pixel error.
An isolated workload root reuses the verified harness/drivers and existing
qualified Q14/Q16 expected samples; encoded input, projection, component samples,
quantization and reference settings remain frozen. The receipt explicitly labels
fresh contexts where the reused harness's descriptive string names retained state.
The input source and original generated artifact directory are untouched.

Only reference tooling changes. There are **zero production, embedded-test,
harness or package builds**. The actual private package96a290c2 is unchanged;
its Windows/Linux6,244full/live checks, relevant scalar/ISA evidence and same-package
consumer/all three examples from [TRP784](TEXT-RENDER-PREPARATION.md) are reused.
Managed total allocation remains20,944B/op. Managed capacity and retention are
unchanged; operation scratch ends with the call. Existing bounded default pools are
unchanged and not remeasured; admitted and physical peaks remain distinct.

[Evidence](jpeg-cmyk-context-boundaries-evidence.json) pins both workers, the
current package DLL, frozen inputs, all actuals and boundaries. Mirrored and
member-verified raw `jpeg-cmyk-context-boundaries.tar.xz`:8,360,864B/299members,
SHA256 `6280e839478456275b3b63a5670f4ce7bbbb4f2f64b9de63531bdcb655278ffb`.
The earlier [two-row column report](JPEG-BATCHED-TWO-ROW-COLUMNS.md) retains its
other unchanged JPEG comparisons and original context view. No new required path
or image/settings grid is added. A startup-only missing-dependency failure was
corrected before any measured operation; completed captures were not repeated.

Next, attribute current horizontal-CMYK managed work before choosing a redesign.
The decoder already stages one admitted MCU into private interleaved output,
then converts after final Adobe-marker interpretation at EOI. Any future fusion
must preserve late-marker behavior, exact pixels, cancellation, caller-sample
publication and owner cleanup. Fresh-reference parity does not authorize a
different pixel or ownership contract.

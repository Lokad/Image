# Exact SIMD JPEG 4:1:0 rows

Complete H4V2 JPEG decode takes **76.7% less time**,3,427.72→799.34µs.
The gap falls from7.144x to **1.669x / upper1.877x**. It remains above parity;
no long confirmation was run. The existing411 control refreshes the H4 selector.

| Complete resident JPEG operation | Lokad / FFmpeg µs | Ratio / upper | Lokad allocated B/op | JPEG / RGBA bytes |
| --- | ---: | ---: | ---: | ---: |
| H4V2 / 4:1:0 → owned RGBA8 | 799.34 / 479.04 | 1.669 / 1.877 | 720,810 | 3,399 / 531,468 |
| H4V1 / 4:1:1, selector control | 608.85 / 458.97 | 1.327 / 1.402 | 751,279 | 3,904 / 531,468 |

One primitive reconstructs eight pixels from four samples in each of two chroma
rows. Exact integer-eighth horizontal and quarter vertical weights retain every
1/32 fraction through binary64 matrix arithmetic and final-only rounding. There
is no intermediate byte rounding, FMA, reduced precision or heap workspace.
Four-byte reads stay inside valid samples; scalar borders/tails and hardware-off
fallback keep the original contract. RGBA packing writes directly to the owner.

A narrow EOI selector requires full Y and quarter-width/half-height Cb/Cr with
YCbCr color and supported SIMD. The general renderer,411 hot loop, entropy decoder,
inverse transforms and component materializer are unchanged. Allocation/admission
order, row work charges, cancellation, orientation and surviving owners remain.

Same original packet, analytical pixels, frozen harness/native workers, compiled
selectors and normal/scalar CLI checks as the [baseline](WARM-JPEG-410.md). Native
recovery, centered vertical interpolation/staging,516-column conversion and
in-place output compaction remain timed. Both sides include complete decode,
normalization, fresh independently owned output and release on one CPU/thread.
Managed pixels stay exact. Native RGB maximum1/RMSE0.136 and exact alpha retain
the unchanged maximum4/RMSE1.5 gate. Startup, I/O, IPC and checks are excluded.

Two short screens retain all six actuals/outliers,50ms native pre/post brackets
and both idle-v2 guards. The existing general-render profile selected this row
primitive; no new profile, fixture, harness, worker or selector build. One immutable
production build per OS throughout the bounded experiment. No timing gain is
claimed for the411 control, whose hot loop is unchanged.

Accepted qualification: **4,212 tests/OS** (1,439 internal,1,845 public,928 live
FFmpeg), **99 hardware-off checks/OS**, and the SAME actual private Windows package
consumer/all three examples on both OS. New tests cover both vertical phases,
randomized binary64 references, clipping, all width residues, unaligned spans and
source/output guards. Existing public tests cover partial blocks, progressive
data, shared budgets, work exhaustion, cancellation and independently surviving
owners. The released0.1.0 package/tag remain unchanged.

The owned410 color path invalidates its old timing;411 refreshes the shared H4
selector. All225 other numeric rows and70passing proofs retain evidence. Other
layouts keep their existing hot loops, four-component rendering returns before
the guard, and caller samples use a separate entry. Scope247=227numeric+20missing
remains70passes/156gaps/one confidence-open case. Largest current gap returns to
CMYK2D sparse1.812x; its prefix trial stays rejected. Next: named async PNG Identify.

[Numeric evidence](warm-jpeg-410-simd-evidence.json). Raw retained on both hosts:

- `h10z-measurements.tar.gz`: SHA `37a582397237b8e135e7bc5f2eb416f71f6418868b899ce13ce981b42b8e2288`.
- `h10z-qualified-windows.tar.gz`: SHA `ceaf59d2880e3e5469fe4dc5f586bd108ff399e88f3a55e328a2a4936842e569`.
- `h10z-qualified-linux.tar.gz`: SHA `5c2091cdc490bd2274a4d231b57101e4afebb102bb18c22f6de2111b7cbe668a`.

Unchanged native binaries/fixtures remain in their cited baseline archives.

# Progressive JPEG444 with a live cancellation token

This earlier production round is retained history. See
[current progressive costs](CURRENT-PROGRESSIVE-JPEG-COSTS.md) for the qualified
native-exact decoder's current cost and allocation.

Complete warm progressive JPEG444 decoding independently confirms **1.935× FFmpeg /
upper 1.940×**, at **6,453.37 µs** and unchanged **2,682,364 managed B/op**. Managed
time falls **3.6%**, closing the original **2.008×** gap. All twelve actuals and both
native brackets remain included; every versioned quiet guard passes.

| Stage / fixed workload | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Original screen | 6,695.35 | 3,334.74 | 2.008 / 2.412 | 2,682,364 |
| Rejected local accounting | 7,116.49 | 3,341.63 | 2.130 / 2.239 | 2,682,364 |
| Accepted screen | 6,409.72 | 3,334.12 | 1.922 / 2.493 | 2,682,364 |
| Independent confirmation | 6,453.37 | 3,335.92 | 1.935 / 1.940 | 2,682,364 |
| Ordinary progressive regression screen | 6,280.26 | 3,334.35 | 1.884 / 1.975 | 2,682,364 |
| Cancelable baseline regression screen | 3,158.79 | 1,770.53 | 1.784 / 5.685 | 2,664,358 |

The existing original 513×259 byte raster, progressive native JPEG444 packet,
independent coefficient proof and per-decoder pixels stay frozen. The managed
token is live and never signaled; token/source setup is excluded. Native has no
equivalent cancellation option. Complete public resident-input decoding, color
conversion and independent owned output acquisition/release are timed on both
subjects. Native codec/conversion contexts and packet/frame wrappers are reused;
each call resets decoder state and reconstructs all pixels. No output is cached.
Startup, I/O, IPC, fixtures, validation and file packing are excluded. Native
allocation totals are unmeasured; managed figures include harness overhead.

One six-second profile places about 45% in bit reads, Huffman lookup and work
admission, plus about 29% in masked refinement. Local progressive block accounting
increases elapsed time and remains rejected without a longer repeat or full
qualification. The accepted primitive instead selects the original per-bit masked
correction loop once for cancelable readers or processors without BMI2. This
removes repeated popcount/bit-deposit preparation for batches that cannot succeed.
Every bit read, work/cancellation check, coefficient write and partial failure
retains its original order. No new table, heap workspace, dependency, unsafe code
or upstream material is added.

Exact managed pixels, opaque alpha, geometry, source and owner independence are
unchanged. Registered normal/scalar native RGB maximum4 / RMSE1.5 bounds remain;
pre-canceled public decoding retains token identity. Strengthened independent
bitwise controls cover 65,536 dense signed masks with normal/cancelable reader
state, plus scoped/unscoped partial work, truncation and coefficient failures.

The same frozen production/harness serves accepted screen, confirmation and both
regression screens. One new benchmark selector reuses the original progressive
packet without a quality sweep. Target short drivers and the native worker are
reused. Shared regression means are read before full qualification; their short
upper bounds do not replace the prior independent confirmations. Only three
workload types and the two existing original rasters are used.

All **3,349 Release tests per OS**, including **896 live FFmpeg comparisons**, and
**458 focused scalar checks per OS** pass. The same actual dependency-free Release
package passes its consumer and all three examples on Windows/Linux. Measurements
are sequential Release on one pinned CPU, one codec/conversion thread, normal SIMD,
tiering off and workstation nonconcurrent GC. Production, benchmark and test inputs
remain unchanged through qualification; only SPEC/resource documentation changes
after measurement. [Numeric evidence](warm-jpegprogressive444cancel-evidence.json)
retains exact jobs, all actuals, guards, allocations, rejected candidate, profile
and actual package/source proof. Stop this fixed pair; full material/current-source
warm parity remains open.

# Current single-component JPEG encoding costs

Both existing controls are independently confirmed gaps on the current qualified
build. These costs replace short screens, without a causal gain/regression claim
against unmatched older observations. No production change or workload is added.

| Complete operation, 513×259 | Managed µs | Native µs | Ratio / upper | Managed B/op | Packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA8 to grayscale JPEG | 1,684.290 | 1,496.427 | 1.126 / 1.137 | 155,436 | 58,910 |
| Caller GRAY8 to JPEG | 1,218.168 | 1,136.372 | 1.072 / 1.090 | 155,439 | 58,910 |

The reference is explicitly approved **SIMD-enabled standalone libjpeg-turbo
3.1.4.1**, for single-component grayscale benchmark tooling only. Production
remains safe managed and dependency-free. Original inputs, Q69 mathematical
quantizers/settings and raw-exact 58,910-byte packets remain frozen; identical
quantizers/complete bytes establish equivalence here. No numerical-quality
matching assumption, search or retuning is used. Both packets reconstruct
identically under the reference decoder; existing managed-decoder bounds, exact
alpha, source and independent surviving output checks pass.

RGBA8 includes once-rounded flattening on 0x204060, full-range BT.601 grayscale
normalization, complete encoding, fresh output ownership/growth and release on
both subjects. Caller GRAY8 borrows resident samples directly, with no RGBA
intermediate or normalization on either side. Native wrappers/options may be
reused; every image is processed and each packet independently acquired/freed.
Managed allocations include harness/output costs; native allocations are
unmeasured. Differences between rows do not precisely attribute normalization.

One current twelve per boundary, all actuals/native brackets retained, one quiet
CPU0, optimized warm JIT and nonconcurrent workstation GC. Both idle-v2 guards
pass. Zero builds, profiles, new inputs/settings, short repeats or qualification
repeats. Cached selectors/workers/fixtures are reused. Older independent gray
costs precede the executed paired code/amplitude entropy writer; later paired
writer and release rows only provide shorts. The twelve-iteration driver survives
that invalidation and is reused without compiling.

The immutable JFN1 core reuses 5,331 checks/OS, the same actual Release package
and three examples on both OSes, plus the separately qualified opaque-predictor
invariant: 5,332 applicable, no fresh full 5,332 claim. Package remains
`228818d7e0026933b6cc800ee58dcece0603e945f7f46bcb02397a63184203f9`. This scope covers two fixed grayscale boundaries,
without a broader content/settings or decoder claim. The full 268-path objective
remains open; these confirmed gaps need no uncertainty retiming.

[Evidence](current-grayscale-jpeg-costs-evidence.json) pins all actuals, confidence,
workers, packet/source identities and qualification reuse. Mirrored/member-verified
raw `current-grayscale-jpeg-costs.tar.xz` SHA256
`e283503207fa1ec95c554161de6738ec3d9b65fee9a4a2683c5ee8c810e6e4e8` (1,329,668 B / 58 members)
retains cached runtime/selector/workers, fixtures, guards and reproduction scripts,
with JFN1/OAP1 dependencies retained.

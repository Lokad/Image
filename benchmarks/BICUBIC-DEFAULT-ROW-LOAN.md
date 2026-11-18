# Output row loans for default RGBA8 bicubic reduction

Eligible narrow RGBA8 reductions now select the existing bounded column
kernel before allocating a separate full-row cache. For the original
513×259 to 256×129 control, twelve unpublished output rows temporarily
hold premultiplied samples and coefficients. The existing 4 KiB working
reservation and cancellation checkpoints apply. Each borrowed row is
overwritten with final pixels after the loan ends. The result retains
its original ownership and capacity.

The dispatch tries the bounded kernel once. If it declines, the existing
cached and other fallback paths remain available. Shared admission refusal
propagates immediately. The reusable resize plan still takes its original
prepared-cache path.

## Measured effects

The original package's partitioned confirmation measures **417.96 /
prepared zimg 419.70 µs**, **199,886 B/op**, interval
**0.982066–1.005567**. Retain the earlier 1.024193 upper. The first private
row-loan screen measures 415.44 → 396.47 µs. Twelve new managed pairs on
the same candidate confirm **451.91 → 422.64 µs**, **6.46%**,
interval **0.906469–0.965200**. Measured allocations fall from about
**199,886 to 112 B/op**. No new retained state or output capacity is added.
Physical and native allocations are unmeasured.

The same private DLL passes standalone at 416.87 / 455.45 µs, upper
0.923582. The first quiet check found 2.15% aggregate I/O wait and stopped
before timing. The resumed run reuses the same build and retains both
guard records.

| Complete operation on the final Release DLL | Lokad.Image µs | Prepared zimg µs | Conservative ratio upper | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA8 reduction, default | 431.10 | 454.16 | 0.956893 | 112 |
| RGBA8 reduction, input + output + 4 KiB | 397.89 | 419.63 | 0.959079 | 112 |

These are the two applicable original direct controls among 61. Both use
the original generated timed delegates and the same immutable module.
The default keeps twelve 496-call units and three interleaved 240-call
native groups per unit. The tight control keeps 448/128. Both preserve
exact managed pixels, native-only bounds, original sources, independent
owners and the prepared zimg graph and scratch boundary. All samples and
both idle-v2 guards pass.

## Qualification

One added check uses default local limits and a shared root that admits
only the result and the 4 KiB loan. It compares exact pixels against an
independent route and verifies a single terminal refusal and cleanup with
one byte less capacity. Windows and Linux each pass 6,852 full/live
checks and 47 resize-focused checks per normal, AVX-512-disabled and
scalar mode, including all three named storage suites. The actual
dependency-free development package `641ef7b9` contains the unchanged
timed Linux DLL `85c00a70`. Its consumer and all three examples pass on
both systems. Publication remains separate.

[Evidence](bicubic-default-row-loan-evidence.json) retains the earlier
uncertainty, private and final measurements, exact sources, archived
checks and package identities. Both original RGBA8 256×129 controls stop.

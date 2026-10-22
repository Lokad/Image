# JPEG444 source EOB-run prices

C187 prices progressive end-of-band runs using their observed shared cost.
Complete-packet parity remains open on the five affected boundaries.

| Existing 513 × 259 procedural control | Previous B | Current / reference packet B | Source-PSNR gap |
| --- | ---: | ---: | ---: |
| Original 4:4:4, RGBA8 | 134,875 | 134,784 / 133,041 | 0.218071 dB |
| Original 4:4:4, RGBA16 | 134,839 | 134,734 / 133,043 | 0.225823 dB |

Savings are 91/105 B: 469 B across the five boundaries, removing 5.1% of their
previous excess. Maximum remaining excess is about 1.31%. Both controls pass the
unchanged 0.25 dB matched source-quality gate, exact alpha and decoder bounds
(maximum 4, RGB RMSE 1.5). Inputs, reference packets, managed quality 69/native
qscale 5, sampling and 0x204060 flattening stay frozen. Equal numerical quality
values do not establish equivalence. These procedural controls establish no
photographic, diagram or general JPEG claim. Passing controls and optional
CompactColor remain separate and retain relevant evidence.

The same 64 fine-geometric training samples, positions, units and choices remain.
Pending EOB runs coalesce separately per component; Cb/Cr share histograms rather
than a pending run. All 176 progressive symbols retain +1 smoothing and a dummy
leaf. Ordinary AC prices use canonical lengths × 256. EOB prices amortize observed
code lengths plus extra bits over the actual terminal-block count, rounded to Q8.
That count is retained separately: the exponent of a three-block run also
represents a two-block run. Final decisions, lambda 100, DC, Original DQT,
precision factors and the global MCU allowance remain unchanged.

At most 930 ushort entries include those counters; the actual all-quality maximum
is 900. Arena, map, component-local pending runs, source builder and MCU scratch
total 7,208 substantial stack bytes within the same 8 KiB fixed / 64 KiB adaptive
admission. Ordered heap leaves replace quadratic frequency sorting with identical
ties. The declared control plans 1,999,981,013 work units below the unchanged 2B
default. No heap owner, cache or frame/input copy is added; terminal refusal,
shared admission, cancellation, asynchronous users and output lifetimes remain.

Both platforms pass **5,048 Release tests** (2,129 internal, 1,983 public,
936 live FFmpeg), plus **218 internal + 62 public scalar checks** each.
The same frozen Linux-built core/internal/public binaries run on Windows.
The first integration failed the independent packet goldens; retaining actual
run lengths fixed its amortization. Both builds and failed checks are preserved.
The final core stays frozen. The same actual private 0.2.0 package and all three
usage examples pass on both platforms. Exact C186 packet hashes retain all
411,840 source-nearest and chosen coefficient checks per depth and twelve
per-decoder normal/scalar/native pixel and ownership proofs. Tooling is reused.

One complete forward444 quiet-VM screen records **1,957,616.722 µs / reference
3,774.486 µs**, ratio 518.645 / upper 599.206, **615,792 B/op**. Previous C185
records 1,908,412.257 µs and 615,832 B/op. Both idle guards pass; native allocations
remain unmeasured. Cost is accepted for compression, without a CPU-parity pass or
independent timing confirmation. Speed-only work remains deferred.

Five packet rows and the forward cost supersede C185. Other affected timings
remain historical; unchanged paths retain evidence. The
[packet ledger](JPEG-COMPLETE-PACKET-AUDIT.md) remains 13 default passes / five
open plus one optional pass. The finite 256-case warm objective and replacement
readiness remain open. [Numeric evidence](jpeg444-source-eob-price-evidence.json)
identifies the source, actual package, original cost receipt and mirrored raw.

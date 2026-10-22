# JPEG444 paired chroma tables

C191 shares a Huffman table between corresponding Cb/Cr spectral bands when
predicted code bits plus the complete DHT cost strictly decreases. The existing
five packet gates remain open.

| Existing 513 × 259 procedural control | Previous B | Current / reference packet B | Source-PSNR gap |
| --- | ---: | ---: | ---: |
| Original 4:4:4, RGBA8 | 134,784 | 134,741 / 133,041 | 0.218071 dB |
| Original 4:4:4, RGBA16 | 134,734 | 134,687 / 133,043 | 0.225823 dB |

Savings are 43/47 B: 219 B across five boundaries, removing 2.53% of their previous
excess. Maximum remaining excess is about 1.28%. Coefficients and decoded pixels
remain identical to C187. The unchanged 0.25 dB source-quality gate, exact alpha
and decoder bounds (maximum 4, RGB RMSE 1.5) pass. Inputs, reference packets,
managed quality 69/native qscale 5, sampling and 0x204060 flattening stay frozen.
These controls establish no photographic, diagram or general JPEG claim.
Optional CompactColor and already passing controls retain separate evidence.

Only corresponding chroma bands are paired, without a grouping or order search.
Y/DC scans, coefficients, source models, DQT and all rate/error decisions stay
unchanged. Split tables win ties. The existing 8 KiB fixed / 64 KiB adaptive
admission and coefficient storage are reused. All 98 enabled Original qualities
fit at most 39 tables plus fewer than 65,536 bookkeeping visits, inside the old
192 × 65,536 fixed work allowance. No frame/input copy or cache is added.

Both platforms pass **5,056 Release tests** (2,137 internal, 1,983 public,
936 live FFmpeg), plus **226 internal + 62 public scalar checks** each. One frozen
Linux core and the same internal/public binaries run on Windows. The first test
run exposed a missing caller-owned EOI in the new internal test; only that helper
was corrected. The production build stayed frozen. The same actual private
0.2.0 package and all three usage examples pass on both platforms. Exact C190
packet hashes retain all 411,840 coefficient checks per depth and eight
normal/scalar/native pixel and ownership checks. Refusal, cleanup, cancellation,
shared admission and surviving output lifetimes pass their affected tests.

One complete forward444 quiet-VM screen records **1,949,513.811 µs / reference
3,761.926 µs**, ratio 518.222 / upper 541.403, **643,160 B/op**. Prior C187 records
1,957,616.722 µs and 615,792 B/op. Both idle guards pass; native allocations remain
unmeasured. Cost is accepted for compression. This establishes no speed claim,
CPU-parity pass or independent timing confirmation.

The five packet rows and forward cost supersede C187. Other affected timings
remain historical; unchanged paths retain evidence. The [packet ledger](JPEG-COMPLETE-PACKET-AUDIT.md)
remains 13 default passes / five open plus one separate optional pass. The finite
256-case warm objective and replacement readiness remain open.
[Numeric evidence](jpeg444-paired-chroma-table-evidence.json) identifies source,
actual package, original cost receipt and mirrored raw.

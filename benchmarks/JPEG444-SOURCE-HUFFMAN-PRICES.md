# JPEG444 source Huffman prices

C185 uses source-derived canonical Huffman lengths for coefficient prices,
conditioned on the existing spectral bands. Complete-packet parity remains open.

| Existing 513 × 259 procedural control | Previous B | Current / reference packet B | Source-PSNR gap |
| --- | ---: | ---: | ---: |
| Original 4:4:4, RGBA8 | 135,237 | 134,875 / 133,041 | 0.218530 dB |
| Original 4:4:4, RGBA16 | 135,144 | 134,839 / 133,043 | 0.224697 dB |

Savings are 362/305 B versus committed C178: 1,753 B across the five affected
boundaries, removing 16.1% of their previous byte excess. Five gates remain open,
with maximum excess about 1.38%. Both controls pass the unchanged 0.25 dB
matched source-quality gate, exact alpha and decoder bounds (maximum 4,
RGB RMSE 1.5). Inputs, reference packets, managed quality 69/native qscale 5,
sampling and 0x204060 flattening remain frozen. Equal numerical quality values
do not establish equivalence. These controls establish no photographic, diagram
or general JPEG claim; passing controls and optional CompactColor stay separate.

The same fine-geometric training coefficients, units, 64 sample positions and
choices remain. Source prices now use canonical Huffman lengths × 256, with
separate models for existing spectral bands and shared Cb/Cr. All 162 legal
symbols retain +1 smoothing, including unreachable runs, and the dummy leaf
reserves the all-ones code. Final local/DAG/pair/triplet decisions, lambda 100,
DC, Original DQT, precision factors and global MCU allowance remain unchanged.
There is no output-price refresh or quality/settings grid.

Sparse counters become ushort prices in place: at most 1,500 B plus 256 B for
scan descriptors, versus the previous 2,048 B. The builder reuses 2,116 stack
bytes sequentially within the same 8 KiB fixed / 64 KiB adaptive admission.
All decision-enabled qualities 1–98 fit the bound and are checked against actual
quantizers. The declared control plans 1,999,990,671 work units below the unchanged
2B default. No heap owner, frame/input copy or cache is added. Shared admission,
terminal refusal, cancellation, asynchronous users and output lifetimes remain.

Both platforms pass **5,040 Release tests** (2,121 internal, 1,983 public,
936 live FFmpeg), plus **210 internal + 62 public scalar checks** each.
The same frozen Linux-built core and test binaries run on Windows. Two core
compiler attempts produce one successful build; nothing rebuilds after freeze.
The same actual private 0.2.0 package and all three usage examples pass on both
platforms. Actual packet hashes match independent C184 prototypes, retaining
all 411,840 source-nearest and chosen coefficient checks per depth and twelve
normal/scalar/native pixel and ownership proofs. Each decoder matches its own
baseline-equivalent packet. Unaffected evidence is reused.

One complete forward444 quiet-VM cost screen records **1,908,412.257 µs /
reference 3,768.964 µs**, ratio 506.349 / upper 540.413, **615,832 B/op**.
Previous C178 records 1,420,508.479 µs and 611,264 B/op. Both idle guards pass;
native allocations remain unmeasured. Two harness launches failed before any
measurement; their logs are retained. Higher CPU/allocation cost is accepted
for compression. There is no CPU-parity pass or independent timing confirmation.

Only changed Original three-component 4:4:4 decision paths invalidate earlier
encoder evidence. Five packet rows and the measured forward cost supersede C178;
other affected timings remain historical. Qualities 99/100, other policies,
4:2:0/4:2:2/grayscale/CompactColor and other codecs retain relevant evidence.
The [packet ledger](JPEG-COMPLETE-PACKET-AUDIT.md) remains 13 default passes /
five open plus one optional pass. The finite 256-case warm objective and
replacement readiness remain open. [Numeric evidence](jpeg444-source-huffman-price-evidence.json)
identifies the frozen source, package, cost receipt and mirrored qualification raw.

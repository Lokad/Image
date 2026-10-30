# Direct VP8 token decisions

The stream companion cost is superseded by [current independent confirmation](CURRENT-VP8-STREAM-DECODE-COST.md).

The existing VP8/ALPH validation control uses **7.20% filtered / 7.23% all-actual
less managed time** in the forward comparison and **8.12% in both views** in the
reverse comparison. Native-normalized point ratios improve 7.26%/12.03%.
Both raw confidence views separate in both time directions, as do all conservative
reference-normalized envelopes: maximum candidate upper 1.381604 < minimum
baseline lower 1.406485. The current cost remains a CPU gap.

| Quiet one-CPU operation | Lokad µs | Ratio / upper | Allocated B/op | Actual/filtered |
| --- | ---: | ---: | ---: | ---: |
| A1 | 7,939.364 | 1.457 / 1.473 | 770,092 | 12/10 |
| B1 | 7,367.434 | 1.351 / 1.382 | 770,092 | 12/10 |
| B2 | 7,834.690 | 1.357 / 1.369 | 770,092 | 12/12 |
| A2 | 8,526.651 | 1.542 / 1.612 | 770,092 | 12/12 |
| StreamNative8 short | 7,638.568 | 1.274 / 2.005 | 1,966,583 | 3/3 |

A1/B1/B2/A2 is one predeclared original/candidate/candidate/original block.
Report B1, the candidate with the larger conservative upper. The first separate
VND1 confirmation remains unqualified because its normalized intervals overlap;
its samples are neither pooled into this block nor relabeled accepted. Every
job retains raw/unfiltered/filtered samples, native brackets and both idle-v2
guards. No production or tooling rebuild/retuning occurs between blocks.

Validation borrows the frozen 513×259 encoded input, validates complete coding
and returns independently owned nine-field source metadata. The reference is
pinned FFmpeg natural-frame software decoding plus the original metadata adapter,
without RGBA normalization/caller raster copies. The existing StreamNative8
companion includes fresh seekable acquisition/copies/EOF, decoding/chroma expansion,
fresh RGBA8 ownership and release on both subjects. Its exact frozen pixels,
normal/scalar CLI, alpha, source/owners and quiet gates pass. It is a current
short screen, not an independent gain/parity confirmation. Allocation is managed
B/op, not native allocation, peak live storage or cooperative-admission charges.

The reader normalizes before each decision, allowing direct token branches.
Category/sign decisions stay branchless. Fixed UInt32 alignment preserves the
same tree/read order, 16-bit refill and 272-byte selection. Final/canceled state
is normalized from known bits without reading input. Independent scalar/cursor/
following-read and canceled-prefix checks plus exact range/padding bounds prove
the state transformation. No new storage, quota, owner, reservation or unsafe
code; pinned native code supplied insight only, with no copied source or tables.

Both platforms pass **5,082 checks**, including 948 live comparisons, 32 affected
internal scalar and 350 public scalar cases. The SAME actual private 0.2.1 Release
package and all three usage examples pass both OS, including the 64×64 buffered
reader/retained-root consumer. One core build per OS serves the entire experiment
and qualification; no harness/native/selector build or new profile. One unused
internal publication parameter is removed, with its existing test call updated.

Only buffered VP8 decoder/validation costs change; normal/cancelable/delivery/
opaque/simple-filter/alpha callers need applicable current evidence before CPU
pass claims. Short-input scalar fallback, encoding packets, other codecs,
metadata-only reads and editing/text retain unchanged-path evidence. No case or
settings grid is added; all 268 required paths remain the objective.

[Evidence](warm-vp8-direct-token-decisions-evidence.json). Mirrored/member-verified
raw archive:
vp8-direct-token-decisions-qualified.tar.xz, 226 members, SHA256
a5022258672d318b8f0183d86d9700b3bebea558b043a624aa2959c40722be0d.

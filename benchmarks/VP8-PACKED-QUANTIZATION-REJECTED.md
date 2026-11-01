# Packed VP8 quantization: rejected

One short photograph/effort4 screen fails the 4.1% continuation gate. It has
no useful complete-operation gain and adds 1,920 B/op. Wide short confidence
does not establish a causal regression. No confirmation, variants or new
profile follow; production and current comparative costs remain unchanged.

| Same complete 513×259 encode | Lokad µs | Ratio / upper | Allocated B/op | Packet B |
|---|---:|---:|---:|---:|
| Unchanged-path baseline, independent12 | 13,899.636 | 1.433 / 1.438 | 809,072 | 17,688 |
| Rejected candidate, short3 | 14,232.493 | 1.454 / 2.396 | 810,992 | 17,688 |

The trial replaces widened ordinary quantization with unsigned16 high/low
products and an exact bias carry, retaining Q17 rounding, level caps, signs,
dequantization and zigzag order. Generated thresholds make a separate zero
mask redundant. Packed operands occupy another 480 bytes per segment in the
same admitted plan owner, reserved before allocation. Scalar and general
borrowed-matrix paths remain. This arithmetic trial differs from the earlier
paired-block width experiment; neither family warrants another variant.

Filtered/all-actual/native-normalized points are respectively 2.395%/2.365%/
1.497% higher. The existing input, effort/settings and FFmpeg/libwebp worker
are unchanged. Complete packets, reconstruction, alpha, independent outputs,
source ownership and both idle-v2 guards pass. The original two edited kernels
match the independent baseline; 30 VP8 source files are unchanged. Three later
decoder-only differences are outside the timed encoding operation.

One Linux core/internal build passes 36 internal and 392 public checks in
normal/scalar modes, including the entire caller coefficient domain, canaries,
frozen packets and shared admission/refusal/cleanup. A failed host launch never
executes the benchmark: its missing dependency manifest is replaced with an
existing pinned manifest, without rebuilding. Exactly three actuals are scored.

All four sources, three trial-only cases and all 53 saved Linux binaries are
restored byte exactly without rebuilding. Restored 33 internal/392 public
checks pass normal/scalar. Windows production and the actual package never
change. Reuse 5,378 checks/OS, 948 live comparisons and the same actual package/
three examples; no full-suite or package proof repeats. All 282 required paths
remain; the summary keeps 110 comparisons and nine audited paths unmeasured.

[Evidence](vp8-packed-quantization-rejected-evidence.json). Retained
vp8-packed-quantization-rejected.tar.xz, **2,814,588 bytes / 76 members**,
SHA256 **d4b86f41af9f66987b549139794b5bc1ec2b7277a9b3499b4b99b240e0ef6892**,
mirrored and member-verified. It retains original/candidate source and producers,
selectors, original12/candidate3 observations, guards, launch failure and exact
restoration. Reproduce with the archived build/measurement scripts, pinned worker
and tracked compression fixture. The [caller-invariants round](WARM-VP8-QUANTIZATION-CALLER-INVARIANTS.md)
retains the independent baseline dependency; the [packed-fill round](WARM-RECTANGLE-FILL-PACKED.md)
retains unchanged full/package qualification.

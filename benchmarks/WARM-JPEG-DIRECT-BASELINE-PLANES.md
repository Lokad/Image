# Reconstruct baseline JPEG directly into sample planes

The existing owned sparse-CMYK control uses **10.3% less elapsed time** in an
independent comparison, with exact pixels. Its **1.484× / upper1.491** cost
remains a gap. No encoder, quality setting or packet changes.

| Existing complete 513×259 operation | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Prior owned sparse-CMYK, independent | 733.93 | 437.75 | 1.677 / 1.688 | 1,085,520 |
| Current owned sparse-CMYK | 658.64 | 443.71 | 1.484 / 1.491 | 1,085,759 |
| Current caller CMYK | 397.07 | 331.53 | 1.198 / 1.216 | 1,085,964 |
| Current owned baseline420 | 1,111.15 | 1,007.07 | 1.103 / 1.109 | 757,362 |
| Current baseline420 coding validation | 603.92 | 678.38 | 0.890 / 0.899 | 9,984 |

Baseline scans now reconstruct each final block immediately into admitted raw
component byteplanes, using one64-int/64-byte stack scratch per scan. The old
four-component control staged **2,196,480 coefficient bytes**; its byteplanes
need **549,120 bytes**. These are payload figures, not complete-operation peaks.
Pixel validation uses block scratch without a raster. Progressive decoding and
bounded coefficient observation retain stored coefficients. Encoded input stays
resident. No new cache, owner, quota or fallback mechanism is introduced.

Pre-admission, shared-root lifetime, exact inverse/color arithmetic, frozen
quantization, restarts, separate scans, uncoded padding and cancellation stay
explicit. Caller writes and owned-output publication wait for complete coding
and EOI checks. Tests establish retained-result aggregate admission, terminal
refusal/cancellation cleanup and subsequent recovery. Warm unrooted coefficient
pooling already avoided those large allocations: **no allocation reduction is
claimed**. Native allocations are unmeasured.

Fresh original/candidate three-actual means are730.471/700.262µs:4.135% less
elapsed and4.537% native-normalized reduction. The owner explicitly accepted this
4.1% result below the initial5% screen. One candidate twelve then reused the valid
original twelve: **10.259% filtered /10.220% all elapsed**, **11.465% normalized**.
Raw99.9% CIs and conservative normalized envelopes separately separate. The three
companions above are current-cost confirmations without fresh paired causal claims.
Packets, descriptors, complete-operation boundaries, native settings, exact pixels,
source/independent ownership and idle-v2 guards pass. No job with actual samples
was repeated. An initial validation setup failure had zero actuals; the retained
confirm driver and refreshed untimed proof preserve all frozen fixtures/settings.

Both OS pass **5,178 checks each** (2,088 internal,2,142 public,948 live), plus
334 internal/339 public focused checks normal/scalar. Seven new public cases cover
memory, cleanup, late failure, padding and quantization freezing. The same actual
Release package, governed async consumer and three examples pass on both OSes.
Two Windows core builds were required by a real pre-scoring memory-invariant fix;
one Linux core build and zero harness/native/selector builds. Verified immutable
cores were reused through timing and qualification. Initial rejection/restoration
and subsequent exact reactivation at the owner's instruction are retained honestly.

These four costs supersede older affected baseline evidence. Other baseline
decode/validation boundaries need applicable current costs before CPU-pass claims;
common progressive dispatch also receives no new CPU-pass claim. Metadata-only
parsing, encoders, other codecs and editing/text retain relevant unchanged proof.
The full **268-path ≤1×** objective remains open. Largest unchanged confirmed gaps
are Lanczos word/byte reduction1.585/1.500; stop retuning this architecture round.

[Evidence](warm-jpeg-direct-baseline-planes-evidence.json),
[prior caller reuse](WARM-JPEG-CALLER-COEFFICIENT-SLOTS.md).
Mirrored/member-verified jpeg-direct-baseline-planes-qualified.tar.xz, 254 members,
SHA256 0c535221ceb98df2d72b47d5d02c00a650853a7b3f5f9af9e1a842ace2067e05; reference/checkpoint dependencies are pinned by hash.

## Current staging profile

After the staging change, one two-second diagnostic on the qualified 4:1:1-strip
producer reuses the existing owned sparse-CMYK control and immutable core. Of
1,933 samples, 29.4% are in baseline entropy routines, 22.6% in the inverse kernel,
and 14.4% in its block coordinator. Instruction mapping locates 136 samples
(7.0% overall) in the eight-row publication loop. Color conversion is 2.7%.
These are profile proportions, without an elapsed-time or parity claim.

Next examine one fused DC/AC traversal before smaller publication changes. Keep
the original eight-bit tables, marker/error cursor and all pixel/resource gates;
no candidate is accepted from this profile. No builds, repeated timing,
passing-case retuning or new workload grid. Existing full/package proof applies.
[Profile evidence](current-jpeg-baseline-profile-evidence.json) pins the current
source and raw map/JIT/samples. Mirrored/member-verified jpeg-baseline-current-profile.tar.xz,
69 members, SHA256 3e2c6e782a934bb3bbcd7e33b88a7c3886433dcc045cec23b449ea78ccd3e267.

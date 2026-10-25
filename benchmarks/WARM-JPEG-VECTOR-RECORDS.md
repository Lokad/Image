# Exact SIMD JPEG record packing

Existing 513×259 controls retain frozen inputs/settings, one CPU and complete
normalization, encoding and fresh output acquisition/release. Color references
use FFmpeg; gray uses the approved standalone SIMD libjpeg-turbo3.1.4.1 exception.
Packets remain exact, with only the approved color JPEG COM attribution difference.

| Quiet VM run | Lokad µs | Reference µs | Ratio/upper | Allocated B/op | Packet B managed/native |
|---|---:|---:|---:|---:|---:|---:|
| CallerRGB420short | 2,658.36 | 1,580.35 | 1.682/2.266 | 373,421 | 52,091/52,093 |
| CallerRGB420independent | 2,617.06 | 1,578.04 | 1.658/1.669 | 373,421 | 52,091/52,093 |
| governed | 2,855.77 | 1,836.48 | 1.555/1.649 | 804,645 | 52,115/52,117 |
| gray | 1,774.27 | 1,409.99 | 1.258/1.417 | 155,445 | 58,910/58,910 |
| callerGray | 1,367.26 | 1,080.21 | 1.266/1.517 | 155,447 | 58,910/58,910 |
| rgba8 | 2,624.06 | 1,762.84 | 1.489/1.716 | 373,415 | 52,091/52,093 |
| rgba16 | 2,763.25 | 1,837.30 | 1.504/1.799 | 373,415 | 52,115/52,117 |
| rgba422 | 3,220.60 | 2,350.36 | 1.370/1.590 | 515,072 | 71,166/71,168 |

Caller RGB420 independently saves **15.1% elapsed** against
[the preceding confirmation](WARM-JPEG-CALLER-CONFIRMATION.md). All 12 actuals,
10 selected samples and both idle-v2 guards pass. Other rows are short screens.
Governed RGBA16 retains the 1,704,856 B observed root peak. Allocation differences
of a few bytes per operation are not claimed as meaningful reductions.

AVX512F/CD batches exact amplitude/category arithmetic; DC, run lengths,
histograms and record order remain scalar and exact. The 256 B stack result fits
the existing 8 KiB admission. Sparse, unsupported and overlapping layouts retain
the original traversal; ownership, work and cancellation are unchanged.

Both platforms pass 5,129 checks, including 947 live comparisons, 9 scalar
record/mask/admission checks and 148 scalar public JPEG checks. Exhaustive signed16
arithmetic and whole-record/state/alias/failure proofs match the original. SIMD
is exercised on the VM; Windows hardware uses the scalar path. The same actual
private 0.2.0 Release package and all three examples pass both platforms.

[Evidence](warm-jpeg-vector-records-evidence.json) records identities and reuse.
Other affected timings remain historical; 444/direct JPEG, decoding, PNG/WebP,
editing and text reuse unchanged evidence. VP8 now leads confirmed encoder gaps;
full 257-case/1× and replacement readiness remain open.

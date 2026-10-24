# Exact JPEG entropy-record replay

Existing 513×259 color JPEG420 controls retain owned Q85/qscale5 conversion,
quantization and full packets. Canonical bytes and normalized pixels are exact;
the approved encoder-attribution COM alone makes packets 52,115/52,117 B for
RGBA16 and 52,091/52,093 B for RGBA8. Numeric quality values do not establish
matching. Complete operations include normalization and fresh owned output/release.
The primary control borrows a caller IMemoryBudget with a 32MiB limit.

| Quiet one-CPU run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| RGBA16 governed short | 4,597.51 | 1,839.25 | 2.500/2.817 | 804,019 |
| RGBA16 governed independent | 4,584.96 | 1,837.34 | 2.495/2.510 | 804,019 |
| RGBA8 current short | 4,017.11 | 1,766.53 | 2.274/2.628 | 372,856 |
| RGBA16 current short | 4,488.07 | 1,839.99 | 2.439/2.727 | 372,859 |

Independent governed confirmation uses 10.7% less elapsed than the
[preceding confirmation](WARM-JPEG-SPARSE-SCAN.md). All 12 actual iterations are
retained, 11 selected; both contribute to the conservative upper. Both idle-v2
guards pass. The other two existing boundaries are current short reconciliations.

The adaptive frequency pass records symbol/VLI events once. Emission avoids a
second coefficient traversal, preserving DC prediction, EOB/ZRL, stuffing,
alignment and every original writer/work/cancellation call. Color conversion,
transforms, quantization and Huffman generation remain. One owned buffer starts
with the previous data capacity, with one admitted doubling for dense records;
old/new capacity overlaps remain charged until replacement ends. Shared refusal
is terminal. The global retained pool ceiling stays 4MiB; governed calls bypass it.

This control needs no growth: allocation increases by 48 B/op for the owner,
and admitted peak rises by 128 B to 1,704,856 B, including its surviving source.
Both platforms pass 5,107 checks each, including 947 live comparisons, 41 scalar
record/stage checks and 148 scalar public JPEG checks. Frozen fragment, native
packet, aggregate admission, growth-peak, refusal/cancellation cleanup and recovery
gates pass. Two old public peak expectations were updated by the exact 128 B;
all packet/work/source checks remain. The same actual private 0.2.0 package and
all three examples pass both platforms.

[Evidence](warm-jpeg-entropy-records-evidence.json) records samples, identities,
qualification and reuse. Other affected color-JPEG timings remain historical;
gray, other codecs, decoders/editing/text reuse unchanged evidence. No cases were
added. VP8 is now the largest measured gap; full 257-case CPU parity stays open.

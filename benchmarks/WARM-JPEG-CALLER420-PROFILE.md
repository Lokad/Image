# Current caller-RGB420 JPEG profile

One bounded capture reuses the existing 513×259 resident packed RGB8 control,
Q85/native qscale5, prepared workers/host and qualified production build. No
production/harness rebuild, image/settings grid or repeated package proof.
The complete operation includes conversion, encoding and fresh independent output
acquisition/release, one CPU. Instrumented times are discarded.

| Exclusive attribution | Samples | Share of workload samples |
| --- | ---: | ---: |
| `JpegEncoder.ReferenceSubsampledPass` | 256 | 33.0% |
| `JpegEntropyRecords.WriteRecords` | 140 | 18.1% |
| `System.Buffer.MemmoveInternal` | 58 | 7.5% |
| `JpegEntropyRecords.RecordBlock` | 55 | 7.1% |
| `JpegEncoder.ReferenceChroma` | 39 | 5.0% |

There are 775 workload-scoped samples, 774 managed. These are diagnostic sample
shares, not exact component timings. Both idle-v2 guards pass; the bounded trace
and source/producer identities are retained. Complete packets stay 52,091/52,093 B,
with only approved COM attribution differences. Resident input, reconstructed
pixels, quality, native frozen packet and independent output ownership checks pass.
No RGBA staging is introduced. [Cost at capture](WARM-CURRENT-ENCODER-ANCHORS.md)
was the unchanged 1.653× short screen; this profile makes no timing/parity claim.

Subsampled-pass attribution includes inlined normalization/filter work; inspect
its actual stages rather than assigning that share to one algorithm. Entropy replay
is a distinct 18.1% lead: every coefficient writes Huffman and amplitude separately,
with repeated validation/cancellation/bit-buffer updates. This lead produced the
qualified [paired append](WARM-JPEG-PAIRED-ENTROPY.md), with independent bit-packing,
stuffing/restart, zero-bit, buffer-boundary and cleanup proofs. This frozen
diagnostic itself contains no production change.

[Evidence](warm-jpeg-caller420-profile-evidence.json) records the raw trace,
guards, original validation and the qualified producer at capture. Its cost and
producer are historical after the paired append; published packages remain
unchanged. Full 253-path warm 1× and replacement readiness stay open.

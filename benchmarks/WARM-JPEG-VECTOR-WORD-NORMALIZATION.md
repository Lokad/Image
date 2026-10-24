# Exact RGBA16 JPEG normalization

The existing 513×259 RGBA16 JPEG420 control retains Q85, owned reference qscale5
quantizers and complete operation boundaries: normalization, encoding and fresh
independent output acquisition/release. One CPU. Its caller-owned root limit is
32 MiB. Canonical bytes and normalized pixels stay exact; only the approved
attribution COM differs (52,115/52,117 B). Reference inputs/settings are unchanged.

| Quiet VM run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| Caller-root short | 4,314.87 | 1,841.78 | 2.343/2.515 | 804,019 |
| Caller-root independent | 4,332.72 | 1,840.05 | 2.355/2.369 | 804,019 |
| Nongoverned current short | 4,188.69 | 1,837.74 | 2.279/2.627 | 372,859 |

Independent confirmation uses 5.5% less elapsed than the
[preceding confirmation](WARM-JPEG-ENTROPY-RECORDS.md). All 12 actual iterations
are retained, 10 selected; both contribute to the conservative upper. Both idle-v2
guards pass. Allocation and shared-root peak remain 804,019 B/op and 1,704,856 B,
including the 1,062,936 B surviving source. The nongoverned row is a current short
reconciliation, without an independent speedup claim.

Safe AVX2 evaluates the existing rational flattening in binary64 lanes. A universal
integer-separation proof establishes exact truncation over every source, alpha and
background value. Required scalar paths preserve portable execution, tails, aliases
and failure write prefixes. No heap storage, owner, scope, copy or charge is added;
scratch fits the existing admission. Work, cancellation and ownership stay intact.

Both platforms pass 5,117 checks each, including 947 live comparisons, 12 scalar
normalization checks and 148 scalar public JPEG checks. The independent proof
compares 10,617,222 pixels and 810 alias/failure boundaries per platform/mode.
Full sample/alpha controls, native packets, zero warm allocation and existing
aggregate admission/cleanup/recovery gates pass. The SAME actual private 0.2.0
Release package and all three examples pass both platforms.

[Evidence](warm-jpeg-vector-word-normalization-evidence.json) records identities,
universal bounds and qualification. Other affected RGBA16 JPEG timings remain
historical. RGBA8, caller samples, other codecs, decoding, editing and text reuse
unchanged evidence. No controls were added. Full warm parity remains open.

# Grayscale JPEG row preparation

The existing513×259 RGBA8-to-gray control retains Q69, owned quantizers,
background0x204060 and raw-exact58,910-byte packets. Its explicitly approved
reference is SIMD-enabled standalone libjpeg-turbo3.1.4.1, benchmark tooling only.
Complete operations include normalization, encoding, fresh owned output and release.

| Quiet one-CPU run | Lokad µs | libjpeg-turbo µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| Short | 5,173.67 | 1,407.80 | 3.675/3.930 | 154,838 |
| Independent confirmation | 5,186.41 | 1,402.84 | 3.697/3.719 | 154,838 |

The independent confirmation uses30.8% less elapsed than the
[preceding confirmation](WARM-JPEG-GRAY-FACTOR.md). All12actual iterations and
both versioned idle guards pass.154,838B/op retains raw GC reporting; this is
within2B of the previous154,840B/op and adds no per-call heap storage.

One row acquisition now prepares up to8valid pixels using existing RGBA8/16
normalization, exact binary64 gray arithmetic and edge replication. Packed caller
RGB uses a bounded24-byte row. Stack scratch fits the existing8KiB workspace;
shared/local admission, work, cancellation and independent output ownership remain.
The direct caller-gray branch is unchanged: its valid4.103× screen and154,843B/op
are reused. No method-cost attribution follows from comparing these boundaries.

Both platforms pass5,084 checks each, including947live comparisons,7affected
scalar gray-stage checks and148scalar public JPEG checks.10,766original/edge
blocks retain689,024exact frozen gray samples. Padded rows, source ownership,
background/length rejection, zero warm allocation and unchanged charges pass.
The same actual private0.2.0 package and all three examples pass both platforms.
Pre-package headroom reserves5MB plus margin.

[Evidence](warm-jpeg-gray-rows-evidence.json) records producer identities, samples,
qualification and precise reuse. Other codecs, color JPEG, decoders/editing/text
remain unchanged. CPU parity and the full257-case objective remain open.

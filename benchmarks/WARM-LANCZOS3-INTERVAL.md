# Exact Lanczos3 rounding intervals

A certified rounding interval cuts complete Lanczos3 time **19.1–38.6%** from
the preceding exact-remainder implementation. All four existing cases remain
measured gaps against independently confirmed warm single-CPU parity.

| Complete operation | Lokad µs | Reference µs | Ratio / conservative upper | Lokad allocated B/op | Time saved |
| --- | ---: | ---: | ---: | ---: | ---: |
| Reduce8 | 4,303.48 | 491.71 | 8.752 / 9.087 | 146,614 | 22.4% |
| Reduce16 | 4,350.34 | 507.61 | 8.570 / 8.980 | 146,614 | 19.1% |
| Enlarge8 | 29,535.64 | 4,063.62 | 7.268 / 7.550 | 351,940 | 38.6% |
| Enlarge16 | 30,398.20 | 3,945.75 | 7.704 / 8.058 | 4,616,301 | 33.3% |

For `0<n<M*d`, `M<=65535` and `2^59<=d<2^80`, split numerator and denominator
into high bits and 32 discarded low bits. The projected quotient's absolute
error is at most `(M+1)/(2^27-1)`. Conversion, division and rounding-bin comparison
add less than `2^-34`; the combined bound is strictly below `2^-10`. Accept a
rounded estimate only when its distance from that integer is strictly less than
`1/2-2^-10`. The entire exact quotient interval then lies in one rounding bin.
Uncertain boundaries and out-of-domain inputs retain exact remainder correction
or exact division. Half ties, saturation and every original output remain exact.

Four new checks cover domain endpoints, rounding margins and extreme discarded
bits against independent BigInteger rationals. All four complete frames match
the protected original references on Windows/Linux. Coefficients, scratch,
admission, work, cancellation and ownership are unchanged. Small allocation
differences reflect amortized harness overhead; no storage was added.

One immutable producer, the prepared four-case child, original fixtures and
unchanged native worker/quality proofs were reused throughout. Native reference
is explicitly **zimg, FFmpeg's zscale backend**; the
[baseline](WARM-LANCZOS3-BASELINE.md) retains complete equivalent operation
boundaries, normalization, output ownership and quality gates. Eight scored
quiet guards pass. One Reduce16 preguard refused I/O wait and launched nothing;
the retry passed fresh guards. Both native brackets and selected/all-actual
confidence views are retained. No long confirmation was run above one.

Qualification: **4,435 Release tests per OS** (1,622 internal, 1,881 public and
932 live FFmpeg), **34 intrinsics-disabled Lanczos checks per OS**, the same
actual private 0.2.0 package consumer and all three packaged usage examples on
Windows/Linux. Twelve font comparisons are reused because their production
paths are unchanged. Published 0.1.0 remains immutable. The other 227 numeric
rows retain their identified historical builds; no broad current parity claim
follows. Next: investigate exact SIMD vertical accumulation on Reduce8, with
proven arithmetic bounds and the existing Int128 fallback.

[Numeric evidence](warm-lanczos3-interval-evidence.json) includes samples,
allocations, identities, mathematical bounds, the failed preguard and reuse.
Qualified source: `d86f0a8d8390c73aa95b1aad4a5d9f1b0af3fc471d660df9679e133caee1e16e`.
Raw `l3i-qualified-windows.tar.gz`:
`76bdac87db8f50d549ba09b67a4bfc4da419da16a1e3b0605240c8eb9ad2ff55`;
`l3i-qualified-linux.tar.gz`:
`4f46360f3ee54e649d956d60349550e5623cc1b640e3c7edef0446f230fe1aab`.
Unchanged runtime dependencies, fixtures and native worker remain in the
baseline archive. Functional compatibility, memory integration and performance
qualification remain separate gates.

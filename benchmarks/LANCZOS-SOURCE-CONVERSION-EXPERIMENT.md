# Prepared source-word conversion

LDP693 is not selected. It prepares exact UInt32-to-double source products once
per horizontal-row miss, borrowing the existing sum row before its first sum
user. Cached filtered rows remain independent; the sum row is then cleared and
the original ordered vertical kernel runs. One typed UInt32/double horizontal
body preserves every FMA/reduction bit. Insufficient capacity or ring slots retain
the original streaming path. No storage, owner, pool or session is added.

| Complete prepared RGBA16 control | Original → candidate µs | Native original → candidate µs | Retained plan B |
| --- | ---: | ---: | ---: |
| Enlargement | 4,121.06 → 4,141.91 | 3,905.12 → 3,900.99 | 333,396 |
| Reduction | 550.84 → 561.48 | 512.01 → 511.19 | 148,040 |

Each job has three actuals, both native brackets, quiet guards and all frozen
exact/native/source/ownership checks. Complete source reads, preparation, clearing,
output ownership and disposal stay timed. Native prepared graph/input/scratch
remain retained and labeled. Broad intervals establish no causal gain, regression
or equivalence; there is no useful complete point lead for confirmation.
No favorable repeat or further variant follows this round.

Allocation is4,265,332/4,265,302 B/op for enlargement and139 B/op for reduction.
The30-byte aggregate difference is amortized measurement accounting, not an
intrinsic saving. Admitted and retained capacity are unchanged; physical/native
peaks are not newly measured. Moving conversion is exact but does not establish
a CPU benefit; doubled source traffic and generated code remain possible costs.

The candidate passes254 internal/729 public focused checks per normal/scalar
mode on each OS. They cover unsigned extremes, bit-identical horizontal values,
guards/tails/long support, zero producer allocation, exact integer complete frames,
cancellation/refusal cleanup and independent surviving outputs. Candidate-only
tests are archived and removed. Production sources, main/embedded modules and
public bindings restore byte for byte, including existing cache-boundary coverage.
The actual package remains unchanged. Existing full/scalar/ISA/package proof is
reused without restoration builds or repeated qualification.

[Evidence](lanczos-source-conversion-evidence.json) preserves the implementation,
all actuals and exact restoration. Mirrored/member-verified raw
`lanczos-source-conversion-experiment.tar.xz` has11,512,372B/93members, SHA256
`a049ce1bedae9d041725aa4b01aff00865ff1411ee1e0bb7ab7db48610779af9`.
It adds no renderer dependency; current costs remain those of
[certified word rows](LANCZOS-WORD-CERTIFIED-ROWS.md). This outcome does not ban
materially revised preparation designs. Continue independent codec work;
compression correctness and the full performance objective remain.

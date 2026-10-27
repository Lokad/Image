# Rejected VP8 trellis state reduction

The fixed 513×259 diagram, Q75/effort6, has a fresh qualified baseline of
**15,778.14 µs / 1.611× reference time**. Collapsing equivalent predecessor rows
and eliminating provably dead states gives **16,326.48 µs / 1.666×**, about 3.5%
slower. Confidence intervals overlap; no useful gain is demonstrated. Reject this
trial and keep the restored production implementation.

Both jobs retain three actual samples, passing idle-v2 guards, the same cached
worker/selector, complete owned-output boundary and exact 6,818-byte packet.
Both allocate 754,344 B/op. Conservative ratio uppers are 1.658× and 1.759×.

An attribution-only profile places 44.5% of managed samples in trellis Apply;
its times are discarded. Private equivalence checks cover 3,528 score/tie boundary
cases and 24,576 whole-trellis cases on each OS, including extreme positive lambda.
These focused proofs establish no speed improvement or broader qualification.

The qualified source and immutable producer are restored on both OS. No candidate
confirmation, repeated full suite or package regeneration is warranted. The
mirrored, member-verified archive in [the evidence](vp8-trellis-state-reduction-rejected-evidence.json)
retains the candidate source, binaries, proofs, profile and actual samples.

The [current queue](WARM-COVERAGE.md) prioritizes VP8L's similar ratio gaps and much
larger absolute costs. Do not renew this state-collapse trial without a materially
different opportunity.

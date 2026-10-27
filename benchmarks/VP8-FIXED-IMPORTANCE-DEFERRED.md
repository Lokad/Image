# Deferred fixed VP8 importance model

All production trellis callers use the same 16 Gaussian weights. A bounded trial
derives their integer values once, removes repeated setup/validation and preserves
coefficient/layout checks, cancellation and strict decisions.

The existing 513×259 opaque diagram, Q75/effort6, measures **15,431.26 µs / 1.567×,
upper1.678** in the short screen: only 2.2% below the unchanged 15,778.14 µs baseline,
with overlapping bounds. Exact 6,818-byte packets and 754,344 B/op are unchanged.
Defer and remove the trial without another confirmation or full/package repeat.

Eighteen focused normal/scalar tests pass on each OS; frozen native fragments and
rolling-state hashes remain exact. The qualified color-trial code and matched
producers were restored. [Evidence](vp8-fixed-importance-deferred-evidence.json)
retains all actuals, quiet guards, derivation, identities and the mirrored,
member-verified 27-member raw archive. This family is closed absent new evidence.

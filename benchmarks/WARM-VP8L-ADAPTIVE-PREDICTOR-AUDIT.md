# Lossless WebP adaptive-predictor audit

Historical C44 screen. The recipe is subsequently reconsidered on the current
grouped basis in the [accepted C91 round](WARM-VP8L-ADAPTIVE-GROUPS.md); the values
and rejection below describe the original producer only.

The 8×8 adaptive-predictor candidate is **not adopted**. It saves 2,652 bytes
(1.44%) on the fixed photograph and nothing on the diagram. Both size gates
remain open; this result does not justify adding its search to production.

| Frozen opaque 513×259 control | Current bytes | Candidate bytes | Reference / ceiling |
| --- | ---: | ---: | ---: |
| Photograph | 184,518 | 181,866 | 160,032 |
| Text/diagram | 7,636 | 7,636 | 7,234 |

The independently implemented selector ranks all fourteen normative predictors
using channel entropy within each tile. Complete header and payload costs,
including the mode map, select between the candidate and the existing encoding.
The diagram retains its original packet. Inputs, effort4 and the frozen libwebp
`exact=1` reference settings are unchanged.

Both packets decode to the exact source with managed and normal/scalar FFmpeg
decoders; source storage and independent outputs are preserved. Thirty-two focused
tests pass with hardware intrinsics enabled and disabled, including independent
channel arithmetic for every mode, single axes, partial tiles, 8/16-bit
normalization, admitted scratch, terminal refusal, cancellation and recovery.

One Linux production build and two internal-test build attempts (the first used
an unavailable fixture helper); no Windows, harness, native or selector build.
The immutable production build is reused throughout. No scored job, allocation
measurement, full qualification or package proof is claimed for this rejected
candidate. The previously qualified producer, test assemblies and incremental
outputs are restored byte-for-byte without rebuilding.

Color decorrelation in the predictor-residual domain and spatial entropy groups
remain distinct opportunities. This audit does not establish either one's value.
The transform arithmetic is defined by [RFC 9649](https://www.rfc-editor.org/rfc/rfc9649.html#section-3.5).

[Numeric evidence](vp8l-adaptive-predictor-audit-evidence.json) records identities
and exactness. Raw candidate source, tests, binaries, packets and logs are retained
on both hosts as `c44-adaptive-predictor-candidate.tar.gz`, SHA256
`45ce46e13efae9736931d3330c1b80b08ce0306a3972fd4906b7e3156257dce6`.

# VP8 validation token materialization

Coding validation now shares the production token readers without materializing
unused dequantized coefficients, inverse-Walsh results or small-mode output. One
geometry/storage scheduler serves validation and decoding; only coefficient
materialization specializes. Validation already omitted pixel planes before this
round. Every category/sign decision, entropy context, cancellation checkpoint and
completed error prefix remains required.

Independent complete validation saves **595.69 µs / 9.97% raw and 10.04%
native-normalized**. Both filtered and all-actual confidence views separate;
candidate/original intervals are [0.895697, 0.904914] raw and [0.891598, 0.907994]
normalized. Native movement is +0.080%. This validation control passes parity;
the five decoder controls retain gaps.

| Frozen 513×259 complete operation | Lokad µs | Reference µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Contemporary original VP8+ALPH validation | 5,974.61 | 5,445.72 | 1.097 / 1.100 | 554,478 |
| Current VP8+ALPH validation | 5,378.92 | 5,450.06 | 0.987 / 0.993 | 545,478 |
| Current VP8+ALPH span decode | 6,224.83 | 5,968.30 | 1.043 / 1.047 | 238,384 |
| Current VP8+ALPH fresh seekable Stream decode | 6,238.72 | 5,987.12 | 1.042 / 1.046 | 238,480 |
| Current opaque decode | 5,157.52 | 4,795.51 | 1.075 / 1.090 | 760,564 |
| Current live opaque decode | 5,289.47 | 4,796.26 | 1.103 / 1.107 | 760,564 |
| Current simple-filter opaque decode | 5,275.88 | 4,890.88 | 1.079 / 1.083 | 760,564 |

Each current measurement has twelve actuals and twelve samples per native bracket,
on one quiet VM CPU with idle-v2 guards. Inputs/settings, independent outputs and
disposal are unchanged. Validation includes complete borrowed-span syntax traversal
and an owned 36-byte metadata descriptor; its FFmpeg reference includes natural-frame
decoding and the original source-metadata adapter. Decoding includes preparation,
reconstruction/filtering/conversion and an exact independently owned 531,468-byte
RGBA frame. Native codec/conversion contexts remain retained and explicitly labeled.
These are scoped controls, not whole-library parity or compression claims.

The omitted 8,976-byte mode payload plus its array header removes **9,000 B/op**.
Both Windows/Linux admission tests retain **536,248 B peak** for first and live
validation: the later ALPH phase dominates. Retained validation storage is zero;
no pool/cache/owner is added. Whole physical peak and native allocation are not
measured. The earlier [alpha handoff](VP8-ALPHA-OUTPUT-HANDOFF.md) keeps its separate
full-decode peak/output evidence; those figures are not validation measurements.

Token magnitudes 0–4 equal their tokens; category tokens produce magnitudes ≥5.
Thus capped entropy contexts and nonzero flags can use tokens directly, before
unchecked dequantization. Category/sign bits still run in original order. The
PixelMode projection of both complete reader source files equals the original,
ignoring whitespace. Fixed-geometry unchecked Walsh/stores have no extra coding
verdict. Actual arrays are reserved before allocation; refusal remains terminal.

Early variants exposed real contradictions. A widened pixel-reader live ranges
and regressed ordinary decode by 2.68%. B restored its kernel but regressed live
decode by 1.91%; C preserved the original pixel source and 3,789-byte live kernel
yet still regressed by 2.01%. D removes the duplicated specialized scheduler.
Its contemporary live comparison is 5,304.11 → 5,289.47 µs, with overlapping raw
and normalized intervals; neither an incremental gain nor loss is established.
The ordinary comparison is likewise inconclusive across both confidence views.
No earlier adverse costs are hidden or used as current proof.

The first screen completed twelve managed actuals with three native samples despite
a driver expectation of three actuals. Its count assertion failed; semantic/quiet
gates passed. The original completed log was recovered without replay and remains
screen-only. Final D uses the full independent twelve/twelve boundary above.
Failed JIT collection attempts and all A–D sources/modules/results are retained.

Windows/Linux each pass **5,772 full/live checks** (2,515 internal, 2,309 public,
948 differential), plus 69 internal / 65 public per normal/scalar mode. Four new
internal rows check token/context/cursor equality, malformed models and cancellation;
window tests cover 24/25 blocks. A public large-frame live sync/async test checks
exact pixels, borrowed input/root and independent surviving outputs. Existing
native/malformed/admission coverage remains; no ISA selection changed.

The SAME private 0.2.1 Release package and all three usage examples pass both OSes:
SHA256 `8b46c650b78f810f1f9b7e4bbe18dd6af28da5bba12831375a7e13a10cb2796e`,
421,197 B. One immutable final main/embedded build per OS is reused. Test-only
public rebuilds support peak logging and the added live case; benchmark/native/
selector/fixture builds are zero. Two displayed comparisons refresh; 123 numerical
rows reuse unchanged paths. Published releases are immutable; publication, a full
actual-package release refresh and all 283-path parity remain open.

[Evidence](vp8-validation-materialization-evidence.json) pins source proofs,
measurements, qualification and mirrored/member-verified raw inputs. Earlier
[live block execution](VP8-LIVE-BLOCK-EXECUTION.md) and
[JPEG lifetimes](JPEG-COEFFICIENT-REFERENCE-LIFETIME.md) retain source-valid history.
Raw `vp8-validation-materialization-qualified.tar.xz`: 5,370,380 B / 592 members,
SHA256 `859ce3a287dddcf9516f4d4898dd0df239274280421372d3808d90741341174f`.

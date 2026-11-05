# Prepared combined VP8 coefficient prices

The operation-owned rate plan now prepares fixed sign/category plus variable
coefficient costs in its existing 96×68-word table. Scalar residual scoring,
SIMD prepared scoring and both trellis predecessor states use those prices.
Magnitudes above67 add the signed difference fixed[actual]−fixed[67] before
lambda multiplication. Candidate order, strict ties and integer arithmetic stay
exact. No additional buffer, owner, pool, cache, session or compatibility view.
The conservative generated-price bound is37,120, within UInt16.

Independent counterbalanced complete-encode confirmations:

- **Photograph Q75/effort4: 197.93 µs / 1.70%**, 11,653.71→11,455.77 µs; ratio 0.983016, interval [0.981354, 0.984680].
- **Diagram Q75/effort6: 183.99 µs / 1.61%**, 11,438.41→11,254.42 µs; ratio 0.983922, interval [0.979941, 0.987910].

Each retains12 alternating pairs ×16 complete encodes after a separate three-pair
screen. Both screens and all observations remain retained. Exact packets stay
17,688/6,818B and paired allocations809,072/754,344B, respectively. Identical
resident-source setup/JIT/checks are excluded; source processing, table creation
and probability refresh, public EncodeWebp/admission, fresh output growth,
independent ownership and disposal remain timed on one quiet pinned CPU.

The current-photo diagnostic motivating the change counted769,122 coefficient
prices, only62 above67. That establishes applicability, not a phase-time saving.
Generated code grows: table preparation514→529B, integrated Luma4 8049→8261B,
trellis search2436→2453B. These are source/code facts, not additional timing or
stack-peak claims. Complete-operation confirmations include the extra preparation.

| Current frozen complete513×259 encode | Managed µs | FFmpeg/libwebp µs | Ratio / upper | B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Opaque RGBA8, effort4 | 15,530.20 | 15,502.43 | 1.002 / 1.012 | 1,201,552 | 72,380 |
| Opaque RGBA16, normalization | 16,404.70 | 15,641.43 | 1.049 / 1.103 | 1,201,552 | 72,380 |
| Opaque photograph, effort4 | 11,403.47 | 9,640.01 | 1.183 / 1.187 | 809,225 | 17,688 |
| Opaque text/diagram, effort4 | 6,739.71 | 5,818.49 | 1.158 / 1.196 | 741,137 | 7,100 |
| Diagram, effort2/distortion | 3,235.41 | 2,690.03 | 1.203 / 1.210 | 741,375 | 7,330 |
| Diagram, effort6/trellis | 12,230.81 | 10,636.61 | 1.150 / 1.163 | 754,344 | 6,818 |
| RGBA8 + lossless alpha, effort4 | 57,549.93 | 55,982.73 | 1.028 / 1.042 | 7,034,392 | 98,590 |
| RGBA16 + alpha, normalization | 55,162.83 | 53,708.06 | 1.027 / 1.040 | 7,033,984 | 98,590 |

Current status: 5 gap, 3 uncertain. Every original input/settings, complete packet,
pixel/alpha/quality and source/output/ownership gate passes. Each row retains12
actuals, filtered and all-actual confidence, both native brackets and quiet
guards. The native reference retains its stated wrapper/private scratch;
normalization/import, full pixel processing and independent output ownership
stay timed. Other six refreshed costs are observations, not independent claims
of incremental gains. Other117 displayed costs reuse unchanged production paths.
The full282-path objective remains open; no release-wide refresh is claimed.

The owned rate-plan capacity remains20,192B; live refresh allocates/reserves
nothing even at a full shared root. No new owned heap capacity or retained
operation scratch. Report actual standalone GC/batch allocation readings
separately from the paired driver. Physical/admitted complete peaks and native
allocations are not newly measured; no allocation or peak saving is claimed.

Both OS pass5,716 full/live checks (2,479 internal,2,289 public,948 differential),
65 internal/137 public focused normal/scalar checks, and the SAME actual private
0.2.1 Release package/all3 examples. One immutable main build perOS is reused;
embedded tests rebuild once after the initial synthetic-test mismatch. Original
arbitrary UInt16 synthetic components could overflow their combined words;
the corrected14bit inputs have an independent scalar oracle and frozen Python
digest across786,432 SIMD comparisons. The failing logs/source remain retained.
Native cost-table/residual digests and encoder packets are unchanged. Coverage
also preserves full signed domain rejection, trellis weights/extreme lambda/DC/
ties/tails, disposal/cancellation, terminal refusal, async and independent outputs.
Unchanged54 AVX512-off/JPEG/wide proofs are reused; published packages immutable.

[Evidence](vp8-combined-rate-preparation-evidence.json) pins sources/modules,
commands, original/selected builds, output gates and package proof. The previous
[shared-geometry](VP8-SHARED-COEFFICIENT-GEOMETRY.md) and
[zero-router](VP8-ZERO-ROUTE-SEARCH.md) gains remain history; current rendering has
one VP8 owner. Raw `vp8-combined-rate-preparation-qualified.tar.xz`: 5,955,316B/291members,
SHA256 `3e241fdd69e366895e934ef75363c124c3bd334c5e8c64cadd34587738d8815f`, mirrored and member-verified. Publication is postponed.

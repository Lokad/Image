# JPEG inverse-transform rounding

The contract is the original direct binary64 evaluation of T.81 A.3.3 using
the frozen basis in `JpegTransform`. It multiplies coefficient by quantizer,
then horizontal and vertical basis values separately, adds terms in v/u order,
divides by four, adds 128, rounds away from zero and clips to a byte.
The mathematical basis comes from [T.81](https://www.itu.int/rec/T-REC-T.81/en).

The AVX-512/VL path can factor the transform into horizontal and vertical
eight-term sums. Reassociation changes binary64 intermediates, so mathematical
equivalence alone does not establish byte equivalence. Every output row needs a
rounding certificate; an uncertain row uses the original direct evaluation.
AVX/scalar and large-magnitude blocks retain the original evaluation.

## Independent error bound

Let `a[v,u]` be the exact coefficient/quantizer product and let
`M = sum(abs(a[v,u]))`. A signed Int32 times UInt16 is an integer of magnitude
less than 2^47. Across 64 entries, `M <= 2^53 - 2^37`; both the products and
the nonnegative accumulation, including `M + 128`, are exact binary64 integers.
The horizontal pass reuses each dequantized product for four independent
magnitude sums, then combines them. Every partial sum stays below 2^53, so
this regrouping gives exactly the original serial M and the same certificate.
All frozen basis entries have magnitude below one. This analysis treats those
stored entries as exact inputs; it does not approximate an ideal cosine.

Use binary64 unit roundoff `u = 2^-53` and `gamma(n) = n*u/(1-n*u)`.
Products of at most n rounding factors differ from one by at most gamma(n).
The background summation bound follows
[Higham's error analysis, section 2](https://nhigham.com/wp-content/uploads/2023/10/high93s.pdf).
The counts and certificate below are specific to this implementation.

Each direct term passes through at most two rounded products and 64 additions;
the final addition of 128 gives at most 67 factors. Each separable term passes
through one horizontal product, eight horizontal additions, one vertical
product, eight vertical additions and the offset addition: at most 19 factors.
The factor of one quarter is exact. Treating the offset as a further term gives
absolute errors, relative to the same exact expression, bounded respectively by
`gamma(67)*(M/4 + 128)` and `gamma(19)*(M/4 + 128)`.
Since each denominator exceeds one half, the difference between the two final
samples is strictly less than `172*u*(M/4 + 128)`.

No intermediate overflows. Underflow is also excluded: every nonzero initial
product has magnitude at least one; horizontal basis entries are at least 0.19
in magnitude. Rounded horizontal terms and their sums lie on a grid no finer
than 2^-55. A nonzero horizontal sum times a vertical basis entry therefore
exceeds 2^-58 in magnitude. Their rounded products lie on a grid no finer than
2^-110, which also bounds nonzero vertical partial sums. Direct terms have a
coarser grid. All intermediate
nonzero results are far inside the normal binary64 range, including division
by four. The hardware instructions use separate multiply/add operations, not
FMA. A zero quantizer contributes exact zero, including at Int32.MinValue.

The implementation chooses the deliberately larger, exactly computed bound
`E = 512*u*(M + 128) = 2^-44*(M + 128)`. If `E >= 0.5`, it keeps the original
whole-block calculation.
The horizontal sums are computed before this check, with no destination writes;
an extreme block discards them and uses the original direct evaluation.
Otherwise it clips the separable samples to [0,255],
subtracts their integer floors and measures distance from the half boundary.
The floor subtraction is exact. The final distance subtraction has absolute
rounding error at most u. The margin between E and the sample-difference bound
exceeds u even at M=0.

Clipping is nonexpansive. If the measured distance exceeds E, the original
clipped sample cannot cross a half boundary, so it produces the same byte.
Endpoint clipping is safe by the same argument. Equality is uncertain, and
every uncertain row evaluates all eight lanes using the original v/u order and
separate products before applying the original rounding/clipping. Half ties
therefore retain the original byte rather than selecting a tolerance.

## Resources and controls

The transform still admits all 4096 logical work units before writing any pixel.
The two passes use eight vector locals, replacing the original eight output-sum
locals; no additional array, image cache, per-block allocation or static table
is introduced. Magnitude partial sums use scalar locals. The original AVX-512
direct path remains the extreme-block path.
Fixed-size fallback loops remain inside the same admitted block operation.

The original twelve scalar pixel hashes, half-boundary sample controls, full
Int32/UInt16 extremes, input/destination guards, exact work admission and
cancellation-before-write checks remain authoritative. Additional original
generated coefficient families pin outputs captured from the previous owned
assembly; they are primitive inputs rather than benchmark images or upstream
test data. Public decode checks retain the existing fixture corpus and native
decoder bounds. Correctness and generated code must pass before performance
measurement on the dedicated VM.

## Sparse integer certificate

The four-component materializer can evaluate at most eight active frequencies
with integer SIMD. Let `Q=65536`, `S=sum(abs(coefficient*quantizer))≤65536`, and
`S_AC` omit the DC term. The original generator uses exact rational nearest-even
rounding of `Q*Cx,u*Cy,v/4`, where C is the frozen owned binary64 basis. The
4096 signed short weights occupy 8 KiB of compiler RVA data. The span getter
allocates no table or cache; these are image-independent format constants.

All weight magnitudes are below16384. Thus every signed product, partial sum,
the `128Q` offset and final half-unit addition fit Int32. Non-DC weight rounding
contributes at most `S_AC/2` Q units. The DC weight is8192 at every position;
its quantization error times maximum S plus the original double-evaluation error
is below one Q unit. Specifically, with `u=2^-53` and
`g=10u/(1-10u)`, the reference error is bounded by
`Q*((S/4)*g + u*((S/4)*(1+g)+128))`. Adding
`S*abs(8192-Q*C0*C0/4)` remains below one at S65536. These rational inequalities
and every generated constant have independent controls.

Set `E=ceil(S_AC/2)+1`. After clipping the integer sum to `[0,255Q]`, a sample
certifies only when its distance to the nearest half boundary exceeds E strictly.
Clipping is nonexpansive, so the original double sample cannot cross a rounding
boundary. All four sixteen-lane groups must certify before any byte is written.
Otherwise the existing ordered double kernel handles the entire block, preserving
half ties and every original byte. Large magnitude and unsupported hardware take
that kernel before any destination write.

The existing4096 logical work charge precedes both paths. At most512 approximate
and512 fallback frequency contributions fit within it; no new array, owner,
reservation or cancellation interval is introduced. Existing DC, horizontal-only,
dense and scalar branches retain their calculation. Exact managed pixels remain
separate from the approved native decoder's quality bounds.

# JPEG forward-transform quantization certificate

The quantized-coefficient contract is the original binary64 forward evaluation
of [T.81 A.3.3](https://www.itu.int/rec/T-REC-T.81/en) with the stored, owned basis:
subtract 128 from each byte sample, multiply by horizontal and vertical basis
values separately, accumulate in y/x order, divide by four, divide by the
quantizer, and round to nearest with ties away from zero. The standalone
`JpegTransform.Forward` retains every original binary64 coefficient bit.

The encoder can combine a separable transform with quantization. Horizontal
eight-term sums precede vertical eight-term sums. Regrouping changes coefficient
bits, so a mathematical DCT identity alone cannot preserve the quantized result.
Every accepted integer needs the certificate below. An uncertain coefficient
uses the original direct y/x evaluation and original quotient/rounding instead.
Only certified or directly evaluated integers are written to the destination.

## Independent bound

Treat every stored basis value as an exact input. Its magnitude is below one,
and every nonzero value has magnitude greater than 0.19. Byte samples shifted
by 128 have magnitude at most 128. Therefore the absolute term sum for any
unscaled coefficient is at most `64 * 128 = 8192`.

Use binary64 unit roundoff `u = 2^-53` and
`gamma(n) = n*u/(1-n*u)`. Products of at most n rounding factors differ from
one by at most gamma(n); see the background model in
[Higham, section 2](https://nhigham.com/wp-content/uploads/2023/10/high93s.pdf).
The operation counts and domain bound here are derived from this implementation.

A direct term sees two products and at most 64 additions: at most 66 factors.
A separable term sees one horizontal product, eight horizontal additions, one
vertical product and eight vertical additions: at most 18 factors. Multiplication
or division by four is exact in this domain. Relative to the same exact
expression, the absolute coefficient errors are bounded by `gamma(66)*2048`
and `gamma(18)*2048`. Both denominators in gamma exceed one half, so the
direct/separable difference is strictly less than `168*u*2048`.

Each computed coefficient has magnitude below 2049. A positive UInt16 quantizer
is exact as a double. Dividing both coefficients by the same quantizer contributes
at most `2*2049*u` further absolute error. The two rounded binary64 quotients
therefore differ by less than `348162*u`, which is less than `2^-34`.
Encoder-generated quantizers are in [1,255]; this bound also covers [1,65535].
A zero quantizer declines the combined path before charging work or writing.

No intermediate overflows or underflows. Nonzero sample/basis products have
magnitude above 0.19 and lie on a binary grid no finer than `2^-55`. Horizontal
partial sums remain on that grid. Multiplying a nonzero horizontal sum by a
vertical basis value gives magnitude above `2^-58`; its rounded products and
their partial sums lie on a grid no finer than `2^-110`. Division by four and
then a positive UInt16 leaves nonzero results far inside the normal binary64
range. Direct terms have coarser bounds. All products and additions are separate;
no fused multiply-add instruction is used.

## Rounding decision

The implementation uses the deliberately larger exact bound `E = 2^-30`.
For the absolute separated quotient, subtract its integer floor and measure
distance from the fractional half boundary. Floor subtraction is exact:
subtracting zero is exact, and otherwise the operands satisfy Sterbenz's ratio
condition. The remaining distance subtraction has absolute error at most u.
The margin between E and the quotient-difference bound exceeds u.

A measured distance strictly greater than E proves the original quotient cannot
cross a half boundary. The original integer and sign are therefore retained.
Equality or a smaller distance is uncertain and triggers the original direct
evaluation for that coefficient. Sign changes near zero still round to integer
zero. Results have magnitude below 2049, within Int32. E is a certificate margin,
not a tolerance that permits a different output value.

## Resources and controls

The path charges the original 4096 work units before computation or destination
writes. Cancellation and failed whole-charge admission retain the same boundary.
Unsupported hardware declines before work, and the caller performs its original
forward transform and quantization. The original coefficient array remains
allocated; 64 horizontal doubles use 512 stack bytes inside the encoder's existing
8192-byte fixed workspace admission. There is no new heap buffer, lookup or cache.
Rows and vertical frequencies are interleaved independently to hide addition
latency. Each sum retains its x or y order. The same scratch holds coefficients
only after every horizontal row has been consumed.

Original constant blocks with values 2 and 3 expose uncertified half-boundary
changes at divisors 32 and 16. Controls compare all 64 quantized coefficients
with the original direct evaluation at every generated divisor and mixed lanes,
including both signs, unaligned inputs, sentinels and unchanged source/quantizers.
Parent work, tight atomic admission and cancellable tokens are checked as well.

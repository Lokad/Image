# JPEG inverse rounding

JPEG inverse blocks target the pinned FFmpeg n8.1 normal x64 arithmetic.
The transform definition is [T.81 A.3.3](https://www.itu.int/rec/T-REC-T.81/en).
`JpegInverseTransform` independently generates its cosine matrix; no upstream
implementation or lookup table is included.

The generated Q14 matrix represents the unit rotation one step below unity.
Dequantized AC coefficients narrow to signed16 bits. DC saturates after the1024
level shift. DC-only rows use the reference word shift; other rows accumulate
modulo32 bits, round at11 fractional bits and saturate to signed words. The column
bias is added in word precision before multiplication; final sums shift20 bits
and clip to bytes. These boundaries matter at ties and large coefficients.
Managed normal and scalar execution implement the same pinned normal policy.
A wide-coefficient arithmetic control deliberately distinguishes native C and
normal AVX results; native C agreement is claimed only where observed.

The existing31x23 baseline444 control has exact independent quantizers and all36
visible coefficient blocks, then exact2,139 decoded Y/Cb/Cr samples. Eighteen
original numeric spectra cover DC ties/clipping, signed impulses, mixed spectra
and wide quantizers. The public decoder checks cooperative cancellation before mutation. Temporary scratch is256
stack bytes; the256-byte static kernel payload is minor fixed storage. No content
cache or per-block owned heap buffer is introduced.

Full-resolution color conversion separately derives BT.601 coefficients from
rational Kr=299/1000 and Kb=114/1000, preserving Q16/range/Q13 rounding. Complete
RGB/RGBA bytes, destination padding and opt-in orientation are checked through
public APIs. The evidence ledger records the exact tested scope and remaining
other decoder confirmation work. Quarter-width color uses dimension-aware Q16
positions, Q14/Q12 weights and Q7/Q9 chroma, with final Q13 color rounding. The
recorded original controls include near-zero leading-tap reduction. CMYK/YCCK RGB/grayscale use
separate fixed-point black adjustment and shared integer color conversion.
Their named constant, varying, saturated, AC and progressive controls have exact
native sample gates. Canonical CMYK is an explicitly requested ink representation. Broader decoder equality and current
CPU/allocation parity remain open; the previous floating inverse certificate and
its alternate implementations are removed.

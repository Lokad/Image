# Exact VP8 texture projection

The existing 513×259 RGBA8 control retains frozen Q75/method4/native settings
and its complete 98,590-byte packet, including exact source alpha.

| Quiet one-CPU run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| short | 179,411.94 | 53,429.27 | 3.358/3.688 | 24,080,952 |
| confirm | 180,626.55 | 53,726.61 | 3.362/3.382 | 24,080,952 |

Independent confirmation uses 8.6% less elapsed time than the
[preceding 197,551.76-µs screen](WARM-VP8L-PAIRED-POPULATION.md), with unchanged
allocations. Both versioned idle guards pass. Complete operations include source
normalization, alpha, encoding, fresh owned output and release. CPU parity remains
open; the comparison covers this fixed control/settings and the stated prior screen.

Four-point integer butterflies replace the general sequency Hadamard projection
used for candidate texture energy. Frequency order, weights and magnitudes remain
exact, with no scaling/rounding change. Source-energy reuse is separate and deferred.
The same 64-byte scratch, borrowed inputs, validation, work/admission/cancellation
and output lifetimes remain. No cache, new mode or upstream code/data is introduced.

Both platforms pass 5,064 checks each, including 947 live comparisons, 11 scalar
texture checks and 29 scalar reference-packet/public-wrapper checks. 102,656 original
mathematical cases retain 1,642,496 exact spectral magnitudes and frozen weighted
energies. The existing 561 native macroblock/prediction pairs and five fragments,
compact strides, invalid layouts and input ownership remain checked. The new warm
allocation check is zero. The same actual private 0.2.0 package and all three usage
examples pass on both platforms. Immutable timed/native/confirmation workers are reused.

[Evidence](warm-vp8-texture-butterfly-evidence.json) records samples, producer
identities, qualification and precise unchanged-path reuse. JPEG, VP8L/lossless
alpha, PNG, decoders, editing and text remain unchanged. JPEG is the larger current
encoder gap; its named governed RGBA16 ownership comparison remains outstanding.

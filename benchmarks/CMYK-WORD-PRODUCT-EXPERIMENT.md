# CMYK word-product experiment

**Rejected; no production change.** Shared unsigned-word products preserved exact
pixels but did not establish a consistent useful complete-operation CPU gain.
Allocations were unchanged and the implementation added specializations.

| Complete decode comparison | Original → candidate µs | Interpretation |
| --- | ---: | --- |
| Retained FFmpeg contexts | 434.53 → 382.14 | 12.06% filtered raw / 11.09% normalized gain; both confidence views separate |
| Fresh contexts | 399.32 → 438.60 | 9.84% filtered raw adversity; both raw views separate; normalized intervals overlap |
| Controlled immutable-DLL pairs | 405.01 → 395.65 | Ratio 0.977; log-derived interval 0.948–1.007 includes one |

The new first contemporary original-build fresh-context measurement exposed the
contradiction; the candidate was not rerun. One controlled public Decode bridge
retained all 12 alternating pairs of 128 operations with exact complete output,
source preservation, checksums and independent owners. Its inconclusive result
does not resolve the standalone disagreement. Neither selected stage costs nor
the pair prove native parity. Adverse and all-actual evidence remain available.

The prototype shared black-lane preparation across AVX512BW/AVX2 word products,
preserving the exact Q16 floor including white=254, scalar tails and EOI/Adobe
semantics. No new heap/admitted/retained storage was introduced. A two-second
profile located 110 in-place-color samples out of 1,993; these are sampled code
locations, not phase timings or removable percentages. Instrumented time is unused.

Windows/Linux full/live, scalar/affected-ISA and actual-package checks had passed
the prototype before final review found the performance contradiction. Those
checks prove candidate correctness, not performance acceptance. The exact accepted
DLL/package are now restored. Their existing 6,600 checks per OS and package
consumer/all three examples remain valid without repetition. Only embedded tests
were rebuilt to keep stronger both-inversion/vector-tail assertions; all four
CMYK tests pass in normal/scalar/AVX512-off/AVX2-off modes on each OS.

[Current accepted costs](CMYK-CURRENT-OWNED-CONFIDENCE.md) are separate from this
rejected implementation. No permanent algorithm-family restriction follows.
The next lead is inverse-transform workspace preparation, requiring attribution
before a prototype. Compression packets, pixels, cancellation, admission and
ownership stay unchanged; publication is postponed.

[Decision evidence](cmyk-word-product-experiment-evidence.json) retains all outcomes,
qualification and byte-exact restoration. Supplementary raw `cmyk-word-product-decision.tar.xz`,
SHA256 `f80eb561e098d68146c42934c4aca534e14134ad789501dd9cfe5d7bc36fea9d`, is mirrored/member-verified. Its dependency
`cmyk-shared-word-products-qualified.tar.xz`, SHA256
`c8722166dbfa01eb5091958927e8153ddbfd9063f0555f1f9bb01a724fce1b3a`,
preserves the prototype, original profiles/captures and package qualification.

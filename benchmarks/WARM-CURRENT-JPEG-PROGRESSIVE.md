# Current progressive JPEG confirmations

The three named gray/420/422 progressive JPEG cases independently pass the 2x
warm single-CPU target on the qualified production DLL. No optimization is
needed; stop these pairs. Existing newer ordinary/live-token progressive 444
proofs are reused. Full required warm coverage remains open.

| Existing case | Input bytes | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op | Actual / selected |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| JPEG progressive gray | 75,112 | 3,343.59 | 1,709.58 | 1.956 / 1.960 | 1,232,501 | 12 / 11 |
| JPEG progressive 4:2:0 | 52,364 | 3,187.59 | 1,952.50 | 1.633 / 1.650 | 1,637,521 | 12 / 11 |
| JPEG progressive 4:2:2 | 71,251 | 3,913.19 | 2,244.21 | 1.744 / 1.752 | 1,996,661 | 12 / 11 |

The older gray/420 confirmations precede the masked-refinement change `b7743b5`.
The 422 proof includes it, but `3f3bb7f` later adds a predicate at the executed
normal `RefineMaskedBits` entry point. Its inner algorithm remains selected;
the changed entry means older whole-operation timing is uncertain. A narrow
current measurement resolves this without assuming a regression or retuning.

Each operation decodes the same resident original 513x259 progressive packet
to independently owned RGBA8. Managed public Decode, admission, every scan/
entropy/refinement/transform, complete materialization and disposal are timed.
FFmpeg's software MJPEG decoder includes per-operation state reset, required
ceil-plane chroma expansion/crop, full-range conversion and output acquisition/
release. Grayscale includes owned GRAY8-to-RGBA expansion. Native contexts and
packet/frame wrappers are warm; every call decodes/converts the complete image.
Padded output rows are exposed without an extra packing copy. No output cache.
Startup, I/O, IPC, generation, hashing and validation are excluded.

Original packet and per-decoder pixel hashes remain exact. The existing normal/
scalar CLI, RGB maximum 4 / RMSE 1.5, exact opaque alpha, source preservation and
simultaneously held independent outputs pass before/after timing. Prepared
progressive packets, coefficient/quantization identities and producer provenance
are reused without regeneration, quality search or another original raster.

Every passing three-actual screen precedes an independent twelve-actual repeat
on the same immutable production, benchmark and native worker. Every actual is
retained. Conservative uppers include selected/all-actual uncertainty against
the faster native bracket's lower confidence bound. Both versioned quiet guards
pass every scored run. Sequential Release uses CPU0, one codec/conversion thread,
normal SIMD, tiering disabled and workstation nonconcurrent GC. Managed
allocations include harness costs; native allocation totals are unmeasured.

Production, benchmark and native rebuild counts are zero. Only cached selection
drivers change; every cached child including WarmDry has verified assemblies.
Production/package inputs remain unchanged from the existing
[qualification](WARM-JPEG-AC-PLANNING.md): 3,483 Release tests per OS including live
FFmpeg comparisons and the same actual package's consumer/all three examples
on Windows/Linux. No profile, full-suite/package repeat or whole-library method
audit is introduced. The [numeric evidence](warm-current-jpeg-progressive-evidence.json)
retains all samples, quiet guards, allocations, exact identities and qualification
reuse. Each result certifies its fixed input/public boundary; the finite
remaining checklist continues with named PNG current-path uncertainty.

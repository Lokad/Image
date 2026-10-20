# Buffered baseline JPEG end-of-block decoding

Completing short EOB codes inside the existing buffered reader saves
**9.9%** on the current sparse-2D CMYK path and **12.2%** on
one-AC CMYK. It avoids repeated prefix probes while preserving every original
pixel, work charge and minimum reader position. No new storage or owner.

The 21 affected existing baseline decode rows are reconciled below. Inputs,
settings, exact managed frame hashes, approved native quality bounds and complete
ownership boundaries match their prior contracts. This establishes current-path
evidence; earlier admission scopes are not treated as measured speedups.

| Existing complete operation | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| Sparse2D | 642.99 | 440.11 | 1.461 / 1.519 | 1,085,689 | Gap (short) |
| OneAc | 547.30 | 389.60 | 1.405 / 1.469 | 1,085,688 | Gap (short) |
| Adobe | 469.48 | 377.61 | 1.243 / 1.306 | 1,085,694 | Gap (short) |
| General | 3281.94 | 3111.92 | 1.055 / 1.105 | 1,085,546 | Gap (short) |
| Decode410 | 591.35 | 477.47 | 1.238 / 1.282 | 720,844 | Gap (short) |
| Decode411 | 588.74 | 456.30 | 1.290 / 1.347 | 751,303 | Gap (short) |
| Ycck420 | 786.35 | 708.51 | 1.110 / 1.178 | 1,470,280 | Gap (short) |
| Ycck420CallerRgb | 865.17 | 604.01 | 1.432 / 1.476 | 1,337,511 | Gap (short) |
| GrayResident | 927.74 | 681.93 | 1.360 / 1.455 | 672,210 | Gap (short) |
| GraySamples | 817.85 | 646.73 | 1.265 / 1.308 | 273,436 | Gap (short) |
| Rgb420Samples | 1075.13 | 1160.67 | 0.926 / 0.931 | 624,524 | Confirmed |
| NativeDecode420 | 1159.27 | 1008.12 | 1.150 / 1.246 | 757,369 | Gap (short) |
| NativeDecode420Restart | 1321.56 | 1013.22 | 1.304 / 1.462 | 761,744 | Gap (short) |
| NativeDecode422 | 1469.44 | 1348.59 | 1.090 / 1.194 | 829,172 | Gap (short) |
| Decode8 | 2391.79 | 1899.20 | 1.259 / 1.304 | 948,060 | Gap (short) |
| NativeDecode8 | 2222.98 | 1720.03 | 1.292 / 1.361 | 966,341 | Gap (short) |
| DirectRgb444 | 2093.21 | 1451.23 | 1.442 / 1.524 | 966,341 | Gap (short) |
| DirectRgb422 | 1401.73 | 1094.82 | 1.280 / 1.405 | 829,172 | Gap (short) |
| DirectRgb420 | 1020.52 | 743.18 | 1.373 / 1.414 | 757,378 | Gap (short) |
| CmykCaller | 467.59 | 330.92 | 1.413 / 1.474 | 1,635,495 | Gap (short) |
| StreamDecode420Restart | 1329.08 | 996.80 | 1.333 / 1.411 | 761,840 | Gap (short) |

RGB420 caller samples retain an independent confirmation with 12 actual iterations at
**0.926x/upper0.931**. All 12 actuals and
11 selected results are retained; the upper is the larger selected/all-actual
bound. Stop this passing case. The other 20 rows remain gaps, with no long repeat
above parity. Native allocation is unmeasured.

Each short retains 3 actuals, native pre/post brackets and both official idle-v2
guards. Three completed shorts failed their post-run I/O guards; their numbers
are excluded. One fresh-prefix retry per rejected case passed. Control-file I/O
was flushed outside measured operations before fresh guards; the versioned guard
and thresholds are unchanged. No new raster, settings grid, worker/harness build
or profile. Both prepared 3/12-iteration selectors are reused.

An EOB admits only its exact code bits, with no amplitude or coefficient write.
Insufficient work preserves the original per-bit failure/partial charges.
Unused whole prefetch bytes are returned on every exit. Zero runs, invalid/long
codes, stuffing, short tails and the separate live-token reader keep their paths.
Focused controls compare every short-code length/cache phase, tight budgets,
truncation/future markers, subsequent bits/restarts and guarded storage. General
AC uses its exact pinned frame hash; its simplified analytical flag is inapplicable.

Qualification: **4,469 Release tests per OS** (1,651 internal, 1,886 public, 932 live
FFmpeg), 22 focused EOB/work/sparse and 30 public CMYK/YCCK controls normally/scalar
per OS. The same actual private 0.2.0 package passes both consumers and all three
examples, including one-AC/two-AC pixels and independent charged survivors.
`test.ps1 -NoBuild` runs every suite against the verified prepared assemblies;
only the differential test project needed building after freeze, with project
references disabled. Production and benchmark code were not rebuilt. Twelve
text comparisons and four Lanczos outputs/timings reuse unchanged paths.

Only these 21 existing rows refresh. Progressive/live-token decoders, metadata,
encoders, other codecs and editing/text retain their identified evidence. The
finite checklist still has 254 cases: 237 numeric rows and 17 unmeasured cases.
Its 206 historical codec/edit rows await current-admission performance proof.
The normal-operation rows do not qualify performance with a supplied shared root.
Full warm parity and release readiness remain open; published 0.1.0 is immutable.

[Evidence](warm-jpeg-buffered-eob-evidence.json) retains statistics, allocations,
all gate/producer identities and source/package hashes. Mirrored raw archives:
`c12-qualified-windows.tar.gz` (`4c5df12b4574688a6131dcc55f25e7667847ae7fe2f48f1dce90d30281bd3ea0`),
`c12-qualified-linux.tar.gz` (`957e860d7046841fbee5a186639ddb4b17a797407de8609bd8da9348781a7baf`),
`c12-stream-boundary-linux.tar.gz` (`5dd363339d71805ecb2985d5531b7b76f5addd878e9d70078b9a276bc343d00e`).
The stream case was already required but omitted from the initial affected list;
it reused the qualified build and package evidence. Unchanged runtime dependencies,
reference fixtures and selectors retain their original qualified archives.

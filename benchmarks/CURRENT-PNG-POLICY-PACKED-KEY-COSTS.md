# Current PNG policy and packed-key costs

Eight existing controls resolve current costs without production changes or a
historical speedup claim. Confirmed passing paths stop.

| Complete 513×259 decode | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Gray2, key0 | 62.741 | 0.126 / 0.127 | 664 | Pass; stop |
| Gray1, key1 | 58.478 | 0.085 / 0.086 | 536 | Pass; stop |
| RGBA16, tight | 2,870.128 | 0.991 / 1.002 | 70,272 | Confidence open |
| RGBA16, cancelable | 2,820.060 | 0.971 / 0.974 | 8,568 | Pass; stop |
| RGBA16, tight + cancelable | 2,944.704 | 1.016 / 1.022 | 70,272 | Gap |
| RGBA8, tight | 1,495.320 | 1.071 / 1.076 | 68,224 | Gap |
| RGBA8, tight + cancelable | 1,934.514 | 1.387 / 1.395 | 68,224 | Gap |
| RGBA8, cancelable | 1,587.248 | 1.140 / 1.143 | 4,472 | Gap |

Each control retains all 12 actuals, native brackets, filtered/unfiltered
confidence envelopes and idle-v2 guards. Original packets/settings, exact RGBA,
alpha/hidden RGB, source ownership and independent outputs pass. Original tight
working-byte limits, one-byte-below rejection and cancelability stay unchanged.
Both subjects include full decoding, normalization, ownership and disposal;
native allocations are unmeasured. The approved native gray1/key1 correction
remains timed; native has no matching managed admission/cancellation policy.

Completed runs stopped at postchecks for descriptor wording. Gray2 adds only
“Hidden RGB retained”; original specialized policy descriptions were restored.
All other gates, including fixed hashes and policy values, were rechecked.
Completed timings were recovered without reruns; failures remain in raw evidence.

Zero builds or repeated qualification: reuse the immutable PGT1 core, generic
selector, native worker, 5,129 checks/OS and same actual package/three examples.
Only five existing PNG stream/composition costs remain unrefreshed; the full
268-path objective stays active.

[Evidence](current-png-policy-packed-key-costs-evidence.json). Mirrored and verified
archive current-png-policy-packed-key-costs.tar.xz, 91 members,
SHA256 1187e409f23e1db6aa39348489dc6a439238f66b09fdaa4f8a9bb392267f4a31; qualified-build dependency pinned by hash.

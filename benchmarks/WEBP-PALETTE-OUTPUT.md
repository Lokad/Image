# Output-owned VP8L palette expansion

[Current live-token VP8L decoding](WEBP-LOSSLESS-LIVE-CURRENT-CONFIDENCE.md)
supersedes that cost and resolves its uncertainty into a gap. The other source-valid
costs below retain their current owners; this report preserves the original round.

WPP695 removes a full-frame ARGB intermediate and its subsequent copy when the
last inverse transform is a palette. On little-endian RGBA decoding, establish the
returned image owner before allocation, convert the retired palette's at-most256
entries once, and expand directly into that output. The packed source and output
remain simultaneously charged until expansion finishes. Other transform orders,
green/ALPH extraction, validation and big-endian fallback retain their original
arithmetic. No new pool, cache, session, table or production dependency.

Independent contemporary confirmation on the existing 513×259 palette16 control
saves **168.22 µs /26.72% raw /26.84% native-normalized**,629.53→461.31 µs.
Filtered/all-actual raw and conservative normalized intervals separate. Total
managed allocation falls **820,190→288,462 B/op**. The earlier paired screen is
retained separately; its larger raw point saving is not the accepted estimate.

| Complete operation / FFmpeg reference | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Palette16 RGBA8 decode | 461.31 | 590.92 | .781 / .789 | 288,462 | Pass |
| Literal/run lossless decode | 1,525.64 | 2,154.27 | .708 / .716 | 10,840 | Pass |
| Resident lossless decode | 3,085.86 | 3,021.48 | 1.021 / 1.035 | 152,728 | Uncertain |
| Live-token lossless decode | 3,155.38 | 3,008.99 | 1.049 / 1.088 | 152,728 | Uncertain |
| Fresh seekable lossless Stream decode | 3,290.70 | 3,268.51 | 1.007 / 1.021 | 152,824 | Uncertain |
| Lossless coding validation | 2,265.37 | 3,067.73 | .738 / .750 | 297,070 | Pass |
| Resident VP8 + alpha decode | 5,980.77 | 5,971.26 | 1.002 / 1.014 | 238,864 | Uncertain |
| Fresh seekable VP8 + alpha Stream decode | 5,868.76 | 5,999.20 | .978 / .995 | 238,960 | Pass |
| VP8 + alpha coding validation | 4,418.36 | 5,461.09 | .809 / .822 | 14,064 | Pass |

Each row retains twelve actuals, native brackets and idle-v2 guards on the quiet
VM, sequential Release/one CPU. Complete parsing, preparation, required storage,
computation, independent output and disposal remain timed. Native retains its
declared contexts; managed live cancellation has no matching native token. All
frozen packets, pixel/descriptor hashes, alpha, hidden RGB and ownership gates
pass. No encoding input/settings, reference or quality gate changes. The palette
input remains11,810 bytes and the exact RGBA output531,468 bytes.

Nine callers of the shared method refresh, without claiming causal companion
gains, regressions or equivalence from historical clocks. Six lossless controls
are3pass/3uncertain; uncertainty does not establish an optimization need. Three
opaque VP8 paths never enter this method and reuse [quantizer evidence](VP8-FRAME-QUANTIZER-PREPARATION.md).
Stop passing controls. [Final publication](VP8L-FINAL-PUBLICATION.md) retains the
preceding gain/adverse history; its palette regression is superseded here.

Total GC differs from admission and retention. The benchmark's returned result
retains531,468 bytes until disposal; retained scratch is zero. An original513×67
palette16 control succeeds with239,128 bytes of shared capacity, below two full
frames(274,968 bytes), including packed-source/result overlap. This is a tested
upper bound, not a measured highwater for the benchmark. Its result retains137,484
bytes. Existing pools and governed bypass remain. Physical/native peaks and the
benchmark's new admission highwater are unmeasured.

Eight new tests cover packing thresholds, exact samples, independent survivors,
aggregate terminal refusal/recovery, cancellation at output admission and actual
async lifetimes. Existing authored cases retain missing entries, transform order,
orientation and ALPH coverage. Windows/Linux each pass **6,058 full/live checks**,
469internal/239public per normal/scalar mode, and the SAME actual Release package
with palette consumers and all three examples. Unchanged SIMD evidence is reused;
one immutable main/embedded candidate per OS, no harness/native/selector rebuild.

[Evidence](webp-palette-output-evidence.json) pins sources, complete costs and
qualification. Mirrored/member-verified raw `webp-palette-output-qualified.tar.xz`,
SHA256 `9827d07098a84952fad67d9a2a4f5a3471036bbc0b437708714cc9aa9ab5cf9c`,
6,384,156 B /187 members, retains the original/candidate modules, scripts, samples
and actual package. The existing palette control is now displayed:127summary
rows/three tables,283required paths, no new timing workload. Publication postponed.

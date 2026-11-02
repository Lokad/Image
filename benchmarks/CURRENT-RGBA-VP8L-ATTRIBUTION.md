# Current RGBA8 lossless WebP cost

One independent twelve-actual confirmation refreshes the existing 513×259 RGBA8
control on the current qualified encoder. No production, harness, selector or
native build; no new input, settings or workload. Both quiet guards pass.

| Complete warm encode | Lokad µs | libwebp µs | Ratio / upper | Allocated B/op | Complete bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| Original RGBA8, lossless | 137,365.44 | 122,207.95 | 1.124 / 1.139 | 26,131,848 | 183,812 / 183,812 |

The reference is explicitly labeled libwebp exact=1, FFmpeg's encoder backend,
method4/Q75. Complete packets, RGBA pixels, alpha and hidden RGB are exact.
The managed operation includes public EncodeWebp, admission, fresh owned stream
growth and disposal. Native normalization/repacking, fresh pixel import, encoding
and independent owned packet acquisition/release are timed; native wrappers and
BGRA scratch are prepared. Every pixel is reprocessed. Startup, I/O, IPC, checks
and fixture preparation are excluded. One CPU; retained native preparation means
this is not an equivalent fresh one-shot context comparison.

This replaces historical 158,892.27 µs / 29,696,928 B/op. It reconciles current
cost, without attributing a causal speedup between different historical producers
and clocks. The CPU gap remains confirmed. Allocation is total B/op, not measured
peak live capacity; encoder scratch is not retained between calls. Sibling
RGBA16, opaque and asynchronous costs remain historical.

One one-second profile yields 783 workload samples. Paired histogram population
costs receive 359 exclusive samples (45.8%), color histogram construction 67
(8.6%). These are attribution, not method timings or guaranteed savings.
Instrumented timing is discarded. Exact packets and both quiet guards pass.
The [run-boundary follow-up](VP8L-RUN-BOUNDARIES-INCONCLUSIVE.md) had an
unpromising screen and was restored; current costs remain unchanged. Further
work needs a different bank-scan or preparation design. The earlier
[window experiment](VP8L-WINDOW-POPULATIONS-DEFERRED.md) was inconclusive;
its implementation and sparse/pair-cache diagnostics are evidence to consider,
not gains to replay or permanent bans on a materially different design.

Unchanged production reuses 5,418 full/live checks per OS and the same actual
0.2.1 Release-package consumer and three examples on Windows/Linux. No package
regeneration or broad qualification is needed for this evidence-only round.
[Evidence](current-rgba-vp8l-attribution-evidence.json) pins all actuals, brackets,
identities and gates. Raw current-rgba-vp8l-attribution.tar.xz: 6,284,596 B /
344 members, SHA256
0c9790d32fcacc6dc91770b2dc1ad222fe4ade87208d5c709a701efb1b6d4607,
mirrored/member-verified. Immutable runtime and indexed production source are
included; broader qualification depends on the retained compact-Deflate archive.

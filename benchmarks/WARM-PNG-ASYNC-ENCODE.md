# Asynchronous nonseekable PNG encoding

Updated 2026-10-20. The current qualified producer **independently passes warm
single-CPU parity** for this named complete operation. Production is unchanged.

| Complete EncodePngAsync | Lokad µs | FFmpeg µs | Ratio / conservative upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Short | 5,411.85 | 17,262.45 | 0.314 / 0.354 | 658,560 |
| Confirm | 5,397.36 | 17,268.11 | 0.313 / 0.314 | 658,560 |

The conservative upper retains the larger selected/all-actual bound. The short
retains all3 actuals; confirmation retains all12, even though BenchmarkDotNet
selects11. Both jobs retain native pre/post brackets and both idle-v2 guards.
One immutable producer/harness/native build is reused throughout this pair.

Both sides encode the same original513×259 borrowed RGBA8 into fresh independently
owned nonseekable caller output. Managed EncodePngAsync includes admitted segmented
staging, actual WriteAsync copies into growing caller storage, awaited completion,
staging release and caller-output disposal. Native FFmpeg includes full PNG encode
into owned packet staging, fresh AVIO/scratch, callback copies/output growth,
internal flush to finish writes, staging/AVIO release and caller-output disposal.
The output survives staging release; no final snapshot copy or deferred completion
is excluded from timing. Callbacks complete immediately on both sides; no forced
scheduling, real I/O, seek/length/output view or cached encoded content. Resident
source/context/wrappers and released private storage may be reused. Native
allocations are unmeasured. Already-RGBA8 input needs no normalization on either side.

The unchanged level6/filterNone profile produces **218,775/210,973B** total and
**218,682/210,283B** IDAT (Lokad/FFmpeg). The original5% IDAT gate passes; source
pixels, alpha and hidden RGB are exact. Managed output is byte-identical to the
existing frozen packet. Normal/scalar native packets are byte-identical, with
exact managed and normal/scalar CLI decodes. No image/settings/depth grid.

Outside timing, controls demonstrate WriteAsync-only delivery and caller left
open, independently retained outputs after peer/source/staging disposal, source
preservation, output/memory/work refusal, pre-cancellation, cancellation after
the first delivery write, second-write sink failure and successful recovery.
No caller FlushAsync is required: native internal AVIO flush completes its own
buffered writes. Existing deferred-await/cooperative-memory qualification is
reused because production is unchanged.

One tooling build/OS, one native build and no producer/selector build. Typed short
and confirmation selectors are reused from their identified source archives.
Earlier prepared runtimes remain intact. Reuse [current qualification](WARM-LANCZOS3-COEFFICIENT-PERMUTATIONS.md):
4,448 tests/OS, affected hardware-off controls, the same actual private package and
all three examples on Windows/Linux. New tooling builds pass on both OS; Linux
provides the complete boundary controls and performance evidence. No repeated
full/package qualification for unchanged production.

The finite scope becomes **254 =234 numeric+20 unmeasured**:71 independent passes,
162 mean gaps,one confidence-open. Historical rows retain their identified builds;
this one current-code passing case does not establish uniform parity or release
readiness. **Stop this case.** Next named boundary: async JPEG RGBA8 output, using
its existing matched-quality anchor and admission/staging contract.

[Evidence](warm-png-async-encode-evidence.json). Raw `h13-qualified-linux.tar.gz`:
`f36ba571d9d7206728f74c25a6589eeff99569a613494811f9480daf41ccd2e8`. Windows tooling `h13-tooling-windows.tar.gz`:
`b6f93dd85ea45c9f44a8bbf2c31465a33bfde18ec0785b54b8c29a6e33a14c57`. Producer
`c12fbeebea4edb6b4959837183a4b12379756be3139bd46b5e3c1798ff97671d`; Linux harness
`316ca924e81e020570204071705fe378f302932505a9c3db019fc7b012001a29`. Old diagnostic bytes were losslessly compressed
with round-trip hashes/modes, and duplicate host dependency copies pruned after
equality checks. Both checkouts remain below5,000,000,000B.

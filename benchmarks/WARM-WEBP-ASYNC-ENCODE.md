# Asynchronous nonseekable lossless WebP encoding

Updated 2026-10-20. The current producer **independently passes warm single-CPU
parity** for this named complete operation. Production is unchanged. The approved
reference is **libwebp exact=1 (FFmpeg encoder backend)**, preserving hidden RGB.

| Complete EncodeWebpAsync /RGBA8 | Lokad µs | Backend µs | Ratio / conservative upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Short | 11,061.90 | 121,158.36 | 0.091 / 0.101 | 2,494,170 |
| Confirm | 11,286.87 | 121,728.04 | 0.093 / 0.094 | 2,494,170 |

The original 513×259 RGBA8 raster and effort 4 profile are unchanged. Native uses
lossless=1/method=4/quality=75/exact=1/thread_level=0. Quality 75 controls compression effort,
not lossy quality. Managed/native packets remain byte-identical to the existing
exact pair: **533,100/183,812 bytes**. Both decode to every original RGBA sample,
including alpha and hidden RGB, using normal/scalar FFmpeg controls. Existing
unchanged public-decoder proofs are reused. This establishes complete-operation
speed for this pair; **compression efficiency parity is not established**.

Managed timing includes public EncodeWebpAsync, admission, the byte-at-a-time
VP8L writer into admitted segmented staging, actual WriteAsync copies/growth,
awaited delivery, staging release and independently owned caller-output disposal.
Native recomputes full-frame RGBA-to-BGRA repacking, fresh pixel import/allocation,
complete encoding into fresh WebPMemoryWriter staging, immediate bounded 64 KiB
append callbacks into fresh growing caller output, staging release and caller-
output disposal. Its backend needs no AVIO adapter or flush. Native structural
and delivery invariants are also checked inside each operation. Caller bytes
survive staging release. Both callbacks complete immediately; no forced scheduling,
real I/O, seek/length/output view, final snapshot copy or cached encoded content.
Resident source/context/wrappers/BGRA workspace and released storage may be reused.
Native allocations are unmeasured; managed figures include the complete caller sink.

Controls verify WriteAsync-only delivery, caller left open, source preservation,
independent simultaneous/surviving outputs after source/peer/staging disposal,
output/memory/work refusal, pre-cancellation, first-write cancellation, second-write
sink failure and successful recovery. Existing deferred-await and shared-budget
qualification remains valid for unchanged production.

The short retains 3 actuals and confirmation all 12 (11 selected),
with native before/after brackets and both idle-v2 guards. Upper is the larger
selected/all-actual confidence bound. The first confirmation preguard failed on
CPU0 I/O wait and launched no benchmark; its snapshot is retained. One fresh
attempt passed. The same immutable binaries serve both scored runs.

One tooling build/OS and one native compiler build; an initial CRLF shell launch
failed before compilation, then the script was corrected before execution.
Original short/confirmation selector bytes are reused from qualified archives.
No producer/selector rebuild or repeated full/package proof. Reuse
[current qualification](WARM-LANCZOS3-COEFFICIENT-PERMUTATIONS.md): 4,448 tests/OS,
affected hardware-off controls, the same actual private 0.2.0 package and all three
examples on both OS. New tooling compiles on both OS; Linux provides controls and
timing. Published 0.1.0 remains unchanged.

Finite scope: **254 =237 numeric+17 unmeasured**; 72 independent passes,164 mean
gaps, one confidence-open. Other 236 observations retain their identified builds;
226 historical codec/edit rows predate the cooperative-memory producer. Full
parity/release readiness remains open. **Stop this case.** Next: existing named
fixed-workspace header-inspection boundaries, without expanding the matrix.

[Evidence](warm-webp-async-encode-evidence.json). Raw `h16-qualified-linux.tar.gz`:
`00b2d57b817ac9509d4fee81d23153f869514e658d34c53986b91a7a79b24ca3`. Windows tooling `h16-tooling-windows.tar.gz`:
`5ae818233d87c25c68651bb69ab0abce5506f49d6a13e12575bf837150001ac7`. Production
`c12fbeebea4edb6b4959837183a4b12379756be3139bd46b5e3c1798ff97671d`; Linux harness
`7aac5b5dcf5b4465edffc347e3fd153c68b568db14bcd62357fd17369b27c8fc`. Superseded loose H12/H13/H14 runtimes were
pruned only after every owned file matched its retained raw archive; shared
current dependencies and all raw/source/release proofs remain. Old diagnostic
compression verifies round-trip bytes/hashes/modes. Both checkouts stay below 5 GB.

# Asynchronous nonseekable JPEG RGBA8 encoding

Updated 2026-10-20. The current qualified producer has a **1.034× gap**
for this already-named complete operation. Production is unchanged; no independent
parity confirmation is claimed and no long run above parity is added.

| Complete EncodeJpegAsync /513×259 RGBA8 | Lokad µs | FFmpeg µs | Ratio / conservative upper | Lokad allocated B/op | JPEG B Lokad / native |
| --- | ---: | ---: | ---: | ---: | ---: |
| Quality69 /nativeqscale5 /444 | 3,982.03 | 3,849.79 | 1.034 / 1.158 | 494,254 | 158,812 /133,041 |

Both sides time complete normalization/alpha flattening onto0x204060, color
conversion,444 sampling, transforms, adaptive/optimal Huffman planning, entropy,
fresh independently owned staging and caller output growth/copies/release.
Managed public EncodeJpegAsync additionally includes its stock-memory adaptive
admission probe with staging capacity reserved, bounded contiguous staging,
unused-tail clearing and awaited WriteAsync delivery. Its worst-case staging
bound is2,781,968B; actual owned capacity stays admitted through completion.
Native recomputes exact flattening and full-range BT601 conversion every operation,
then delivers the fresh owned packet through fresh nonseekable AVIO/callback output.
Prepared normalization is only an untimed check, never the timed encoder input.

Caller output survives staging release and is disposed by its owner inside each
operation. No final snapshot copy, deferred completion, seek/length/output view,
real I/O, forced scheduling or cached encoded content. Callbacks complete
immediately; native internal AVIO flush finishes its own buffered writes without
requiring a managed caller FlushAsync. Resident sources, native contexts/wrappers
and released private storage may be reused. Native allocations are unmeasured.

Both packets are byte-identical to the frozen anchor. Each encoder's decoded
pixels/alpha and source-error sample revalidate on the current producer/native
decoder. PSNR27.156779688/27.239219826dB gives **0.082440138dB** gap inside0.25;
numeric quality values are encoder-specific. Earlier normal/scalar native decoder
evidence is reused because packets and the checked FFmpeg executable are unchanged.
Setup/cleanup reuse those exact packet hashes instead of repeating quality work.
No quality sweep, new raster, shape or sampling grid; compression-efficiency
parity is not established.

Controls check actual WriteAsync-only delivery, caller left open, unchanged
borrowed pixels, independent simultaneous/surviving outputs after peer/source/
staging disposal, output/memory/work refusal, pre-cancellation, cancellation after
the first write, second-write sink failure and subsequent successful encoding.
The previously qualified output sink is reused unchanged. Existing cooperative-
memory/deferred-await qualification remains valid because production is unchanged.

One quiet three-actual short, three native samples before/after, both idle-v2
guards and the larger selected/all-actual confidence bound. One tooling build/OS,
one native build and no producer/selector build. Cached typed selectors and
unchanged runtime dependencies are reused; earlier prepared runtimes stay intact.
Reuse [current qualification](WARM-LANCZOS3-COEFFICIENT-PERMUTATIONS.md):4,448 tests/OS,
affected hardware-off checks, same actual private package and all three examples
on Windows/Linux. Tooling builds pass on both OS; Linux supplies controls/timing.
No repeated full/package qualification for unchanged production.

The finite scope becomes **254 =235 numeric+19 unmeasured**:71 independent passes,
163 mean gaps,one confidence-open. Other234 selected observations retain their
identified builds. Full parity/release qualification remains open. Next: the
separately named async direct caller-RGB420 output boundary, without an RGBA frame.

[Evidence](warm-jpeg-async-encode-evidence.json). Raw `h14-qualified-linux.tar.gz`:
`169c9cff749ba78299eb8cd8b214d6aaf906afb3e7944d7905b8161f2afd1df9`. Windows tooling `h14-tooling-windows.tar.gz`:
`7676347a0b9715b125e9a6ff36b96ee4580af481eec45cc2d66f3c29db55f599`. Producer
`c12fbeebea4edb6b4959837183a4b12379756be3139bd46b5e3c1798ff97671d`; Linux harness
`d5e9ee24e94b4f0dcd40766cc0f5b5174e34b11e164a89a4d5958717d0e20cd2`. Lossless old-diagnostic compression and
verified duplicate dependency pruning keep both checkouts below5,000,000,000B;
raw/source/release/current consumers and earlier prepared runtimes are retained.

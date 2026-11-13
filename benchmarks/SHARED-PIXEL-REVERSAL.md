# Shared pixel reversal

Byte half turn independently saves **4.64 µs / 36.18% raw**,
**27.22% reference-normalized**, against a contemporary
12.82 / FFmpeg 11.43 µs baseline. Both raw confidence views and
normalized confidence separate; the native clock moves -12.31%.

| Complete original 513×259 control | Lokad / FFmpeg µs | Upper / status | GC B/op |
| --- | ---: | ---: | ---: |
| Half turn RGBA8 | 8.18 / 10.02 | 0.838 / pass | 112 |
| Half turn RGBA16 | 19.17 / 22.02 | 0.879 / pass | 112 |
| Horizontal flip RGBA8 | 9.23 / 9.41 | 1.004 / uncertain | 112 |
| Horizontal flip RGBA16 | 21.68 / 26.96 | 0.812 / pass | 112 |

One shared uint/ulong kernel now serves horizontal row placement and flat half-turn
placement. Both existing horizontal bodies are copied verbatim, including the
16-pixel AVX512F byte path and Vector256/Vector128/scalar tails. Duplicate half-turn
kernels are removed: **37 fewer production lines**. No generic pixel policy,
extra pass, owner, buffer, field, cache, reservation or public API. Every channel bit,
hidden RGB and full word sample stays exact, with 1,024-pixel cancellation blocks.

The original short screen precedes independent primary confirmation. All 12
alternating 64-operation pairs and three native brackets remain, including adverse
samples. Complete public admission, independent output acquisition, materialization
and disposal stay timed. FFmpeg's public graph/source/frame wrappers remain prepared;
fresh independent pixel allocation, processing and release are timed. Exact native
normal/scalar pixels, source, owners and CPU0 idle-v2 gates pass.

Byte horizontal flip is now narrowly uncertain, upper 1.004. Its raw point slightly
improves and its normalized interval overlaps the old near-boundary pass; no
regression is demonstrated. Both word controls pass and stop. Historical/native
clock movement establishes no causal companion saving. No favorable repeat follows.
The unchanged orientation dispatcher remains 243 bytes with identical instructions,
arguments, call targets and constants after relocation normalization. Other 16 edit
controls, PNG compositions, codec, resize and text paths reuse valid source evidence.

Allocation stays **112 B/op**, with no added owned/admitted capacity or retained
scratch. Source/result each hold 531,468 / 1,062,936 B for byte/word pixels. The
two-slot / 4 MiB idle pool and caller-budget bypass remain. Consumer peaks for a
source and two results are 15,540 / 31,080 B. Physical/native/JIT peaks are unmeasured.

Windows/Linux each pass **6,605 full/live checks**: 3,243 internal / 2,414 public /
948 live. Focused 25 internal / 237 public checks pass per normal, scalar,
AVX512-off and AVX2-off mode on each OS. Three independent public placement cases
cover flat 15/16/17-pixel boundaries; existing chunk tails, metadata, admission,
cancellation and survivor coverage remain. The **same actual Release package**,
containing the timed Linux DLL, passes both OS consumers and all three examples.
Four new reversal consumers preserve hidden RGB, full words, cancellation after
admission, terminal aggregate refusal and independent survivors, including inverse
recovery. One core/embedded/public build per OS, no harness/native/fixture build,
one Release/--no-build pack and no production rebuild after timing.

Current image views: **175 pass / 3 gaps / 13 uncertain**, 191 comparisons; the 20
independent-result edits have 14 passes / three gaps / three uncertainties. All
289 material paths remain required. Scope stays narrow; publication is postponed.

[Evidence](shared-pixel-reversal-evidence.json) owns sources, identities, all four
costs, confidence, ownership and qualification. Raw `shared-pixel-reversal-qualified.tar.xz`:
3,352,192 B / 164 members, SHA256
`b6b7db044ea33967352c197e4c957967b361ee852505311fa0ef298e2643ddb6`, mirrored and member-verified. Dependencies retain the previous
qualified package, original fixtures, reference tools and source recipes.

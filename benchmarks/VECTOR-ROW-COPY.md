# Safe vector row copy

Complete RGBA8 vertical flip independently saves **1.39 µs / 14.12% raw**,
**7.25% reference-normalized**, against a contemporary 9.872 µs baseline.
Both raw confidence views and normalized confidence separate; the native clock
moves -7.40%. Current byte parity remains uncertain; word flip passes.

| Existing original 513×259 control | Lokad / FFmpeg µs | Upper / status | GC B/op |
| --- | ---: | ---: | ---: |
| Vertical flip RGBA8 | 8.478 / 8.390 | 1.045 / uncertain | 112 |
| Vertical flip RGBA16 | 17.719 / 18.051 | 0.996 / pass | 112 |
| Normal orientation RGBA16, current unchanged body | 17.296 / 16.642 | 1.066 / uncertain | 112 |
| Clone RGBA16, current unchanged body | 17.630 / 16.812 | 1.123 / uncertain | 112 |

The private independent-row primitive uses safe span `Vector512` copies with
four-vector groups, remaining-vector and byte tails. Small/unsupported cases keep
the actual BCL fallback. Row placement, 1,024-pixel cancellation blocks, admission,
metadata, exception cleanup and independent owners stay unchanged. Every channel
bit, hidden RGB and full word sample remains exact. No public API, cache or buffer.
This differs from the rejected setup-only [screen](VERTICAL-COPY-PREPARATION-SCREEN.md).

Complete operations include owned output acquisition, processing and disposal.
FFmpeg retains its graph/source/frame wrappers and may emit a negative-stride view;
fresh independent pixel allocation/copy/release remain timed. Both original native
normal/scalar output checks, source/owners and quiet CPU0 idle-v2 guards pass.
Twelve alternating 64-operation pairs and three native brackets remain, including
adverse samples. A short screen precedes independent confirmation. Word raw time
is +4.75% against history while native moves +2.09%; normalized intervals overlap.
No historical companion speedup or demonstrated regression is inferred.

The broader full-raster extension is **not retained**. Its confirmation gives
17.630 → 17.016 µs: raw/all-actual and normalized gains are inconclusive. The
filtered 2.14% gain and upper outlier remain; there is no favorable replay or
permanent family ban. Exact row-only source/core/embedded/public modules are
restored without rebuilding, and the temporary shared helper is removed.

The final row-only DLL's word-clone baseline is reused as current confidence,
superseding the old gap. Normal word orientation gets one current confirmation
with the existing driver's supported **256 matched operations per subject**, up
from 64 to reduce short-block uncertainty. Its boundary, inputs/reference/quality
and all12pairs remain; it also stays uncertain. Neither update is a production
copy speedup. Other18edit kernels and PNG compositions reuse valid source paths;
the shared orientation dispatcher remains243B with identical normalized code.

Allocation stays **112 B/op**; new owned/admitted capacity and operation buffers
are zero. Source and result each occupy531468/1062936B for byte/word frames.
The default two-slot/4MiB idle pool and caller-root bypass stay unchanged. New
95×37 packaged vertical cases admit42180/84360B for source plus two outputs,
including one/three remaining vectors and scalar tails. Allocation, live capacity
and idle storage remain distinct; physical/native/JIT stack peaks are unmeasured.

Windows/Linux each pass **6,620 full/live checks**:3248internal/2424public/948live.
Focused **25 internal / 200 public** pass normal, scalar, AVX512-off and AVX2-off
on each OS. Six new independent placement cases cover the vector threshold and
channel tails; live-token results match the same oracle. The **same actual Release
package**, containing the timed Linux DLL, passes both OS consumers/all3examples,
including new vertical cancellation, terminal aggregate refusal, inverse recovery,
remaining vectors and independent survivors. Encoders/decoder pixels stay correct.

One accepted core/embedded/public build perOS; one extra Linux core/embedded build
for the inconclusive extension, then byte-identical restoration. No accepted-artifact
rebuild after timing, harness/native/fixture build or second pack. Full qualification
follows performance reconciliation; unchanged codec/resize/text evidence is reused.

Current **191 image views:173 pass / 0 gaps / 18 uncertain**. The20independent-result
edits have14passes/six uncertainties. This does not establish parity: uncertainty,
in-place drawing/probe gaps and the full **289 material paths** remain required.
Publication is postponed; published packages remain immutable.

[Evidence](vector-row-copy-evidence.json) owns sources, identities, current costs,
deferred extension and qualification. Raw `vector-row-copy-qualified.tar.xz`: 3,529,060 B /
231 members, SHA256 `8e1ea39a078f534e788daded6450a1bc1da64728bd5a8b355e058f252bacd5bf`, mirrored and member-verified.
Dependencies retain earlier packages, fixtures, reference workers and recipes.

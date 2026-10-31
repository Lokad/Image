# Mixed bicubic geometry reuse

Complete tight RGBA16 mixed resizing independently improves **8.90% filtered /
8.28% all elapsed**, **8.62% native-normalized**, with separated raw/normalized
confidence: **1,221.634 → 1,112.921 µs**, **1.308× / upper1.369**. Warm allocation
is **1,060,367 → 1,060,371 B/op** (4 B measurement noise); owned capacity is unchanged.
The CPU gap remains open.

| Existing complete operation | Lokad µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: |
| TightMixed16 | 1,112.921 | 1.308 / 1.369 | 1,060,371 |
| TightMixed8 | 1,167.235 | 1.350 / 1.367 | 530,400 |
| Mixed8 | 1,158.746 | 1.327 / 1.341 | 530,400 |
| Mixed16 | 1,168.040 | 1.366 / 1.406 | 1,060,367 |
| TightVertical8 | 762.967 | 1.309 / 1.351 | 265,012 |
| TightVertical16 | 753.161 | 1.250 / 1.314 | 529,814 |

Mixed controls use the original 513×259 rasters to1027×129 (width up, height down),
with default or input+output+4 KiB allowance as named. TightVertical keeps width513
and reduces height to129. Full normalization, independent output and release are
included. The five companion costs have no paired causal gain claims.

The two mixed kernels used to prepare identical horizontal geometry for every
eight-row band. A shared helper now prepares exact coefficients/indices once in
an unpublished output suffix. A whole-band bound stops all plan reads before any
output overlaps it; original on-demand geometry fills the suffix. For these
controls the 86,268-byte plan reduces17 geometry passes to5(byte)/4(word).
There is no new allocation, owner, quota, precision or across-call cache. Existing
4 KiB admission covers3,992 fixed stack-array bytes. Insufficient suffix and
identity-width geometry retain on-demand calculation. Every pixel is overwritten
before publication; cancellation/failure disposes the private result.

Mixed pixels match exact rational and frozen frames; byte vertical pixels retain
the documented25 rational alpha halves. All frozen native RGB/alpha max1/RMSE≤.25,
zero-alpha RGB, source/output ownership, complete boundaries and quiet-v2 gates
pass. Reference is zimg, FFmpeg's scaler backend, with original halos/settings/
packing; native allocations are unmeasured. One candidate short and independent
twelve reuse the immediately preceding valid original twelve, with no additional
original score. Both raw views and normalized means clear the default5%gate;
native point drift is−0.30%. No variant or quality exception.

Both OSes pass **5,314 Release checks each** (2,164 internal, 2,202 public, 948 live),
plus31 internal/544 public focused checks normal/scalar and Linux AVX512-off.
Four new public checks cover complete prefix/tail pixels, independent owners,
shared refusal, cancellation cleanup and released-capacity reuse. One immutable
core/internal build perOS is reused; a test-only portable-peak correction requires
a second public assembly build, with no production rebuild. Zero harness/native/
selector builds, profiles or new inputs. The same actual Windows package/DLL
passes both OSes, governed/retained/async consumers and all three usage examples,
including both frozen mixed frames and aggregate admission/survivor lifetimes.

Development0.2.1 package 388,636 bytes, SHA256
f339e08d6f4541ee278349fe8f9dbfe65150b00d2a3c545716819f7801d34640; published releases stay immutable.
[Evidence](warm-mixed-bicubic-geometry-evidence.json) identifies all six changed
named paths and exact reuse of other kernels/codecs/text. Mirrored/member-verified
raw mixed-bicubic-geometry-qualified.tar.xz SHA256
4da6072b63e5264743ed6cc710724424049f664421daa2fb2c330727cc7d8a57 (6,612,076 bytes, 220 members) retains source,
binaries, actual package, tests, scripts, selectors, workers, fixtures, actuals
and guards; PIM1/TMC1archive dependencies remain. Close this primitive without
plan-size/band/width variants. All268paths stay required; full parity is unproven.

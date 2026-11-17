# Current separate-word copy confidence

The existing RGBA16 separate-buffer copy remains **uncertain**:
**3.777152 / FFmpeg 3.785687 µs**, conservative interval
**0.993513–1.001075**, **0 B/op**. No production change or speedup is claimed.
Keep the uncertainty open without another favorable timing retry.

One prespecified fourfold integration increases the previous 65,536 complete
operations to **262,144 per subject**, roughly one-second batches. The unchanged
driver retains twelve alternating adjacent pairs, twelve native samples before
and after, and the conservative paired/all-actual/fastest-native-bracket rule.
Only its operation-count guard and error text change. Every actual remains;
the previous [uncertainty](WORD-ALIAS-DISPATCH.md) is retained.

The frozen 513×259 RGBA16 source and separate borrowed canvas, source (127,65),
256×129 rectangle and destination (17,19) stay unchanged. Complete managed public
validation, clipping, cancellation and mutation remain timed. The original FFmpeg
n8.1 public `av_image_copy_plane` reference uses prevalidated arguments; that
boundary difference remains explicit. Repeated overwrites are idempotent, with
no reset/output allocation on either side. Exact full canvas, alpha, hidden RGB,
untouched pixels, source integrity and caller ownership pass. One CPU0 operation
runs at a time with optimized JIT, natural nonconcurrent GC and idle-v2 guards.

The first capture is **unqualified**: a shared worker replaced for the separate
clipped-copy control now times clipping. The post-capture identity assertion
detects the mismatch. Its samples/guards remain, excluded from current costs.
Before the eligible capture, an isolated tooling root restores the exact original
worker **f0bcf912…**, source **34517087…** and build identity from the retained
archive. Those identities are checked before and after measurement. No reference
rebuild, fixture change, correctness relaxation or canonical-tool replacement.
The isolated root and exact executable/source/patch recipes make this correction
reproducible. Future captures must pin the reference before timing.

The actual immutable package DLL **2f833ea5…**, package **4a2ae52d…** and compiled
original harness **2a4b04d9…** are reused without rebuilding. One small count-guard
tool build; zero production, embedded/public-test, existing harness/native/fixture/
package builds or broad qualification repeats. Reuse **6,790 full/live checks**,
the same actual package and all three examples each OS.

Allocated, operation-owned and retained workspace remain **zero**. Caller source
and canvas each hold 1,062,936 B outside timing. Physical/native/JIT peaks are
unmeasured. This resolves no other copy or transform uncertainty and makes no
claim of full 289-path parity.

[Evidence](separate-word-copy-current-confidence-evidence.json) pins producers,
all actuals, original boundaries and qualification reuse. Mirrored/member-verified
raw `separate-word-copy-current-confidence.tar.xz`: **6,453,512 B /85 members**,
SHA256 `dacc6957b358c8ade955ce8e470c57a4c560c143211ad9dfebd1876af85807e2`.
It retains both captures, guards, complete isolated runtime/reference and
`dwc1177-*` recipes. Publication remains postponed.

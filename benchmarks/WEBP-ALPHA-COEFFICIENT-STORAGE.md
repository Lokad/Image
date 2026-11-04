# ALPH-to-frame storage reuse

Completed alpha working storage now backs the later VP8 coefficient stream when
its physical capacity fits. Existing alpha/frame owners transfer the continuous
charge within the same operation. The coefficient prefix is cleared and exposed
at its logical length; the full physical tail stays charged. Selected ALPH bytes
and output partitions remain independently owned through actual async users.
No phase reordering, new owner, pool, session, cache or admission fallback.

Complete encoding allocates **449,224 B less for RGBA8 (5.9%)** and **448,816 B
less for RGBA16 (5.9%)**, against unchanged-path prior measurements at the same
four-operation batching. Avoided coefficient array payload is 448,800 B; measured
allocation includes array/owner/run overhead. Current costs are shown below.

Actual-DLL admission observations retain **3,656,309 B peak / zero retained
scratch** for alpha8, and 3,470,059 B for the unchanged lossless photograph.
The coefficient phase carries 82,668 B extra capacity because it borrows the larger
531,468 B array. Admission peak and total allocation are distinct; physical/native
peaks are unmeasured. Caller input/output and diagnostic storage are excluded.

| Existing complete 513×259 control | Managed µs | Reference µs | Ratio / upper | Allocated B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| VP8 + ALPH RGBA8 | 64,709.57 | 53,431.55 | 1.211 / 1.219 | 7,165,880 | 98,590 |
| VP8 + ALPH RGBA16 / normalization | 69,162.74 | 54,409.27 | 1.271 / 1.367 | 7,165,472 | 98,590 |
| VP8 opaque RGBA8 | 21,216.18 | 17,040.99 | 1.245 / 1.254 | 1,201,552 | 72,380 |
| VP8 opaque RGBA16 / normalization | 22,291.75 | 16,939.47 | 1.316 / 1.331 | 1,201,552 | 72,380 |
| VP8 photograph | 14,823.50 | 10,540.55 | 1.406 / 1.415 | 809,072 | 17,688 |
| VP8 text/diagram | 8,383.74 | 6,338.83 | 1.323 / 1.334 | 741,137 | 7,100 |
| VP8 diagram / effort2 | 3,668.86 | 2,782.13 | 1.319 / 1.330 | 741,375 | 7,330 |
| VP8 diagram / effort6 | 14,711.12 | 10,635.25 | 1.383 / 1.391 | 754,344 | 6,818 |

All eight existing VP8 costs receive twelve-actual confirmation on one immutable
producer. Alpha8 also has an independent three-actual screen. These are allocation
and current-cost results, without a CPU gain, equivalence or causal regression
claim. Word alpha's native brackets rise 7.25% within its run; its 65.51–72.82ms
managed interval overlaps the prior estimate. Its adverse point and wide upper
remain, without another unchanged sample. All actuals, including the filtered
distortion observation, are retained.

References keep the declared FFmpeg/libwebp complete-operation boundaries:
managed normalization, preparation, fresh independent output and disposal stay
timed; native wrappers/private scratch have their documented retained lifetimes,
with source processing, encoding and independent output ownership timed. Frozen
inputs/settings, complete packets, pixels/alpha, safety and ownership gates pass.
This does not establish one-shot context equivalence or new content/settings scope.

Both OSes pass **5,603 full/live checks** (2,368 internal, 2,287 public, 948 live),
plus 840 internal/357 public affected checks in normal/scalar modes. Sixteen new
cases cover dirty reuse, all efforts, physical fits/misses, logical bounds,
header retries, wrong-operation guards, terminal refusal/cancellation cleanup,
independent partitions/ALPH payloads and subsequent root reuse. The SAME actual
Release package/all three examples pass Windows/Linux, including exact sync/async
ALPH packets, early/late terminal refusal and independent output ownership.

One core and internal-test build per OS; public/harness/native/audit/selectors
reused, one package. An initial Windows core compile is corrected. Source-linked
checks preceded the separate core build: public baseline runs are explicitly
excluded, then affected public checks run on the actual candidate. A missing
receipt name and an already-parsed-output collector stop are corrected without
replaying any timing. Published packages are immutable; publication is postponed.

[Evidence](webp-alpha-coefficient-storage-evidence.json) pins producers, source,
all current costs, allocation/admission observations and package qualification.
Mirrored/member-verified raw `webp-alpha-coefficient-storage-qualified.tar.xz`: 6,383,452 B /
395 members, SHA256 `2099f230bee1b3e162eb4e735b3f01ba67d75fdd172391247ec8458b9f2d9192`.
Sources, immutable binaries/package, original/stage failures, tools and exact
receipts remain reproducible; baseline/reference dependencies are pinned.
[Earlier match-index reuse](WEBP-ALPHA-MATCH-INDEX-REUSE.md) retains the unchanged
VP8L costs and uncertain photograph follow-up. No lossless-photo resampling,
new performance grid or completed 282-path parity claim follows this stage.

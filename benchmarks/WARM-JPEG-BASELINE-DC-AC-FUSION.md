# Fuse baseline JPEG DC and AC traversal

The existing owned sparse-CMYK control improves **4.48% filtered / 4.57% all
elapsed** with exact pixels. Its confirmed **1.406x / upper 1.413** remains a gap.

| Complete owned RGBA8 decode, 513x259 | Lokad us | FFmpeg us | Ratio / upper | Managed B/op | Encoded input B |
| --- | ---: | ---: | ---: | ---: | ---: |
| Original sparse-CMYK, independent | 658.64 | 443.71 | 1.484 / 1.491 | 1,085,759 | 22,708 |
| Fused sparse-CMYK, independent | 629.11 | 447.48 | 1.406 / 1.413 | 1,085,758 | 22,708 |

One generic baseline traversal shares ordinary-word lookahead across the DC and
AC symbols. It validates DC category/amplitude/predictor before committing and
consumes only validated AC prefixes. Long codes, escaped bytes, tails, markers,
zero runs and syntax failures hand remaining work to the existing traversal at
the exact logical byte/bit position. No speculative coefficient replay. Live
cancelable tokens keep their actual traversal and cooperative checkpoints.

Only scalar locals are added. Existing admitted tables, coefficient scratch,
planes and retained output owners are reused. No additional table, buffer,
substantial workspace, heap/frame copy, cache, budget or owner. Terminal admission
refusal, cleanup and asynchronous lifetimes remain. Allocation changes by
-1 B/op on the primary; no allocation improvement is claimed.

Fresh three-actual screens improve 3.494% raw elapsed and 3.652% normalized point,
clearing the predeclared 2% stage-simplification gate. One candidate twelve reuses
the valid original twelve: 4.483% filtered, 4.570% all elapsed and 5.286% normalized
point gain. Raw 99.9% intervals and conservative normalized envelopes both show
separation. Frozen inputs/settings, independent/native/scalar pixels, source,
ownership and idle-v2 guards pass. No variant or successful timing repeat.

Six existing affected controls receive one independent current-cost confirmation
each. These are not paired causal gains. YCCK420, 4:1:0 and coding validation
still pass; stop them at this scope. All inputs/settings and complete output-
ownership boundaries are frozen.

| Complete existing operation, 513x259 | Lokad us | FFmpeg us | Ratio / upper | Managed B/op | Encoded input B |
| --- | ---: | ---: | ---: | ---: | ---: |
| Caller CMYK, DC-only | 348.53 | 331.27 | 1.052 / 1.058 | 1,085,961 | 10,895 |
| Owned baseline420 | 1,085.36 | 1,007.70 | 1.077 / 1.082 | 757,362 | 52,093 |
| Owned YCCK420 | 421.45 | 707.63 | 0.596 / 0.607 | 895,547 | 6,704 |
| Owned 4:1:1 | 527.41 | 450.16 | 1.172 / 1.192 | 751,295 | 3,904 |
| Owned 4:1:0 | 367.40 | 507.52 | 0.724 / 0.735 | 720,838 | 3,399 |
| Baseline420 coding validation | 589.35 | 673.56 | 0.875 / 0.880 | 9,984 | 65,163 |

Both OS pass **5,193 checks each** (2,097 internal, 2,148 public, 948 live), plus
343 internal/345 public focused checks normal/scalar. Five added cases verify
actual fast completion, pending bits, partial handoff, immutable misses,
truncation/failure state, guards/source and zero heap. The same actual Release
package, governed async consumer and three examples pass on both OSes.

One core build per OS is frozen throughout; zero harness/native/selector builds.
The read companion reuses the existing twelve-actual selector and refreshes only
its untimed proof with identical fixtures/settings. Ordinary420's completed
timing initially hit a postcheck expecting an optional field absent from its
established schema; retain the failure and recheck its original fields/boundary.
The timing was not repeated and no pixel/native gate was relaxed.

Other affected baseline decoding/validation boundaries need applicable current
costs before CPU-pass claims. Progressive decoding, encoders, metadata-only
inspection, other codecs and editing/text retain relevant unchanged-path evidence.
The motivating profile is historical after this entropy change. The full
**268-path <=1x** objective remains open; close this fusion primitive.

[Evidence](warm-jpeg-baseline-dc-ac-fusion-evidence.json),
[baseline staging](WARM-JPEG-DIRECT-BASELINE-PLANES.md).
Mirrored/member-verified jpeg-baseline-dc-ac-fusion-qualified.tar.xz, 253 members,
SHA256 0d3c0ef4843b59d7b64a2f0520be6e7d2820ea6ef7f2bd71a505c850dd2b434d; QTS1/JDS1/JBP1 dependencies are pinned.

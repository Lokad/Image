# Rejected VP8 group-emission screen

One existing 513×259 RGBA8 VP8 Q75/method4 complete-operation screen tests grouped
interval normalization on one CPU. The candidate retains the exact 98,590-byte
packet, source/settings, output ownership and both idle-v2 guards.

| Quiet short | Lokad µs | Reference µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Group emission | 92,559.50 | 53,880.69 | 1.718 / 1.899 | 23,358,400 |

Mean elapsed is only 0.3% lower than the selected independent run,
with unchanged allocation. This offers no useful complete-operation benefit;
there is no paired gain or parity claim. The prototype is removed. No independent
confirmation, full-suite/package repeat or representation/inlining sweep follows.

The [arithmetic proof/profile](WARM-CURRENT-VP8-EMITTER-PROFILE.md) and original
native symbol, carry, termination and complete-packet tests passed. Both platforms
pass 45 focused checks on the candidate and restored actual writer/frame code.
One retained Fact covers output limits at fragment/growth boundaries, structured
failure, capacity ownership, repeated disposal and successful later exact output;
it also passes scalar on both OS. It exercises the actual production writer.

Selected source and Windows/Linux binaries are restored byte-for-byte. The
4,919-check cross-platform proof, affected scalar evidence and SAME actual package
qualification are reused. No alternative writer, compatibility wrapper or new
production API remains. Current benchmark rows stay unchanged.

[Evidence](warm-vp8-group-emission-screen-evidence.json) retains the candidate,
result, restoration, immutable identities and mirrored raw archive. Stop emission
tuning and reconcile the existing JPEG444 forward-only cost after the shared JPEG
writer change. Full 253-path warm parity and replacement readiness remain open.

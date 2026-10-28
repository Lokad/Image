# VP8 trellis caller invariants

The existing Q75/effort6 diagram control uses 4.98% less filtered elapsed and
5.14% less elapsed with all actuals in a fresh independent comparison. Both
confidence-interval forms separate; faster-native-bracket drift is 0.058%.
The one-candidate short screen passes its 5% gate (5.90%). No repeated variants.

| Existing 513×259 RGBA8 diagram | Lokad µs | Ratio / upper | Allocated B/op | Both packet bytes |
| --- | ---: | ---: | ---: | ---: |
| Baseline short | 14,746.890 | 1.502 / 1.574 | 754,344 | 6,818 |
| Candidate short | 13,877.536 | 1.411 / 1.810 | 754,344 | 6,818 |
| Baseline confirmation | 14,851.464 | 1.514 / 1.541 | 754,344 | 6,818 |
| Candidate confirmation | 14,112.310 | 1.439 / 1.451 | 754,344 | 6,818 |

The candidate remains a confirmed CPU gap. All twelve actuals, filtered and
unfiltered statistics, native brackets and passing idle-v2 guards are retained.
Warm one-CPU complete operations include normalization, public encoding/admission,
fresh independent packet ownership and release. Reference inputs/settings and
FFmpeg/libwebp worker are unchanged; packet bytes and reconstructed pixels match
exactly. Allocation is unchanged; no buffer or admitted capacity is introduced.

A six-second current-build diagnostic captures 5,911 samples, including 2,395 in
Apply. All Apply instruction addresses map to its same-process 3,434-byte JIT body.
Explicit validation accounts for 307 samples (5.2%); entry including validation
accounts for 633 (10.7%), with only some entry work removable. These are diagnostic
shares, not component timing or a predicted gain.

All four production call sites already provide disjoint 16-sample blocks, live
plans, generated weights and bounded policy/context. Byte residuals establish the
DCT domain. The change removes their repeated private validation; public input
and setting checks stay intact. Cancellation remains before mutation, with safe
span access and disposed-rate checks. The search body is identical after line
ending normalization: candidates, arithmetic, strict ties, reconstruction, tails,
resource admission and ownership are unchanged. Tests retain cancellation and
behavioral coverage; only unreachable invalid-private-argument assertions go.

Windows/Linux each pass 5,074 checks plus 21 focused normal/scalar tests. The SAME
actual private 0.2.1 package passes both platforms and three examples, including
existing frozen effort5/6 synchronous/asynchronous packets and shared ownership.
Published packages remain immutable. Internal tests compile linked sources: an
initial unchanged production snapshot was caught before timing. One explicit
core build per OS establishes new immutable snapshots; no scored run is discarded.

Only this diagram/effort6 control has refreshed timing. Other method5/6 callers
need applicable current costs before CPU-pass claims. Methods0–4, other codecs,
decoders, editing/text and confirmed passing resize cases retain unchanged
applicable evidence. This adds no workload grid or parity pass and does not
complete the full 268-path objective.

[Evidence](warm-vp8-trellis-caller-invariants-evidence.json) pins samples, source,
producers, preparation correction and qualification. Mirrored/member-verified
vp8-trellis-caller-invariants-qualified.tar.xz: 180 members, SHA256
25c6d3a8dba9d7ba424f8d21a34f48556f86e43f8d952800d6424fdfe54113dd.

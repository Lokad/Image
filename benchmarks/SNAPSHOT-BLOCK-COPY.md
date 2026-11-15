# Grouped block copying

Current borrowed-copy costs are in [prepared copy rows](DRAWING-ROW-PREPARATION.md); the grouped kernel and owned-edit evidence remain unchanged.
Current flip costs are in [owned-flip confidence](OWNED-FLIP-CURRENT-CONFIDENCE.md):
word passes; byte remains uncertain. The earlier costs below are retained history.

Checked 256-byte block refs now expose fixed offsets while keeping four vector
loads before any group store. One shared loop replaces per-vector indexing;
the existing BCL tail and small/scalar fallback cover all remaining bytes.
No unchecked access, extra kernel, workspace, cache, owner or fixture dispatch.

| Existing complete 513×259 boundary | Lokad / reference µs | Conservative upper / status | Allocated B/op |
| --- | ---: | ---: | ---: |
| WarmVerticalFlip8 | 8.483 / 8.321 | 1.023752 / uncertain | 56.0 |
| WarmVerticalFlip16 | 17.641 / 17.595 | 1.015394 / uncertain | 56.0 |
| DrawingCopy8 | 2.028 / 1.915 | 1.063284 / gap | 0 |
| DrawingCopyOverlap8 | 3.720 / 3.804 | 0.983513 / pass | 0 |
| DrawingCopy16 | 3.797 / 3.818 | 0.997099 / pass | 0 |

Independent immutable-DLL confirmations save **1.30% / 0.111 µs** for byte flipping
and **1.44% / 0.029 µs** for separate byte copying. Both retain twelve alternating
4,096/8,192-call pairs, natural GC, original pixels/owners and idle-v2 guards.
These scoped effects are separate from standalone reference costs. Both flips
remain uncertain; separate byte copying remains a gap. Stop the two new passes.
The word-copy guard retains its BCL path: its current pass is no causal speedup claim.

Separate byte copying is higher than its historical 1.976 µs point. One targeted
unchanged-DLL control through the SAME current standalone harness measures 2.191 µs,
versus the candidate's 2.028 µs. That resolves the apparent direction conflict;
the paired 1.44% and standalone difference are distinct contexts, not one universal
percentage or an inferred alignment/JIT cause. All adverse and historical samples
remain; no candidate timing is repeated to obtain a pass. The preceding rejected
block assignment and its [compiler audit](ROW-BLOCK-COPY-CURRENT-SCREEN.md) remain history.

Actual generated flip code uses fixed 0/64/128/192 offsets and four vector registers,
with all four loads before stores. Caller code shrinks 894→645 bytes; every
instrumented elapsed value is discarded. Code size alone supplies no CPU evidence.
Disjoint chunks, traversal, clipping, cancellation checkpoints, admission, exact
hidden RGB/full-word samples, exception cleanup and independent owners remain.

Windows/Linux each pass **6,721 full/live checks**, plus **25 internal/190 public**
focused normal/scalar checks. The SAME actual private Release package and all three
examples pass both platforms, including byte/word vertical tails, shared refusal,
cancellation, surviving results and overlapping borrowed copies. One core/embedded
build per OS, no public/harness/native/helper/fixture build, one pack and no Linux
production rebuild after timing. Published packages are unchanged; publication postponed.

Owned flips retain **56 B/op**, borrowed copies **0 B/op**. Source/result payloads
stay 531,468/1,062,936 B per byte/word frame, with zero added admitted capacity or
retained workspace. Default two-slot/4 MiB idle storage and caller-root bypass stay;
physical/native/JIT peaks are unmeasured. Codec, resize, fill/composite and text/font
paths reuse valid source-specific evidence. All 289 material paths remain required.

[Evidence](snapshot-block-copy-evidence.json) pins all five current boundaries,
unchanged-DLL control, immutable producers, actual package and qualification.
Raw `snapshot-block-copy-qualified.tar.xz`: 4,580,372 B/160 members, SHA256
`ebebf3684cd540a9b9a3bb492f06d4d9a3e5d93f08a75a1382be447024f93adc`, mirrored/member-verified with retained dependencies.

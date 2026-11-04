# Word PNG execution attribution before local parser state

This capture describes the pre-change HSP200 parser. The subsequent
[execution-local state change](PNG-LOCAL-EXECUTION-STATE.md) invalidates its hot
offsets and component shares as current attribution. Retain the trace and JIT as
the reproducible basis for that experiment; they are not remaining savings.

One one-second quiet-VM capture reuses the qualified production DLL, frozen
513×259 RGBA16 Optimal control and equivalent one-shot reference worker. The
963,202 B packet, full samples, source/ownership/settings/native checks and both
quiet guards pass. No build or production change. Profiled elapsed is discarded;
sample counts and instruction skid are not component times or predicted savings.

| Location | Samples, out of 992 |
| --- | ---: |
| Lazy parser and inlined decisions | 653 |
| Bit output | 82 |
| Token coding | 51 |
| Huffman tree ordering | 48 |
| Block construction | 41 |
| History-origin advance | 29 |
| Huffman heap sink | 16 |
| Standalone dictionary insertion | 11 |

The exact-module lazy-parser JIT body is 1,497 B. Its hot relative offsets include
match/deferred decisions, pruning/prefix comparisons and state/token updates.
This is broader work than a single hash or cancellation poll. Existing byte
attribution remains applicable history for its unchanged path; the word capture
does not establish that removing checks or changing insertion alone would help.

The source audit compares parser-state loads/stores and publication with the
retained bounded-region implementation before the execution-local experiment.
Preserve exact matching/ties/packets, writer-failure prefixes, cancellation,
admission and independently owned output. Prepared input state must include its
complete costs; no new cross-operation cache or numerical-policy change.

[Evidence](png-word-current-attribution-evidence.json) pins the current module,
producer, gates and selected attribution. Raw
`png-current-word-execution-attribution.tar.xz`: 1,061,104 B / 28 members, SHA256
`e87850d5218d41b6988d3efddacbbdba46e8c3397d381d463a8fc07fadc63eb2`,
mirrored/member-verified, with the qualified word-boundary archive as dependency.
The original trace, exact-source JIT, offsets and reproduction are retained.
Unchanged-path qualification remains applicable; the accepted parser has its own
current measurements and full package qualification.

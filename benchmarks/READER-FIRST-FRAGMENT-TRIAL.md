# First-fragment reader trial

**Not retained.** Moving the uncommon synchronous fragment loop out of the
initial field read did not demonstrate a useful complete-operation gain.

The original three-pair quiet-VM JPEG async probe screen measured
0.253272 → 0.254963 µs, ratio
1.006528, interval 0.695521–1.456603,
**568 B/op on both sides**. This is inconclusive, with a slightly adverse point;
it establishes neither a slowdown nor a gain. All original pairs are retained.
These are paired managed-DLL effects, not an FFmpeg parity claim.

The candidate returned directly after a complete first fragment and kept the
remaining immediate-read loop in a nonasync helper. Only actually pending reads
entered the unchanged await helper. No extra async transition, storage, cache,
owner or policy was introduced. The initial offset was already zero; partial
assembly retained the same requests, counters, error sequence and polling.
Every other reader method reconstructs byte-for-byte from the qualified source.

Linux passes **10 internal /148 public focused checks in normal and scalar
modes**, including fragmented/pending reads, metadata, limits and ownership.
One core and embedded-test build; no harness/native/fixture or Windows build.
No confirmation, codegen, profiling or broad/package campaign followed this
unpromising screen. This implementation is closed; the measurement does not
ban materially different future designs.

The original source and Linux core/embedded/public modules were restored exactly,
without rebuilding. Windows modules and the actual package never changed.
[Refill completion](REFILL-COMPLETION-INLINING.md) remains the current producer
and five-cost owner, with its existing Windows/Linux/package qualification.
No allocation, admitted-capacity, retained-state or current-cost claim changes;
physical/native/JIT peaks remain unmeasured.

[Evidence](reader-first-fragment-trial-evidence.json) retains identities, checks,
restoration and all samples. Raw `reader-first-fragment-trial.tar.xz`,
SHA256 `97093aa3a1ace42e499ab3b4351feb7bc404bbce9aff40a569cf21f8601163fd`, 776,540 B/49 members,
is mirrored and member-verified with the current qualification archive as a
dependency. The full 289-path objective remains open; publication is postponed.

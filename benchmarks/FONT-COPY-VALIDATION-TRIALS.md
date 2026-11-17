# Font copy/validation scheduling trials

Neither implementation is promoted. Two bounded complete owned-load screens reuse
the same immutable E1 baseline, original DejaVu face/public worker and quiet VM.
These are private DLL comparisons, not standalone FreeType or FFmpeg costs.

| Complete owned load / same baseline | Baseline →candidate µs | Paired ratio interval | Managed GC B/op |
| --- | ---: | ---: | ---: |
| Copy state inside eight-record validation | 13.815232 →14.679146 | 1.004875–1.123501 /slower | 176 →176 |
| Outer scheduling, unchanged numeric kernel | 13.852645 →13.897912 | 0.981458–1.025565 /inconclusive | 176 →176 |

Both keep one admitted 678,016-byte array, existing offsets/bytes and independent
ownership. Metadata copies stay timed; 128 KiB bands initialize glyph data before
header reads. Short/cancelable/portable/small or overlapping cases retain complete
copying. Every fallback completes copying before the original ordered validator;
unreferenced glyph tails are initialized before mapping/publication. Admission
refusal never selects a fallback. No parsed cache, output alias or new heap buffer.

The first implementation adds a live state reference/copy check in each eight-record
group. Actual code grows 1,731→1,842 B and spills/reloads the current vector around
the conditional call. These observations motivate a coherent revision, not a
causal time attribution. The revision moves copying outside, nominates ready groups
by bounded binary search, then checks every selected offset against initialized
data using the **unchanged** numeric validator. Its actual kernel returns to
**1,731 B**; outer scheduling is 511 B and band copying 100 B. Code size does not
prove a gain. The revised screen is near-neutral and confidence spans parity.
No independent timing confirmation or band/settings sweep is justified.

Source review establishes the private failure boundary: no face escapes incomplete
construction, actual ownership precedes allocation, failed storage is not cached
and surviving outputs remain independent. A pre-timing review adds a guard for an
admission callback expanding the directory into deferred data. The copied directory
is initialized before any entry read. The first untimed version and its artifacts
remain; the corrected implementation is the one screened. An authored fixture's
moved header is aligned before testing. No frozen benchmark fixture changes.

Each screened implementation passes **318 internal** normal/AVX2-disabled/scalar
and **222 public** normal/scalar checks on **both OSes**. Twelve real-constructor
controls cover crossing/long/empty records, trailing data, ordered errors, metadata
overlap, moved/expanded directory callbacks, retained owners, refusal, cancellation
and recovery. An independent finite **8,192-case** model preserves first error
category/index and initialized-read bounds; it is not production testing or a CPU
claim. The unchanged baseline retains **6,819 checks per OS**, the SAME actual
package/all three examples. Zero broad/package repeats or canonical builds.

All three alternating 16,384-operation pairs per screen, warmups and idle-v2 guards
remain. Natural nonconcurrent GC, optimized JIT and sequential CPU0 boundaries stay.
Instrumented code-generation clocks are discarded. Candidate-only disassembly
reuses baseline evidence; its initial transfer list incorrectly expects an omitted
baseline file. Fetching the completed output fixes transport, with no operation
replay. No reference/fixture/quality gate relaxation or production dependency.

No verified elapsed/allocation/retention saving offsets the extra complexity.
Keep the current [prefix implementation](FONT-RETAINED-PREFIX.md) and its identified
costs. These results close the tested implementations, not a permanent family ban.
Continue ordinary-text uncertainty before assuming another optimization is needed;
passing paths and the full 289-path objective remain unchanged.

[Evidence](font-copy-validation-trials-evidence.json) pins sources, immutable DLLs,
all actuals, model and qualification reuse. Mirrored/member-verified
`font-copy-validation-trials.tar.xz`: **5,617,916 B /143 members**,
SHA256 `5e7a66f4215a82c5ed210505a7c6c10d1f246621fa1b86fe152c356cfcaeb593`. Archived `sva1214-*`, `gsc1215-*` and
`gso1216-*` recipes, plus the identified baseline/runtime dependency, reproduce it.
No renderer dependency is added because no displayed production cost changes.

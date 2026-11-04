# PNG search execution-boundary investigation

Separating dictionary search from the lazy parser does not establish a useful
complete-operation lead. Both stages are restored exactly; current timing rows,
allocations and the qualified package remain unchanged. These short points are
adverse evidence, not independently confirmed causal regressions or parity proof.

Stage 241 confines chain traversal to a small non-inlined routine, borrowing
history, links and cancellation once per search. Pending match/output state stays
in the existing local parser. It adds no storage, cache or admission requests.
Candidate order, search limits, prefix/tail tests, ties, short-match rules, output
ownership and direct cancellation checkpoints are preserved.

One two-second candidate capture records 1,958 target-process samples: search
775, parser 653. Parser/search JIT bodies are 1,238/751 B, versus the original
2,146 B parser. The out-result pointer remains live in r13; the chain counter
still spills. Stage 243 therefore returns a named two-int value pair. This is a
concrete representation revision, not favorable resampling. Samples, skid and
smaller code are not phase times or predicted CPU savings. Profiled elapsed is
discarded; both capture guards and the frozen packet pass.

| Existing complete PNG9 operation, three actuals | Managed µs | Ratio / upper | Allocated B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: |
| Original short, reused, RGBA16 | 23,466.10 | 1.124 / 1.245 | 3,420,454 | 963,202 |
| Original short, reused, RGBA8 | 18,215.77 | 1.056 / 1.149 | 932,438 | 210,973 |
| 241 separated search, RGBA16 | 24,234.91 | 1.161 / 1.214 | 3,420,454 | 963,202 |
| 241 separated search, RGBA8 | 18,349.53 | 1.065 / 1.094 | 932,438 | 210,973 |
| 243 value result, RGBA16 | 24,317.41 | 1.163 / 1.211 | 3,420,454 | 963,202 |
| 243 value result, RGBA8 | 18,318.74 | 1.062 / 1.089 | 932,438 | 210,973 |

Neither stage has a useful lead; no independent confirmation follows. Both reuse
the original baseline screens and current-cost evidence. Word/byte short runs
batch eight/eleven complete operations per actual, equally for both candidates
and baseline. Small allocation differences versus the byte confirmation reflect
batch amortization, not a library saving. Both sides acquire/dispose fresh codec/
zlib/packet state and independent output; native word endian-frame allocation,
conversion and release remain timed. No new image/settings matrix.

Each stage passes **563 internal/211 public PNG checks per OS and normal/scalar
mode**. Existing Deflate oracles cover history origins/slides, overlapping runs,
input delivery, packet decisions, cancellation/failure cleanup and later root
reuse. Frozen complete packets, full samples, alpha/hidden RGB, source immutability,
surviving independent outputs, caller stream ownership and all quiet guards pass.
One core/internal version per stage/OS; zero harness/native/probe/selector/public/
package builds. An inherited tooling-only receipt flag is corrected after the
first screen, preserving originals. Producer/source hashes were already correct;
actuals and operation boundaries are unchanged and no timing is repeated.

All original production sources, DLL/PDB/XML, internal assemblies and actual
package are restored byte-for-byte, with **zero restoration builds**. No new
tests remain from these stages. Reuse the qualified build’s 5,549 full/live checks
per OS and same actual Release-package/all-three-example proof, supplemented by
the separately qualified five reader and two statistics cases. No broad repeat
or fresh full-suite/package result is claimed. Deflate admission stays 384 KiB,
retained scratch zero; no new physical/complete admitted peak or native allocation
measurement is inferred.

Next count prefix/tail rejections and stronger-filter eligibility on the two
current level-9 controls before choosing another implementation. The existing
word matcher remains useful and unchanged. Do not replay unchanged word/prefix
or helper-ABI candidates. The full 282-path objective stays open.

[Evidence](png-search-execution-inconclusive-evidence.json) pins both source/core
versions, all actuals/native brackets, current-source JIT, profile, metadata
correction and restoration. Mirrored/member-verified raw `png-search-execution-inconclusive.tar.xz`: 2,880,128 B / 238 members,
SHA256 `ad3e322e76665de865fdbd28c2e8df9d89a69046912187abab15aa6bb135436f`. Original snapshots, immutable runtimes, trace/map,
guards, frozen-output validation and reproduction are retained; qualified-core/
package and current one-shot-reference archives are pinned dependencies.
Publication is postponed.

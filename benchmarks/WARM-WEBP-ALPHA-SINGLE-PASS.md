# WebP alpha compression under the work limit

The existing 32M-work control shrinks **268,028 → 208,216 B (22.3%)**. Its alpha
payload falls **114,363 → 54,552 B**; decoded RGBA, source quality and alpha remain
exactly unchanged. The complete **98,590 B** reference target remains open.

| Existing control | Managed / reference packet B | Managed / reference µs | Managed allocated B/op | Short ratio / upper |
| --- | ---: | ---: | ---: | ---: |
| EncodeBudget8 | 208,216 / 98,590 | 20,607.27 / 53,585.56 | 1,707,064 | 0.385 / 0.418 |

A single matching pass uses fixed prefix codes and one recent position per hash.
Three rolling residual samples avoid a filtered plane. The borrowed shared root
admits history and raw-sized encoded staging before allocation. History ends after
matching; staging stays charged through output. The old run-only cost is computed
in that pass, and complete padded sizes select the smaller finished candidate.
No resource or cancellation failure becomes a retry. Every actual work charge
remains enforced: **31,997,511 units**, within the unchanged 32M cap.

Both platforms pass **4,644 tests**, including nine new staging, no-gain, refusal,
cancellation, interrupted-output and late-work cases; 172 internal, eight public
and two differential scalar checks also pass. Normal/scalar packets are identical.
The same actual private 0.2.0 package and all three examples pass on both OSes.
Existing public/differential/reference/harness/native binaries are reused. Linux
uses an initial byte-screen build and one final build to preserve chunked-output
cancellation; Windows builds once. No rebuild follows final freezing.

One guarded three-actual cost screen records the figures above; native allocations
are unmeasured and CPU parity is deferred. Four unrestricted records remain exact,
and passing primary content/alpha paths are unchanged. All five procedural packet
gates remain open. Inputs, references, settings and quality tolerances are frozen;
equal numeric quality settings do not establish equivalence.

[Structured evidence](webp-alpha-single-pass-evidence.json) records hashes, work,
quality, ownership, build reuse, superseded historical harness descriptions and
retained raw archives. The preceding prototype's four writer calls exceeded the
available work; two packed calls produce identical bytes within the allowance.

# Output-resident bicubic row cache: unqualified

The independent candidate mean is1,051.074µs versus fresh original1,377.968µs:
23.723%lower in both filtered/unfiltered views, with separate confidence
intervals. However, faster-native-bracket drift is7.688%, exceeding the
predeclared3%limit. **Do not adopt or claim a causal gain.** Candidate ratio
1.264× / upper1.374,1,063,419B/op still does not establish parity.

The trial stores full-width decoded and filtered rows in an unpublished,
already charged output suffix. Its91,168B workspace leaves474prefix rows;
original strips overwrite45tail rows after the helper returns. No additional
allocation or ownership charge is introduced. One4KiB stack reservation,
horizontal/vertical tap order, precision guards, exact replay and cancellation
remain. This differs from changing horizontal SIMD width.

All43 focused public cases pass normal/scalar on both OS and Linux AVX512-off.
Six new behavioral controls cover exact rational pixels, odd/even widths,
prefix/tail boundaries, tiny alpha, independent retained owners, aggregate
admission, terminal refusal and cancellation cleanup. Exact frozen pixels,
approved native bounds, ownership and idle-v2 gates pass in every scored job.
One core build perOS; reference/harness/selectors reused, no new profile/grid.

Production source and exact qualified DLL/PDB/XML are restored; all43focused
checks pass again. Keep the six behavioral tests independently of this trial.
Unchanged full5,065checks/OS and same actual package/three-example proof carry
forward, supplemented by these tests and nine coefficient controls. No repeated
full/package proof. Current accepted costs and the268-path objective stay open.

The bounded VM check exposes no frequency control or cgroup throttling; it does
not establish the cause of native drift. No system settings changed. Preserve
the promising candidate without variants or repeated confirmation; reconsider
only after a concrete measurement-control improvement.

[Evidence](bicubic-output-row-cache-unqualified-evidence.json) includes the
predeclared decision, allactuals, native brackets and restoration. Mirrored,
member-verified raw archive:
bicubic-output-row-cache-unqualified.tar.xz, 85members, SHA256
40037a9fd2dc53257277d5392c4ef7fd218642b68bfc7eee79516ae254359175.

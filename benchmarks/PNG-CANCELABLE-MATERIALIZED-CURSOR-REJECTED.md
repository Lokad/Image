# Cancelable materialized PNG cursor: rejected

The short screen misses the predeclared 5% continuation gate: **4.34%** less
elapsed and **4.20%** native-normalized point reduction, with overlapping intervals.
The candidate is removed without variants or independent confirmation.

| Existing complete 513×259 RGBA8 decode | Mean µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| Original, three actuals | 1,589.543 | 1.140 / 1.174 | 4,472 |
| Rejected cursor, three actuals | 1,520.493 | 1.092 / 1.224 | 4,472 |

The trial reused the local bit cursor in bounded cancellation slabs and removed
duplicated token handling. It added no buffer, owner or admission mechanism.
Exact pixels, alpha/hidden RGB, input/settings, native reference, ownership and
idle-v2 checks pass. Short screens establish no parity pass or accepted gain.

All 130 internal and 258 public focused checks pass normal/scalar on each OS.
Eight candidate tests cover slab boundaries, end codes, truncated/excess output,
guards and cancellation/refund/recovery; they are removed with the trial.
An initial test canceled before context construction; its setup was corrected
without rebuilding production. Original source, test assemblies and qualified
DLL/PDB/XML are restored; 122 restored internal checks pass normal/scalar per OS.
The unchanged 5,153 checks/OS, same actual package and three examples remain valid.
One production build/OS, zero harness/selector/native builds, no repeated full or
package qualification. Accepted costs and all 268 required paths remain unchanged.

[Evidence](png-cancelable-materialized-cursor-rejected-evidence.json) retains all
raw/filtered samples, native brackets, failures, producers and restoration.
Mirrored/member-verified archive png-cancelable-materialized-cursor-rejected.tar.xz, 132 members,
SHA256 a72369a9917a6c36cfca99adfb75ee6a7aeb95093174d7efc662fac1771129b8; reproduction dependencies are pinned by hash.

# PNG ordinary and bounded-row cursor publication

The shared ordinary/slab inflater keeps its bit cache local through each block
or bounded history slab. Three exception-time publication regions are removed.
Completed EOB/slab stops and canonical-helper returns still publish exact state.
Only the failed private reader is abandoned: no production caller observes or
resumes it. Public errors, partial destination stores, full prefix lookahead,
cancellation checkpoints and outer ownership cleanup remain unchanged.

The bounded-word profile found 1,686 of 1,994 samples in this cursor. Its three
exception regions and stack cache homes motivated the change; sample locations
do not establish phase costs or prove a register-allocation mechanism. No new
array, pool, codec session, public API or numerical policy is introduced.

| Former demonstrated gap | Before / current µs | FFmpeg µs | Current ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Tight RGBA16 | 3,019.22 / 2,826.77 | 2,894.02 | 0.977 / 0.981 | 70,272 |
| Default RGBA16 | 2,952.03 / 2,827.31 | 2,897.12 | 0.976 / 0.978 | 8,568 |
| Default RGB16 | 2,336.31 / 2,120.01 | 2,229.39 | 0.951 / 0.957 | 6,520 |
| Tight live-cancellation RGBA16 | 3,005.70 / 2,789.14 | 2,893.47 | 0.964 / 0.976 | 70,272 |
| Two retained RGBA8 images, shared caller root | 2,870.80 / 2,610.86 | 2,847.04 | 0.917 / 0.920 | 1,638,214 |

All five now **pass and stop**. Savings are respectively 192.45, 124.72, 216.30,
216.56 and 259.94 µs. Raw and reference-normalized confidence intervals separate
in both filtered and all-actual views. Original source-valid confirmations are
reused; native point changes are recorded. This does not establish a CPU gain
for every refreshed case: grayscale16 is effectively unchanged.

An independent untimed RFC1951 header audit and the unchanged decode routing
identify **43 affected controls** among the existing 47 resident decodes/four
coding validators. Each receives one twelve-actual confirmation; the primary
confirmation is reused, without a new image/settings grid. All 43 pass their
original output, quiet-VM and complete-operation gates. Eight unaffected cases
reuse their named producers: separate materialized live cursors, stored-only
packets and non-RGBA Adam7 counting/BCL routes. The separate two-output shared-root
operation also receives one complete confirmation.

The [current inventory](PNG-CURRENT-FRAMING-COSTS.md) is **49 pass, one gap, one
uncertain**. The unchanged materialized live RGBA16 control remains 2,919.94 /
2,894.87 µs, upper 1.011700; live RGBA8 remains uncertain at upper 1.003815.
Resolve uncertainty before assuming more tuning. Older stream/composition costs
remain historical. Header-only inspection and encoding do not enter this cursor.

Decode includes parsing, tree/workspace preparation, inflation, full-depth RGBA
normalization, independent output acquisition and disposal. Native decode includes
its declared conversion/output ownership while retaining codec/packet/frame
wrappers. Coding validation retains its own metadata scope and prepared native
boundary. These are not fresh-native-context measurements. Exact pixels, alpha,
hidden RGB, source immutability, native normal/scalar CLI checks, tight ceilings
and borrowed-root behavior retain their existing gates.

All 51 inventory allocation counts and the shared-root allocation count remain
unchanged. Framing admission stays 4 KiB; existing rolling history, local ceilings
and output owners are unchanged. Shared-root admitted peak is 1,766,641 B,
including 218,775 B of caller input; its retained output pair is 1,062,936 B.
Scratch retained after each operation is zero. Existing released-output/filter
pool retention is separate and unremeasured. Total GC allocation is not peak
live capacity; complete physical/native peaks are not remeasured.

Windows/Linux each pass **5,907 full/live checks** (2,642 internal, 2,317 public,
948 differential), plus 709 internal/246 public per normal/scalar mode. Direct
production-cursor tests check eight bit phases, slab stops, EOB/following reads,
exact partial stores/errors and shared-root failure recovery. The same actual
private Release package and all three usage examples pass both OSes, including
default/live byte/word sync/async samples, overlapping retained owners, terminal
refusal, cancellation cleanup, subsequent success and independent survivors.

One valid main/embedded build per OS is reused throughout. There are no native,
harness, selector or fixture builds, no repeated original confirmation and no
favorable resampling. Two preflights fail before timing (missing control map and
Windows selector path separators); both are retained. Read-boundary producer
proofs are borrowed and restored, with actual module/input/reference checks.

[Evidence](png-normal-cursor-publication-evidence.json) owns the merged current
inventory, scope audit, source proof, observations, package identities and raw
archive identity. Earlier [prepared prefixes](PNG-PREPARED-PREFIX.md) and
[live cursor](PNG-LIVE-PREPARED-CURSOR.md) retain their historical comparisons.
The raw archive is mirrored and member-verified, with original tools, inputs,
modules, logs and reproduction scripts. Published packages remain immutable;
publication and the full 283-path parity objective remain open.

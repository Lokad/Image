# WebP parsing lifetimes

Count-only instrumentation of the existing photograph and transparent controls
finds one full-size parsing call per candidate family. Both encodes keep exact
packets, unchanged source pixels and an empty shared root after disposal.

| Observed family | Selected tokens / cache bits | Discarded token capacity B | Clustering histogram B | Whole clustering admission B |
| --- | ---: | ---: | ---: | ---: |
| Photograph | 125,974 / 0 | 1,062,936 | 7,147,140 | 7,949,820 |
| Transparent, first filter | 78,270 / 10 | 1,062,936 | 980,424 | 1,051,488 |
| Transparent, second filter | 18,989 / 5 | 1,062,936 | 373,320 | 444,384 |

There are no successive cached/uncached/boxed calls within these families.
Reusing a rejected buffer therefore needs a wider lifetime across the two alpha
filter preparations, or a handoff into clustering. The photograph histogram
does not fit that buffer. Retaining it only for the alpha histogram would add
82,512 or 689,616 B of live capacity compared with releasing it and allocating
that histogram. These are capacity calculations, not measured allocation or
whole-operation peak changes. Other clustering arrays and surviving candidates
remain independent owners.

This evidence does not justify a direct buffer handoff. It also does not rule out
a redesigned representation or other required controls. Existing matching heads
already borrow seed capacity, chain/output indexes already share storage, and
histogram populations are released after canonical code construction. Those
lifetimes are not new opportunities.

Two private source-projection builds first count parsing owners, then add
clustering sizes. Each runs one warmup and one observed complete encode per
existing control. Original selection, ties and writes drive output. Production,
benchmark harnesses, reference workers and settings remain unchanged. The second
run's completed results were retrieved after correcting a local filename handoff;
no encode or build was repeated for that transfer. Timings are not collected or
claimed. Current [trace-storage qualification](WEBP-TRACE-OUTPUT-STORAGE.md) remains
valid without repeating broad tests or package verification.

Next, attribute the current photograph's expensive phases before choosing a
histogram or matching redesign. No new performance control or production candidate
is justified merely by these capacity counts.

[Evidence](webp-parsing-lifetimes-audit-evidence.json) retains the observations and
source identities. Raw webp-parsing-lifetimes-audit.tar.xz: 459,836 B / 36 members,
SHA256 70849d06040a49a24349bdc5ded06c201ff235f9dfb544e7aedb6079620274c0,
mirrored/member-verified, with the qualified production/source dependency pinned.

## Reuse after alpha-filter emission

An audit before the handoff change, on the existing transparent Q75/effort4 control
finds a different handoff: both filters retain a 1,062,936-byte main-token owner
until emission, then dispose it before preparing the next filter. The selected
encoded ALPH payload has independent ownership.

Modeling retention of that capacity through the next transform, then reuse as
its matching seed, leaves the admitted peak at 3,847,888 B on this control. Before
the second matching stage, actual charges are 952,321 B; adding retained capacity
gives 2,015,257 B. This supports a prototype, not an accepted allocation/CPU gain.
No storage or admission actually changes in the audit. It does not establish
physical heap peaks, refusal behavior or compatibility with other representations.

One private source-projection tool build and one untimed complete encode preserve
the exact 98,590-byte packet, source pixels and empty borrowed root. Quiet guards
pass. A diagnostic method named Main initially triggered the compiler's entrypoint
warning-as-error; the corrected tool builds once successfully. No production,
harness, native, selector or package rebuild and no timing/repeated confirmation.

The [completed handoff](WEBP-ALPHA-TOKEN-REUSE.md) qualifies actual allocation,
admitted peak, failure/cancellation cleanup, palette/all-family boundaries and
the same package on both OSes. It establishes no CPU gain or regression. This
is separate from the rejected token-to-histogram handoff, which increased
modeled live capacity.

[Current evidence](webp-alpha-interfilter-lifetime-evidence.json) pins the model's
assumptions and limited scope. Raw `webp-alpha-interfilter-lifetime-audit.tar.xz`:
872,920 B / 273 members, SHA256
`a08ee17b76f383bce47da51c1621a874011321787b4d7459b60a398290f56e61`,
mirrored/member-verified, retaining sources, tool, frozen input, gates and correction.

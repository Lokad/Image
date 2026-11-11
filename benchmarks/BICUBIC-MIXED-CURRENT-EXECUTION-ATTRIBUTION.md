# Current mixed-word preparation and execution

MIA795 profiles the accepted immutable package on existing TightMixed16,
513×259→1027×129 RGBA16 with input+output+4 KiB. Its declared 893.16/native851.29 µs
cost (~42 µs point gap) is reused; this is not a new timing confirmation. Word
source is byte-identical to the current cost provider; shared math and geometry
are pinned to the current qualified package. Complete managed preparation,
normalization, filtering/repair and independent output/disposal stay inside timing.
The labeled zimg reference retains graph/input/scratch as before. These are not
equivalent fresh-graph or bare-kernel operations.

One two-second CPU0 capture, sampled from CPU1, has 1,993 locations:

| Exclusive sampled location | Locations |
| --- | ---: |
| 64-row result-backed execution | 1,574 |
| Eight-row suffix execution | 253 |
| Kernel evaluation | 43 |
| Single-word publication | 34 |
| Original exact repair | 28 |
| Geometry preparation | 27 |

The actual two driver bodies are 8,484/8,378 bytes. Locations span vertical
multiply/add, memory/index traversal, full word division, margin preparation and
certification. Inlining and skid prevent interpreting counts as phase time,
removable shares, stalls or a gain forecast. Instrumented elapsed is discarded.
Frozen source/output/native bounds, normal/scalar frames, ownership and idle-v2
checks pass. No build, new image/settings grid or qualification repeat.

The source model finds an 86,268 B output-suffix geometry view, safe through row 118,
and a 28,136 B result-backed working bank, safe through row 115. Prefix bands are 64
and 51 rows. The bounded suffix is 8+6 rows; default is 14. Whole-band guards prevent
geometry reads after its storage can be overwritten. Horizontal scalar Kernel
evaluations total 12,327 bounded versus 8,218 default: one extra 4,109-evaluation
pass. These are logical counts, not measured setup costs or free reusable state.
All preparation, restoration and repeated work remain in the complete boundary.

The kernel already publishes quartets. Replaying the recent vertical-quartet
change would duplicate it; earlier unselected word overlap code also remains
history. A different lead is shared word quotient preparation: full division is
present at the sampled publication sites. An exact-rational conditional model
bounds two fused Newton corrections plus multiplication below 2^-50 relative
error, assuming the existing byte-path RCP14 bound and normal finite arithmetic.
At magnitudes≤65,537 its bound is 2.274e-11. A proposed additional certificate
margin 2^-32 is 2.328e-10. This arithmetic bound does not prove all clipping,
invalid-value, conservative-acceptance or repaired-pixel behavior, and establishes
no CPU gain. Extra dependent instructions, register pressure and refusals may hurt.

## Bounded quotient experiment

RWP796 tried two materially distinct reciprocal/certificate strategies. Both
retain the original exact repair, extreme-divisor division and independent output
ownership. Hardware models reuse25,584 original numerical controls and30,000
clipped endpoints. A duplicate cached predicate can classify a synthetic clipped
endpoint differently after quotient approximation; this is an acceptance-model
finding, not a demonstrated complete-image pixel defect. The candidates use the
shared conservative writer instead. Actual row/repair and all focused Windows/Linux
normal/scalar checks pass (364 internal/776 public, all named storage suites).

| Existing complete operation | Qualified division µs | Two corrections µs | One correction µs |
| --- | ---: | ---: | ---: |
| TightMixed16 | 935.28 | 980.13 | 1,140.48 |
| Cross16 | 778.82 | 839.41 | 941.56 |

These are three-actual screens, not independent regression confirmations. Native
points stay~846–852/~821–824µs. Reference-normalized adverse point changes are
4.94%/7.72% for two corrections and22.85%/20.38% for one. No favorable repeat,
broad/package qualification or CPU gain claim follows. The shorter dependency
chain and larger rejection margin lose at complete-operation level. Models are
conditional arithmetic bounds, not a broader numerical-policy change.

Both variants preserve exact frozen pixels, source/reference/ownership and idle-v2
checks. TightMixed16 allocates114B/op; Cross16~199.9kB is not an allocation-saving
claim (the aggregate counters vary by a few bytes). No owned capacity or retained
scratch is added; physical/JIT-stack peaks are unmeasured. All attempted sources,
models, actual row tests and immutable binaries remain in the raw archive.

The accepted full-division sources, embedded tests and production binaries are
restored byte-for-byte. Unchanged6,248 full/live/resize-ISA and SAME actual-package/
all three examples from [wide quartets](WIDE-BICUBIC-SHARED-QUARTETS.md) are reused;
no package or release is changed. This rejects these implementations, not all
future word-publication designs. No renderer dependency or displayed cost changes.

Mirrored/member-verified raw `bicubic-word-reciprocal-experiment.tar.xz`:
4,586,624B/323members, SHA256
`5f4933ee3466b04fe3723ff285740c5d21ad57d75f2fa09af8965a277c5c83aa`.
It depends on the attribution and qualified baseline archives below. Next resolve
the existing substantial WebP photograph uncertainty with the already prepared
adjacent counterbalanced driver; retain its stricter confidence policy, exact
packet/settings and earlier global-bracket observations. No production tuning
need is assumed from an uncertain measurement. Publication stays postponed.

[Evidence](bicubic-mixed-current-execution-attribution-evidence.json) pins current
source/modules, profile, logical model and conditional bounds. Mirrored/member-
verified raw `mixed-word-current-attribution.tar.xz`: 125,648 B / 27 members,
SHA256 `b316d7a1bf0c019ef168e7646430706c5bca649465f6b15f82f9ddbf7c87d95b`; its BWF794 dependency supplies the
actual DLL/package and unchanged tools/fixtures/qualification. The
[earlier BRM538 attribution](https://github.com/lokad/image/blob/489ed20e7eeffc8607d7f5dc282283d002a304c3/benchmarks/BICUBIC-MIXED-CURRENT-EXECUTION-ATTRIBUTION.md)
and raw a88958cb remain history; those offsets predate the current row-bank design.
This audit adds no renderer dependency or displayed timing claim. All 289 required
paths remain in scope.

# Reader field-inlining trial

The field-reader inlining hint is not retained. Independent complete async JPEG
pairs favor the original by **2.17%**, with unchanged **568 B/op**. Production
sources and Linux core/embedded/public modules are restored exactly; Windows
modules were never rebuilt.

| Complete public async JPEG probe | Baseline →candidate µs | Paired ratio / interval |
| --- | ---: | ---: |
| Three-pair screen | .275859 →.272726 | .988819 / .614268–1.591754 |
| Independent twelve pairs | .271068 →.276942 | 1.021745 / 1.005230–1.038532 |

The hint was honored in the actual original/candidate benchmark contexts: JPEG
parser call sites to `ReadAsync` fall **6 →0**, code grows **7,451 →10,424 B**,
and the prologue stack subtraction grows **296 →600 B**. Dispatch code grows
3,452 →5,051 B and its three reader call sites also disappear. These are generated
code observations, not total stack footprints or attribution of the elapsed-time
change. All instrumented timings are discarded. A baseline-receipt lookup was
corrected using the completed capture and existing frozen boundary receipt;
no code capture was repeated.

Only a namespace import and `AggressiveInlining` attribute changed. Removing
them reconstructs the entire original reader: all loops, checks, counters,
partial/pending reads, cancellation and ownership remain verbatim. No wrapper,
buffer, cache, owner, API or dependency is added. This differs from the earlier
outer completion-wrapper trial; neither result imposes a permanent family ban.

Both quiet guards, original input/descriptor/acquisition/independent-ownership
checks and every natural-GC pair are retained. These are scoped DLL effects,
not standalone FFmpeg costs. Linux passes **10 internal/148 public** checks in
normal/scalar modes. One core/embedded build; zero Windows/public/harness/native/
fixture builds. The settled adverse confirmation ends this implementation:
no variant, favorable retry, uninstrumented candidate/native run, broad/package
or new fixture campaign follows.

Owned capacity and retention are unchanged; physical/native/JIT peaks are
unmeasured. The [current qualified package](BOUNDED-PROBE-REQUESTS.md), 6,721 checks
per OS and same-package consumers/three examples remain valid. Current timings
and the full 289-path objective remain unchanged. Further reader changes need
current hot-path evidence; fewer calls alone do not justify adoption.

[Evidence](reader-field-inlining-trial-evidence.json) retains identities, samples,
generated code, receipt recovery and restoration. Mirrored/member-verified raw
`reader-field-inlining-trial.tar.xz`: 826,560 B /77 members, SHA256
`cd8aad2129289ec5753911bc736301fb131d8eb62dc55458b91bbb8ade4cadc3`.

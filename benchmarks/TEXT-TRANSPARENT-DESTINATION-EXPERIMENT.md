# Transparent-destination text composition experiment

TZA703 removes three general channel quotients when destination alpha is zero.
For positive effective source alpha, exact word RGB equals the source; byte RGB
equals `(source + 128) / 257`. Original alpha rounding, zero-source no-op and
nonzero-destination arithmetic remain unchanged, including hidden RGB when the
stored alpha rounds to zero. No array, owner, cache or API is added.

| Complete prepared-layout drawing | Original µs | Candidate µs | Filtered raw / normalized gain |
| --- | ---: | ---: | ---: |
| RGBA8 | 91.24 | 91.08 | 0.18% / −0.18% |
| RGBA16 | 95.96 | 96.90 | −0.97% / −0.81% |

Independent reversed twelve-actual confirmation has overlapping raw and
normalized intervals in both filtered and all-actual views. All-actual raw points
are +4.29%/−0.55%; neither establishes a useful gain. **Not selected.** Exact
original sources, main/embedded modules and public bindings are restored on both
OSes. No causal regression, equivalence or permanent design ban is inferred.
The original/candidate three-actual screens and all adverse results are retained;
no further variant or timing confirmation follows.

General factor/rounding proofs and 655,360 integer sanity comparisons cover every
16-bit channel at five effective-alpha edges. Actual production composition passes
the existing 44,608 BigInteger cases and 240 targeted threshold, low-coverage,
hidden-RGB and no-op cases. Each OS passes 172 internal/185 public checks per
normal/scalar mode; all eight existing frozen frames and native visual gates pass.
Full 16-bit samples and exact managed pixels remain mandatory.

Latin allocation remains 59,000 B/draw; admitted drawing workspace remains
42,336 B. Narrow, Arabic and fitting peaks also remain unchanged; retained scratch
is zero. These admitted peaks are not physical/native peaks. No memory benefit is
claimed. Original resource evidence is reused from the unchanged TED698 text path.

The frozen DejaVu Sans 18px input, prepared font/layout, 384×160 canvas and reset
exclusions are unchanged. Geometry, events, integration, composition and disposal
remain timed. The reference is ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 on ImageSharp
3.1.12, explicitly a complete public-API integration pipeline, not FFmpeg or bare
shaping. The separate full integration includes every layout call and fitting
trial. Intended Windows Release DLLs are measured on the quiet single-CPU VM;
Linux-native code is separately tested.

One main/embedded build per OS serves this experiment. Linux preflight caught a
physical test-file line-ending difference before Linux mutation; normalized
originals were verified and saved, then only its pending build/tests resumed.
Windows results were reused. Orchestration initially continued after that failed
preflight; the failed guards and recovery are retained. No fixture/reference build
or repeated full/package proof follows byte-exact restoration. Current JGT702
6,073-check qualification and actual package remain valid. Selected benchmark
costs are unchanged, and this report is not a renderer dependency.

[Evidence](text-transparent-destination-experiment-evidence.json) pins both
implementations, modules, every job, guards, resource checks and restoration.
Mirrored, member-verified raw `text-transparent-destination-experiment.tar.xz`:
4,984,780 B / 148 members, SHA256
`f6d014d7b45a9b2bc24a7217643e24bfe2944e876592c148dbc4e9aeb843f22b`.
Dependencies are the retained TED698 text and JGT702 package archives. The full
283-path objective remains open; publication is postponed.

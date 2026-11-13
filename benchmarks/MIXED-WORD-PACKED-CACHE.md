# Mixed-word bicubic packed cache

[Current adjacent confidence](BICUBIC-CURRENT-ADJACENT-CONFIDENCE.md) supersedes
only the default mixed-word uncertainty below. That complete operation passes
on the current immutable package DLL; original captures remain history, without
a production speedup claim. Bounded and tight-vertical passes remain stopped.

Accepted revision MWB915B, 2026-11-13. The large mixed-word column bank retains exact
UInt32 alpha products and expands them at each weighted read. Its 256-entry
domain removes the hot byte-index bounds check and uses 8,192 B instead of
9,216 B. Small banks keep their original typed allocation; product aliases exist
only inside the large-bank branches. This changes one
production file; no public API, owner, reservation, pool or arithmetic policy changes.

| Existing RGBA16 control | Managed / zimg µs | Ratio / upper | GC B/op | Status |
| --- | ---: | ---: | ---: | --- |
| 513×259 → 1027×129, input + output + 4 KiB | 820.00 / 846.10 | 0.969 / 0.978473 | 113 | Pass; stop |
| Same resize, default workspace | 891.75 / 909.47 | 0.981 / 1.004773 | 114 | Uncertain |
| 513×259 → 513×129, input + output + 4 KiB | 458.03 / 591.94 | 0.774 / 0.777575 | 113 | Pass; stop |

The primary independent comparison saves **35.58 µs / 4.16%** raw, with separated
confidence intervals. Retaining all actuals gives **36.25 µs / 4.23%**, also
separated. The reference clock moved −0.81%; the **3.38% normalized gain** also
has separated intervals. The contemporary baseline was uncertain; the selected
revision has a confirmed incremental gain and current parity pass. Tight vertical
independently saves **23.04 µs / 4.79%** raw and **4.96% normalized**, both separated.
Default-workspace measurements retain a different reference clock and uncertainty;
do not infer a causal raw improvement there. No previously passing affected case
becomes a demonstrated gap. Stop confirmed cases without favorable resampling.

Candidate A is not retained. Its 1.30% raw primary saving and normalized uncertainty
initially accompanied a passing vertical cost. A late same-producer comparison
found **31.65 µs / 6.58% raw vertical adversity**, with separated raw intervals;
normalized intervals overlapped. B restores the original small-bank typed view
and removes the extra outer byte/product views. The regression, initial compile
failure, A qualification and every original sample remain retained.

The borrowed workspace view shrinks **28,136 → 27,112 B**. This is within already
admitted result capacity: **total allocated, owned, admitted and retained capacity
do not decrease**. The reported 113–114 GC B/op does not establish a buffer-capacity
reduction. The frozen result-loan prefix stays 121
rows; the existing 67×211 → 139×107 test changes from 77 to 78 safe rows and passes
exact poison/retirement checks. Physical/native/JIT peaks are unmeasured.

Every product is at most 65,535², exactly representable in binary64. Conversion
moves to each weighted read; multiply/add order, horizontal certificates and exact
repair remain unchanged. The frozen prefix trades 253 expansions for 973 per
column pair. That count and the JIT's removed indexed check explain the design,
not a separately measured phase saving. Three remaining `cmp 256` instructions
are unrelated checks. B's large body is 7,836 bytes versus A's 8,246; its eight-row
tail returns to the original 8,067-byte size, which alone is not byte-identity
proof or a phase timing. No C# unsafe code is introduced.

These are complete public `Resize` operations with admission, independent output
and disposal. The explicitly labeled **zimg 3.0.6 FFmpeg scaler backend** retains
its graph/input/scratch; source deinterleaving, clamped halo, filtering, output
allocation, packing, quantization and release remain timed. This is not an
equivalent one-shot preparation comparison. Exact managed rational/full-word
pixels, native-only RGB/alpha error ≤1 and RMSE ≤0.25, unchanged inputs, zero-alpha
behavior and independent outputs pass. No new image, shape or settings grid.

A has one paired three-actual screen and independent twelve-actual comparison,
plus two companion captures and one late prior-build vertical confirmation.
B has bounded primary/vertical screens and one independent twelve-actual current
capture per affected case, reusing the same immutable prior producer. CPU0,
sequential Release, idle-v2 guards before/after; all samples and adverse/uncertain
views retained. No worker, fixture, selector or benchmark rebuild. Each compiled
candidate is immutable; the selected Linux DLL is reused through packaging and
Windows builds the same source. No broad/package repeat for text work.

Both OSes pass **6,551 full/live checks**, focused **48 internal / 21 public**
normal/scalar checks, and the **same actual Release-package consumer / three usage
examples**. The three named resize-storage suites are included. Existing tests
cover tails, extreme alpha, generalization, result-loan retirement, root refusal,
cancellation, cleanup and independent survivors. Unchanged codec, Lanczos, drawing,
text and other-ISA evidence is reused. Published releases remain immutable;
publication is postponed.

[Evidence](mixed-word-packed-cache-evidence.json) pins source, timed/package DLLs,
inputs, outputs, reference identities, observations and the mirrored, member-checked
raw archive. Run retained `tmp/mwb915-screen.py`, `mwb915-confirm.py` and
`mwb915-companions.py` on the quiet VM with their pinned prerequisites; the archive
contains invocation scripts, guards, raw logs, JIT, compile failure, source patch,
producer artifacts and package proofs. Dependencies retain unchanged harnesses,
fixtures and previous producers. [Vertical-row experiments](MIXED-WORD-VERTICAL-ROWS.md)
remain history, not a permanent prohibition on revised designs.

Stop the confirmed bounded mixed-word control. Default mixed-word uncertainty
is not evidence of a gap. Next investigate sharing vertical work across paired
RGBA16 Lanczos output rows, using the accepted byte schedule as a lead. Preserve
the byte pass, word arithmetic and output-loan lifetimes; no gain is assumed.
Bounded byte-wide bicubic also retains its demonstrated gap. The **289-path
objective remains open**.

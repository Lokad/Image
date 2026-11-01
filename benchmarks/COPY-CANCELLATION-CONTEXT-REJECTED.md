# Rectangle-copy cancellation dataflow trial

One short screen supplies no useful gain. The candidate reads the original
cancellation-token argument instead of the identical operation-context property.
The VM JIT removes the dependent per-chunk field load and reduces the method
from 1,146 to 1,108 bytes, preserving every checkpoint, admission and disposal.

| Existing RGBA8 copy control | Lokad µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| Retained independent twelve | 2.127 | 1.100 / 1.114 | 56 |
| Candidate, one short three | 2.240 | 1.121 / 1.476 | 56 |

Candidate filtered/all points are 5.32% higher and native-normalized points 1.95%
higher. Confidence overlaps: this is no useful gain, not a confirmed regression.
There is no independent confirmation, variant or companion retiming.

Both subjects copy 256×129 pixels from (127,65) to (17,19) in separate resident
513×259 RGBA8 images. Complete public validation, admission, cancellation and
scope cleanup are included. Source/canvas setup, I/O and verification are excluded
equally; neither allocates an output. Full managed/native pixels, alpha, hidden
RGB, untouched pixels and source match exactly. Both quiet-VM guards pass.

The retained [copy-span confirmation](WARM-RECTANGLE-COPY-SPANS.md) remains valid:
subsequent production changes affected FillRectangle, leaving this copy path
unchanged. The candidate and restored code each pass 72 focused public checks in
normal and scalar modes. Original source, source manifest and all 53 saved Linux
binaries are restored byte-exactly without rebuilding. Windows production and
the actual package never changed. Existing 5,378 checks per OS and the same actual
Release-package consumer proof are reused from the
[packed-fill qualification](WARM-RECTANGLE-FILL-PACKED.md).

[Numeric evidence](copy-cancellation-context-rejected-evidence.json) preserves
filtered/all actuals, native brackets, costs, identities and the rejection.
Raw copy-cancellation-context-rejected.tar.xz: 610,876 bytes / 52 members,
SHA256 2150bb7e9da24f63263f44d17c3da9d046910e790cdfa093f525510c4fc59121,
mirrored and member-verified. It retains both JIT listings, source variants,
actual producers, build/focused/measurement logs, guards, restoration and replay
scripts. With the retained copy-span runtime/reference, run ccg73-build-vm.py,
the DrawingCopy8 short-vm.py and ccg73-restore-vm.py from the repository root;
the caller-side wrappers preserve immutable inputs and parse the results.

The current copy gap and full 282-path objective remain open. This substitution
is closed. Keep the accepted kernels; no passing case is retimed.

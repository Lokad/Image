# Current progressive JPEG costs

Two existing uncertain controls now have independent quiet-VM costs on the
unchanged qualified production library. Both remain CPU gaps. Managed output
matches the frozen native pixels exactly in normal and scalar modes.

| Existing complete warm one-CPU decode | Lokad µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| ProgressiveGray | 2,475.155 | 1.442 / 1.476 | 683,126 |
| ProgressiveCancelable444 | 4,833.602 | 1.438 / 1.447 | 984,212 |

The original 513×259 progressive grayscale and 4:4:4 packets and independent
coefficient proofs are reused. Complete public resident-input decoding,
conversion, fresh independently owned RGBA8 output and release are timed on
both subjects. The FFmpeg software MJPEG decoder resets state per operation;
its codec/conversion contexts and packet/frame wrappers are reused. Grayscale
includes GRAY8 expansion into independent RGBA8. The 4:4:4 token is live and
never signaled; setup is excluded and native has no equivalent token option.
Startup, I/O, IPC and validation are excluded. Native allocation is unmeasured.

An initial grayscale setup fails before any actual iteration because its
prepared managed-pixel hash predates the qualified byte-exact inverse. Four
untimed public decodes on the actual immutable library verify normal/scalar
equality, borrowed input lifetime and independent output after clearing input.
Both current outputs equal the already frozen native hashes. Only temporary
managed-hash header fields are refreshed. Packets, quantization, coefficients,
reference settings and native pixels stay frozen; no tolerance is relaxed.
Both original preparation files are restored byte-exactly after measurement.

All twelve actuals per case and both native brackets are retained. Exact
managed/native frames, CLI/scalar agreement, opaque alpha, source and output
ownership and quiet-v2 guards pass. Both conservative lower bounds exceed 1×.
These are current costs, without causal speedup claims against older producers.

No production, test, harness, selector or native build; no profile, new fixture,
quality sweep or successful timing repeat. Existing 5,267 checks per OS, live
comparisons, the same actual Windows Release package on both OSes and three
examples remain applicable. The full 268-path objective stays open.

[Numeric evidence](current-progressive-jpeg-costs-evidence.json) includes every
actual, confidence view, allocation, current/frozen pixel hash, preparation
correction and restoration record. Raw current-progressive-jpeg-costs.tar.xz,
SHA256 3e47edc85c95f8fb34f8b7af94b7eb5a9e411f220076086cc79cc83aba062b4b,
is mirrored with all 76 members verified, including selectors, workers, actual
library, packets, coefficients, frames, receipts and guards. Original source
archives are pinned as dependencies. Reproduce these named controls with the
[reference tooling](../tools/WarmReference/README.md).

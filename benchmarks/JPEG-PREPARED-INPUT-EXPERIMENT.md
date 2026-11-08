# Bounded JPEG entropy preparation

JCA601/JPS602, unselected. A scan owns one buffer of at most 64KiB, removes byte
stuffing in chunks and retains up to 31 bits between blocks. The same coefficient
grammar and MCU scheduler handle encoded and prepared input. Restart, live-token
and progressive scans keep the encoded route. Shared admission precedes allocation;
refusal is terminal and the buffer is released after its actual users return.

| Complete decode | Qualified original µs | Three-actual screen µs | GC B/op, original → screen |
| --- | ---: | ---: | ---: |
| RGB444 | 1,530.34 | 1,529.75 | 42,936 → 108,496 |
| Sparse Adobe CMYK | 511.72 | 507.00 | 20,944 → 43,496 |

No useful complete-operation gain is established for the added allocation. RGB
is essentially unchanged; the small CMYK point gain has wide uncertainty. No
independent confirmation, further buffer variant, broad suite or package run
follows. These results do not permanently close a materially revised preparation
design. The pinned native source informed the phase audit; no upstream code is
copied into production.

Frozen inputs, exact 531,468B independent outputs, native pixel/alpha/source gates,
complete preparation/copy/admission/decode/disposal and quiet single-CPU boundaries
remain. Native codec/input/frame capacity retention is explicitly labeled. The
temporary array charge is capacity+24B (at most 65,560B); retained scratch is zero.
Physical/native peaks were not measured. Allocation is not a peak measurement.

Each OS passes **410 internal/375 public checks per normal/scalar mode**. Initial
checks exposed a long-prefix bug when the extended cache already held eight bits;
it was corrected before any timing. Three actual fused/canonical/carry/padding
controls cover the fix. The failed Windows module/checks and initial field-order
compile error are retained. Public grammar and correctness gates are not weakened.

Qualified sources, main/embedded modules, bindings and source manifest are restored
byte for byte; prototype-only files/tests are removed. Existing 5,909 full/live
checks per OS and SAME actual package/three-example qualification are reused.
Current numeric summaries and published packages remain unchanged.

[Evidence](jpeg-prepared-input-experiment-evidence.json) pins sources, work scope,
all actuals/guards, fixes and restoration. Mirrored/member-verified raw
`jpeg-prepared-input-experiment.tar.xz`, SHA256
`2b2c48bdb9562e65a37eef143660fbf422e3588767e30f469fb38965feed4984`,
12,168,604B/211 members, retains reproduction and qualified input/module providers.

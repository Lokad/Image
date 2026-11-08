# Mixed-word bicubic execution attribution

BRM538 captures the qualified producer after shared word preparation changed the
inlined code. Older BRM493 offsets remain historical. Production, the actual
package and all displayed costs stay unchanged; profiled elapsed is discarded.

One two-second capture reuses the existing complete 513×259 → 1027×129 RGBA16
Mixed16 workload and cached worker. CPU0 executes; CPU1 samples. All original
source, exact managed rational pixels, bounded native float pixels, scalar,
independent-output and idle-v2 gates pass. No build, new fixture/settings grid,
performance confirmation, broad suite or package verification is repeated.
Managed geometry/workspace/output preparation remains inside each operation;
native graphs and scratch remain prepared as declared.

| Exclusive sampled location | Samples, out of 1,995 |
| --- | ---: |
| Mixed-word driver, including inlined certification | 1,830 |
| Kernel evaluation | 55 |
| Single-word publication | 27 |
| Geometry preparation | 20 |
| Scalar exact repair | 19 |

Actual driver code is 8,262 bytes; the JIT reports an 872-byte local frame.
Hot offsets span paired sample decoding/storage, vertical multiply/add, quotient
division, exponent-margin preparation, certification and clipping/packing. These
locations include inlining and skid: they are not phase times, stall proof,
predicted savings or a measured cost of exactness. Scalar repair's small sample
count does not establish that the surrounding exactness guarantees are free.
Native float arithmetic does not supply equivalent rational/certified guarantees.

The source audit finds output-suffix geometry, paired column decoding, vertical
cache, quartet/pair/single publication and shared certification already present.
A useful next boundary to investigate is retiring paired-column decode/vertical
filtering from the long-lived horizontal/certification driver. Preserve one actual
column implementation, specialized kernels, original tap order, checks and repair;
avoid a generic pixel loop. Shorter-lived state and code size alone cannot justify
acceptance: a helper call or additional staging can outweigh the intended benefit.

Existing explicit stack arrays total 3,992 bytes under a 4,096-byte workspace
reservation. A proposed helper could own the 1,536-byte decoded array while the
caller retains 32-byte publication scratch: explicit overlapping arrays would
total 4,024 bytes. This is a source-derived design bound, not measured total stack
or physical/admitted peak. The JIT frame and array counts are not additive peak
measurements. Heap/retained storage and current ownership remain unchanged.

First prove that lifetime boundary and actual dispatch scope. Reuse existing
Mixed16/TightMixed16 and, if affected, TightVertical16; default vertical selects
the existing cached path. Keep exact managed results and native bounds. Focused
checks precede short complete screens and independent confirmation if promising;
accepted coherent stages receive affected costs and consolidated package/OS
qualification. No numerical-policy change or automatic merge is proposed.

[Evidence](bicubic-mixed-current-execution-attribution-evidence.json) pins source,
producer, gates, samples, JIT and bounded next hypothesis. Raw
`bicubic-mixed-current-execution-attribution.tar.xz`, SHA256
`a88958cbfc6dae4ec1555f698edaca50be23afe776b8a5ae84867a5a167d68ce`,
6,222,684 bytes / 137 members, is mirrored/member-verified with qualified-core and
word-preparation providers. It supplies no displayed timing or renderer dependency.
Publication is postponed; the full 283-path objective remains open.

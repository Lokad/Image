# Current sparse JPEG cost diagnostic

One completed one-second diagnostic of the current sparse-2D CMYK JPEG decode
collects963samples. Both official idle-v2 guards pass. These are sampled CPU
locations, not comparative timings or a new performance confirmation.

- Sparse integer inverse transform:196samples,20.4%.
- Plain baseline AC reader:139,14.4%.
- Scan processing:98,10.2%; four-component plane materialization:95,9.9%.
- Baseline block reader:66,6.9%; Huffman read:35,3.6%; bit read:26,2.7%.

Current exact analytical pixels, alpha, CMYK interpretation, native normal/scalar
quality gates, source preservation and independent output ownership pass before
profiling. All relevant JPEG and owner/context paths are unchanged since the
[qualified JPEG round](WARM-JPEG-BUFFERED-EOB.md), whose largest JPEG gap is1.461x.
The current core is explicitly bound to the existing harness and compiled selector.
Zero production, harness, selector or native builds; no full/package repeat,
new fixture or settings grid. Accepted4,484tests/OS and same actual private
0.2.0 package qualification remain applicable. Selected rows/counts are unchanged.

Next: bound paired signed16 frequency products inside the existing sparse integer
inverse transform. Keep exact Q16 sums, the original strict certificate/fallback,
work, cancellation and ownership. Reuse the existing independently generated8KiB
basis; no new table or scratch allocation. Other numerical/hardware domains keep
their existing path. One original Sparse2D short first; determine precisely which
four-component fixtures can reach this helper before accepting a change. The
independently passing caller RGB420 case stays stopped.

[Evidence](warm-jpeg-sparse-cost-evidence.json) includes sanitized sampled symbols,
quiet guards, actual identities and the mirrored archive hash. Uniform warm
parity and replacement readiness remain open.

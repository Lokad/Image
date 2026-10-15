# Single-pixel bicubic reduction

The existing RGBA8 513×259 → 1×1 public resize independently confirms
**0.213× / upper 0.227×** the explicitly labeled **zimg backend used by FFmpeg
zscale**, at **130 managed B/op**. Production code is unchanged.

| Quiet observation | Lokad.Image µs | zimg backend µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Short screen | 1,158.20 | 4,958.19 | 0.234 / 0.279 | 130 |
| Independent confirmation | 1,045.31 | 4,897.54 | 0.213 / 0.227 | 130 |

Only one fixed geometry and the existing original byte raster are used. The
row cache excludes width below two and support above 2,048; other bounded
generated-coefficient paths remain available. This screen finds no performance
gap requiring a production change or profile.

Complete public Resize includes admission, allocation, materialization and
owned-output disposal at the default working limit. The native operation times
packed-source deinterleaving, the complete required clamped halo, premultiplied
FLOAT filtering, unrounded-alpha division, quantization, zero-alpha RGB clearing,
independent output allocation/repacking and release. Horizontal/vertical halos
are 770/389, covering support [-770,1282]/[-389,647]; unused square-halo rows are
excluded. Graph/input/scratch contexts are reused, processing all samples each
call. Startup, I/O, IPC, generation and validation are excluded. This is a backend
comparison; it does not measure FFmpeg CLI startup.

Managed, normal native and scalar native pixels match the independent exact
rational reference byte for byte, including alpha. Source and simultaneous
independent owners are checked. Thirteen native negative/protocol controls pass.
The existing native RGB/alpha ≤1 and RMSE ≤0.25 policy is unchanged.

Both sequential Release runs pass the versioned pre/post quiet guards on CPU0,
one filter thread, disabled tiering and workstation nonconcurrent GC. The
independent confirmation retains all twelve actuals (ten selected); its upper
includes selected/all-actual intervals and the faster native bracket's lower
bound. Allocations include harness overhead; native total allocations are
unmeasured.

One changed harness build and one native worker build suffice; the same verified
immutable core/harness/worker serves both runs. Existing 3,483-tests-per-OS/live/
same-actual-package qualification is reused because production inputs are
unchanged. No production rebuild, profile or repeated package proof is needed.

[Retained evidence](warm-bicubic-single-pixel-evidence.json) includes every actual,
allocation, guard, identity, control and raw archive hash. Stop this fixed pair;
remaining material/current-source coverage stays open.

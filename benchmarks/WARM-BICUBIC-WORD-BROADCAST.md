# Packed word bicubic broadcasts

The existing tight-memory RGBA16 reduction is **13.7% faster** with
bit-identical coefficient lanes, arithmetic, pixels, workspace and allocation.
It remains a qualified three-actual parity gap.

| Existing operation | Lokad / reference µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: |
| RGBA16 513×259 →64×32, input+output+4 KiB | 527.505 / 383.981 | 1.374 / 2.313 | 16,505 |

One six-second diagnostic identified horizontal tap processing as53.2% of
samples, including repeated scalar shuffles to build each weight vector. One
expression now broadcasts each weight and explicitly inserts the upper vector,
as already used by the byte kernel. No precision, FMA, filtering order, cache,
admission, work, cancellation, ownership or exact replay change is introduced.
Fixed arrays remain4,044B inside the existing4,096B charge.

Both sides retain complete operations: public Resize, admission, fresh owned
output/materialization/disposal; native **zimg (FFmpeg zscale backend)** includes
halo16, deinterleaving, premultiplied B0/C0.5 FLOAT filtering, unpremultiplication,
quantization and independent output/release. Startup, IPC, I/O, setup, hashing and
validation are excluded. Original513×259 word raster only; no image/settings grid.
Native normal/scalar RGB/alpha maximum1 and RMSE≤0.25, exact managed rational
pixels, source and independent owners, and all worker/source/compiler/flags/archive
pins pass. Compression sizes do not apply to resizing; no codec packet changed.
Managed allocation includes harness amortization; native allocation is unmeasured.

One quiet short retains all3actuals and both5s idle-v2 guards. The reported upper
is the larger selected/all-actual managed bound against the faster native bracket's
lower bound. Native bracket uncertainty leaves a wide2.313x short upper; this
screen does not establish parity. No12actual repeat while the mean remains above parity. No controls
were invalidated: other206comparisons/all61passes retain their exact executed paths.
The old independent word result is superseded by this qualified short.

All **3,722 Release cases perOS** pass:1,388internal,1,436public and898liveFFmpeg.
The129affected public checks pass perOS; unchanged scalar guards/fallback reuse
the prior138bicubic checks/OS. The **same actual Release package** passes its
consumer and allthree usage examples on Windows/Linux; DLL/PDB bytes match the
screened producer. Published0.1.0 artifacts/tag stay unchanged.

[Numeric evidence](warm-bicubic-word-broadcast-evidence.json) retains all actuals,
quiet guards, source reconciliation, qualification and raw archive hashes.
The207-case parity goal remains open:61passes,145gaps,1confidence case;
126independent selected confirmations and81qualified shorts. No coefficient,
broadcast, layout or inlining tuning grid follows this accepted primitive.

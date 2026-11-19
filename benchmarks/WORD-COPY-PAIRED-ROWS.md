# Separate RGBA16 copy with paired rows

The complete borrowed RGBA16 copy passes at **3.423 / FFmpeg 3.788 µs**,
with a conservative ratio interval of **0.861085–0.945815** and **0 B/op**.
Release package `865b2c68` contains the exact screened DLL `18210aad`.
The original private and production intervals remain in the
[copy boundary report](COPY-OPERATION-BOUNDARY.md#placement-balanced-rgba16-confirmation).

A 2 KiB row needs one snapshot group. The AVX-512 implementation
completes each row before reading the next and processes two rows per
loop test. Odd counts use one initial row. Wider aligned rows retain
the full-group loop. Live cancellation and portable paths use the
existing mover. The single source helper expands four times in the
2,100-byte Linux JIT method, with no vector spills.

Across 32 matched pairs on eight backings, the change saves **125.3 ns**
on average against the preceding private snapshot. Every backing
average improves. The independent reference confirmation retains all
12 units with 16 subdivisions, 262,144 complete calls per unit and the
original conservative confidence calculation. Eight predefined
backing pairs appear in every unit in both roles. The worker,
dynamic clipping, full-frame pixels, hidden RGB and borrowed owners
match the original checks. Both quiet guards pass.

Windows and Linux each pass 386 exact footprint cases, nine rejected
footprints, five public row counts, and live and precanceled token
checks in normal and scalar modes. Linux exercises the AVX-512 path.
The focused rectangle suite passes all six checks. The full final-package
benchmark refresh and release qualification remain pending.
[Source, raw observations and immutable identities](word-copy-paired-rows-evidence.json)

# Buffered PNG inflation with original failure ordering

Noninterlaced, noncancelable PNG decoding may admit an 8 KiB BCL output buffer
after the original current/previous rows. All remaining row work must already
fit the budget; tight memory/work, cancellation and Adam7 retain original reads.
The buffer remains admitted until inflater disposal. Larger reads reduce repeated
per-row inflater calls, while preserving complete output reconstruction.

Buffered reads are speculative. If BCL inflation reports a compressed-data error
before the current row has been checked, the reader closes that inflater, reopens
the same owned input, skips only bytes already delivered and resumes unbuffered
reads. Work charges and row/filter/palette/error ordering remain original.
There is at most one replay, no output publication on failure, and only one native
BCL inflater workspace is live. No native bindings or upstream code are added.

One unchanged palette8+full tRNS PNG uses the same original RGBA8 projection and
complete public Decode/owned-output release boundary. Native PAL8 unpacking,
one-thread conversion and independent output acquisition/release remain timed;
wrappers are reused. Normal/scalar pixels, source, alpha/hidden RGB and held owners
are exact. Startup/I/O/IPC/preparation/validation stay outside timing.

| Quiet screen | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Original per-row inflation | 867.02 | 361.58 | 2.398 / 2.597 | 2,760 |
| Retained 8 KiB buffered inflation | 737.54 | 363.65 | 2.028 / 2.446 | 10,976 |
| 32 KiB trial, not retained | 736.28 | 362.43 | 2.031 / 2.468 | 35,552 |

The retained screen reduces managed mean 14.9%, but **2.028x / upper 2.446x
remains open**. Every unchanged guard passes; all three actuals are retained and
the upper takes the larger filtered/all-actual bound. No long confirmation was
launched after parsing the above-2x mean. One bounded 32 KiB follow-up gave no
material timing benefit and raised allocation; exact 8 KiB production was restored
and its method bodies/flags re-proved before final validation. No shape/image/
quality sweep was performed. Allocation includes harness overhead; native totals
are unmeasured. Held/first-use/tight-budget boundaries remain distinct.

Each OS passes 2,663 Release tests (896 internal, 871 public, 896 live FFmpeg),
48 focused PNG public controls normal/scalar, and the same actual dependency-free
Release package with all three usage examples. All 591 final fresh/timed/profile
method bodies and flags match the package. Paired buffered/original controls cover
filter and palette errors combined with bad checksums, including late failures
after delivered prefixes, plus exact old memory/work thresholds and all 72
original authored PNG profiles with poisoned storage and held owners.

A six-second diagnostic profile has 5,986 samples: BCL inflation 29.5%, literal
framing 28.1%, framing Validate 11.2%, canonical/long decoding 1.8%, and palette
expansion 24.0%. Both guards pass. Target substantial remaining framing or palette
work rather than repeated buffer-size changes. This profile is not a parity claim.

The [numeric evidence](warm-pnginflate-buffer-evidence.json) retains all actuals,
brackets, source/input/raster hashes, four child identities, raw/QA/package/profile
archives and the unretained larger-buffer trial. No original performance image
was added. Other reported gaps, material unknowns and the full required uniform
warm target remain open.

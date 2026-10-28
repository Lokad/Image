# Closed VP8 coefficient trials

Both bounded trials use the existing opaque 513×259 RGBA16 method4 control and
preserve its exact 72,380-byte packet, alpha, source and independent ownership.
Quiet CPU0 guards and equivalent complete-operation boundaries pass.

| Trial | Quiet elapsed µs | Allocated B/op | Decision |
| --- | ---: | ---: | --- |
| CHB1 block-level output headroom, short | 21,108.985 | 1,201,552 | 4.16% below applicable prior confirmation; below the 5% screen gate |
| CT1 independent baseline | 20,677.753 | 1,201,552 | Fresh comparison, 12 actual iterations |
| CT1 retained coefficient tokens | 20,426.793 | 2,331,536 | Reject the storage tradeoff |

CT1 first screened at 20,692.171 µs. Independent confirmation finds only 1.214%
less filtered elapsed (1.342% across all actuals), with separate intervals in
both forms. Admitted frame capacity grows from 461,396 to 1,598,132 bytes.
The measured reduction does not justify the extra storage. CHB1 had no variants
or independent repeat; CT1 had no variants or companion timing expansion.

All prototype source and dedicated tests were removed. Matching PSC1 producers
and focused normal/scalar checks were restored on both platforms before PNG
compression work began. The actual package was unchanged. These rejected
producers are not current performance evidence; neither family remains an open
optimization lead without new evidence.

Candidate focused checks: CHB1 55 internal/46 public and CT1 56 internal/46 public
on each OS/mode. Restoration checks cover 54 internal tests on each OS/mode;
CHB1 also reran 46 public Windows checks in both modes. Full/package proofs for
unchanged accepted production were reused.

[Machine-readable results](vp8-coefficient-trials-evidence.json) identify producers
and costs. Mirrored/member-verified raw archives retain source patches, frozen
candidate/baseline producers, selectors, guards, receipts and restoration logs:

- `coefficient-headroom-rejected.tar.xz`, SHA256
  `7beedeb8c1cb922e46347c925ea289f7c747647cf0791571b29ccaf0e723195d`.
- `coefficient-token-retention-rejected.tar.xz`, SHA256
  `6b95cecb209e733e123e76eecc4cd312105708738e7c5b81581aa7d6db791779`.

The preceding six-second attribution and matching JIT maps explain why these
primitives were tested; sample shares are not elapsed-time claims. Their raw
archive is `current-vp8-opaque16-attribution.tar.xz`, SHA256
`6645f4baf975270619a3c803f7b4bac463d5e2a22d964f64caa4ca4c96c8f08f`.
No reference implementation source or tables were copied.

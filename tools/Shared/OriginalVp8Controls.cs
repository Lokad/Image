namespace Lokad.Image.Tooling;

// Original sources and already frozen complete libwebp1.6.0 packet identities.
// No new content/settings grid: see benchmarks/VP8-INPUT-STAGE.md and its ledger.
internal static class OriginalVp8Controls
{
    public static IEnumerable<object[]> Cases()
    {
        yield return ["opaque", 8, 4, 75, 72380, "39eb4095d1aaebc21fee2a92aa45180fb6adcd92bc5538589f914f53aa9ed88f"];
        yield return ["diagram", 8, 4, 75, 7100, "0b71843059a6c3224cec9df38a460ce6087ad21e1e0f499978f7767747a1a050"];
        yield return ["diagram", 8, 3, 75, 7082, "3bea8726b4473a4f1436a621229d12a489603537ebbd54945044241d3d478dda"];
        yield return ["photograph", 8, 4, 75, 17688, "e2df1f461cb04516aa8630f326efdc959e1ae8a7dce578ef3d064b20c788719b"];
        yield return ["diagram", 8, 0, 75, 9980, "1c454bdfaff968c856a479c9828f8396137556c0708354c85a5a9b37d2bbe5d5"];
        yield return ["diagram", 8, 1, 75, 9244, "50d50af60b76e99dcb1f49eda60efc548a564dbaaaa931d06fc0164aa28c0bea"];
        yield return ["diagram", 8, 2, 75, 7330, "325a9d5b55b300db2554681735642edcdc8dcb866a88760501d3fe95fac1423b"];
        yield return ["diagram", 8, 0, 100, 27668, "eb4cd33ce7a5b39698b49fd94b4dd59a7ec9c0a0d8d5ac0bf12b10d8a0afbdba"];
        yield return ["diagram", 8, 4, 1, 2304, "28bff919826e67d69d393f457d3920017ffff21cc3732e3db06d55a80f873f54"];
        yield return ["diagram", 8, 4, 85, 8898, "51a4abeda30c4f8c8b1dea563ab6690a89a5feed93b69c467398f9b736aeb536"];
        yield return ["diagram", 8, 4, 100, 15172, "6a4332bb32c3b93248a76eb804a0dc74d2c519d4955f310dcb94ef798cd2616e"];
        yield return ["diagram", 8, 5, 75, 7026, "0e994ebda4b10234f2e9f00caf7ea6c27ae03ce82c1f3cada8eda355430cba68"];
        yield return ["diagram", 8, 6, 75, 6818, "3c98407656841ff97b185e241e76f750f036c972cfe2a0b704ae2eb35cf0adee"];
        yield return ["asymmetric", 8, 4, 75, 98590, "8fcc1c935850d7813ebfce329a8b00c41fe9b02683d2be40b91dedfd74192a10"];
        yield return ["asymmetric", 8, 0, 75, 93386, "eb12853d220295a4327d151827462eda69df74bc9a21b938af5ca31f40cf134d"];
        yield return ["asymmetric", 8, 1, 75, 106588, "9491f97d6c703e0d8869c02eb4ba0edabcde831992f7173cae5ef5b16ab330ce"];
        yield return ["asymmetric", 8, 6, 75, 85796, "e58f431cec4412c38b3ee49da4ff62c25cc896fa71bcffe9f131dd08e1bccc96"];
        yield return ["asymmetric", 8, 2, 75, 102582, "e4d5cf3f3c83e2c4c882a6863f1eb3c003b3b5a4647c8169ef8a951148feeefe"];
        yield return ["asymmetric", 8, 3, 75, 102502, "5d3ccf6fbbec6308132e7832b757122ba05007706e7786ef8296901862659e93"];
        yield return ["asymmetric", 8, 5, 75, 98602, "2467239de7717bbaef94fdfb2fde2fe872b13c14a1a0e6010b9fc25842df797e"];
        yield return ["opaque", 16, 4, 75, 72380, "39eb4095d1aaebc21fee2a92aa45180fb6adcd92bc5538589f914f53aa9ed88f"];
        yield return ["asymmetric", 16, 4, 75, 98590, "8fcc1c935850d7813ebfce329a8b00c41fe9b02683d2be40b91dedfd74192a10"];
    }
}

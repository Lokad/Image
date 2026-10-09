using System.Security.Cryptography;

// Development-only exact raw sequence validation. Never a production build input.
internal static class RawFrameEvidence
{
    public static object Validate(string output, string reference, int frames, int frameBytes)
    {
        if (frames < 1 || frameBytes < 1) throw new ArgumentException("Raw evidence requires positive frame geometry/count.");
        using var actual = File.OpenRead(output);
        using var expected = File.OpenRead(reference);
        var length = checked((long)frames * frameBytes);
        if (actual.Length != length || expected.Length != length)
            throw new InvalidOperationException("Raw output/reference frame count or precision differs.");
        var actualBuffer = new byte[Math.Min(frameBytes, 65536)];
        var expectedBuffer = new byte[actualBuffer.Length];
        var receipts = new List<object>();
        using var outputHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        for (var frame = 0; frame < frames; frame++)
        {
            using var actualHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            using var expectedHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            for (var offset = 0; offset < frameBytes;)
            {
                var count = Math.Min(actualBuffer.Length, frameBytes - offset);
                actual.ReadExactly(actualBuffer.AsSpan(0, count)); expected.ReadExactly(expectedBuffer.AsSpan(0, count));
                if (!actualBuffer.AsSpan(0, count).SequenceEqual(expectedBuffer.AsSpan(0, count)))
                    throw new InvalidOperationException($"Raw frame {frame} differs from independent reference samples at byte offset {offset}.");
                actualHash.AppendData(actualBuffer.AsSpan(0, count)); expectedHash.AppendData(expectedBuffer.AsSpan(0, count));
                outputHash.AppendData(actualBuffer.AsSpan(0, count)); offset += count;
            }
            receipts.Add(new { frame, exact = true, allRgbaSamplesExact = true,
                actualSha256 = Convert.ToHexStringLower(actualHash.GetHashAndReset()),
                expectedSha256 = Convert.ToHexStringLower(expectedHash.GetHashAndReset()) });
        }
        if (actual.ReadByte() != -1 || expected.ReadByte() != -1)
            throw new InvalidOperationException("Raw output/reference contains trailing samples.");
        return new { outputBytes = length, outputSha256 = Convert.ToHexStringLower(outputHash.GetHashAndReset()), frameValidation = receipts };
    }

    public static void RequireOutput(string path, long bytes, string hash)
    {
        if (new FileInfo(path).Length != bytes || JpegBenchmarkCase.HashFile(path) != hash)
            throw new InvalidOperationException("Raw workload execution changed the validated output.");
    }

    internal static void ValidateCleanupControls(string name,
        Func<(Action Setup, Action Cleanup, Func<string> Directory, Func<string> NativeOutput, Func<string> ManagedOutput)> factory)
    {
        var controls = 0;
        foreach (var managed in new[] { false, true })
        foreach (var append in new[] { false, true })
        {
            var workload = factory();
            try
            {
                workload.Setup();
                var directory = workload.Directory(); var output = managed ? workload.ManagedOutput() : workload.NativeOutput();
                using (var file = File.Open(output, FileMode.Open, FileAccess.ReadWrite))
                {
                    if (append) { file.Position = file.Length; file.WriteByte(1); }
                    else { var first = file.ReadByte(); file.Position = 0; file.WriteByte((byte)(first ^ 1)); }
                }
                var rejected = false;
                try { workload.Cleanup(); }
                catch (InvalidOperationException) { rejected = true; }
                if (!rejected || System.IO.Directory.Exists(directory))
                    throw new InvalidOperationException("Raw cleanup failed to reject changed output and remove its owned directory.");
                controls++;
            }
            finally { workload.Cleanup(); }
        }
        Console.WriteLine($"All {controls} {name} cleanup controls rejected changed outputs and removed owned temporary directories.");
    }
}

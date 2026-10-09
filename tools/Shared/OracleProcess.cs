using System.Diagnostics;

namespace Lokad.Image.Tooling;

internal static class OracleProcess
{
    public static string Ffmpeg => Environment.GetEnvironmentVariable("LOKAD_IMAGE_FFMPEG") ?? "ffmpeg";

    public static byte[] Run(string executable, IEnumerable<string> arguments, byte[]? input = null,
        int maximumOutput = 128 * 1024 * 1024, TimeSpan? timeout = null)
    {
        using var cancellation = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(60));
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(executable)
            {
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true
            }
        };
        foreach (var argument in arguments) process.StartInfo.ArgumentList.Add(argument);
        try
        {
            process.Start();
            var errorTask = process.StandardError.ReadToEndAsync(cancellation.Token);
            var outputTask = ReadOutput(process.StandardOutput.BaseStream, maximumOutput, cancellation.Token);
            var inputTask = WriteInput(process.StandardInput.BaseStream, input, cancellation.Token);
            Task.WhenAll(inputTask, outputTask, errorTask, process.WaitForExitAsync(cancellation.Token)).GetAwaiter().GetResult();
            if (process.ExitCode != 0) throw new InvalidOperationException($"{Path.GetFileName(executable)} failed: {errorTask.Result}");
            return outputTask.Result;
        }
        finally
        {
            try { if (!process.HasExited) process.Kill(true); }
            catch (InvalidOperationException) { }
        }
    }

    private static async Task WriteInput(Stream stream, byte[]? input, CancellationToken cancellation)
    {
        using (stream)
            if (input is not null) await stream.WriteAsync(input, cancellation);
    }

    private static async Task<byte[]> ReadOutput(Stream stream, int maximum, CancellationToken cancellation)
    {
        using var output = new MemoryStream();
        var buffer = new byte[8192];
        while (true)
        {
            var read = await stream.ReadAsync(buffer, cancellation);
            if (read == 0) break;
            if (read > maximum - output.Length) throw new InvalidOperationException("Tool output exceeded its limit.");
            output.Write(buffer, 0, read);
        }
        return output.ToArray();
    }
}

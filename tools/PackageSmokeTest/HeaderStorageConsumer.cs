namespace Lokad.Image.Tooling;

internal static class HeaderStorageConsumer
{
    internal static async Task Verify()
    {
        var png = AuthoredPng.Build(new("packaged-header-storage", 6, 8, 33, 17, false));
        var expected = ImageCodec.Identify(png);
        using var small = new TestMemoryBudget(512);
        var smallLimits = new ImageLimits { MemoryBudget = small, MaxWorkingBytes = 512 };
        using (var input = new MemoryStream(png, writable: false))
        {
            var info = await ImageCodec.InspectHeadersAsync(input, smallLimits);
            if (info != expected || input.Position != input.Length || !input.CanRead || small.Bytes != 0 || small.Peak != 512)
                throw new Exception("Packaged small header storage or independent metadata differs.");
        }
        using (var input = new FragmentedInput(png))
        {
            var info = await ImageCodec.ProbeAsync(input, new() { Limits = smallLimits });
            if (info.Format != expected.Format || info.Width != expected.Width || info.Height != expected.Height
                || info.BytesRead != input.Position || input.Position >= input.Length || info.Orientation != null
                || !input.CanRead || small.Bytes != 0 || small.Peak != 512)
                throw new Exception("Packaged dimension probe changed partial fields, prefix scope or ownership.");
        }
        byte[] prefix = [255,216,255,192,0,11,8,0,11,0,17,1,1,0x11,0,255,218,0,8,1,1,0,0,63,0];
        var tiny = new byte[prefix.Length + 2]; prefix.CopyTo(tiny, 0); tiny[^2] = 255; tiny[^1] = 217;
        using (var input = new MemoryStream(tiny, writable: false))
            if (ImageCodec.InspectHeaders(input, smallLimits).Format != ImageFormat.Jpeg || small.Bytes != 0 || small.Refusals != 0)
                throw new Exception("Packaged tiny JPEG allocated bulk storage for EOF.");
        // Authored structural-only scan; InspectHeaders does not validate compressed pixels.
        var jpeg = new byte[prefix.Length + 9000 + 2]; prefix.CopyTo(jpeg, 0); jpeg[^2] = 255; jpeg[^1] = 217;
        using (var refused = new MemoryStream(jpeg, writable: false))
        {
            try { ImageCodec.InspectHeaders(refused, new() { MemoryBudget = small }); throw new Exception("Packaged bulk refusal ignored."); }
            catch (ImageLimitException e) when (e.Scope == ImageLimitScope.SharedBudget && e.Limit == ImageLimitKind.WorkingBytes) { }
            if (refused.Position != 256 || small.Bytes != 0 || small.Refusals != 1 || !refused.CanRead)
                throw new Exception("Packaged bulk refusal retried, read ahead or leaked storage.");
        }
        using (var subsequent = new MemoryStream(png, writable: false))
            if (ImageCodec.InspectHeaders(subsequent, smallLimits) != expected || small.IsDisposed || small.Bytes != 0)
                throw new Exception("Packaged borrowed header budget is not reusable.");
        using var root = new TestMemoryBudget(8448);
        using (var input = new MemoryStream(jpeg, writable: false))
        {
            var info = ImageCodec.InspectHeaders(input, new() { MemoryBudget = root, MaxWorkingBytes = 8448 });
            if (info.Format != ImageFormat.Jpeg || info.Width != 17 || info.Height != 11 || input.Position != input.Length
                || root.Bytes != 0 || root.Peak != 8448 || !input.CanRead)
                throw new Exception("Packaged bulk replacement exceeds its original ceiling or changes metadata.");
        }
        foreach (var (bulk, dimensions) in new[] { (false, false), (true, false), (false, true) })
        {
            using var cancellation = new CancellationTokenSource();
            using var input = new HeldInput(bulk ? jpeg : png, root, bulk, dimensions);
            Task pending = dimensions
                ? ImageCodec.ProbeAsync(input, new() { Limits = new() { MemoryBudget = root } }, cancellation.Token).AsTask()
                : ImageCodec.InspectHeadersAsync(input, new() { MemoryBudget = root }, cancellation.Token).AsTask();
            await input.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            try
            {
                cancellation.Cancel();
                if (pending.IsCompleted || root.Bytes != (bulk ? 8448 : 512))
                    throw new Exception("Packaged pending header read released storage before its actual user finished.");
            }
            finally { input.Resume.TrySetResult(); }
            try { await pending; throw new Exception("Packaged header cancellation ignored."); }
            catch (OperationCanceledException e) when (e.CancellationToken == cancellation.Token) { }
            if (!input.SawLiveCharge || root.Bytes != 0 || root.IsDisposed || !input.CanRead)
                throw new Exception("Packaged async header ownership or cancellation cleanup differs.");
        }
        Console.WriteLine("Packaged dimension probes preserve partial fields and pending user ownership.");
        Console.WriteLine("Packaged seekable headers preserve small storage, bounded bulk replacement, terminal refusal, cancellation and actual async lifetimes.");
    }

    private sealed class FragmentedInput(byte[] data) : MemoryStream(data, writable: false)
    {
        public override int Read(Span<byte> buffer) => base.Read(buffer[..Math.Min(1, buffer.Length)]);
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(Read(buffer.Span));
    }

    private sealed class HeldInput(byte[] data, TestMemoryBudget root, bool bulk, bool afterFragment) : MemoryStream(data, writable: false)
    {
        internal TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource Resume { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal bool SawLiveCharge;
        private bool held;
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (afterFragment && Position == 0) return Read(buffer.Span[..1]);
            if (!held && (bulk ? buffer.Length > 256 : Position == (afterFragment ? 1 : 0)))
            {
                held = true; Entered.SetResult(); await Resume.Task.ConfigureAwait(false);
                SawLiveCharge = root.Bytes >= buffer.Length;
            }
            return Read(buffer.Span);
        }
    }
}

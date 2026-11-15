namespace Lokad.Image.PublicApi.Tests;

public sealed class InspectionOwnerTests
{
    [Fact]
    public async Task CombinedReaderOwnershipKeepsPublicValidationPrecedence()
    {
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        var invalidLimits = new ImageLimits { MaxDimension = 0 };
        var invalidPolicy = (ImageMetadataPolicy)99;
        using var input = new MemoryStream([], writable: false);
        foreach (var asynchronous in new[] { false, true })
        {
            async Task Headers(Stream? source, ImageLimits limits, ImageMetadataPolicy policy, CancellationToken token)
            {
                if (asynchronous) await ImageCodec.InspectHeadersAsync(source!, limits, token, policy);
                else ImageCodec.InspectHeaders(source!, limits, token, policy);
            }
            async Task Probe(Stream? source, ImageProbeOptions options, CancellationToken token)
            {
                if (asynchronous) await ImageCodec.ProbeAsync(source!, options, token);
                else ImageCodec.Probe(source!, options, token);
            }
            var policyError = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                Headers(null, invalidLimits, invalidPolicy, canceled.Token));
            Assert.Equal("metadataPolicy", policyError.ParamName);
            var limitsError = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                Headers(null, invalidLimits, ImageMetadataPolicy.Strict, canceled.Token));
            Assert.Equal(nameof(ImageLimits), limitsError.ParamName);
            var cancellation = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                Headers(null, ImageLimits.Default, ImageMetadataPolicy.Strict, canceled.Token));
            Assert.Equal(canceled.Token, cancellation.CancellationToken);
            Assert.Equal("input", (await Assert.ThrowsAsync<ArgumentNullException>(() =>
                Headers(null, ImageLimits.Default, ImageMetadataPolicy.Strict, default))).ParamName);

            var options = new ImageProbeOptions { Facts = 0, Limits = invalidLimits, MetadataPolicy = invalidPolicy };
            Assert.Equal("input", (await Assert.ThrowsAsync<ArgumentNullException>(() =>
                Probe(null, options, canceled.Token))).ParamName);
            Assert.Equal("options", (await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                Probe(input, options, canceled.Token))).ParamName);
            options = options with { Facts = ImageProbeFacts.Dimensions };
            Assert.Equal("metadataPolicy", (await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                Probe(input, options, canceled.Token))).ParamName);
            options = options with { MetadataPolicy = ImageMetadataPolicy.Strict };
            Assert.Equal(nameof(ImageLimits), (await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                Probe(input, options, canceled.Token))).ParamName);
            options = options with { Limits = ImageLimits.Default };
            cancellation = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                Probe(input, options, canceled.Token));
            Assert.Equal(canceled.Token, cancellation.CancellationToken);
            Assert.Equal(0, input.Position);
            Assert.True(input.CanRead);
        }
    }
}

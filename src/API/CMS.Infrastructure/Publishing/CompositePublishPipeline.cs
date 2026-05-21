using CMS.Domain.Contents;

namespace CMS.Infrastructure.Publishing;

/// <summary>
/// Kayıtlı tüm pipeline handler'larını sırayla çalıştırır.
/// DI'a IPublishPipelineHandler[] inject edilir.
/// </summary>
public sealed class CompositePublishPipeline(
    IEnumerable<IPublishPipelineHandler> handlers) : IPublishPipeline
{
    public async Task OnPublishedAsync(Content content, CancellationToken ct = default)
    {
        foreach (var h in handlers)
            await h.OnPublishedAsync(content, ct);
    }

    public async Task OnUnpublishedAsync(Content content, CancellationToken ct = default)
    {
        foreach (var h in handlers)
            await h.OnUnpublishedAsync(content, ct);
    }
}

public interface IPublishPipelineHandler
{
    Task OnPublishedAsync(Content content, CancellationToken ct);
    Task OnUnpublishedAsync(Content content, CancellationToken ct);
}

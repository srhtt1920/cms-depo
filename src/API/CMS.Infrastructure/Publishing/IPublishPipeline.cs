using CMS.Domain.Contents;

namespace CMS.Infrastructure.Publishing;

/// <summary>
/// İçerik yayın pipeline'ı. Dinamik ve Statik handler'lar buraya bağlanır.
/// ContentPublishedEvent handler'ından tetiklenir.
/// </summary>
public interface IPublishPipeline
{
    Task OnPublishedAsync(Content content, CancellationToken ct = default);
    Task OnUnpublishedAsync(Content content, CancellationToken ct = default);
}

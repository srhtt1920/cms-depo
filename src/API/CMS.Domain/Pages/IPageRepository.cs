using CMS.Domain.Pages.Dtos;
using CMS.Domain.Tenants;

namespace CMS.Domain.Pages;

public interface IPageRepository
{
    Task<Page?> GetByIdAsync(PageId id, CancellationToken ct = default);

    Task<bool> SlugExistsAsync(
        string slug, string languageCode, Guid tenantId,
        PageId? excludePageId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Tüm tree — yalnızca yapısal alanlar (Id, ParentId, Order, tip, görünürlük).
    /// Admin sol menüsü / drag-drop için hafif endpoint.
    /// </summary>
    Task<IReadOnlyList<PageFlatDto>> GetFlatAsync(TenantId tenantId, CancellationToken ct = default);

    /// <summary>
    /// Tüm tree — çeviriler dahil tam veri.
    /// Admin düzenleme paneli için.
    /// </summary>
    Task<IReadOnlyList<PageFlatDetailDto>> GetFlatDetailAsync(
        TenantId tenantId, CancellationToken ct = default);

    /// <summary>Belirli node'un root'a kadar breadcrumb path'i.</summary>
    Task<IReadOnlyList<PageBreadcrumbItemDto>> GetBreadcrumbAsync(
        PageId id, string languageCode, CancellationToken ct = default);

    /// <summary>Döngüsel referans kontrolü için bir node'un tüm torunları.</summary>
    Task<IReadOnlyList<PageId>> GetDescendantIdsAsync(PageId id, CancellationToken ct = default);

    /// <summary>
    /// Dashboard sayfa sayısı için node listesi döner (GetFlatAsync'in alias'ı).
    /// </summary>
    Task<IReadOnlyList<PageFlatDto>> GetTreeAsync(
        TenantId tenantId, bool includeInactive = false, CancellationToken ct = default);

    Task<Page> AddAsync(Page page, CancellationToken ct = default);
    Task<Page> UpdateAsync(Page page, CancellationToken ct = default);
}
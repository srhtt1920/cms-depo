using CMS.Domain.Tenants;

namespace CMS.Application.Common.Abstractions;

public interface ITenantContext
{
    Guid TenantId { get; }
    string TenantName { get; }
    TenantType TenantType { get; }
    bool IsSystem { get; }
    bool IsResolved { get; }
    void Set(Guid tenantId, string tenantName, TenantType tenantType = TenantType.Business);
}

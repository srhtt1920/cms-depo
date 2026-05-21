using CMS.Application.Common.Abstractions;
using CMS.Domain.Tenants;

namespace CMS.Infrastructure.Identity;

public sealed class TenantContext : ITenantContext
{
    public Guid TenantId { get; private set; }
    public string TenantName { get; private set; } = string.Empty;
    public TenantType TenantType { get; private set; } = TenantType.Business;
    public bool IsSystem => TenantType == TenantType.System;
    public bool IsResolved { get; private set; }

    public void Set(Guid tenantId, string tenantName,
        TenantType tenantType = TenantType.Business)
    {
        TenantId = tenantId;
        TenantName = tenantName;
        TenantType = tenantType;
        IsResolved = true;
    }
}

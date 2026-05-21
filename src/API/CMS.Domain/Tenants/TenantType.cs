namespace CMS.Domain.Tenants;

public enum TenantType
{
    System = 1,  // Platform yönetim context'i — silinemez, rename edilemez
    Business = 2   // Normal müşteri workspace'i
}

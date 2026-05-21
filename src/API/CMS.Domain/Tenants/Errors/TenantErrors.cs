using CMS.SharedKernel.Result;

namespace CMS.Domain.Tenants.Errors;

public static class TenantErrors
{
    public static Error NameAlreadyExists(string name) =>
         Error.Conflict("Tenant.NameAlreadyExists", $"'{name}' adında bir tenant zaten mevcut.");
    
    public static Error NotFound(Guid id) =>
        Error.NotFound("Tenant.NotFound", $"Tenant '{id}' bulunamadı.");

    // Root guard hataları
    public static readonly Error SystemCannotBeDeleted =
        Error.Forbidden("Tenant.System.Delete", "System tenant silinemez.");

    public static readonly Error SystemCannotBeRenamed =
        Error.Forbidden("Tenant.System.Rename", "System tenant yeniden adlandırılamaz.");

    public static readonly Error SystemCannotBeDeactivated =
        Error.Forbidden("Tenant.System.Deactivate", "System tenant devre dışı bırakılamaz.");

    public static readonly Error SystemCannotAssignNormalUser =
        Error.Forbidden("Tenant.System.NormalUser", "System tenant'a normal kullanıcı atanamaz.");

    public static readonly Error RSystemNotFound =
        Error.NotFound("Tenant.System.Missing", "System tenant bulunamadı.");
}

namespace CMS.Domain.Tenants;

public enum TenantStatus
{
    Draft           = 0,        // Oluşturuldu, provisioning başlamadı
    Provisioning    = 1,        // Default data oluşturuluyor (async)
    Active          = 2,        // Kullanılabilir
    Suspended       = 3,        // Geçici askıya alındı (erişim yok, data silinmez)
    Archived        = 4         // Kalıcı kapatıldı (readonly, silinmeye hazır)
}

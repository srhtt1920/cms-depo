namespace CMS.Domain.Pages;

/// <summary>
/// Yalnızca PageType.Normal için geçerlidir.
/// Category sayfaları için bu alan null olmalıdır.
/// </summary>
public enum PageContentType
{
    Announcement    = 1,  // Duyuru
    Blog            = 2,  // Blog yazısı
    Homepage        = 3,  // Anasayfa (sadece bir tane olabilir)
    Car             = 4,  // Araç ilanı
    Contact         = 5,  // İletişim
    Gallery         = 6,  // Galeri
    Product         = 7,  // Ürün
    Service         = 8,  // Hizmet
    Custom          = 99, // Özel/serbest sayfa
}

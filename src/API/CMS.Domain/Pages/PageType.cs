namespace CMS.Domain.Pages;

public enum PageType
{
    Default     = 1,  // Normal sayfa — Sabit Sayfa, Duyuru, Blog, Anasayfa vb. ContentType dolu olmalıdır
    Custom      = 2,  // Özel sayfa — ContentType null, ExternalUrl dolu olabilir
    Category    = 3,  // Kategori — alt sayfaları gruplar, ContentType null
}

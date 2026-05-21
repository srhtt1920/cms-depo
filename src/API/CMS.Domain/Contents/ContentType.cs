namespace CMS.Domain.Contents;

public enum ContentType
{
    Page            = 1,    // Statik sayfa
    HomePage        = 2,    // Anasayfa sayfa
    Announcement    = 3,    // Duyuru
    Blog            = 4,    // Blog yazısı — PublishDate, ReadTime, Excerpt
    Faq             = 5,    // SSS
    Email           = 6,    // E-posta şablonu
    Contact         = 7,    // İletişim sayfası
    Gallery         = 8,    // Galeri
    Card            = 9,    // Kart içeriği (örneğin ürün veya hizmet tanıtımı)
    Slider          = 10,   // Slider içeriği
    Footer          = 11,   // Alt bilgi içeriği
    Custom          = 99,   // Özel/serbest içerik
}


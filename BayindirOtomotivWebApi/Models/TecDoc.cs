using System.Collections.Generic;

namespace BayindirOtomotivWebApi.Models
{
    // Eğer API yanıtı bir dizi ise, bu sınıfı List<TecDocArticleOEMDto> şeklinde deserialize edebilirsiniz.
    // Eğer yanıt { "articles": [ ... ] } şeklinde dönüyorsa, aşağıdaki yapı uygundur.
    public class TecDocArticleOEMSearchResponse
    {
        public List<TecDocArticleOEMDto> articles { get; set; }
    }

    public class TecDocArticleOEMDto
    {
        public int articleId { get; set; }
        public string articleSearchNo { get; set; }
        public string articleNo { get; set; }
        public string articleProductName { get; set; }
        public int manufacturerId { get; set; }
        public string manufacturerName { get; set; }
        public int supplierId { get; set; }
        public string supplierName { get; set; }
        public int articleMediaType { get; set; }
        public string articleMediaFileName { get; set; }
        public string imageLink { get; set; }
        public string imageMedia { get; set; }
        public string s3ImageLink { get; set; }
    }
}

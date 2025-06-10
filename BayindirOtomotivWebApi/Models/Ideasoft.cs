namespace BayindirOtomotivWebApi.Models
{
    public class IdeaSoftResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

    // Örnek product DTO
    public class IdeaSoftProductDto
    {
        public string name { get; set; }
        public string sku { get; set; }
        public string barcode { get; set; }
        public double stockAmount { get; set; }
        public double price1 { get; set; }
        public int taxIncluded { get; set; } // 0 => KDV dahil değil
        public int tax { get; set; }         // 18 => %18 KDV
        public int status { get; set; }      // 1 => aktif
        public string metaDescription { get; set; }
        public string pageTitle { get; set; }
        public string shortDetails { get; set; }
        public string searchKeywords { get; set; }

        public IdeaSoftDetailDto detail { get; set; }
        public List<IdeaSoftCategoryDto> categories { get; set; }
        public IdeaSoftCurrencyDto currency { get; set; }
        public List<IdeaSoftImageDto> images { get; set; }
        public IdeaSoftBrandDto brand { get; set; }
    }

    public class IdeaSoftCategoryDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public int sortOrder { get; set; }
        public object showcaseSortOrder { get; set; } // null
        public string pageTitle { get; set; }
        public string metaDescription { get; set; }
        public string metaKeywords { get; set; }
        public string canonicalUrl { get; set; }
        public string tree { get; set; }
        public string imageUrl { get; set; }
    }

    public class IdeaSoftCurrencyDto
    {
        public int id { get; set; } // 3 => TL (örnek)
        public string label { get; set; }
        public string abbr { get; set; }
    }

    public class IdeaSoftImageDto
    {
        public string filename { get; set; }
        public string extension { get; set; }
        public int sortOrder { get; set; }
        public string attachment { get; set; } // data:image/jpeg;base64,....
    }

    public class IdeaSoftBrandDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public int status { get; set; }
        public int sortIrder { get; set; }
        public int displayShowcaseContent { get; set; }
        public int displayShowcaseFooterContent { get; set; }
    }

    public class IdeaSoftDetailDto
    {
        public int id { get; set; }
        public string sku { get; set; }
        public string details { get; set; }
        public string extraDetails { get; set; }
        public List<IdeaSoftProductDto> product { get; set; }
    }
}

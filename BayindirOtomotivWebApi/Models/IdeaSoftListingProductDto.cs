using System;
using System.Collections.Generic;

namespace BayindirOtomotivWebApi.Models
{
    /// <summary>
    /// IdeaSoft'tan /admin-api/products veya benzeri endpoint'te
    /// tek bir ürün objesini temsil eder. JSON dizisi [ {...}, {...} ] şeklinde olduğundan
    /// deserialize işlemi List<IdeaSoftListingProductDto> olarak yapılır.
    /// </summary>
    public class IdeaSoftListingProductDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public string fullName { get; set; }
        public string slug { get; set; }
        public string sku { get; set; }
        public string barcode { get; set; }
        public double stockAmount { get; set; }
        public double price1 { get; set; }

        public CurrencyDto currency { get; set; }

        public double discount { get; set; }
        public int discountType { get; set; }
        public double moneyOrderDiscount { get; set; }
        public double buyingPrice { get; set; }
        public object marketPriceDetail { get; set; }

        public int taxIncluded { get; set; }
        public int tax { get; set; }
        public int warranty { get; set; }
        public double volumetricWeight { get; set; }
        public string stockTypeLabel { get; set; }
        public int customShippingDisabled { get; set; }
        public double customShippingCost { get; set; }
        public object distributor { get; set; }
        public int hasGift { get; set; }
        public object gift { get; set; }
        public int status { get; set; }
        public int hasOption { get; set; }
        public string shortDetails { get; set; }
        public string installmentThreshold { get; set; }

        public object homeSortOrder { get; set; }
        public object popularSortOrder { get; set; }
        public object brandSortOrder { get; set; }
        public object featuredSortOrder { get; set; }
        public object campaignedSortOrder { get; set; }
        public object newSortOrder { get; set; }
        public object discountedSortOrder { get; set; }

        public int categoryShowcaseStatus { get; set; }
        public object midblockSortOrder { get; set; }

        public string pageTitle { get; set; }
        public string metaDescription { get; set; }
        public string metaKeywords { get; set; }
        public object canonicalUrl { get; set; }

        // Brand, Detail gibi alt nesneler
        public BrandDto brand { get; set; }
        public DetailDto detail { get; set; }

        // Dizi şeklinde gelen alanlar
        public List<CategoryDto> categories { get; set; }
        public List<PriceDto> prices { get; set; }
        public List<ImageDto> images { get; set; }
        public List<object> optionGroups { get; set; }

        public DateTime updatedAt { get; set; }
        public DateTime createdAt { get; set; }
    }

    public class CurrencyDto
    {
        public int id { get; set; }
        public string label { get; set; }
        public string abbr { get; set; }
    }

    public class BrandDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public string pageTitle { get; set; }
        public string metaDescription { get; set; }
        public string metaKeywords { get; set; }
        public string canonicalUrl { get; set; }
        public string imageUrl { get; set; }
    }

    public class DetailDto
    {
        public int id { get; set; }
        public string details { get; set; }
        public string extraDetails { get; set; }
    }

    public class CategoryDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public int sortOrder { get; set; }
        public object showcaseSortOrder { get; set; }
        public string pageTitle { get; set; }
        public string metaDescription { get; set; }
        public string metaKeywords { get; set; }
        public string canonicalUrl { get; set; }
        public string tree { get; set; }
        public string imageUrl { get; set; }
    }

    public class PriceDto
    {
        public int id { get; set; }
        public int type { get; set; }
        public double value { get; set; }
    }

    public class ImageDto
    {
        public int id { get; set; }
        public string filename { get; set; }
        public string extension { get; set; }
        public string thumbUrl { get; set; }
        public string originalUrl { get; set; }
    }
}

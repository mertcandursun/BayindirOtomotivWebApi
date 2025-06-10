namespace BayindirOtomotivWebApi.Models
{
    public class IdeaSoftListingCategoryDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public int sortOrder { get; set; }
        public int status { get; set; }
        public int displayShowcaseContent { get; set; }
        public int showcaseContentDisplayType { get; set; }
        public int displayShowcaseFooterContent { get; set; }
        public int showcaseFooterContentDisplayType { get; set; }
        public int hasChildren { get; set; }
        public int isCombine { get; set; }
        public object showcaseSortOrder { get; set; } // null
        public string pageTitle { get; set; }
        public string metaDescription { get; set; }
        public string metaKeywords { get; set; }
        public string canonicalUrl { get; set; }
        public string tree { get; set; }
        public string imageUrl { get; set; }
    }

    public class IdeaSoftCategoryListResponse
    {
        public List<IdeaSoftListingCategoryDto> items { get; set; } = new();
        public int total { get; set; }
        public int page { get; set; }
        public int limit { get; set; }
    }
}

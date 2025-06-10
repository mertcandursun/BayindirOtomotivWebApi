namespace BayindirOtomotivWebApi.Models.Response
{
    public class IdeaSoftCategoryListResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        // Listeyi tutan property
        public List<IdeaSoftListingCategoryDto> Categories { get; set; }
    }
}

using System.Collections.Generic;

namespace BayindirOtomotivWebApi.Models
{
    /// <summary>
    /// Servis metodundan döneceğiniz sarmalayıcı;
    /// IsSuccess, Message gibi bilgileri ekler
    /// ve asıl ürün listesini "Products" içinde tutar.
    /// </summary>
    public class IdeaSoftProductListResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        // Listeyi tutan property
        public List<IdeaSoftListingProductDto> Products { get; set; }
    }
}

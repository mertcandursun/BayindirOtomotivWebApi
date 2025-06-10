using BayindirOtomotivWebApi.Models;

namespace BayindirOtomotivWebApi.Helpers
{
    public static class GlobalCategoryCache
    {
        /// <summary>İdeaSoft → “KATEGORİ ADI” ⟶ id</summary>
        public static IReadOnlyDictionary<string, int> Dict { get; private set; } =
            new Dictionary<string, int>(StringComparer.Ordinal);

        /// <summary>Uygulama açılırken tek sefer çağır.</summary>
        public static void Init(IEnumerable<IdeaSoftListingCategoryDto> cats)
        {
            Dict = cats
                   .GroupBy(c => c.name.Trim().ToUpperInvariant())
                   .ToDictionary(g => g.Key, g => g.First().id,
                                 StringComparer.Ordinal);
        }
    }
}

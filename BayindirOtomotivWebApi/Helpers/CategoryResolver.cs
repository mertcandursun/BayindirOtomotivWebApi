using BayindirOtomotivWebApi.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

public static class CategoryResolver
{
    /* ─────────────────────────────────────────────────────────────────────────────
       1)  IdeaSoft’tan çekilen kategori listesini (Product-List/ideasoft-categories.json)
           belleğe sözlük olarak yükler.
           - Aynı “Name”e sahip birden çok satır varsa **ilk** görüleni saklarız
             (Duplicate key hatasını engeller).                                             */
    private static readonly Lazy<Dictionary<string, int>> _modelMap = new(() =>
    {
        var path = Path.Combine(AppContext.BaseDirectory,
                                "Product-List", "ideasoft-categories.json");

        if (!File.Exists(path))
            throw new FileNotFoundException($"Category file missing: {path}");

        var json = File.ReadAllText(path);
        var cats = JsonSerializer.Deserialize<List<IdeaSoftCategoryDto>>(json,
                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

        var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var c in cats.Where(c => !string.IsNullOrWhiteSpace(c.name)))
        {
            var key = c.name.Trim().ToUpperInvariant();

            // Daha önce eklenmişse atla (ilk kayıt geçerli kalsın)
            if (!dict.ContainsKey(key))
                dict[key] = c.id;
        }

        return dict;
    });

    /* ─────────────────────────────────────────────────────────────────────────────
       2)  “m” alanını parse edip ilgili kategori Id’lerini döndürür               */
    public static List<int> ResolveIds(string? mField)
    {
        if (string.IsNullOrWhiteSpace(mField))
            return new();

        /* ----------  a) normalize  ------------------------------------------------ */
        //  '/'  '-'  '.'  '_'  '|'  →  boşluk
        var cleaned = Regex.Replace(mField.ToUpperInvariant(),
                                    @"[\/\-\|\._]+", " ");

        //  ardışık boşluklar → tek boşluk
        cleaned = Regex.Replace(cleaned, @"\s{2,}", " ").Trim();

        var tokens = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        /* ----------  b) token’lardan model listesi  ------------------------------ *
         *  -  WORD + LETTER  → “ASTRA H”, “ZAFIRA B”  (tek harf “model yılı” değil) *
         *  -  Tek kelime MODEL → “CRUZE”, “CAPTIVA”, “208”…                         */
        var models = new List<string>();

        for (int i = 0; i < tokens.Length; i++)
        {
            string cur = tokens[i];

            // WORD + 1-harf (“ASTRA” + “H”)
            if (cur.Length > 1 &&
                i + 1 < tokens.Length &&
                tokens[i + 1].Length == 1)
            {
                models.Add($"{cur} {tokens[i + 1]}");
                i++;                       // harfi tükettik, sonraki tokene atla
                continue;
            }

            // Tek kelime model
            if (cur.Length > 1)
                models.Add(cur);

            // 1 harflik standalone tokenları IGNORE ediyoruz
        }

        /* ----------  c) sözlük eşleşmesi  ---------------------------------------- */
        var ids = new HashSet<int>();

        foreach (var model in models.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (_modelMap.Value.TryGetValue(model, out int id))
                ids.Add(id);

            // eş-anlamlı / özel durum
            if (model is "AVEO T300" or "AVEO T")
                if (_modelMap.Value.TryGetValue("YENİ AVEO", out int altId))
                    ids.Add(altId);
        }

        return ids.ToList();
    }
}

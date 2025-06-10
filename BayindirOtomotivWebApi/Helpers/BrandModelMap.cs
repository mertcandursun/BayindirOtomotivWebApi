namespace BayindirOtomotivWebApi.Helpers;

public static class BrandModelMap
{
    // key = MODEL ADI (büyük harf, boşluklu), value = MARKA ADI
    public static readonly Dictionary<string, string> Dict =
        new(StringComparer.Ordinal)
        {
            /* ── OPEL ───────────────────────── */
            { "ASTRA G" , "OPEL" }, { "ASTRA H" , "OPEL" }, { "ASTRA J" , "OPEL" },
            { "ASTRA K" , "OPEL" }, { "ASTRA L" , "OPEL" },
            { "CORSA B" , "OPEL" }, { "CORSA C" , "OPEL" }, { "CORSA D" , "OPEL" },
            { "CORSA E" , "OPEL" }, { "CORSA F" , "OPEL" },
            { "CROSSLAND", "OPEL" }, { "GRANDLAND", "OPEL" },
            { "INSIGNIA A", "OPEL" }, { "INSIGNIA B", "OPEL" },
            { "MERIVA A" , "OPEL" }, { "MERIVA B" , "OPEL" },
            { "MOKKA"    , "OPEL" },
            { "VECTRA A" , "OPEL" }, { "VECTRA B" , "OPEL" }, { "VECTRA C" , "OPEL" },
            { "ZAFIRA A"   , "OPEL" }, { "ZAFIRA B", "OPEL" }, { "ZAFIRA C", "OPEL" },

            /* ── CHEVROLET ──────────────────── */
            { "AVEO"      , "CHEVROLET" },
            { "AVEO T"    , "CHEVROLET" },   // alias
            { "AVEO T300" , "CHEVROLET" },
            { "CAPTIVA"   , "CHEVROLET" },
            { "CRUZE"     , "CHEVROLET" },
            { "KALOS"     , "CHEVROLET" },
            { "LACETTI"   , "CHEVROLET" },
            { "REZZO"     , "CHEVROLET" },
            { "SPARK"     , "CHEVROLET" },

            /* ── PEUGEOT ────────────── */
            { "106", "PEUGEOT" }, { "107", "PEUGEOT" }, { "108", "PEUGEOT" },
            { "2008","PEUGEOT" }, { "206", "PEUGEOT" }, { "207", "PEUGEOT" }, { "208", "PEUGEOT" },
            { "3008", "PEUGEOT" }, { "301", "PEUGEOT" }, { "306", "PEUGEOT" },
            { "307", "PEUGEOT" }, { "308", "PEUGEOT" }, { "308 - 2022>", "PEUGEOT" },
            { "406", "PEUGEOT" }, { "407", "PEUGEOT" }, { "408", "PEUGEOT" },
            { "5008", "PEUGEOT" }, { "508", "PEUGEOT" }, { "BIPPER", "PEUGEOT" },
            { "PARTNER", "PEUGEOT" }, { "PARTNER TEPEE", "PEUGEOT" }, { "RIFTER", "PEUGEOT" },

            /* ── Citroen ────────────── */
            { "BERLINGO 2003-",  "CITROEN" },
            { "BERLINGO 2009-",  "CITROEN" },
            { "BERLINGO 2019-",  "CITROEN" },
            { "C-ELYSEE",        "CITROEN" },
            { "C1",              "CITROEN" },
            { "C2",              "CITROEN" },
            { "C3 2009-2015",    "CITROEN" },
            { "C3 2016-2020",    "CITROEN" },
            { "C3 AIRCROSS",     "CITROEN" },
            { "C3 PICASSO",      "CITROEN" },
            { "C4 2005-2010",    "CITROEN" },
            { "C4 2011-2017",    "CITROEN" },
            { "C4 2018-2025",    "CITROEN" },
            { "C4 CACTUS",       "CITROEN" },
            { "C4 GRAND",        "CITROEN" },
            { "C4 PICASSO",      "CITROEN" },
            { "C5",              "CITROEN" },
            { "C5 AIRCROSS",     "CITROEN" },
            { "NEMO 2008-",      "CITROEN" },
            { "SAXO 1997-",      "CITROEN" }
        };
}

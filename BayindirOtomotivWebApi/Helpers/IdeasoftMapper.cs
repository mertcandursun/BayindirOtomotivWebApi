using BayindirOtomotivWebApi.Models;
using BayindirOtomotivWebApi.Helpers;
using BayindirOtomotivWebApi.Models.Basbug;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BayindirOtomotivWebApi.Helpers
{
    public class IdeasoftMapper
    {
        /// <summary>
        /// Maps the Basbug MalzemeAraDto data to an IdeaSoftProductDto.
        /// The 'brandMapping' parameter is an IdeaSoftBrandDto obtained from the overall brands list.
        /// </summary>
        public static IdeaSoftProductDto MapToIdeaSoftDto(MalzemeAraDto item)
        {
            /* -------------------------------------------------- STOK ----------------------------------- */
            int totalStock = item.sMrk + item.sIzm + item.sAnk + item.sAdn + item.sErz;

            /* -------------------------------------------------- KATEGORİ ------------------------------- */
            // 1) “m” alanından model + marka Id’lerini çıkar
            var catIds = CategoryResolver.ResolveIds(item.m);

            // 2) Hiç kategori bulunamadıysa en azından lgk (ana marka) Id’sini ekle
            if (!catIds.Any())
            {
                int fallbackId = CategoryMapper.GetCategoryId(item.lgk);
                catIds.Add(fallbackId);
            }

            /* -------------------------------------------------- FİYAT ---------------------------------- */
            double rawPrice = item.dc?.ToUpperInvariant() switch
            {
                "EUR" => item.nf * 43,
                "USD" => item.nf * 38,
                _ => item.nf
            };

            double finalPrice = rawPrice switch
            {
                <= 12_500 => rawPrice * 1.35,
                <= 20_000 => rawPrice * 1.17,
                _ => rawPrice * 1.12
            };

            /* -------------------------------------------------- GÖRSEL --------------------------------- */
            List<IdeaSoftImageDto>? images = null;
            if (!string.IsNullOrEmpty(item.imgBase64))
            {
                images = new List<IdeaSoftImageDto>
        {
            new IdeaSoftImageDto
            {
                filename   = item.no.Replace(" ", "_"),
                extension  = "jpg",
                sortOrder  = 1,
                attachment = $"data:image/jpeg;base64,{item.imgBase64}"
            }
        };
            }

            /* -------------------------------------------------- DTO ------------------------------------ */
            return new IdeaSoftProductDto
            {
                /* — temel alanlar — */
                name = $"{item.ac} - {item.uk} - {item.oe}",
                sku = item.no,
                barcode = item.oe,
                stockAmount = totalStock,
                price1 = finalPrice,
                taxIncluded = 0,
                tax = 20,
                status = 1,

                /* — SEO / açıklama — */
                metaDescription =
                    $"Bayındır Otomotiv \n{item.ac} {item.ac2} \n{item.m} - {item.mo} - {item.y} " +
                    "\nParça ile ilgili sorularınız için bizimle Whatsapp üzerinden iletişime geçebilirsiniz.",
                pageTitle = $"{item.m} - {item.mo} - {item.y}",
                shortDetails = $"{item.ac} {item.ac2} | {item.y} - {item.m} - {item.mo}",
                searchKeywords = $"{item.no}, {item.ac}, {item.oe}",

                /* — görseller — */
                images = images,

                /* — detay HTML’i — */
                detail = new IdeaSoftDetailDto
                {
                    sku = item.no,
                    details =
                        "<div><strong><br /><table style=\"border-collapse:collapse;width:100%;\"><tbody>" +
                        "<tr><td>&nbsp;Detay</td></tr>" +
                        $"<tr><td>&nbsp;Marka</td><td>&nbsp;{item.uk}</td></tr>" +
                        $"<tr><td>&nbsp;Açıklama</td><td>&nbsp;{item.ac} {item.ac2}</td></tr>" +
                        $"<tr><td>&nbsp;Uyumlu Modeller</td><td>&nbsp;{item.m} - {item.mo} - {item.y}</td></tr>" +
                        "<tr><td colspan=\"2\">Parça ile ilgili sorularınız için bizimle Whatsapp üzerinden " +
                        "iletişime geçebilirsiniz.</td></tr>" +
                        "</tbody></table></strong></div><br/>"
                },

                /* — kategoriler — */
                categories = catIds.Select(id => new IdeaSoftCategoryDto { id = id }).ToList(),

                /* — para birimi — */
                currency = new IdeaSoftCurrencyDto
                {
                    id = 3,
                    label = "TL",
                    abbr = "TL"
                }
            };
        }


        public static IdeaSoftProductDto UpdateMapToIdeaSoftDto(MalzemeAraDto update)
        {
            /* -------------------------------------------------- STOK ----------------------------------- */
            int totalStock = update.sMrk + update.sIzm + update.sAnk +
                             update.sAdn + update.sErz;

            /* -------------------------------------------------- KATEGORİ ------------------------------- */
            var catIds = CategoryResolver.ResolveIds(update.m);
            if (!catIds.Any())
                catIds.Add(CategoryMapper.GetCategoryId(update.lgk));

            /* -------------------------------------------------- FİYAT ---------------------------------- */
            double rawPrice = update.dc?.ToUpperInvariant() switch
            {
                "EUR" => update.nf * 43,
                "USD" => update.nf * 38,
                _ => update.nf
            };

            double finalPrice = rawPrice switch
            {
                <= 12_500 => rawPrice * 1.35,
                <= 20_000 => rawPrice * 1.17,
                _ => rawPrice * 1.12
            };

            /* -------------------------------------------------- GÖRSEL --------------------------------- */
            List<IdeaSoftImageDto>? images = null;
            if (!string.IsNullOrEmpty(update.imgBase64))
            {
                images = new()
                {
                    new IdeaSoftImageDto
                    {
                        filename   = update.no.Replace(" ", "_"),
                        extension  = "jpg",
                        sortOrder  = 1,
                        attachment = $"data:image/jpeg;base64,{update.imgBase64}"
                    }
                };
            }

            /* -------------------------------------------------- DTO ------------------------------------ */
            return new IdeaSoftProductDto
            {
                /* — temel alanlar — */
                name = $"{update.ac} - {update.uk} - {update.oe}",
                sku = update.no,
                barcode = update.oe,
                stockAmount = totalStock,
                price1 = finalPrice,
                taxIncluded = 0,
                tax = 20,
                status = 1,

                /* — SEO / açıklama — */
                metaDescription =
                    $"Bayındır Otomotiv \n{update.ac} {update.ac2} \n{update.m} - {update.mo} - {update.y} " +
                    "\nParça ile ilgili sorularınız için bizimle Whatsapp üzerinden iletişime geçebilirsiniz.",
                pageTitle = $"{update.m} - {update.mo} - {update.y}",
                shortDetails = $"{update.ac} {update.ac2} | {update.y} - {update.m} - {update.mo}",
                searchKeywords = $"{update.no}, {update.ac}, {update.oe}",

                /* — görseller — */
                images = images,

                /* — detay HTML’i — */
                detail = new IdeaSoftDetailDto
                {
                    sku = update.no,
                    details =
                        "<div><strong><br /><table style=\"border-collapse:collapse;width:100%;\"><tbody>" +
                        "<tr><td>&nbsp;Detay</td></tr>" +
                        $"<tr><td>&nbsp;Marka</td><td>&nbsp;{update.uk}</td></tr>" +
                        $"<tr><td>&nbsp;Açıklama</td><td>&nbsp;{update.ac} {update.ac2}</td></tr>" +
                        $"<tr><td>&nbsp;Uyumlu Modeller</td><td>&nbsp;{update.m} - {update.mo} - {update.y}</td></tr>" +
                        "<tr><td colspan=\"2\">Parça ile ilgili sorularınız için bizimle Whatsapp üzerinden " +
                        "iletişime geçebilirsiniz.</td></tr>" +
                        "</tbody></table></strong></div><br/>"
                },

                /* — kategoriler — */
                categories = catIds.Select(id => new IdeaSoftCategoryDto { id = id }).ToList(),

                /* — para birimi — */
                currency = new IdeaSoftCurrencyDto
                {
                    id = 3,
                    label = "TL",
                    abbr = "TL"
                }
            };
        }


        public static IdeaSoftProductDto CreateMapToLightCategory(MalzemeAraDto create)
        {
            int catId = 1980;

            // Stock calculation
            int totalStock = create.sMrk + create.sIzm + create.sAnk + create.sAdn + create.sErz;

            // rice calculation (using nf, etc.)
            double rawPrice; // convert nf to TL according to currency conversion rules
            switch (create.dc?.ToUpperInvariant())
            {
                case "EUR":
                    rawPrice = create.nf * 43;
                    break;
                case "USD":
                    rawPrice = create.nf * 38;
                    break;
                default:
                    rawPrice = create.nf;
                    break;
            }

            double finalPrice;
            if (rawPrice <= 12500)
            {
                // 35% margin
                finalPrice = rawPrice * 1.35;
            }
            else if (rawPrice <= 20000)
            {
                // 17% margin
                finalPrice = rawPrice * 1.17;
            }
            else // rawPrice > 20000
            {
                // 12% margin
                finalPrice = rawPrice * 1.12;
            }
            // Apply tax (if desired, e.g., multiply by 1.20), adjust if necessary:
            // finalPrice *= 1.20;

            // 4) Image mapping (using base64 string already stored in item.imgBase64)
            List<IdeaSoftImageDto> imageList = null;
            if (!string.IsNullOrEmpty(create.imgBase64))
            {
                imageList = new List<IdeaSoftImageDto>
                {
                    new IdeaSoftImageDto
                    {
                        filename = create.no.Replace(" ", "_"),
                        extension = "jpg",
                        sortOrder = 1,
                        attachment = $"data:image/jpeg;base64,{create.imgBase64}"
                    }
                };
            }

            return new IdeaSoftProductDto
            {
                name = $"{create.ac} - {create.uk} - {create.oe}",
                sku = create.no,
                barcode = create.oe,
                categories = new List<IdeaSoftCategoryDto>
                {
                    new IdeaSoftCategoryDto { id = catId }
                },
                stockAmount = totalStock,
                price1 = finalPrice,
                taxIncluded = 0,
                tax = 20,
                status = 1,
                metaDescription = $"Bayındır Otomotiv \n{create.ac} {create.ac2} \n{create.m} - {create.mo} - {create.y} \nParça ile ilgili sorularınız için bizimle Whatsapp üzerinden iletişime geçebilirsiniz.",
                pageTitle = $"{create.m} - {create.mo} - {create.y}",
                shortDetails = $"{create.ac} {create.ac2} | {create.y} - {create.m} - {create.mo}",
                searchKeywords = $"{create.no}, {create.ac}, {create.oe}",
                images = imageList,
                detail = new IdeaSoftDetailDto
                {
                    sku = create.no,
                    details = "<div><strong><br /><table style=\"border-collapse:collapse;width:100%;\"><tbody>" +
                    "<tr><td>&nbsp;Detay</td></tr>" +
                    $"<tr><td>&nbsp;Marka</td><td>&nbsp;{create.uk}</td></tr>" +
                    $"<tr><td>&nbsp;Açıklama</td><td>&nbsp;{create.ac} {create.ac2}</td></tr>" +
                    $"<tr><td>&nbsp;Uyumlu Modeller</td><td>&nbsp;{create.m} - {create.mo} - {create.y}</td></tr>" +
                    "<tr><td colspan=\"2\">Parça ile ilgili sorularınız için bizimle Whatsapp üzerinden iletişime geçebilirsiniz.</td></tr>" +
                              "</tbody></table></strong></div><br/>"
                },
                currency = new IdeaSoftCurrencyDto
                {
                    id = 3,
                    label = "TL",
                    abbr = "TL"
                },
            };
        }

        public static IdeaSoftProductDto UpdateMapToLightCategory(MalzemeAraDto update)
        {
            // Stock calculation
            int totalStock = update.sMrk + update.sIzm + update.sAnk + update.sAdn + update.sErz;

            // rice calculation (using nf, etc.)
            double rawPrice; // convert nf to TL according to currency conversion rules
            switch (update.dc?.ToUpperInvariant())
            {
                case "EUR":
                    rawPrice = update.nf * 43;
                    break;
                case "USD":
                    rawPrice = update.nf * 38;
                    break;
                default:
                    rawPrice = update.nf;
                    break;
            }

            double finalPrice;
            if (rawPrice <= 12500)
            {
                // 35% margin
                finalPrice = rawPrice * 1.35;
            }
            else if (rawPrice <= 20000)
            {
                // 17% margin
                finalPrice = rawPrice * 1.17;
            }
            else // rawPrice > 20000
            {
                // 12% margin
                finalPrice = rawPrice * 1.12;
            }
            // Apply tax (if desired, e.g., multiply by 1.20), adjust if necessary:
            // finalPrice *= 1.20;

            // 4) Image mapping (using base64 string already stored in item.imgBase64)
            List<IdeaSoftImageDto> imageList = null;
            if (!string.IsNullOrEmpty(update.imgBase64))
            {
                imageList = new List<IdeaSoftImageDto>
                {
                    new IdeaSoftImageDto
                    {
                        filename = update.no.Replace(" ", "_"),
                        extension = "jpg",
                        sortOrder = 1,
                        attachment = $"data:image/jpeg;base64,{update.imgBase64}"
                    }
                };
            }

            return new IdeaSoftProductDto
            {
                name = $"{update.ac} - {update.uk} - {update.oe}",
                sku = update.no,
                barcode = update.oe,
                stockAmount = totalStock,
                price1 = finalPrice,
                taxIncluded = 0,
                tax = 20,
                status = 1,
                metaDescription = $"Bayındır Otomotiv \n{update.ac} {update.ac2} \n{update.m} - {update.mo} - {update.y} \nParça ile ilgili sorularınız için bizimle Whatsapp üzerinden iletişime geçebilirsiniz.",
                pageTitle = $"{update.m} - {update.mo} - {update.y}",
                shortDetails = $"{update.ac} {update.ac2} | {update.y} - {update.m} - {update.mo}",
                searchKeywords = $"{update.no}, {update.ac}, {update.oe}",
                images = imageList,
                detail = new IdeaSoftDetailDto
                {
                    sku = update.no,
                    details = "<div><strong><br /><table style=\"border-collapse:collapse;width:100%;\"><tbody>" +
                              "<tr><td>&nbsp;Detay</td></tr>" +
                              $"<tr><td>&nbsp;Marka</td><td>&nbsp;{update.uk}</td></tr>" +
                              $"<tr><td>&nbsp;Açıklama</td><td>&nbsp;{update.ac} {update.ac2}</td></tr>" +
                              $"<tr><td>&nbsp;Uyumlu Modeller</td><td>&nbsp;{update.m} - {update.mo} - {update.y}</td></tr>" +
                              "<tr><td colspan=\"2\">Parça ile ilgili sorularınız için bizimle Whatsapp üzerinden iletişime geçebilirsiniz.</td></tr>" +
                              "</tbody></table></strong></div><br/>"
                },
                currency = new IdeaSoftCurrencyDto
                {
                    id = 3,
                    label = "TL",
                    abbr = "TL"
                },
            };
        }

        // For demonstration, you could also have a default category mapper as before.
        public static class CategoryMapper
        {
            private static Dictionary<string, int> map = new Dictionary<string, int>
            {
                { "OPEL", 1 },
                { "CHEVROLET", 2 },
                { "PEUGEOT", 3 },
                { "CITROEN", 4 },
                { "RENAULT", 5 },
                { "FIAT", 6 },
                { "VW", 7 },
                { "FORD", 8 },
                { "PSA", 1967 },
            };

            public static int GetCategoryId(string lgk)
            {
                if (map.TryGetValue(lgk, out int cid))
                    return cid;
                return 1;
            }
        }
    }
}

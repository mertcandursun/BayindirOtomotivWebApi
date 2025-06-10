namespace BayindirOtomotivWebApi.Models.Basbug
{
    public class MalzemeleriGetirResponse
    {
        public List<MalzemeDto> malzemeListesi { get; set; }
    }

    public class MalzemeDto
    {
        public string no { get; set; }
        public string ac { get; set; }
        public string ac2 { get; set; }
        public string mkk { get; set; }
        public string oe { get; set; }
        public string uk { get; set; }
        public string lgk { get; set; }
        public string m { get; set; }
        public string mo { get; set; }
        public string y { get; set; }
        public string b { get; set; }
        public string dc { get; set; }
        public double lf { get; set; }
    }


    public class MalzemeAraResponse
    {
        public List<MalzemeAraDto> malzemeListesi { get; set; }
    }

    public class MalzemeAraDto
    {
        public string no { get; set; }
        public string ac { get; set; }
        public string ac2 { get; set; }
        public string mkk { get; set; }
        public string oe { get; set; }
        public string uk { get; set; }
        public string lgk { get; set; }
        public string m { get; set; }
        public string mo { get; set; }
        public string y { get; set; }
        public string b { get; set; }
        public string dc { get; set; }
        public double lf { get; set; }
        public double nf { get; set; }
        public double mif { get; set; }
        public int k { get; set; }
        public int sMrk { get; set; }
        public int sIzm { get; set; }
        public int sAnk { get; set; }
        public int sAdn { get; set; }
        public int sErz { get; set; }
        public int sYol { get; set; }
        public string imgBase64 { get; set; }
        public string imgUrl { get; set; }
    }
}

namespace Definitivo.Models
{
    public class faqElemento
    {
        public int id { get; set; }
        public string pergunta { get; set; }
        public string resposta { get; set; }
    }

    public class faqModel
    {
        public int id { get; set; }
        public List<faqElemento> elementos { get; set; }
    }
}

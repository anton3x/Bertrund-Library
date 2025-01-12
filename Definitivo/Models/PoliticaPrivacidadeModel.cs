namespace Definitivo.Models
{
    public class PoliticaPrivacidadeModel
    {
        public int id { get; set; }
        public List<TopicoPoliticaPrivacidade> topicos { get; set; }
        public DateTime dataUltimaModificacao { get; set; } = DateTime.Now;
    }

    public class TopicoPoliticaPrivacidade
    {
        public int id { get; set; }
        public string titulo { get; set; }
        public string conteudo { get; set; }
    }
}

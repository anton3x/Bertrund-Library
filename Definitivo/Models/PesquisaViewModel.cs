namespace Definitivo.Models
{
    public class PesquisaViewModel
    {
        public IEnumerable<Livro> Livros { get; set; }
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public string TermoPesquisa { get; set; }
    }

}

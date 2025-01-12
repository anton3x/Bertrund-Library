namespace Definitivo.Models
{
    public class LivroCatalogoViewModel
    {
        public List<Livro> Livros { get; set; }
        public List<Categoria> Categorias { get; set; }
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalLivros { get; set; }
        public int TamanhoPagina { get; set; }
        public string SortOrder { get; set; }
        public List<int> CategoriasSelecionadas { get; set; }
        public List<string> EstadosSelecionados { get; set; }
    }

}

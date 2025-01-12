namespace Definitivo.Models
{
    public class AutorViewModel
    {
        public Autor Autor { get; set; }
        public List<Categoria> Generos { get; set; }
        public List<Livro> Livros { get; set; }
        public List<Livro> ObrasNotaveis { get; set; }
    }

}
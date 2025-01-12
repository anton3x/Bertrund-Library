namespace Definitivo.Models
{
    public class BookDetailsViewModel
    {
        public Livro Book { get; set; }
        public bool IsInFavorites { get; set; }
        public bool PrecisaIrBuscarLivro { get; set; }
        public bool EstaEmprestadoAEle { get; set; }
        public bool EstaReservadoAEle { get; set; }
        public int? PaginaAtual { get; set; }
        public int? TotalPaginas { get; set; }
        public string? SortOrder { get; set; }
    }
}

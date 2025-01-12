namespace Definitivo.Models
{
    public class FormularioLivroViewModel
    {
        public int LivroId { get; set; }
        public string BotaoTexto { get; set; }
        public string ActionLivro { get; set; }

        //Apenas quando usado no botao dos favoritos
        public string? FavoritosIcon { get; set; }
        public string? FavoritosClasse { get; set; }
    }

}

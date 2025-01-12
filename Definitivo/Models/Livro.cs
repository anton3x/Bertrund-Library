using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Definitivo.Models
{
    public class Livro
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required(ErrorMessage = "O campo 'Biblioteca' é obrigatório.")]
        [ForeignKey("Biblioteca")]
        public int BibliotecaId { get; set; }
        public Biblioteca? Biblioteca { get; set; }

        [Required(ErrorMessage = "O campo 'Categoria' é obrigatório.")]
        [ForeignKey("Categoria")]
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        [Required(ErrorMessage = "O campo 'Autor' é obrigatório.")]
        [ForeignKey("Autor")]
        public int AutorId { get; set; }
        public Autor? Autor { get; set; }

        [Required(ErrorMessage = "O campo 'ISBN' é obrigatório.")]
        [MaxLength(13)]
        [RegularExpression(@"^(?:\d{9}[\dX]|\d{13})$", ErrorMessage = "O ISBN deve ter 9 ou 13 dígitos.")]
        public string ISBN { get; set; }

        [Required(ErrorMessage = "O campo 'Dimensões' é obrigatório.")]
        [MaxLength(50, ErrorMessage = "As dimensões não podem exceder 50 caracteres")]
        [RegularExpression(@"^\d{1,3}\s*x\s*\d{1,3}\s*x\s*\d{1,3}\s*(mm|cm)$",
    ErrorMessage = "Formato inválido. Use: 000 x 000 x 000 mm/cm")]
        [Display(Name = "Dimensões")]
        public string Dimensoes { get; set; }

        [Required(ErrorMessage = "O número de páginas é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "O número de páginas deve ser maior que 0.")]
        public int NumeroPaginas { get; set; }

        [Required(ErrorMessage = "O campo 'Idioma' é obrigatório.")]
        [MaxLength(50)]
        public string Idioma { get; set; }

        [Required(ErrorMessage = "O campo 'Bibliotecário Que Inseriu' é obrigatório.")]
        [ForeignKey("BibliotecarioInseriu")]
        public string IdBibliotecarioInseriu { get; set; }
        public Perfil? BibliotecarioInseriu { get; set; } //bibliotecario inseriu

        [Required(ErrorMessage = "A data de inserção é obrigatória.")]
        public DateTime DataInsercao { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "O ano de publicação é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "O ano de publicação deve ser maior que 0.")]
        public int AnoPublicacao { get; set; } = DateTime.Now.Year;

        [MaxLength(50)]
        [RegularExpression("^(Disponível|Indisponivel|Desativado)$")]
        public string Estado { get; set; }

        [Required(ErrorMessage = "O número de exemplares disponíveis é obrigatório.")]
        [Range(0, int.MaxValue, ErrorMessage = "O número de exemplares disponíveis deve ser maior ou igual a 0.")]
        public int NumeroExemplaresDisponiveis { get; set; }

        [Required(ErrorMessage = "O número total de exemplares é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "O número total de exemplares deve ser maior que 0.")]
        public int NumeroExemplaresTotal { get; set; }

        [Required(ErrorMessage = "O campo 'Título' é obrigatório.")]
        [MaxLength(255)]
        public string Titulo { get; set; }

        [MaxLength(255)]
        public string? FotoNome { get; set; }

        [MaxLength(2000)]
        [Required(ErrorMessage = "A sinopse é obrigatória.")]
        public string Sinopse { get; set; }

        [Range(0, int.MaxValue)]
        public int? Cliques { get; set; } = 0;


        public ICollection<Emprestimo>? Emprestimos { get; set; }

        public ICollection<Perfil>? PerfisAFavoritar { get; set; }

        // Relacionamento das reservas que o utilizador fez
        public ICollection<Perfil>? ReservadoPor { get; set; }

        //reviews do livro, se existirem
        public ICollection<Review>? Reviews { get; set; }
    }
}

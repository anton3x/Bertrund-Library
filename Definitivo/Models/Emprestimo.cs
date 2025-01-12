using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Definitivo.Models
{
    public class Emprestimo
    {
        [Key]
        public int Id { get; set; }
 
        [Required]
        public DateTime DataEmprestimo { get; set; } = DateTime.Now;
        public DateTime? DataPrevista { get; set; }
        public DateTime? DataDevolucao { get; set; }

        [Required]
        [ForeignKey("Perfil")]
        public string PerfilId { get; set; }
        public Perfil? Perfil { get; set; }

        [Required]
        [ForeignKey("Livro")]
        public int LivroId { get; set; }
        public Livro? Livro { get; set; }

        [ForeignKey("BibliotecarioEntregou")]
        public string? Id_bibliotecario_entregou { get; set; }
        public Perfil? BibliotecarioEntregou { get; set; }

        [ForeignKey("BibliotecarioRecebeu")]
        public string? Id_bibliotecario_recebeu { get; set; }
        public Perfil? BibliotecarioRecebeu { get; set; }
    }
}

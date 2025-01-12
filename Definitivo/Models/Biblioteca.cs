using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Definitivo.Models
{
    public class Biblioteca
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(13)]
        [RegularExpression(@"^\+351 [0-9]{9}$")]
        public string Telefone { get; set; }

        [MaxLength(255)]
        public string Morada { get; set; }

        [MaxLength(8)]
        [RegularExpression(@"^\d{4}-\d{3}$")]
        public string CodigoPostal { get; set; }

        [MaxLength(100)]
        public string Cidade { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }
    }
}

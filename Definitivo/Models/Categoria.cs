using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Definitivo.Models
{
    public class Categoria
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(20, ErrorMessage = "O nome deve ter no máximo 20 caracteres")]
        [MinLength(1, ErrorMessage = "O nome não pode estar vazio")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória")]
        [StringLength(100, ErrorMessage = "A descrição deve ter no máximo 100 caracteres")]
        [MinLength(1, ErrorMessage = "A descrição não pode estar vazia")]
        public string Descricao { get; set; }

        [Required]
        public bool Estado { get; set; } // Estado (false -> desativada, true -> ativada)

    }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Definitivo.Models
{
    public class Autor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [MinLength(1, ErrorMessage = "O nome não pode estar vazio")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "A biografia é obrigatória")]
        [StringLength(1000, ErrorMessage = "A biografia deve ter no máximo 100 caracteres")]
        [MinLength(1, ErrorMessage = "A biografia não pode estar vazia")]
        public string Biografia { get; set; }

        [StringLength(255)]
        public string? FotoNome { get; set; }  // Nome da foto do autor, opcional

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "A data de nascimento é obrigatória")]
        public DateOnly DataNascimento { get; set; }  // Data de nascimento do autor

        [DataType(DataType.Date)]
        public DateOnly? DataFalecimento { get; set; }  // Data de falecimento do autor, opcional

        [Required(ErrorMessage = "A nacionalidade é obrigatória")]
        public string Nacionalidade { get; set; }  // Nacionalidade do autor

    }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Definitivo.Models
{
    public class Perfil : IdentityUser
    {

        [Required(ErrorMessage = "O nome de utilizador é obrigatório")]
        [StringLength(21, MinimumLength = 6, ErrorMessage = "O nome de utilizador deve ter entre 6 e 21 caracteres")]
        public override string UserName { get; set; }

        
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "O email não é válido")]
        [StringLength(256, ErrorMessage = "O email deve ter no máximo 256 caracteres")]
        public override string Email { get; set; }

        [Phone(ErrorMessage = "O número de telemóvel não é válido")]
        [Display(Name = "Telemóvel")]
        [RegularExpression(@"^\+\d{1,4}\d{4,14}$",
    ErrorMessage = "O número de telemóvel deve começar com o indicativo internacional (por exemplo, +351) seguido do número de telefone.")]
        public override string? PhoneNumber { get; set; }

        [MaxLength(255, ErrorMessage = "A morada deve ter no máximo 255 caracteres")]
        public string? Morada { get; set; }

        [Required(ErrorMessage = "O 'Estado' é obrigatório")]
        [MaxLength(50, ErrorMessage = "O 'Estado' deve ter no máximo 50 caracteres")]
        [RegularExpression("^(PorAtivar|Ativo|Bloqueado)$")]
        public string EstadoAtivacao { get; set; } = "Ativo";

        //so para os administrador que sao criados
        public string? IdAdministradorQueCriou { get; set; }

        public string? FotoNome { get; set; }

        public int NumeroEmprestimosCanceladosPorEntregar { get; set; } = 0;

        public ICollection<Emprestimo>? Emprestimos { get; set; }

        public ICollection<Livro>? LivroFavoritos { get; set; }

        // Relacionamento para perfis bloqueados por este perfil (como administrador)
        public ICollection<BloqueiaUser>? Bloqueados { get; set; }

        // Relacionamento 1 para 1: Este perfil foi bloqueado (pode ser bloqueado apenas uma vez)
        public BloqueiaUser? Bloqueio { get; set; }

        // Relacionamento para perfis que foram aprovados por este administrador
        public ICollection<AprovaBibliotecario>? BibliotecariosAprovados { get; set; }

        // Relacionamento para perfis que aprovaram este bibliotecário
        public ICollection<AprovaBibliotecario>? AprovadoPor { get; set; }

        // Relacionamento das reservas que o utilizador fez
        public ICollection<Livro>? Reserva { get; set; }
    }
}

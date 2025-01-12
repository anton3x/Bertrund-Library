using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Definitivo.Models
{
    public class BloqueiaUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }  // Id para a entidade BloqueiaUser

        [Required]
        public string ID_PerfilUser { get; set; } // Chave estrangeira para o perfil do usuário bloqueado
        public Perfil? PerfilUser { get; set; }  // Propriedade de navegação para o perfil do usuário bloqueado

        [Required]
        public string ID_PerfilAdmin { get; set; } // Chave estrangeira para o perfil do administrador que bloqueou
        public Perfil? PerfilAdmin { get; set; } // Propriedade de navegação para o perfil do administrador que bloqueou

        [Required]
        [MaxLength(100)]
        public string Motivo { get; set; } // Motivo do bloqueio

        public DateTime dataBloqueio { get; set; } = DateTime.Now;
    }
}

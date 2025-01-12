using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Definitivo.Models
{
    public class AprovaBibliotecario
    {
        public string ID_PerfilBibliotecario { get; set; } // Chave estrangeira para o perfil do bibliotecário
        public Perfil? PerfilBibliotecario { get; set; } //

        public string ID_PerfilAdmin { get; set; } // Chave estrangeira para o perfil do administrador
        public Perfil? PerfilAdmin { get; set; }

    }
}

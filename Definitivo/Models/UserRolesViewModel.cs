namespace Definitivo.Models
{
    public class UserRolesViewModel
    {
        public Perfil User { get; set; }
        public Perfil? AdministradorQueOCriou { get; set; } = null;
        public string Role { get; set; }
    }
}

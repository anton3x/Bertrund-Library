namespace Definitivo.Models
{
    public class ChatViewModel
    {
        public string? UserId { get; set; } //id do utilizador com quem o utilizador logado está a falar
        public string? UserName { get; set; } //nome do utilizador com quem o utilizador logado está a falar
        public string? UserImage { get; set; } = "userDefault.png"; //nome do utilizador com quem o utilizador logado está a falar
        public List<Message>? Messages { get; set; } //mensagens trocadas enbtre eles
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}

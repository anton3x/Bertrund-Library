namespace Definitivo.Models.Configuration
{
    public class EmailConfiguration
    {
        public string FromEmail { get; set; }
        public string DisplayName { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}

namespace Definitivo.Models
{
    public class ChatsViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserImage { get; set; }
        public string LastMessage { get; set; }
        public DateTime? LastMessageTimeStamp { get; set; }
        public bool online { get; set; } = false;
        public int UnreadMessages { get; set; } = 0;
        public bool onCall { get; set; } = false;
    }
}

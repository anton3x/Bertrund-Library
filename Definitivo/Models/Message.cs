using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Collections.Specialized.BitVector32;

namespace Definitivo.Models
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string SenderId { get; set; }
        public Perfil? Sender { get; set; }

        [Required]
        [MaxLength(450)]
        public string ReceiverId { get; set; }
        public Perfil? Receiver { get; set; }

        [Required]
        public string Content { get; set; }

        public bool Seen { get; set; } = false;

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
        public int? ReplyTo { get; set; }
        public string? FileName { get; set; } = null;
        public bool? Edited { get; set; } = false;

    }

    public class Reaction
    {
        public int Id { get; set; } // Chave primária

        [Required]
        public string Emoji { get; set; } // Representa o emoji, ex.: "👍"

        [Required]
        public string UserId { get; set; } // Usuário que reagiu

        public int MessageId { get; set; } // Relacionamento com a mensagem
        public Message Message { get; set; } // Navegação para a mensagem
    }

}

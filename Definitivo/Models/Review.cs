namespace Definitivo.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "O comentário não pode ter mais de 1000 caracteres.")]
        public string Text { get; set; }

        public string TituloLivro { get; set; } //apenas usado no perfilPublico

        [Required]
        [Range(1, 5, ErrorMessage = "A nota deve ser entre 1 e 5.")]
        public int Rating { get; set; }

        [Required]
        public string UserId { get; set; }

        public Perfil? User { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool Lido { get; set; } = false;
    }

}

namespace Definitivo.Models
{
    public class UserProfileViewModel
    {
        public Perfil User { get; set; }
        public List<Review> Reviews { get; set; }
        public int TotalReviews { get; set; }
        public int BooksRead { get; set; }
        public List<string> FavoriteGenres { get; set; }
        public List<string> CurrentlyReading { get; set; }
        public List<string> Achievements { get; set; }
    }
}

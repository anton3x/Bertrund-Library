using Definitivo.Data;

namespace Definitivo.Models.Services
{
    public class BibliotecaService
    {
        private readonly ApplicationDbContext _context;

        public BibliotecaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Biblioteca GetBibliotecaInfo()
        {
            return _context.Biblioteca.FirstOrDefault();
        }
    }
}

using Definitivo.Data;
using Microsoft.EntityFrameworkCore;
using Mscc.GenerativeAI;

namespace Definitivo.Models.Services;

public class ProcessarReservasService
{
    private readonly ApplicationDbContext _context;

    public ProcessarReservasService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HandleReservasLivro(int idLivro)
    {
        Livro livro = await _context.Livro
            .Include(l => l.ReservadoPor)
            .FirstOrDefaultAsync(l => l.ID == idLivro);

        if (livro == null)
        {
            return false;
        }

        if (livro.ReservadoPor == null ||
            !livro.ReservadoPor.Any())
        {
            return true;
        }

        var perfisARemover = new List<Perfil>();

        foreach (var perfil in livro.ReservadoPor)
        {
            if (livro.NumeroExemplaresDisponiveis <= 0)
            {
                break;
            }

            var emprestimo = new Emprestimo
            {
                LivroId = idLivro,
                PerfilId = perfil.Id,
                DataEmprestimo = DateTime.Now
            };

            _context.Emprestimo.Add(emprestimo);
            perfisARemover.Add(perfil);
            livro.NumeroExemplaresDisponiveis--;
        }

        foreach (var perfil in perfisARemover)
        {
            livro.ReservadoPor.Remove(perfil);
        }

        livro.Estado = livro.NumeroExemplaresDisponiveis > 0 ? "Disponível" : "Indisponível";

        await _context.SaveChangesAsync();

        return true;
    }
}


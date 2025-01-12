namespace Definitivo.Models.BackgroundServices
{
    // BackgroundServices/EmprestimoCleanupService.cs
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.EntityFrameworkCore;
    using global::Definitivo.Data;
    using global::Definitivo.Models.Configuration;
    using Microsoft.Extensions.Options;

    namespace Definitivo.BackgroundServices
    {
        public class EmprestimoCleanupService : BackgroundService
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly ILogger<EmprestimoCleanupService> _logger; 
            private readonly EmprestimoCleanupConfig _config;

            public EmprestimoCleanupService(
                IServiceProvider serviceProvider,
                ILogger<EmprestimoCleanupService> logger,
                IOptions<EmprestimoCleanupConfig> options)
            {
                _serviceProvider = serviceProvider;
                _logger = logger;
                _config = options.Value;
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await ExcluirEmprestimosAtrasados();
                        await Task.Delay(TimeSpan.FromHours(_config.IntervaloVerificacaoHoras), stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao executar limpeza de empréstimos");
                    }
                }
            }

            private async Task ExcluirEmprestimosAtrasados()
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var dataLimite = _config.DiasParaExclusao > 0
                    ? DateTime.Now.AddDays(-_config.DiasParaExclusao)
                    : DateTime.Now;

                var emprestimosParaExcluir = await context.Emprestimo
                    .Where(e => e.DataEmprestimo <= dataLimite &&
                               e.Id_bibliotecario_entregou == null)
                    .ToListAsync();

                if (emprestimosParaExcluir.Any())
                {
                    var contadorPorUsuario = emprestimosParaExcluir
                        .GroupBy(e => e.PerfilId)
                        .ToDictionary(g => g.Key, g => g.Count());

                    foreach (var item in contadorPorUsuario)
                    {
                        var user = await context.Users.FindAsync(item.Key);
                        if (user != null)
                        {
                            user.NumeroEmprestimosCanceladosPorEntregar += item.Value;
                            context.Users.Update(user);
                        }
                    }

                    var contadorPorLivro = emprestimosParaExcluir
                        .GroupBy(e => e.LivroId)
                        .ToDictionary(g => g.Key, g => g.Count());

                    foreach (var item in contadorPorLivro)
                    {
                        var livro = await context.Livro.FindAsync(item.Key);
                        if (livro != null)
                        {
                            if (livro.NumeroExemplaresDisponiveis == 0)
                            {
                                livro.Estado = "Disponível";
                            }
                            livro.NumeroExemplaresDisponiveis += item.Value;
                            context.Livro.Update(livro);
                        }
                    }

                    context.Emprestimo.RemoveRange(emprestimosParaExcluir);
                    await context.SaveChangesAsync();
                }
            }
        }
    }

}

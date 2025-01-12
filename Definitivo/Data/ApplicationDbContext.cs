using Definitivo.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Definitivo.Data
{
    public class ApplicationDbContext : IdentityDbContext<Perfil>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Autor> Autor { get; set; } = default!;
        public DbSet<Categoria> Categoria { get; set; } = default!;
        public DbSet<MembroEquipa> MembroEquipa { get; set; } = default!;
        public DbSet<Contacto> Contacto { get; set; } = default!;
        public DbSet<HoraFuncionamento> HoraFuncionamento { get; set; } = default!;
        public DbSet<SobreNosModel> SobreNosModel { get; set; } = default!;
        public DbSet<Objetivo> Objetivo { get; set; } = default!;
        public DbSet<Biblioteca> Biblioteca { get; set; } = default!;
        public DbSet<Emprestimo> Emprestimo { get; set; } = default!;
        public DbSet<Livro> Livro { get; set; } = default!;
        public DbSet<Perfil> Perfil { get; set; } = default!;
        public DbSet<AprovaBibliotecario> AprovaBibliotecario { get; set; } = default!;
        public DbSet<BloqueiaUser> BloqueiaUser { get; set; } = default!;
        public DbSet<TopicoPoliticaPrivacidade> TopicoPoliticaPrivacidade { get; set; } = default!;
        public DbSet<PoliticaPrivacidadeModel> PoliticaPrivacidadeModel { get; set; } = default!;
        public DbSet<Definitivo.Models.faqElemento> faqElemento { get; set; } = default!;
        public DbSet<Definitivo.Models.faqModel> faqModel { get; set; } = default!;
        public DbSet<Definitivo.Models.Review> Review { get; set; } = default!;
        public DbSet<Definitivo.Models.Message> Messages { get; set; } = default!;
        public DbSet<Definitivo.Models.Reaction> Reactions { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<Emprestimo>()
            //.HasKey(e => new { e.DataEmprestimo, e.PerfilId, e.LivroId });

            // Configurar SenderId
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict); // Evitar exclusão em cascata

            // Configurar o relacionamento entre Message e Reaction
            modelBuilder.Entity<Reaction>()
                .HasOne(r => r.Message)
                .WithMany(m => m.Reactions)
                .HasForeignKey(r => r.MessageId)
                .OnDelete(DeleteBehavior.Cascade); // Apaga reações ao apagar a mensagem

            // Configurar ReceiverId
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict); // Evitar exclusão em cascata

            // Configurando o relacionamento entre Emprestimo e Perfil (quem fez o empréstimo)
            modelBuilder.Entity<Emprestimo>()
                .HasOne(e => e.Perfil)
                .WithMany(p => p.Emprestimos)
                .HasForeignKey(e => e.PerfilId)
                .OnDelete(DeleteBehavior.Cascade);  // Configura a ação de exclusão

            // Configurando o relacionamento para BibliotecarioEntregou
            modelBuilder.Entity<Emprestimo>()
                .HasOne(e => e.BibliotecarioEntregou)
                .WithMany()
                .HasForeignKey(e => e.Id_bibliotecario_entregou)
                .OnDelete(DeleteBehavior.Restrict);  // Evita exclusões em cascata

            // Configurando o relacionamento para BibliotecarioRecebeu
            modelBuilder.Entity<Emprestimo>()
                .HasOne(e => e.BibliotecarioRecebeu)
                .WithMany()
                .HasForeignKey(e => e.Id_bibliotecario_recebeu)
                .OnDelete(DeleteBehavior.Restrict);  // Evita exclusões em cascata

            // Configuração para relacionamento entre Livro e Bibliotecario que inseriu
            modelBuilder.Entity<Livro>()
                .HasOne(l => l.BibliotecarioInseriu)
                .WithMany()
                .HasForeignKey(l => l.IdBibliotecarioInseriu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Livro>()
                .HasMany(l => l.ReservadoPor)
                .WithMany(p => p.Reserva)
                .UsingEntity(j => j.ToTable("Reservar"));


            // Configuração para relacionamento um para um (Perfil <-> Bloqueio)
            modelBuilder.Entity<Perfil>()
                .HasOne(p => p.Bloqueio)  // Perfil pode ser bloqueado apenas uma vez
                .WithOne(b => b.PerfilUser)  // BloqueiaUser referencia o Perfil bloqueado
                .HasForeignKey<BloqueiaUser>(b => b.ID_PerfilUser)
                .OnDelete(DeleteBehavior.Restrict);  // Evita exclusão em cascata

            // Configuração para relação muitos para muitos entre Perfil e Livro (favoritos)
            modelBuilder.Entity<Perfil>()
                .HasMany(p => p.LivroFavoritos)
                .WithMany(l => l.PerfisAFavoritar)
                .UsingEntity(j => j.ToTable("Favoritar"));

            // Configuração para BloqueiaUser (administrador que realizou o bloqueio)
            modelBuilder.Entity<BloqueiaUser>()
                .HasOne(b => b.PerfilAdmin)  // BloqueiaUser tem um administrador
                .WithMany(p => p.Bloqueados)  // Administrador pode bloquear vários perfis
                .HasForeignKey(b => b.ID_PerfilAdmin)
                .OnDelete(DeleteBehavior.Restrict);  // Evita exclusão em cascata

            // Configuração para AprovaBibliotecario (chave composta)
            modelBuilder.Entity<AprovaBibliotecario>()
                .HasKey(a => new { a.ID_PerfilBibliotecario, a.ID_PerfilAdmin }); // Definir chave composta

            // Configuração para relação entre PerfilBibliotecario e AprovaBibliotecario
            modelBuilder.Entity<AprovaBibliotecario>()
                .HasOne(a => a.PerfilBibliotecario)  // Bibliotecário que foi aprovado
                .WithMany(p => p.AprovadoPor)        // Relacionamento com perfis que aprovaram este perfil
                .HasForeignKey(a => a.ID_PerfilBibliotecario)
                .OnDelete(DeleteBehavior.Restrict);  // Evita exclusão em cascata

            // Configuração para relação entre PerfilAdmin e AprovaBibliotecario
            modelBuilder.Entity<AprovaBibliotecario>()
                .HasOne(a => a.PerfilAdmin)          // Administrador que aprovou o bibliotecário
                .WithMany(p => p.BibliotecariosAprovados) // Relacionamento com perfis aprovados por este administrador
                .HasForeignKey(a => a.ID_PerfilAdmin)
                .OnDelete(DeleteBehavior.Restrict);  // Evita exclusão em cascata

        }

    }
}

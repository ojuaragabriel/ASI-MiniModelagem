using ASI.MiniModelagem.Models;
using Microsoft.EntityFrameworkCore;

namespace ASI.MiniModelagem.Data;

/// <summary>
/// Ponte entre suas classes (Models) e o banco de dados.
/// O EF Core usa este contexto para criar as tabelas e mapear os relacionamentos.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Cada DbSet representa uma TABELA no banco
    public DbSet<Departamento> Departamentos => Set<Departamento>();
    public DbSet<Professor> Professores => Set<Professor>();
    public DbSet<Projeto> Projetos => Set<Projeto>();
    public DbSet<ProfessorProjeto> ProfessoresProjetos => Set<ProfessorProjeto>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ---- Nomes das tabelas (opcional, só para ficar explícito)
        mb.Entity<Departamento>().ToTable("Departamentos");
        mb.Entity<Professor>().ToTable("Professores");
        mb.Entity<Projeto>().ToTable("Projetos");
        mb.Entity<ProfessorProjeto>().ToTable("ProfessoresProjetos");

        // ---- PK composta da tabela de junção N:N (Professor x Projeto)
        mb.Entity<ProfessorProjeto>()
          .HasKey(pp => new { pp.ProfessorId, pp.ProjetoId });

        // ---- Relacionamentos da junção
        mb.Entity<ProfessorProjeto>()
          .HasOne(pp => pp.Professor)
          .WithMany(p => p.ProfessoresProjetos)
          .HasForeignKey(pp => pp.ProfessorId);

        mb.Entity<ProfessorProjeto>()
          .HasOne(pp => pp.Projeto)
          .WithMany(p => p.ProfessoresProjetos)
          .HasForeignKey(pp => pp.ProjetoId);

        // ---- 1 Departamento -> N Professores
        mb.Entity<Professor>()
          .HasOne(p => p.Departamento)
          .WithMany(d => d.Professores)
          .HasForeignKey(p => p.DepartamentoId)
          .OnDelete(DeleteBehavior.Restrict); // evita cascade delete indesejado

        // ---- SEED: dados iniciais só para você testar no Swagger
        mb.Entity<Departamento>().HasData(
            new Departamento { Id = 1, Nome = "Departamento de Engenharias e Computação", Sigla = "DEC" },
            new Departamento { Id = 2, Nome = "Departamento de Ciências Exatas", Sigla = "DCEX" }
        );

        mb.Entity<Professor>().HasData(
            new Professor { Id = 1, Nome = "Larissa",   DepartamentoId = 1 },
            new Professor { Id = 2, Nome = "Gabriel", DepartamentoId = 2 }
        );

        mb.Entity<Projeto>().HasData(
            new Projeto { Id = 1, Nome = "Projeto ASI" },
            new Projeto { Id = 2, Nome = "Projeto math" }
        );

        mb.Entity<ProfessorProjeto>().HasData(
            new ProfessorProjeto { ProfessorId = 1, ProjetoId = 1 },
            new ProfessorProjeto { ProfessorId = 2, ProjetoId = 1 }
        );
    }
}

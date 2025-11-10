namespace ASI.MiniModelagem.Models;

public class ProfessorProjeto
{
    // PK composta: (ProfessorId, ProjetoId)
    public int ProfessorId { get; set; }
    public Professor Professor { get; set; } = default!;

    public int ProjetoId { get; set; }
    public Projeto Projeto { get; set; } = default!;
}

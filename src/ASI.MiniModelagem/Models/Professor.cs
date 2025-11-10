namespace ASI.MiniModelagem.Models;

public class Professor
{
    public int Id { get; set; }                   // PK
    public string Nome { get; set; } = default!;  // ex.: "Ana"

    // FK obrigatória: todo professor pertence a um departamento
    public int DepartamentoId { get; set; }
    public Departamento? Departamento { get; set; } // navegação (lado 1)

    // N:N com Projeto (via tabela de junção)
    public ICollection<ProfessorProjeto> ProfessoresProjetos { get; set; } = [];
}

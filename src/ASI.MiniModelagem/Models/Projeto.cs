namespace ASI.MiniModelagem.Models;

public class Projeto
{
    public int Id { get; set; }                   // PK
    public string Nome { get; set; } = default!;  // ex.: "IA no Campus"
    public ICollection<ProfessorProjeto> ProfessoresProjetos { get; set; } = [];
}

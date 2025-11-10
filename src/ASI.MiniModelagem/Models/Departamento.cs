namespace ASI.MiniModelagem.Models;

public class Departamento
{
    public int Id { get; set; }                  // PK no banco
    public string Nome { get; set; } = default!; // ex.: "Computação"
    public string Sigla { get; set; } = default!;// ex.: "DCC"

    // Navegação 1:N -> "um departamento tem vários professores"
    public ICollection<Professor> Professores { get; set; } = [];
}

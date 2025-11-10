using ASI.MiniModelagem.Data;
using ASI.MiniModelagem.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// EF Core + SQLite (arquivo asi.db na pasta do projeto)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=asi.db"));

// Swagger (UI para testar a API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Evitar ciclos de referência ao serializar (Professor <-> Departamento, etc.)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();

// Swagger só no Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Se não tiver porta HTTPS configurada, esse middleware só gera um aviso.
// Se quiser, pode comentar a linha abaixo.
app.UseHttpsRedirection();

// Agrupar rotas em /api
var api = app.MapGroup("/api");

// ------------------- DEPARTAMENTOS -------------------
api.MapGet("/departamentos", async (AppDbContext db)
    => await db.Departamentos.ToListAsync());

api.MapGet("/departamentos/{id:int}", async (int id, AppDbContext db)
    => await db.Departamentos.FindAsync(id) is { } d ? Results.Ok(d) : Results.NotFound());

api.MapPost("/departamentos", async (Departamento dto, AppDbContext db) =>
{
    db.Departamentos.Add(dto);
    await db.SaveChangesAsync();
    return Results.Created($"/api/departamentos/{dto.Id}", dto);
});

api.MapPut("/departamentos/{id:int}", async (int id, Departamento input, AppDbContext db) =>
{
    var d = await db.Departamentos.FindAsync(id);
    if (d is null) return Results.NotFound();
    d.Nome = input.Nome;
    d.Sigla = input.Sigla;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

api.MapDelete("/departamentos/{id:int}", async (int id, AppDbContext db) =>
{
    var d = await db.Departamentos.FindAsync(id);
    if (d is null) return Results.NotFound();
    db.Departamentos.Remove(d);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ------------------- PROFESSORES -------------------
api.MapGet("/professores", async (AppDbContext db) =>
    await db.Professores
        .Include(p => p.Departamento)
        .Include(p => p.ProfessoresProjetos).ThenInclude(pp => pp.Projeto)
        .ToListAsync());

api.MapGet("/professores/{id:int}", async (int id, AppDbContext db)
    => await db.Professores
        .Include(p => p.Departamento)
        .Include(p => p.ProfessoresProjetos).ThenInclude(pp => pp.Projeto)
        .FirstOrDefaultAsync(p => p.Id == id) is { } p
        ? Results.Ok(p) : Results.NotFound());

api.MapPost("/professores", async (Professor dto, AppDbContext db) =>
{
    if (!await db.Departamentos.AnyAsync(d => d.Id == dto.DepartamentoId))
        return Results.BadRequest($"Departamento {dto.DepartamentoId} não existe");
    db.Professores.Add(dto);
    await db.SaveChangesAsync();
    return Results.Created($"/api/professores/{dto.Id}", dto);
});

api.MapPut("/professores/{id:int}", async (int id, Professor input, AppDbContext db) =>
{
    var p = await db.Professores.FindAsync(id);
    if (p is null) return Results.NotFound();

    if (!await db.Departamentos.AnyAsync(d => d.Id == input.DepartamentoId))
        return Results.BadRequest($"Departamento {input.DepartamentoId} não existe");

    p.Nome = input.Nome;
    p.DepartamentoId = input.DepartamentoId;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

api.MapDelete("/professores/{id:int}", async (int id, AppDbContext db) =>
{
    var p = await db.Professores.FindAsync(id);
    if (p is null) return Results.NotFound();
    db.Professores.Remove(p);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ------------------- PROJETOS -------------------
api.MapGet("/projetos", async (AppDbContext db)
    => await db.Projetos
        .Include(pr => pr.ProfessoresProjetos).ThenInclude(pp => pp.Professor)
        .ToListAsync());

api.MapGet("/projetos/{id:int}", async (int id, AppDbContext db)
    => await db.Projetos
        .Include(pr => pr.ProfessoresProjetos).ThenInclude(pp => pp.Professor)
        .FirstOrDefaultAsync(p => p.Id == id) is { } pr
        ? Results.Ok(pr) : Results.NotFound());

api.MapPost("/projetos", async (Projeto dto, AppDbContext db) =>
{
    db.Projetos.Add(dto);
    await db.SaveChangesAsync();
    return Results.Created($"/api/projetos/{dto.Id}", dto);
});

api.MapPut("/projetos/{id:int}", async (int id, Projeto input, AppDbContext db) =>
{
    var pr = await db.Projetos.FindAsync(id);
    if (pr is null) return Results.NotFound();
    pr.Nome = input.Nome;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

api.MapDelete("/projetos/{id:int}", async (int id, AppDbContext db) =>
{
    var pr = await db.Projetos.FindAsync(id);
    if (pr is null) return Results.NotFound();
    db.Projetos.Remove(pr);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ----------- Vincular/Desvincular Professor <-> Projeto (N:N) ------------
api.MapPost("/projetos/{projetoId:int}/professores/{professorId:int}", async (int projetoId, int professorId, AppDbContext db) =>
{
    if (!await db.Projetos.AnyAsync(p => p.Id == projetoId)) return Results.NotFound("Projeto não encontrado");
    if (!await db.Professores.AnyAsync(p => p.Id == professorId)) return Results.NotFound("Professor não encontrado");

    var exists = await db.ProfessoresProjetos.FindAsync(professorId, projetoId);
    if (exists is not null) return Results.Conflict("Relação já existe");

    db.ProfessoresProjetos.Add(new ProfessorProjeto { ProfessorId = professorId, ProjetoId = projetoId });
    await db.SaveChangesAsync();
    return Results.NoContent();
});

api.MapDelete("/projetos/{projetoId:int}/professores/{professorId:int}", async (int projetoId, int professorId, AppDbContext db) =>
{
    var link = await db.ProfessoresProjetos.FindAsync(professorId, projetoId);
    if (link is null) return Results.NotFound();
    db.ProfessoresProjetos.Remove(link);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
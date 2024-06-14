using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Neo4j.Driver;

namespace GestaoVarejo.Domain;

[Table("categoria")]
[Display(Name = "Categoria")]
public class Categoria : IQueryableEntity<Categoria>
{
    [Column("nome")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    public void FillValues(IReadOnlyDictionary<string, object> values)
    {
        if (values.TryGetValue("nome", out var nome))
        {
            Nome = nome.ToString()!;
        }
    }

    public static List<Categoria> GetAll(IAsyncSession session)
    {
        var query = @"
            MATCH (c:categoria)
            RETURN c";

        var categorias = new List<Categoria>();
        var result = session.RunAsync(query).Result;

        result.ForEachAsync(record =>
        {
            var categoriaNode = record["c"].As<INode>();
            var categoria = new Categoria();
            categoria.FillValues(categoriaNode.Properties);
            categorias.Add(categoria);
        }).Wait();

        return categorias;
    }


    public override string ToString()
    {
        return $"Categoria: {Nome}";
    }

    public static void Create(IAsyncSession session)
    {
        var categoria = new Categoria();

        Console.WriteLine("Digite o nome da categoria (obrigatório)*: ");
        categoria.Nome = Console.ReadLine()!;
        
        if (string.IsNullOrWhiteSpace(categoria.Nome))
        {
            Console.WriteLine("O nome é obrigatório e não pode ser vazio.");
            return;
        }

        var query = @"
            CREATE (c:categoria {nome: $nome})
            RETURN c";

        var result = session.ExecuteWriteAsync(async tx =>
        {
            var result = await tx.RunAsync(query, new { nome = categoria.Nome });
            return await result.SingleAsync();
        }).Result;

        var createdNode = result["c"].As<INode>();

        Console.WriteLine($"Categoria criada com sucesso: {createdNode["nome"].As<string>()}");
    }

}
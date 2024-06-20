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

    public static void Delete(IAsyncSession session)
    {
        // Listar todas as categorias
        var categorias = GetAll(session);
        if (categorias.Count == 0)
        {
            Console.WriteLine("Não há categorias disponíveis para deletar.");
            return;
        }

        Console.WriteLine("Selecione a categoria que deseja deletar:");
        for (int i = 0; i < categorias.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {categorias[i]}"); // Mostra os detalhes de cada categoria
        }

        Console.WriteLine("Digite o número da categoria para deletar:");
        if (!int.TryParse(Console.ReadLine(), out int categoriaIndex) || categoriaIndex < 1 || categoriaIndex > categorias.Count)
        {
            Console.WriteLine("Seleção inválida. Por favor, selecione um número válido da lista.");
            return;
        }

        var categoriaEscolhida = categorias[categoriaIndex - 1];

        // Construir e executar a query de deleção
        var query = @"
            MATCH (c:categoria {nome: $nomeCategoria}) DELETE c";

        session.ExecuteWriteAsync(async tx =>
        {
            await tx.RunAsync(query, new { nomeCategoria = categoriaEscolhida.Nome });
        }).Wait();

        Console.WriteLine($"Categoria '{categoriaEscolhida.Nome}' deletada com sucesso.");
    }

    public static void Update(IAsyncSession session)
    {
        // Listar todas as categorias
        var categorias = GetAll(session);
        if (categorias.Count == 0)
        {
            Console.WriteLine("Não há categorias disponíveis para atualizar.");
            return;
        }

        Console.WriteLine("Selecione a categoria que deseja atualizar:");
        for (int i = 0; i < categorias.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {categorias[i]}");
        }

        Console.WriteLine("Digite o número da categoria para atualizar:");
        if (!int.TryParse(Console.ReadLine(), out int categoriaIndex) || categoriaIndex < 1 || categoriaIndex > categorias.Count)
        {
            Console.WriteLine("Seleção inválida. Por favor, selecione um número válido da lista.");
            return;
        }

        var categoriaEscolhida = categorias[categoriaIndex - 1];

        // Solicitar novos valores
        Console.WriteLine($"Nome atual: {categoriaEscolhida.Nome}");
        Console.WriteLine("Digite um novo nome ou pressione ENTER para manter o atual (ou digite 'null' para limpar):");
        var nome = Console.ReadLine();
        if (string.IsNullOrEmpty(nome))
        {
            nome = categoriaEscolhida.Nome; // Mantém o atual se ENTER for pressionado sem alterações
        }
        else if (nome.Trim().ToLower() == "null")
        {
            nome = null; // Permitir nulo se explicitamente indicado
        }
        else if (string.IsNullOrWhiteSpace(nome))
        {
            Console.WriteLine("O nome é obrigatório e não pode ser vazio, exceto se definido explicitamente para 'null'.");
            return;
        }

        // Atualizar a categoria
        var updateQuery = @"
            MATCH (c:categoria {nome: $nomeAtual})
            SET c.nome = $nome
            RETURN c";

        var updatedCategoria = session.ExecuteWriteAsync(async tx =>
        {
            var result = await tx.RunAsync(updateQuery, new
            {
                nomeAtual = categoriaEscolhida.Nome,
                nome
            });
            return await result.SingleAsync();
        }).Result;

        var updatedNode = updatedCategoria["c"].As<INode>();
        Console.WriteLine($"Categoria atualizada com sucesso: Nome: {updatedNode["nome"].As<string>()}");
    }


}
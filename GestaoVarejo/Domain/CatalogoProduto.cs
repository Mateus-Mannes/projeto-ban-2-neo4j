using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Neo4j.Driver;

namespace GestaoVarejo.Domain;

[Table("catalogo_produto")]
[Display(Name = "Catálogo De Produtos")]
public class CatalogoProduto : IQueryableEntity<CatalogoProduto>
{
    [Column("nome")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;
    [Column("descricao")]
    [Display(Name = "Descrição")]
    public string Descricao { get; set; } = string.Empty;
    [Column("preco")]
    [Display(Name = "Preço")]
    public decimal Preco { get; set; }

    public Categoria Categoria { get; set; } = null!;

    public void FillValues(IReadOnlyDictionary<string, object> values)
    {
        if (values.TryGetValue("nome", out var nome))
        {
            Nome = nome.ToString()!;
        }
        if (values.TryGetValue("descricao", out var descricao))
        {
            Descricao = descricao.ToString()!;
        }
        if (values.TryGetValue("preco", out var preco))
        {
            Preco = decimal.Parse(preco.ToString()!);
        }
    }

    public static List<CatalogoProduto> GetAll(IAsyncSession session)
    {
        var query = @"
            MATCH (p:catalogo_produto)<-[:CONTAINS]-(c:categoria)
            RETURN p, c";

        var catalogoProdutos = new List<CatalogoProduto>();

        var result = session.RunAsync(query).Result;
        result.ForEachAsync(record =>
        {
            var produtoNode = record["p"].As<INode>();
            var categoriaNode = record["c"].As<INode>();

            var catalogoProduto = new CatalogoProduto();
            catalogoProduto.FillValues(produtoNode.Properties);

            var categoria = new Categoria();
            categoria.FillValues(categoriaNode.Properties);
            catalogoProduto.Categoria = categoria;

            catalogoProdutos.Add(catalogoProduto);
        }).Wait();

        return catalogoProdutos;
    }

    public override string ToString()
    {
        return $"Nome: {Nome}, Descrição: {Descricao}, Preço: {Preco}, Categoria: {Categoria?.Nome ?? "N/A"}";
    }

    public static void Create(IAsyncSession session)
    {
        Console.WriteLine("Digite o nome do produto (obrigatório)*: ");
        var nomeProduto = Console.ReadLine()!;

        if (string.IsNullOrWhiteSpace(nomeProduto))
        {
            Console.WriteLine("O nome é obrigatório e não pode ser vazio.");
            return;
        }

        Console.WriteLine("Digite a descrição do produto (obrigatório)*: ");
        var descricaoProduto = Console.ReadLine()!;

        if (string.IsNullOrWhiteSpace(descricaoProduto))
        {
            Console.WriteLine("A descrição é obrigatória e não pode ser vazia.");
            return;
        }

        Console.WriteLine("Digite o preço do produto (obrigatório)*: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal precoProduto) || precoProduto <= 0)
        {
            Console.WriteLine("Preço inválido. Por favor, insira um número positivo.");
            return;
        }

        // Recuperar categorias disponíveis usando GetAll
        Console.WriteLine("Escolha uma categoria para o produto:");
        var categorias = Categoria.GetAll(session);
        if (categorias.Count == 0)
        {
            Console.WriteLine("Não há categorias disponíveis.");
            return;
        }

        for (int i = 0; i < categorias.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {categorias[i].Nome}");
        }

        Console.WriteLine("Selecione o número da categoria:");
        if (!int.TryParse(Console.ReadLine(), out int categoriaIndex) || categoriaIndex < 1 || categoriaIndex > categorias.Count)
        {
            Console.WriteLine("Seleção inválida. Por favor, selecione um número da lista.");
            return;
        }
        var categoriaEscolhida = categorias[categoriaIndex - 1];

        var query = @"
            MATCH (c:categoria {nome: $nomeCategoria})
            CREATE (cp:catalogo_produto {nome: $nomeProduto, descricao: $descricaoProduto, preco: $precoProduto})<-[:CONTAINS]-(c)
            RETURN cp";

        // Executar a criação do produto e retornar o resultado
        var result = session.ExecuteWriteAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, new { nomeCategoria = categoriaEscolhida.Nome, nomeProduto, descricaoProduto, precoProduto });
            return await cursor.SingleAsync();
        }).Result;

        var createdNode = result["cp"].As<INode>();
        Console.WriteLine($"Produto criado com sucesso: {createdNode["nome"].As<string>()}");
    }

    public static void Delete(IAsyncSession session)
    {
        // Listar todos os catálogos de produtos
        var catalogoProdutos = GetAll(session);
        if (catalogoProdutos.Count == 0)
        {
            Console.WriteLine("Não há catálogos de produtos para deletar.");
            return;
        }

        Console.WriteLine("Selecione o catálogo de produtos que deseja deletar:");
        for (int i = 0; i < catalogoProdutos.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {catalogoProdutos[i]}");
        }

        Console.WriteLine("Digite o número do catálogo de produtos para deletar:");
        if (!int.TryParse(Console.ReadLine(), out int catalogoIndex) || catalogoIndex < 1 || catalogoIndex > catalogoProdutos.Count)
        {
            Console.WriteLine("Seleção inválida. Por favor, selecione um número válido da lista.");
            return;
        }

        var catalogoEscolhido = catalogoProdutos[catalogoIndex - 1];

        // Verificar se o catálogo de produtos está relacionado a algum produto
        var checkRelationQuery = @"
            MATCH (cp:catalogo_produto {nome: $nomeProduto})<-[:CATEGORIZADO_COM]-(p:produto)
            RETURN COUNT(p) AS count";

        var checkResult = session.ExecuteReadAsync(async tx =>
        {
            var result = await tx.RunAsync(checkRelationQuery, new { nomeProduto = catalogoEscolhido.Nome });
            return await result.SingleAsync();
        }).Result;

        if (checkResult["count"].As<int>() > 0)
        {
            Console.WriteLine("Não é possível deletar o catálogo de produtos porque existem produtos associados.");
            return;
        }

        // Deletar o catálogo de produtos desfazendo a relação com a categoria
        var deleteQuery = @"
            MATCH (cp:catalogo_produto {nome: $nomeProduto})<-[r:CONTAINS]-(c:categoria)
            DELETE r
            WITH cp
            DELETE cp";

        session.ExecuteWriteAsync(async tx =>
        {
            await tx.RunAsync(deleteQuery, new { nomeProduto = catalogoEscolhido.Nome });
        }).Wait();

        Console.WriteLine($"Catálogo de produtos '{catalogoEscolhido.Nome}' deletado com sucesso.");
    }


}
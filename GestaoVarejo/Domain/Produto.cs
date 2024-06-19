using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Neo4j.Driver;

namespace GestaoVarejo.Domain;

[Table("produto")]
[Display(Name = "Produto")]
public class Produto : IQueryableEntity<Produto>
{
    public string Codigo { get; set; } = null!;
    [Column("data_fabricacao")]
    [Display(Name = "Data Fabricação")]
    public DateTime DataFabricacao { get; set; }
    [Column("data_validade")]
    [Display(Name = "Data Validade")]
    public DateTime? DataValidade { get; set; }
    [Column("data_entrega")]
    [Display(Name = "Data Entrega")]
    public DateTime? DataEntrega { get; set; } 
    [Column("valor_compra")]
    [Display(Name = "Valor Compra")]
    public decimal ValorUnitarioCompra { get; set; }

    public Venda Venda { get; set; } = null!;
    public CatalogoProduto CatalogoProduto { get; set; } = null!;
    public Compra Compra { get; set; } = null!;

    public INode Node { get; set; } = null!;

    public override string ToString()
    {
        return $"Codigo: {Codigo}, " +
               $"Produto: {CatalogoProduto.Nome}, " +
               $"Fabricação: {DataFabricacao.ToString("yyyy-MM-dd")}, " +
               $"Validade: {(DataValidade.HasValue ? DataValidade.Value.ToString("yyyy-MM-dd") : "N/A")}, " +
               $"Entrega: {(DataEntrega.HasValue ? DataEntrega.Value.ToString("yyyy-MM-dd") : "N/A")}, " +
               $"Valor Unitário de Compra: {ValorUnitarioCompra}, " +
               $"Compra: {Compra.Nfe} " +
               $"Venda: {Venda?.Nfe ?? "N/A"}";
    }

    public void FillValues(IReadOnlyDictionary<string, object> values)
    {
        if (values.TryGetValue("codigo", out var codigo))
        {
            Codigo = codigo.ToString()!;
        }
        if (values.TryGetValue("data_fabricacao", out var dataFabricacao))
        {
            DataFabricacao = DateTime.Parse(dataFabricacao.ToString()!);
        }
        if (values.TryGetValue("data_validade", out var dataValidade))
        {
            DataValidade = DateTime.Parse(dataValidade.ToString()!);
        }
        if (values.TryGetValue("data_entrega", out var dataEntrega))
        {
            DataEntrega = DateTime.Parse(dataEntrega.ToString()!);
        }
        if (values.TryGetValue("valor_compra", out var valorCompra))
        {
            ValorUnitarioCompra = Convert.ToDecimal(valorCompra);
        }
    }

    public static List<Produto> GetAll(IAsyncSession session)
    {
        var query = @"
            MATCH (p:produto)-[:CATEGORIZADO_COM]->(cp:catalogo_produto),
                  (p)-[:PARTE_DE]->(compra)
            OPTIONAL MATCH (p)-[:VENDIDO]->(venda:venda)
            RETURN p, cp, compra, venda";

        var produtos = new List<Produto>();
        var result = session.RunAsync(query).Result;

        result.ForEachAsync(record =>
        {
            var produtoNode = record["p"].As<INode>();
            var catalogoProdutoNode = record["cp"].As<INode>();
            var compraNode = record["compra"].As<INode>();
            var vendaNode = record.ContainsKey("venda") ? record["venda"].As<INode>() : null;

            var produto = new Produto() { Node = produtoNode };
            produto.FillValues(produtoNode.Properties);

            var catalogoProduto = new CatalogoProduto();
            catalogoProduto.FillValues(catalogoProdutoNode.Properties);
            produto.CatalogoProduto = catalogoProduto;

            var compra = new Compra();
            compra.FillValues(compraNode.Properties);
            produto.Compra = compra;

            if (vendaNode != null)
            {
                var venda = new Venda();
                venda.FillValues(vendaNode.Properties);
                produto.Venda = venda;
            }

            produtos.Add(produto);
        }).Wait();

        return produtos;
    }

    public static void Create(IAsyncSession session, INode? compraNode = null)
    {
        Console.WriteLine("Digite o código do produto (obrigatório)*:");
        var codigo = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(codigo))
        {
            Console.WriteLine("O código é obrigatório e não pode ser vazio.");
            return;
        }

        // Seleção do catálogo de produtos
        Console.WriteLine("Escolha um catálogo de produtos para o produto:");
        var catalogos = CatalogoProduto.GetAll(session);
        if (catalogos.Count == 0)
        {
            Console.WriteLine("Não há catálogos de produtos disponíveis.");
            return;
        }

        for (int i = 0; i < catalogos.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {catalogos[i].Nome}");
        }

        Console.WriteLine("Selecione o número do catálogo:");
        if (!int.TryParse(Console.ReadLine(), out int catalogoIndex) || catalogoIndex < 1 || catalogoIndex > catalogos.Count)
        {
            Console.WriteLine("Seleção inválida. Por favor, selecione um número da lista.");
            return;
        }
        var catalogoEscolhido = catalogos[catalogoIndex - 1];

        // Se nenhuma compra foi especificada, selecione uma
        if (compraNode == null)
        {
            Console.WriteLine("Selecione uma compra existente para vincular ao produto:");
            var compras = Compra.GetAll(session);
            if (compras.Count == 0)
            {
                Console.WriteLine("Não há compras disponíveis.");
                return;
            }

            for (int i = 0; i < compras.Count; i++)
            {
                Console.WriteLine($"{i + 1}. NFE: {compras[i].Nfe}, Data: {compras[i].Data.ToString("yyyy-MM-dd")}");
            }

            Console.WriteLine("Selecione o número da compra:");
            if (!int.TryParse(Console.ReadLine(), out int compraIndex) || compraIndex < 1 || compraIndex > compras.Count)
            {
                Console.WriteLine("Seleção inválida. Por favor, selecione um número da lista.");
                return;
            }
            compraNode = compras[compraIndex - 1].Node;
        }

        // Detalhes do produto
        Console.WriteLine("Digite a data de fabricação do produto (formato YYYY-MM-DD):");
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime dataFabricacao))
        {
            Console.WriteLine("Data de fabricação inválida. Por favor, use o formato YYYY-MM-DD.");
            return;
        }

        Console.WriteLine("Digite a data de validade do produto (formato YYYY-MM-DD, opcional):");
        DateTime? dataValidade = null;
        var dataValidadeStr = Console.ReadLine();
        if (!string.IsNullOrEmpty(dataValidadeStr) && DateTime.TryParse(dataValidadeStr, out DateTime validade))
        {
            dataValidade = validade;
        }

        Console.WriteLine("Digite o valor de compra do produto:");
        if (!decimal.TryParse(Console.ReadLine(), out decimal valorCompra))
        {
            Console.WriteLine("Valor de compra inválido. Por favor, insira um valor decimal válido.");
            return;
        }

        // Opção de vincular uma venda ao produto
        Console.WriteLine("Deseja vincular uma venda ao produto? (sim/não)");
        INode? vendaNode = null;
        if (Console.ReadLine()!.Trim().ToLower() == "sim")
        {
            var vendas = Venda.GetAll(session);
            if (vendas.Count == 0)
            {
                Console.WriteLine("Não há vendas disponíveis.");
            }
            else
            {
                for (int i = 0; i < vendas.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. NFE: {vendas[i].Nfe}, Data: {vendas[i].Data.ToString("yyyy-MM-dd")}");
                }
                Console.WriteLine("Selecione o número da venda:");
                if (int.TryParse(Console.ReadLine(), out int vendaIndex) && vendaIndex >= 1 && vendaIndex <= vendas.Count)
                {
                    vendaNode = vendas[vendaIndex - 1].Node;
                }
            }
        }

        var query = @"
            MATCH (cp:catalogo_produto {nome: $catalogoNome}), (compra:compra {nfe: $compraNfe})
            CREATE (p:produto {
                codigo: $codigo,
                data_fabricacao: $dataFabricacao,
                data_validade: $dataValidade,
                valor_compra: $valorCompra
            })-[:CATEGORIZADO_COM]->(cp),
            (p)-[:PARTE_DE]->(compra)";
        
        if (vendaNode != null)
        {
            query += @"
            WITH p
            MATCH (venda:venda {nfe: $vendaNfe})
            CREATE (p)-[:VENDIDO]->(venda)";
        }

        query += " RETURN p";

        var createdProduct = session.ExecuteWriteAsync(async tx =>
        {
            var result = await tx.RunAsync(query, new
            {
                catalogoNome = catalogoEscolhido.Nome,
                codigo,
                compraNfe = compraNode.Properties["nfe"].As<string>(),
                dataFabricacao,
                dataValidade,
                valorCompra,
                vendaNfe = vendaNode?.Properties["nfe"]?.As<string>()
            });
            return (await result.SingleAsync())["p"].As<INode>();
        }).Result;

        Console.WriteLine($"Produto criado com sucesso: Data de fabricação: {createdProduct["data_fabricacao"].As<DateTime>().ToString("yyyy-MM-dd")}");
    }

    public static void Create(IAsyncSession session)
    {
        Create(session, null);
    }

    public static void Delete(IAsyncSession session)
    {
        // Listar todos os produtos
        var produtos = GetAll(session);
        if (produtos.Count == 0)
        {
            Console.WriteLine("Não há produtos disponíveis para deletar.");
            return;
        }

        Console.WriteLine("Selecione o produto que deseja deletar:");
        for (int i = 0; i < produtos.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {produtos[i]}"); // Mostra os detalhes de cada produto
        }

        Console.WriteLine("Digite o número do produto para deletar:");
        if (!int.TryParse(Console.ReadLine(), out int produtoIndex) || produtoIndex < 1 || produtoIndex > produtos.Count)
        {
            Console.WriteLine("Seleção inválida. Por favor, selecione um número válido da lista.");
            return;
        }

        var produtoEscolhido = produtos[produtoIndex - 1];

        // Construir e executar a query de deleção
        var query = @"
            MATCH (p:produto {codigo: $codigo})
            DELETE p";

        session.ExecuteWriteAsync(async tx =>
        {
            await tx.RunAsync(query, new { codigo = produtoEscolhido.Codigo });
        }).Wait();

        Console.WriteLine($"Produto com código '{produtoEscolhido.Codigo}' deletado com sucesso.");
    }

}

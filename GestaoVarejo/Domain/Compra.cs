using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Neo4j.Driver;

namespace GestaoVarejo.Domain;

[Table("compra")]
[Display(Name = "Compra")]
public class Compra : IQueryableEntity<Compra>
{
    [Column("nfe")]
    [Display(Name = "NFE")]
    public string Nfe { get; set; } = string.Empty;
    [Column("data")]
    [Display(Name = "Data")]
    public DateTime Data { get; set; } = DateTime.Now;

    public Fornecedor Fornecedor { get; set; } = null!;

    public INode Node { get; set; } = null!;

    public override string ToString()
    {
        return $"NFE: {Nfe}, Data: {Data.ToString("yyyy-MM-dd")}, Fornecedor Cnpj: {Fornecedor.Cnpj}";
    }

    public void FillValues(IReadOnlyDictionary<string, object> values)
    {
        if (values.TryGetValue("nfe", out var nfe))
        {
            Nfe = nfe.ToString()!;
        }
        if (values.TryGetValue("data", out var data))
        {
            Data = DateTime.Parse(data.ToString()!);
        }
    }

    public static List<Compra> GetAll(IAsyncSession session)
    {
        var query = @"
            MATCH (c:compra)<-[:FORNECE]-(f:fornecedor)
            RETURN c, f";

        var compras = new List<Compra>();
        var result = session.RunAsync(query).Result;

        result.ForEachAsync(record =>
        {
            var compraNode = record["c"].As<INode>();
            var fornecedorNode = record["f"].As<INode>();

            var compra = new Compra() { Node = compraNode };
            compra.FillValues(compraNode.Properties);

            var fornecedor = new Fornecedor();
            fornecedor.FillValues(fornecedorNode.Properties);
            compra.Fornecedor = fornecedor;

            compras.Add(compra);
        }).Wait();

        return compras;
    }

    public static void Create(IAsyncSession session)
    {
        Console.WriteLine("Digite a NFE da compra (obrigatório)*:");
        var nfe = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(nfe))
        {
            Console.WriteLine("A NFE é obrigatória e não pode ser vazia.");
            return;
        }

        Console.WriteLine("Digite a data da compra (formato YYYY-MM-DD) (obrigatório)*:");
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime data))
        {
            Console.WriteLine("Data inválida. Por favor, use o formato YYYY-MM-DD.");
            return;
        }

        Console.WriteLine("Escolha um fornecedor para a compra:");
        var fornecedores = Fornecedor.GetAll(session);
        if (fornecedores.Count == 0)
        {
            Console.WriteLine("Não há fornecedores disponíveis.");
            return;
        }

        for (int i = 0; i < fornecedores.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {fornecedores[i].Cnpj} - {fornecedores[i].Email}");
        }

        Console.WriteLine("Selecione o número do fornecedor (obrigatório)*:");
        if (!int.TryParse(Console.ReadLine(), out int fornecedorIndex) || fornecedorIndex < 1 || fornecedorIndex > fornecedores.Count)
        {
            Console.WriteLine("Seleção inválida. Por favor, selecione um número da lista.");
            return;
        }
        var fornecedorEscolhido = fornecedores[fornecedorIndex - 1];

        var createCompraQuery = @"
            MATCH (f:fornecedor {cnpj: $cnpj})
            CREATE (compra:compra {nfe: $nfe, data: $data})<-[:FORNECE]-(f)
            RETURN compra";

        var compraNode = session.ExecuteWriteAsync(async tx =>
        {
            var result = await tx.RunAsync(createCompraQuery, new
            {
                cnpj = fornecedorEscolhido.Cnpj,
                nfe,
                data
            });
            return (await result.SingleAsync())["compra"].As<INode>();
        }).Result;

        Console.WriteLine("Quantos produtos deseja adicionar à compra?");
        if (int.TryParse(Console.ReadLine(), out int numberOfProducts) && numberOfProducts > 0)
        {
            for (int i = 0; i < numberOfProducts; i++)
            {
                Console.WriteLine($"Adicionando produto {i + 1} de {numberOfProducts}");
                Produto.Create(session, compraNode);
                Console.WriteLine();
            }
        }

        Console.WriteLine($"Compra criada com sucesso: NFE: {nfe}, Data: {data.ToString("yyyy-MM-dd")}");
    }

    public static void Delete(IAsyncSession session)
    {
        // Listar todas as compras
        var compras = GetAll(session);
        if (compras.Count == 0)
        {
            Console.WriteLine("Não há compras disponíveis para deletar.");
            return;
        }

        Console.WriteLine("Selecione a compra que deseja deletar:");
        for (int i = 0; i < compras.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {compras[i]}"); // Mostra os detalhes de cada compra
        }

        Console.WriteLine("Digite o número da compra para deletar:");
        if (!int.TryParse(Console.ReadLine(), out int compraIndex) || compraIndex < 1 || compraIndex > compras.Count)
        {
            Console.WriteLine("Seleção inválida. Por favor, selecione um número válido da lista.");
            return;
        }

        var compraEscolhida = compras[compraIndex - 1];

        // Construir e executar a query de deleção
        var query = @"
            MATCH (c:compra {nfe: $nfe}) DELETE c";

        session.ExecuteWriteAsync(async tx =>
        {
            await tx.RunAsync(query, new { nfe = compraEscolhida.Nfe });
        }).Wait();

        Console.WriteLine($"Compra '{compraEscolhida.Nfe}' deletada com sucesso.");
    }

}

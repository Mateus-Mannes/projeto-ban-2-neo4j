using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Neo4j.Driver;

namespace GestaoVarejo.Domain;

[Table("venda")]
[Display(Name = "Venda")]
public class Venda : IQueryableEntity<Venda>
{
    [Column("nfe")]
    [Display(Name = "NFE")]
    public string Nfe { get; set; } = string.Empty;
    [Column("data")]
    [Display(Name = "Data")]
    public DateTime Data { get; set; }
    [Column("valor")]
    [Display(Name = "Valor")]
    public decimal Valor { get; set; }

    public Cliente Cliente { get; set; } = null!;
    public Funcionario Funcionario { get; set; } = null!;

    public INode Node { get; set; } = null!;

    public override string ToString()
    {
        return $"NFE: {Nfe}, Data: {Data.ToString("yyyy-MM-dd")}, Valor: {Valor}, Cliente Cpf: {Cliente.Cpf}, Funcionário Cpf: {Funcionario.Cpf}";
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
        if (values.TryGetValue("valor", out var valor))
        {
            Valor = Convert.ToDecimal(valor);
        }
    }

    public static List<Venda> GetAll(IAsyncSession session)
    {
        var query = @"
         MATCH (v:venda)<-[:FEZ]-(c:cliente), (v)-[:ATENDIDO_POR]->(f:funcionario)
            RETURN v, c, f
        ";

        var vendas = new List<Venda>();
        var result = session.RunAsync(query).Result;

        result.ForEachAsync(record =>
        {
            var vendaNode = record["v"].As<INode>();
            var clienteNode = record["c"].As<INode>();
            var funcionarioNode = record["f"].As<INode>();

            var venda = new Venda() { Node = vendaNode };
            venda.FillValues(vendaNode.Properties);

            var cliente = new Cliente();
            cliente.FillValues(clienteNode.Properties);
            venda.Cliente = cliente;

            var funcionario = new Funcionario();
            funcionario.FillValues(funcionarioNode.Properties);
            venda.Funcionario = funcionario;

            vendas.Add(venda);
        }).Wait();

        return vendas;
    }

    public static void Create(IAsyncSession session)
    {
        Console.WriteLine("Digite a NFE da venda (obrigatório)*:");
        var nfe = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(nfe))
        {
            Console.WriteLine("A NFE é obrigatória e não pode ser vazia.");
            return;
        }

        Console.WriteLine("Digite a data da venda (formato YYYY-MM-DD):");
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime data))
        {
            Console.WriteLine("Data inválida. Por favor, use o formato YYYY-MM-DD.");
            return;
        }

        Console.WriteLine("Digite o valor total da venda:");
        if (!decimal.TryParse(Console.ReadLine(), out decimal valor))
        {
            Console.WriteLine("Valor inválido. Por favor, insira um valor decimal válido.");
            return;
        }

        // Selecionar o cliente
        Console.WriteLine("Selecione um cliente para a venda:");
        var clientes = Cliente.GetAll(session);
        if (clientes.Count == 0)
        {
            Console.WriteLine("Não há clientes disponíveis.");
            return;
        }

        for (int i = 0; i < clientes.Count; i++)
        {
            Console.WriteLine($"{i + 1}. CPF: {clientes[i].Cpf}, Nome: {clientes[i].Nome} {clientes[i].UltimoNome}");
        }

        Console.WriteLine("Selecione o número do cliente:");
        if (!int.TryParse(Console.ReadLine(), out int clienteIndex) || clienteIndex < 1 || clienteIndex > clientes.Count)
        {
            Console.WriteLine("Seleção inválida. Por favor, selecione um número da lista.");
            return;
        }
        var clienteEscolhido = clientes[clienteIndex - 1];

        // Selecionar o funcionário
        Console.WriteLine("Selecione um funcionário para registrar a venda:");
        var funcionarios = Funcionario.GetAll(session);
        if (funcionarios.Count == 0)
        {
            Console.WriteLine("Não há funcionários disponíveis.");
            return;
        }

        for (int i = 0; i < funcionarios.Count; i++)
        {
            Console.WriteLine($"{i + 1}. CPF: {funcionarios[i].Cpf}, Nome: {funcionarios[i].Nome} {funcionarios[i].UltimoNome}");
        }

        Console.WriteLine("Selecione o número do funcionário:");
        if (!int.TryParse(Console.ReadLine(), out int funcionarioIndex) || funcionarioIndex < 1 || funcionarioIndex > funcionarios.Count)
        {
            Console.WriteLine("Seleção inválida. Por favor, selecione um número da lista.");
            return;
        }
        var funcionarioEscolhido = funcionarios[funcionarioIndex - 1];

        // Criar a venda
        var query = @"
            CREATE (v:venda {nfe: $nfe, data: $data, valor: $valor})
            WITH v
            MATCH (c:cliente {cpf: $clienteCpf}), (f:funcionario {cpf: $funcionarioCpf})
            CREATE (c)-[:FEZ]->(v)-[:ATENDIDO_POR]->(f)
            RETURN v";

        var vendaNode = session.ExecuteWriteAsync(async tx =>
        {
            var result = await tx.RunAsync(query, new
            {
                nfe,
                data,
                valor,
                clienteCpf = clienteEscolhido.Cpf,
                funcionarioCpf = funcionarioEscolhido.Cpf
            });
            return (await result.SingleAsync())["v"].As<INode>();
        }).Result;

        Console.WriteLine("Venda criada com sucesso. Deseja associar produtos a esta venda? (sim/não)");
        if (Console.ReadLine()!.ToLower() == "sim")
        {
            var produtos = Produto.GetAll(session).Where(x => x.Venda == null).ToList();
            if (produtos.Count == 0)
            {
                Console.WriteLine("Não há produtos disponíveis para vender.");
                return;
            }

            for (int i = 0; i < produtos.Count; i++)
            {
                Console.WriteLine($"{i + 1}. Produto: {produtos[i].CatalogoProduto.Nome}, Codigo: {produtos[i].Codigo}, Fabricado: {produtos[i].DataFabricacao.ToString("yyyy-MM-dd")}");
            }

            Console.WriteLine("Digite os números dos produtos para associar à venda, separados por vírgula:");
            var produtoIndexes = Console.ReadLine()!.Split(',').Select(int.Parse);
            foreach (var idx in produtoIndexes)
            {
                if (idx >= 1 && idx <= produtos.Count)
                {
                    var produtoEscolhido = produtos[idx - 1];
                    var linkQuery = @"
                        MATCH (p:produto {codigo: $codigo}), (v:venda {nfe: $vendaNfe})
                        CREATE (p)-[:VENDIDO]->(v)";
                    session.ExecuteWriteAsync(tx => tx.RunAsync(linkQuery, new { codigo = produtoEscolhido.Codigo, vendaNfe = nfe }));
                    Console.WriteLine($"Produto {produtoEscolhido.CatalogoProduto.Nome} adicionado à venda.");
                }
            }
        }
    }


    public static void Delete(IAsyncSession session)
    {
        throw new NotImplementedException();
    }
}

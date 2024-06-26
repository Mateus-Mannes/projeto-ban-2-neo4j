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
        // Listar todas as vendas disponíveis
        var vendas = GetAll(session);
        if (vendas.Count == 0)
        {
            Console.WriteLine("Não há vendas disponíveis para deletar.");
            return;
        }

        Console.WriteLine("Selecione a venda que deseja deletar:");
        for (int i = 0; i < vendas.Count; i++)
        {
            Console.WriteLine($"{i + 1}. NFE: {vendas[i].Nfe}, Data: {vendas[i].Data.ToString("yyyy-MM-dd")}, Valor: {vendas[i].Valor}");
        }

        Console.WriteLine("Digite o número da venda para deletar:");
        if (!int.TryParse(Console.ReadLine(), out int vendaIndex) || vendaIndex < 1 || vendaIndex > vendas.Count)
        {
            Console.WriteLine("Seleção inválida. Por favor, selecione um número válido da lista.");
            return;
        }

        var vendaEscolhida = vendas[vendaIndex - 1];

        // Executar a query de deleção
        var query = @"
            MATCH (v:venda {nfe: $nfe})
            OPTIONAL MATCH (v)<-[r1:FEZ]-(c:cliente)
            OPTIONAL MATCH (v)-[r2:ATENDIDO_POR]->(f:funcionario)
            OPTIONAL MATCH (v)<-[r3:VENDIDO]-(p:produto)
            DELETE r1, r2, r3, v";

        session.ExecuteWriteAsync(async tx =>
        {
            await tx.RunAsync(query, new { nfe = vendaEscolhida.Nfe });
        }).Wait();

        Console.WriteLine($"Venda com NFE '{vendaEscolhida.Nfe}' deletada com sucesso.");
    }

    public static void Update(IAsyncSession session)
    {
        // Listar todas as vendas disponíveis
        var vendas = GetAll(session);
        if (vendas.Count == 0)
        {
            Console.WriteLine("Não há vendas disponíveis para atualizar.");
            return;
        }

        Console.WriteLine("Selecione a venda que deseja atualizar:");
        for (int i = 0; i < vendas.Count; i++)
        {
            Console.WriteLine($"{i + 1}. NFE: {vendas[i].Nfe}, Data: {vendas[i].Data.ToString("yyyy-MM-dd")}, Valor: {vendas[i].Valor}");
        }

        Console.WriteLine("Digite o número da venda para atualizar:");
        if (!int.TryParse(Console.ReadLine(), out int vendaIndex) || vendaIndex < 1 || vendaIndex > vendas.Count)
        {
            Console.WriteLine("Seleção inválida. Por favor, selecione um número válido da lista.");
            return;
        }

        var vendaEscolhida = vendas[vendaIndex - 1];

        // Solicitar novos valores e validar
        Console.WriteLine($"NFE atual: {vendaEscolhida.Nfe}");
        Console.WriteLine("Digite uma nova NFE ou pressione ENTER para manter a atual:");
        var nfe = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(nfe))
        {
            nfe = vendaEscolhida.Nfe; // Mantém o atual se ENTER for pressionado sem alterações
        }

        Console.WriteLine($"Data atual: {vendaEscolhida.Data.ToString("yyyy-MM-dd")}");
        Console.WriteLine("Digite uma nova data (formato YYYY-MM-DD) ou pressione ENTER para manter a atual:");
        var dataStr = Console.ReadLine();
        DateTime data;
        if (string.IsNullOrWhiteSpace(dataStr))
        {
            data = vendaEscolhida.Data; // Mantém o atual se ENTER for pressionado sem alterações
        }
        else if (!DateTime.TryParse(dataStr, out data))
        {
            Console.WriteLine("Data inválida. Por favor, use o formato YYYY-MM-DD.");
            return;
        }

        Console.WriteLine($"Valor atual: {vendaEscolhida.Valor}");
        Console.WriteLine("Digite um novo valor ou pressione ENTER para manter o atual:");
        var valorStr = Console.ReadLine();
        decimal valor;
        if (string.IsNullOrWhiteSpace(valorStr))
        {
            valor = vendaEscolhida.Valor; // Mantém o atual se ENTER for pressionado sem alterações
        }
        else if (!decimal.TryParse(valorStr, out valor))
        {
            Console.WriteLine("Valor inválido. Por favor, insira um valor decimal válido.");
            return;
        }

        // Verificar se campos obrigatórios ainda são nulos após entrada do usuário
        if (string.IsNullOrWhiteSpace(nfe) || data == default || valor <= 0)
        {
            Console.WriteLine("Os campos obrigatórios (NFE, Data e Valor) devem ser preenchidos.");
            return;
        }

        // Atualizar a venda no banco de dados
        var updateQuery = @"
            MATCH (v:venda {nfe: $oldNfe})
            SET v.nfe = $nfe, v.data = $data, v.valor = $valor
            RETURN v";

        var updatedVenda = session.ExecuteWriteAsync(async tx =>
        {
            var result = await tx.RunAsync(updateQuery, new
            {
                oldNfe = vendaEscolhida.Nfe,
                nfe,
                data,
                valor
            });
            return await result.SingleAsync();
        }).Result;

        var updatedNode = updatedVenda["v"].As<INode>();
        Console.WriteLine($"Venda atualizada com sucesso: NFE: {updatedNode["nfe"].As<string>()}, Data: {updatedNode["data"].As<DateTime>().ToString("yyyy-MM-dd")}, Valor: {updatedNode["valor"].As<decimal>()}");
    }

}

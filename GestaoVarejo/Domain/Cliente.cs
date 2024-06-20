using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Neo4j.Driver;

namespace GestaoVarejo.Domain;

[Table("cliente")]
[Display(Name = "Cliente")]
public class Cliente : IQueryableEntity<Cliente>
{
    [Column("cpf")]
    [Display(Name = "CPF")]
    public string Cpf { get; set; } = string.Empty;
    [Column("nome")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;
    [Column("ultimo_nome")]
    [Display(Name = "Último Nome")]
    public string UltimoNome { get; set; } = string.Empty;
    [Column("telefone")]
    [Display(Name = "Telefone")]
    public string Telefone { get; set; } = string.Empty;
    [Column("email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    public Endereco Endereco { get; set; } = null!;
    
    public void FillValues(IReadOnlyDictionary<string, object> values)
    {
        if (values.TryGetValue("cpf", out var cpf))
        {
            Cpf = cpf.ToString()!;
        }
        if (values.TryGetValue("nome", out var nome))
        {
            Nome = nome.ToString()!;
        }
        if (values.TryGetValue("ultimo_nome", out var ultimoNome))
        {
            UltimoNome = ultimoNome.ToString()!;
        }
        if (values.TryGetValue("telefone", out var telefone))
        {
            Telefone = telefone.ToString()!;
        }
        if (values.TryGetValue("email", out var email))
        {
            Email = email.ToString()!;
        }
    }

    public static List<Cliente> GetAll(IAsyncSession session)
    {
        var query = @"
            MATCH (c:cliente)-[:RESIDE_EM]->(e:endereco)
            RETURN c, e";

        var clientes = new List<Cliente>();
        var result = session.RunAsync(query).Result;

        result.ForEachAsync(record =>
        {
            var clienteNode = record["c"].As<INode>();
            var enderecoNode = record["e"].As<INode>();

            var cliente = new Cliente();
            cliente.FillValues(clienteNode.Properties);

            var endereco = new Endereco();
            endereco.FillValues(enderecoNode.Properties);
            cliente.Endereco = endereco;

            clientes.Add(cliente);
        }).Wait();

        return clientes;
    }

    public override string ToString()
    {
        return $"Cliente: {Nome} {UltimoNome}, CPF: {Cpf}, Telefone: {Telefone}, Email: {Email}, Rua: {Endereco.Rua} ";
    }

    public static void Create(IAsyncSession session)
    {
        Console.WriteLine("Digite o CPF do cliente (obrigatório)*:");
        var cpf = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(cpf))
        {
            Console.WriteLine("O CPF é obrigatório e não pode ser vazio.");
            return;
        }

        Console.WriteLine("Digite o nome do cliente (obrigatório)*:");
        var nome = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(nome))
        {
            Console.WriteLine("O nome é obrigatório e não pode ser vazio.");
            return;
        }

        Console.WriteLine("Digite o último nome do cliente:");
        var ultimoNome = Console.ReadLine();

        Console.WriteLine("Digite o telefone do cliente:");
        var telefone = Console.ReadLine();

        Console.WriteLine("Digite o email do cliente:");
        var email = Console.ReadLine();

        // Implementação para escolher o endereço do cliente
        Console.WriteLine("Escolha um endereço para o cliente:");
        var enderecos = Endereco.GetAll(session);
        if (enderecos.Count == 0)
        {
            Console.WriteLine("Não há endereços disponíveis.");
            return;
        }

        for (int i = 0; i < enderecos.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {enderecos[i].ToString()}");
        }

        Console.WriteLine("Selecione o número do endereço:");
        if (!int.TryParse(Console.ReadLine(), out int enderecoIndex) || enderecoIndex < 1 || enderecoIndex > enderecos.Count)
        {
            Console.WriteLine("Seleção inválida. Por favor, selecione um número da lista.");
            return;
        }

        var enderecoEscolhido = enderecos[enderecoIndex - 1];

        var query = @"
            MATCH (e:endereco {cidade: $cidade, bairro: $bairro, rua: $rua, numero: $numero, estado: $estado})
            CREATE (c:cliente {cpf: $cpf, nome: $nome, ultimo_nome: $ultimoNome, telefone: $telefone, email: $email})-[:RESIDE_EM]->(e)
            RETURN c";

        var result = session.ExecuteWriteAsync(tx =>
        {
            return tx.RunAsync(query, new
            {
                cpf,
                nome,
                ultimoNome,
                telefone,
                email,
                cidade = enderecoEscolhido.Cidade,
                bairro = enderecoEscolhido.Bairro,
                rua = enderecoEscolhido.Rua,
                numero = enderecoEscolhido.Numero,
                estado = enderecoEscolhido.Estado
            });
        }).Result;

        Console.WriteLine("Cliente criado com sucesso!");
    }

    public static void Delete(IAsyncSession session)
    {
        // Listar todos os clientes
        var clientes = GetAll(session);
        if (clientes.Count == 0)
        {
            Console.WriteLine("Não há clientes disponíveis para deletar.");
            return;
        }

        Console.WriteLine("Selecione o cliente que deseja deletar:");
        for (int i = 0; i < clientes.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {clientes[i]}"); // Mostra os detalhes de cada cliente
        }

        Console.WriteLine("Digite o número do cliente para deletar:");
        if (!int.TryParse(Console.ReadLine(), out int clienteIndex) || clienteIndex < 1 || clienteIndex > clientes.Count)
        {
            Console.WriteLine("Seleção inválida. Por favor, selecione um número válido da lista.");
            return;
        }

        var clienteEscolhido = clientes[clienteIndex - 1];

        // Verificar se o cliente está envolvido em alguma venda
        var checkVendaQuery = @"
            MATCH (c:cliente {cpf: $cpf})-[:FEZ]->(v:venda)
            RETURN COUNT(v) AS count";

        var checkResult = session.ExecuteReadAsync(async tx =>
        {
            var result = await tx.RunAsync(checkVendaQuery, new { cpf = clienteEscolhido.Cpf });
            return await result.SingleAsync();
        }).Result;

        if (checkResult["count"].As<int>() > 0)
        {
            Console.WriteLine("Não é possível deletar o cliente porque ele está associado a vendas.");
            return;
        }

        // Deletar o cliente desfazendo a relação com o endereço
        var deleteQuery = @"
            MATCH (c:cliente {cpf: $cpf})-[r:RESIDE_EM]->(e:endereco)
            DELETE r
            WITH c
            DELETE c";

        session.ExecuteWriteAsync(async tx =>
        {
            await tx.RunAsync(deleteQuery, new { cpf = clienteEscolhido.Cpf });
        }).Wait();

        Console.WriteLine($"Cliente '{clienteEscolhido.Nome} {clienteEscolhido.UltimoNome}' deletado com sucesso.");
    }

    public static void Update(IAsyncSession session)
    {
        // Listar todos os clientes
        var clientes = GetAll(session);
        if (clientes.Count == 0)
        {
            Console.WriteLine("Não há clientes disponíveis para atualizar.");
            return;
        }

        Console.WriteLine("Selecione o cliente que deseja atualizar:");
        for (int i = 0; i < clientes.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {clientes[i]}");
        }

        Console.WriteLine("Digite o número do cliente para atualizar:");
        if (!int.TryParse(Console.ReadLine(), out int clienteIndex) || clienteIndex < 1 || clienteIndex > clientes.Count)
        {
            Console.WriteLine("Seleção inválida. Por favor, selecione um número válido da lista.");
            return;
        }

        var clienteEscolhido = clientes[clienteIndex - 1];

        // Solicitar novos valores e validar
        Console.WriteLine($"Nome atual: {clienteEscolhido.Nome}");
        Console.WriteLine("Digite um novo nome ou pressione ENTER para manter o atual:");
        var nome = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(nome))
        {
            nome = clienteEscolhido.Nome; // Mantém o atual se ENTER for pressionado sem alterações
        }

        Console.WriteLine($"Último Nome atual: {clienteEscolhido.UltimoNome}");
        Console.WriteLine("Digite um novo último nome ou pressione ENTER para manter o atual:");
        var ultimoNome = Console.ReadLine();
        ultimoNome = string.IsNullOrEmpty(ultimoNome) ? clienteEscolhido.UltimoNome : ultimoNome;

        // Telefone e Email podem ser nulos
        Console.WriteLine($"Telefone atual: {clienteEscolhido.Telefone}");
        Console.WriteLine("Digite um novo telefone ou pressione ENTER para manter o atual (ou digite 'null' para limpar):");
        var telefone = Console.ReadLine();
        if (string.IsNullOrEmpty(telefone))
        {
            telefone = clienteEscolhido.Telefone; // Mantém o atual se ENTER for pressionado sem alterações
        }
        else if (telefone.Trim().ToLower() == "null")
        {
            telefone = null; // Permitir nulo se explicitamente indicado
        }

        Console.WriteLine($"Email atual: {clienteEscolhido.Email}");
        Console.WriteLine("Digite um novo email ou pressione ENTER para manter o atual (ou digite 'null' para limpar):");
        var email = Console.ReadLine();
        if (string.IsNullOrEmpty(email))
        {
            email = clienteEscolhido.Email; // Mantém o atual se ENTER for pressionado sem alterações
        }
        else if (email.Trim().ToLower() == "null")
        {
            email = null; // Permitir nulo se explicitamente indicado
        }

        // Atualizar o cliente no banco de dados
        var updateQuery = @"
            MATCH (c:cliente {cpf: $cpf})
            SET c.nome = $nome, c.ultimo_nome = $ultimoNome, c.telefone = $telefone, c.email = $email
            RETURN c";

        var updatedCliente = session.ExecuteWriteAsync(async tx =>
        {
            var result = await tx.RunAsync(updateQuery, new
            {
                cpf = clienteEscolhido.Cpf,
                nome,
                ultimoNome,
                telefone,
                email
            });
            return await result.SingleAsync();
        }).Result;

        var updatedNode = updatedCliente["c"].As<INode>();
        Console.WriteLine($"Cliente atualizado com sucesso: Nome: {updatedNode["nome"].As<string>()}, Email: {updatedNode["email"].As<string>()}");
    }


}
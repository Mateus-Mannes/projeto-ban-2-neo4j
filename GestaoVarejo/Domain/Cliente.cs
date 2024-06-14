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

}
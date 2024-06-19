using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Neo4j.Driver;

namespace GestaoVarejo.Domain;

[Table("funcionario")]
[Display(Name = "Funcionário")]
public class Funcionario : IQueryableEntity<Funcionario>
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
    [Column("salario")]
    [Display(Name = "Salário")]
    public decimal Salario { get; set; }
    [Column("email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    public Endereco Endereco { get; set; } = null!;

    public override string ToString()
    {
        return $"Funcionário: {Nome} {UltimoNome}, CPF: {Cpf}, Salário: {Salario}, Email: {Email}, Rua: {Endereco.Rua}";
    }

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
        if (values.TryGetValue("salario", out var salario))
        {
            Salario = Convert.ToDecimal(salario);
        }
        if (values.TryGetValue("email", out var email))
        {
            Email = email.ToString()!;
        }
    }

    public static List<Funcionario> GetAll(IAsyncSession session)
    {
        var query = @"
            MATCH (f:funcionario)-[:TRABALHA_EM]->(e:endereco)
            RETURN f, e";

        var funcionarios = new List<Funcionario>();
        var result = session.RunAsync(query).Result;

        result.ForEachAsync(record =>
        {
            var funcionarioNode = record["f"].As<INode>();
            var enderecoNode = record["e"].As<INode>();

            var funcionario = new Funcionario();
            funcionario.FillValues(funcionarioNode.Properties);

            var endereco = new Endereco();
            endereco.FillValues(enderecoNode.Properties);
            funcionario.Endereco = endereco;

            funcionarios.Add(funcionario);
        }).Wait();

        return funcionarios;
    }

    public static void Create(IAsyncSession session)
    {
        Console.WriteLine("Digite o CPF do funcionário (obrigatório)*:");
        var cpf = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(cpf))
        {
            Console.WriteLine("O CPF é obrigatório e não pode ser vazio.");
            return;
        }

        Console.WriteLine("Digite o nome do funcionário (obrigatório)*:");
        var nome = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(nome))
        {
            Console.WriteLine("O nome é obrigatório e não pode ser vazio.");
            return;
        }

        Console.WriteLine("Digite o último nome do funcionário:");
        var ultimoNome = Console.ReadLine();

        Console.WriteLine("Digite o salário do funcionário (obrigatório)*:");
        if (!decimal.TryParse(Console.ReadLine(), out decimal salario) || salario <= 0)
        {
            Console.WriteLine("Salário inválido. Por favor, insira um número positivo.");
            return;
        }

        Console.WriteLine("Digite o email do funcionário:");
        var email = Console.ReadLine();

        // Selecionar um endereço existente para o funcionário
        Console.WriteLine("Escolha um endereço para o funcionário:");
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
            CREATE (f:funcionario {cpf: $cpf, nome: $nome, ultimo_nome: $ultimoNome, salario: $salario, email: $email})-[:TRABALHA_EM]->(e)
            RETURN f";

        var result = session.ExecuteWriteAsync(async tx =>
        {
            var result = await tx.RunAsync(query, new
            {
                cpf,
                nome,
                ultimoNome,
                salario,
                email,
                cidade = enderecoEscolhido.Cidade,
                bairro = enderecoEscolhido.Bairro,
                rua = enderecoEscolhido.Rua,
                numero = enderecoEscolhido.Numero,
                estado = enderecoEscolhido.Estado
            });

            return await result.SingleAsync();
        }).Result;

        var createdNode = result["f"].As<INode>();
        Console.WriteLine($"Funcionário criado com sucesso: CPF: {createdNode["cpf"].As<string>()}, Nome: {createdNode["nome"].As<string>()}");
    }

    public static void Delete(IAsyncSession session)
    {
        throw new NotImplementedException();
    }
}

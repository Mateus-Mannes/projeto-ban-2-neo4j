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
        // Listar todos os funcionários
        var funcionarios = GetAll(session);
        if (funcionarios.Count == 0)
        {
            Console.WriteLine("Não há funcionários disponíveis para deletar.");
            return;
        }

        Console.WriteLine("Selecione o funcionário que deseja deletar:");
        for (int i = 0; i < funcionarios.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {funcionarios[i]}"); // Mostra os detalhes de cada funcionário
        }

        Console.WriteLine("Digite o número do funcionário para deletar:");
        if (!int.TryParse(Console.ReadLine(), out int funcionarioIndex) || funcionarioIndex < 1 || funcionarioIndex > funcionarios.Count)
        {
            Console.WriteLine("Seleção inválida. Por favor, selecione um número válido da lista.");
            return;
        }

        var funcionarioEscolhido = funcionarios[funcionarioIndex - 1];

        // Verificar se o funcionário está envolvido em alguma venda
        var checkVendaQuery = @"
            MATCH (f:funcionario {cpf: $cpf})<-[:ATENDIDO_POR]-(v:venda)
            RETURN COUNT(v) AS count";

        var checkResult = session.ExecuteReadAsync(async tx =>
        {
            var result = await tx.RunAsync(checkVendaQuery, new { cpf = funcionarioEscolhido.Cpf });
            return await result.SingleAsync();
        }).Result;

        if (checkResult["count"].As<int>() > 0)
        {
            Console.WriteLine("Não é possível deletar o funcionário porque ele está associado a vendas.");
            return;
        }

        // Deletar o funcionário desfazendo a relação com o endereço
        var deleteQuery = @"
            MATCH (f:funcionario {cpf: $cpf})-[r:TRABALHA_EM]->(e:endereco)
            DELETE r
            WITH f
            DELETE f";

        session.ExecuteWriteAsync(async tx =>
        {
            await tx.RunAsync(deleteQuery, new { cpf = funcionarioEscolhido.Cpf });
        }).Wait();

        Console.WriteLine($"Funcionário '{funcionarioEscolhido.Nome} {funcionarioEscolhido.UltimoNome}' deletado com sucesso.");
    }

    public static void Update(IAsyncSession session)
    {
        // Listar todos os funcionários
        var funcionarios = GetAll(session);
        if (funcionarios.Count == 0)
        {
            Console.WriteLine("Não há funcionários disponíveis para atualizar.");
            return;
        }

        Console.WriteLine("Selecione o funcionário que deseja atualizar:");
        for (int i = 0; i < funcionarios.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {funcionarios[i]}");
        }

        Console.WriteLine("Digite o número do funcionário para atualizar:");
        if (!int.TryParse(Console.ReadLine(), out int funcionarioIndex) || funcionarioIndex < 1 || funcionarioIndex > funcionarios.Count)
        {
            Console.WriteLine("Seleção inválida. Por favor, selecione um número válido da lista.");
            return;
        }

        var funcionarioEscolhido = funcionarios[funcionarioIndex - 1];

        // Solicitar novos valores e validar
        Console.WriteLine($"CPF atual: {funcionarioEscolhido.Cpf}");
        Console.WriteLine("Digite um novo CPF ou pressione ENTER para manter o atual:");
        var cpf = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(cpf))
        {
            cpf = funcionarioEscolhido.Cpf; // Mantém o atual se ENTER for pressionado sem alterações
        }

        Console.WriteLine($"Nome atual: {funcionarioEscolhido.Nome}");
        Console.WriteLine("Digite um novo nome ou pressione ENTER para manter o atual:");
        var nome = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(nome))
        {
            nome = funcionarioEscolhido.Nome; // Mantém o atual se ENTER for pressionado sem alterações
        }

        Console.WriteLine($"Último Nome atual: {funcionarioEscolhido.UltimoNome}");
        Console.WriteLine("Digite um novo último nome ou pressione ENTER para manter o atual:");
        var ultimoNome = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(ultimoNome))
        {
            ultimoNome = funcionarioEscolhido.UltimoNome; // Mantém o atual se ENTER for pressionado sem alterações
        }

        Console.WriteLine($"Salário atual: {funcionarioEscolhido.Salario}");
        Console.WriteLine("Digite um novo salário ou pressione ENTER para manter o atual:");
        var salarioInput = Console.ReadLine();
        decimal salario;
        if (string.IsNullOrWhiteSpace(salarioInput))
        {
            salario = funcionarioEscolhido.Salario; // Mantém o atual se ENTER for pressionado sem alterações
        }
        else if (!decimal.TryParse(salarioInput, out salario) || salario <= 0)
        {
            Console.WriteLine("Salário inválido. Por favor, insira um número positivo.");
            return;
        }

        Console.WriteLine($"Email atual: {funcionarioEscolhido.Email}");
        Console.WriteLine("Digite um novo email ou pressione ENTER para manter o atual (ou digite 'null' para limpar):");
        var email = Console.ReadLine();
        if (string.IsNullOrEmpty(email))
        {
            email = funcionarioEscolhido.Email; // Mantém o atual se ENTER for pressionado sem alterações
        }
        else if (email.Trim().ToLower() == "null")
        {
            email = null; // Permitir nulo se explicitamente indicado
        }

        // Verificar se campos obrigatórios ainda são nulos após entrada do usuário
        if (string.IsNullOrWhiteSpace(cpf) || string.IsNullOrWhiteSpace(nome) || salario <= 0)
        {
            Console.WriteLine("Os campos obrigatórios (CPF, Nome e Salário) devem ser preenchidos.");
            return;
        }

        // Atualizar o funcionário no banco de dados
        var updateQuery = @"
            MATCH (f:funcionario {cpf: $oldCpf})
            SET f.cpf = $cpf, f.nome = $nome, f.ultimo_nome = $ultimoNome, f.salario = $salario, f.email = $email
            RETURN f";

        var updatedFuncionario = session.ExecuteWriteAsync(async tx =>
        {
            var result = await tx.RunAsync(updateQuery, new
            {
                oldCpf = funcionarioEscolhido.Cpf,
                cpf,
                nome,
                ultimoNome,
                salario,
                email
            });
            return await result.SingleAsync();
        }).Result;

        var updatedNode = updatedFuncionario["f"].As<INode>();
        Console.WriteLine($"Funcionário atualizado com sucesso: CPF: {updatedNode["cpf"].As<string>()}, Nome: {updatedNode["nome"].As<string>()}, Salário: {updatedNode["salario"].As<decimal>()}");
    }

}

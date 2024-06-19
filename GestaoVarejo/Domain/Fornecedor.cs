using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Neo4j.Driver;

namespace GestaoVarejo.Domain;

[Table("fornecedor")]
[Display(Name = "Fornecedor")]
public class Fornecedor : IQueryableEntity<Fornecedor>
{
    [Column("cnpj")]
    [Display(Name = "CNPJ")]
    public string Cnpj { get; set; } = string.Empty;
    [Column("email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
    [Column("telefone")]
    [Display(Name = "Telefone")]
    public string Telefone { get; set; } = string.Empty;

    public Endereco Endereco { get; set; } = null!;

    public override string ToString()
    {
        return $"CNPJ: {Cnpj}, Email: {Email}, Telefone: {Telefone}, Rua: {Endereco.Rua}";
    }

    public void FillValues(IReadOnlyDictionary<string, object> values)
    {
        if (values.TryGetValue("cnpj", out var cnpj))
        {
            Cnpj = cnpj.ToString()!;
        }
        if (values.TryGetValue("email", out var email))
        {
            Email = email.ToString()!;
        }
        if (values.TryGetValue("telefone", out var telefone))
        {
            Telefone = telefone.ToString()!;
        }
    }

    public static List<Fornecedor> GetAll(IAsyncSession session)
    {
        var query = @"
            MATCH (f:fornecedor)-[:LOCALIZADO_EM]->(e:endereco)
            RETURN f, e";

        var fornecedores = new List<Fornecedor>();
        var result = session.RunAsync(query).Result;

        result.ForEachAsync(record =>
        {
            var fornecedorNode = record["f"].As<INode>();
            var enderecoNode = record["e"].As<INode>();

            var fornecedor = new Fornecedor();
            fornecedor.FillValues(fornecedorNode.Properties);

            var endereco = new Endereco();
            endereco.FillValues(enderecoNode.Properties);
            fornecedor.Endereco = endereco;

            fornecedores.Add(fornecedor);
        }).Wait();

        return fornecedores;
    }

   public static void Create(IAsyncSession session)
    {
        Console.WriteLine("Digite o CNPJ do fornecedor (obrigatório)*:");
        var cnpj = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(cnpj))
        {
            Console.WriteLine("O CNPJ é obrigatório e não pode ser vazio.");
            return;
        }

        Console.WriteLine("Digite o email do fornecedor (obrigatório)*:");
        var email = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("O email é obrigatório e não pode ser vazio.");
            return;
        }

        Console.WriteLine("Digite o telefone do fornecedor:");
        var telefone = Console.ReadLine();

        // Selecionar um endereço existente para o fornecedor
        Console.WriteLine("Escolha um endereço para o fornecedor:");
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
            CREATE (f:fornecedor {cnpj: $cnpj, email: $email, telefone: $telefone})-[:LOCALIZADO_EM]->(e)
            RETURN f";

        var result = session.ExecuteWriteAsync(async tx =>
        {
            var result = await tx.RunAsync(query, new
            {
                cnpj,
                email,
                telefone,
                cidade = enderecoEscolhido.Cidade,
                bairro = enderecoEscolhido.Bairro,
                rua = enderecoEscolhido.Rua,
                numero = enderecoEscolhido.Numero,
                estado = enderecoEscolhido.Estado
            });
            return await result.SingleAsync();
        }).Result;

        var createdNode = result["f"].As<INode>();
        Console.WriteLine($"Fornecedor criado com sucesso: CNPJ: {createdNode["cnpj"].As<string>()}, Email: {createdNode["email"].As<string>()}");
    }

    public static void Delete(IAsyncSession session)
    {
        // Listar todos os fornecedores
        var fornecedores = GetAll(session);
        if (fornecedores.Count == 0)
        {
            Console.WriteLine("Não há fornecedores disponíveis para deletar.");
            return;
        }

        Console.WriteLine("Selecione o fornecedor que deseja deletar:");
        for (int i = 0; i < fornecedores.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {fornecedores[i]}"); // Mostra os detalhes de cada fornecedor
        }

        Console.WriteLine("Digite o número do fornecedor para deletar:");
        if (!int.TryParse(Console.ReadLine(), out int fornecedorIndex) || fornecedorIndex < 1 || fornecedorIndex > fornecedores.Count)
        {
            Console.WriteLine("Seleção inválida. Por favor, selecione um número válido da lista.");
            return;
        }

        var fornecedorEscolhido = fornecedores[fornecedorIndex - 1];

        // Verificar se o fornecedor está envolvido em alguma compra
        var checkCompraQuery = @"
            MATCH (f:fornecedor {cnpj: $cnpj})-[:FORNECE]->(c:compra)
            RETURN COUNT(c) AS count";

        var checkResult = session.ExecuteReadAsync(async tx =>
        {
            var result = await tx.RunAsync(checkCompraQuery, new { cnpj = fornecedorEscolhido.Cnpj });
            return await result.SingleAsync();
        }).Result;

        if (checkResult["count"].As<int>() > 0)
        {
            Console.WriteLine("Não é possível deletar o fornecedor porque ele está associado a compras.");
            return;
        }

        // Deletar o fornecedor desfazendo a relação com o endereço
        var deleteQuery = @"
            MATCH (f:fornecedor {cnpj: $cnpj})-[r:LOCALIZADO_EM]->(e:endereco)
            DELETE r
            WITH f
            DELETE f";

        session.ExecuteWriteAsync(async tx =>
        {
            await tx.RunAsync(deleteQuery, new { cnpj = fornecedorEscolhido.Cnpj });
        }).Wait();

        Console.WriteLine($"Fornecedor '{fornecedorEscolhido.Cnpj}' deletado com sucesso.");
    }


}

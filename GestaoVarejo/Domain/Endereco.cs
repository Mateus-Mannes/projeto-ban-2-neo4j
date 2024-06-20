using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Neo4j.Driver;

namespace GestaoVarejo.Domain;

[Table("endereco")]
[Display(Name = "Endereço")]
public class Endereco  : IQueryableEntity<Endereco>
{
    [Column("cidade")]
    [Display(Name = "Cidade")]
    public string Cidade { get; set; } = string.Empty;
    [Column("bairro")]
    [Display(Name = "Bairro")]   
    public string Bairro { get; set; } = string.Empty;
    [Column("rua")]
    [Display(Name = "Rua")]
    public string Rua { get; set; } = string.Empty;
    [Column("numero")]
    [Display(Name = "Número")]
    public string Numero { get; set; } = string.Empty;
    [Column("estado")]
    [Display(Name = "Estado")]
    public string Estado { get; set; } = string.Empty;

    public void FillValues(IReadOnlyDictionary<string, object> values)
    {
        if (values.TryGetValue("cidade", out var cidade))
        {
            Cidade = cidade.ToString()!;
        }
        if (values.TryGetValue("bairro", out var bairro))
        {
            Bairro = bairro.ToString()!;
        }
        if (values.TryGetValue("rua", out var rua))
        {
            Rua = rua.ToString()!;
        }
        if (values.TryGetValue("numero", out var numero))
        {
            Numero = numero.ToString()!;
        }
        if (values.TryGetValue("estado", out var estado))
        {
            Estado = estado.ToString()!;
        }
    }

    public static List<Endereco> GetAll(IAsyncSession session)
    {
        var query = @"
            MATCH (e:endereco)
            RETURN e";

        var enderecos = new List<Endereco>();
        var result = session.RunAsync(query).Result;

        result.ForEachAsync(record =>
        {
            var enderecoNode = record["e"].As<INode>();

            var endereco = new Endereco();
            endereco.FillValues(enderecoNode.Properties);

            enderecos.Add(endereco);
        }).Wait();

        return enderecos;
    }

    public override string ToString()
    {
        return $"Endereço: {Rua}, Nº {Numero}, Bairro: {Bairro}, Cidade: {Cidade}, Estado: {Estado}";
    }

    public static void Create(IAsyncSession session)
    {
        Console.WriteLine("Digite a cidade do endereço (obrigatório)*:");
        var cidade = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(cidade))
        {
            Console.WriteLine("A cidade é obrigatória e não pode ser vazia.");
            return;
        }

        Console.WriteLine("Digite o bairro do endereço (obrigatório)*:");
        var bairro = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(bairro))
        {
            Console.WriteLine("O bairro é obrigatório e não pode ser vazio.");
            return;
        }

        Console.WriteLine("Digite a rua do endereço (obrigatório)*:");
        var rua = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(rua))
        {
            Console.WriteLine("A rua é obrigatória e não pode ser vazia.");
            return;
        }

        Console.WriteLine("Digite o número do endereço (obrigatório)*:");
        var numero = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(numero))
        {
            Console.WriteLine("O número é obrigatório e não pode ser vazio.");
            return;
        }

        Console.WriteLine("Digite o estado do endereço (obrigatório)*:");
        var estado = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(estado))
        {
            Console.WriteLine("O estado é obrigatório e não pode ser vazio.");
            return;
        }

        var query = @"
            CREATE (e:endereco {cidade: $cidade, bairro: $bairro, rua: $rua, numero: $numero, estado: $estado})
            RETURN e";

        // Executar a criação do endereço e retornar o resultado
        var result = session.ExecuteWriteAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, new { cidade, bairro, rua, numero, estado });
            return await cursor.SingleAsync();
        }).Result;

        var createdNode = result["e"].As<INode>();
        Console.WriteLine($"Endereço criado com sucesso: {createdNode["cidade"].As<string>()}, {createdNode["bairro"].As<string>()}, {createdNode["rua"].As<string>()}, {createdNode["numero"].As<string>()}, {createdNode["estado"].As<string>()}");
    }

    public static void Delete(IAsyncSession session)
    {
        // Listar todos os endereços
        var enderecos = GetAll(session);
        if (enderecos.Count == 0)
        {
            Console.WriteLine("Não há endereços disponíveis para deletar.");
            return;
        }

        Console.WriteLine("Selecione o endereço que deseja deletar:");
        for (int i = 0; i < enderecos.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {enderecos[i]}"); // Mostra os detalhes de cada endereço
        }

        Console.WriteLine("Digite o número do endereço para deletar:");
        if (!int.TryParse(Console.ReadLine(), out int enderecoIndex) || enderecoIndex < 1 || enderecoIndex > enderecos.Count)
        {
            Console.WriteLine("Seleção inválida. Por favor, selecione um número válido da lista.");
            return;
        }

        var enderecoEscolhido = enderecos[enderecoIndex - 1];

        // Construir e executar a query de deleção
        var query = @"
            MATCH (e:endereco {cidade: $cidade, bairro: $bairro, rua: $rua, numero: $numero, estado: $estado})
            DELETE e";

        session.ExecuteWriteAsync(async tx =>
        {
            await tx.RunAsync(query, new
            {
                cidade = enderecoEscolhido.Cidade,
                bairro = enderecoEscolhido.Bairro,
                rua = enderecoEscolhido.Rua,
                numero = enderecoEscolhido.Numero,
                estado = enderecoEscolhido.Estado
            });
        }).Wait();

        Console.WriteLine($"Endereço '{enderecoEscolhido.Rua}, Nº {enderecoEscolhido.Numero}, Bairro: {enderecoEscolhido.Bairro}, Cidade: {enderecoEscolhido.Cidade}, Estado: {enderecoEscolhido.Estado}' deletado com sucesso.");
    }

    public static void Update(IAsyncSession session)
    {
        // Listar todos os endereços
        var enderecos = GetAll(session);
        if (enderecos.Count == 0)
        {
            Console.WriteLine("Não há endereços disponíveis para atualizar.");
            return;
        }

        Console.WriteLine("Selecione o endereço que deseja atualizar:");
        for (int i = 0; i < enderecos.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {enderecos[i]}");
        }

        Console.WriteLine("Digite o número do endereço para atualizar:");
        if (!int.TryParse(Console.ReadLine(), out int enderecoIndex) || enderecoIndex < 1 || enderecoIndex > enderecos.Count)
        {
            Console.WriteLine("Seleção inválida. Por favor, selecione um número válido da lista.");
            return;
        }

        var enderecoEscolhido = enderecos[enderecoIndex - 1];

        // Solicitar novos valores e validar
        Console.WriteLine($"Cidade atual: {enderecoEscolhido.Cidade}");
        Console.WriteLine("Digite uma nova cidade ou pressione ENTER para manter a atual:");
        var cidade = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(cidade))
        {
            cidade = enderecoEscolhido.Cidade; // Mantém a atual se ENTER for pressionado sem alterações
        }

        Console.WriteLine($"Bairro atual: {enderecoEscolhido.Bairro}");
        Console.WriteLine("Digite um novo bairro ou pressione ENTER para manter o atual:");
        var bairro = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(bairro))
        {
            bairro = enderecoEscolhido.Bairro; // Mantém o atual se ENTER for pressionado sem alterações
        }

        Console.WriteLine($"Rua atual: {enderecoEscolhido.Rua}");
        Console.WriteLine("Digite uma nova rua ou pressione ENTER para manter a atual:");
        var rua = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(rua))
        {
            rua = enderecoEscolhido.Rua; // Mantém a atual se ENTER for pressionado sem alterações
        }

        Console.WriteLine($"Número atual: {enderecoEscolhido.Numero}");
        Console.WriteLine("Digite um novo número ou pressione ENTER para manter o atual:");
        var numero = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(numero))
        {
            numero = enderecoEscolhido.Numero; // Mantém o atual se ENTER for pressionado sem alterações
        }

        Console.WriteLine($"Estado atual: {enderecoEscolhido.Estado}");
        Console.WriteLine("Digite um novo estado ou pressione ENTER para manter o atual:");
        var estado = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(estado))
        {
            estado = enderecoEscolhido.Estado; // Mantém o atual se ENTER for pressionado sem alterações
        }

        // Verificar se campos obrigatórios ainda são nulos após entrada do usuário
        if (string.IsNullOrWhiteSpace(cidade) || string.IsNullOrWhiteSpace(bairro) ||
            string.IsNullOrWhiteSpace(rua) || string.IsNullOrWhiteSpace(numero) || string.IsNullOrWhiteSpace(estado))
        {
            Console.WriteLine("Todos os campos obrigatórios devem ser preenchidos.");
            return;
        }

        // Atualizar o endereço no banco de dados
        var updateQuery = @"
            MATCH (e:endereco {cidade: $oldCidade, bairro: $oldBairro, rua: $oldRua, numero: $oldNumero, estado: $oldEstado})
            SET e.cidade = $cidade, e.bairro = $bairro, e.rua = $rua, e.numero = $numero, e.estado = $estado
            RETURN e";

        var updatedEndereco = session.ExecuteWriteAsync(async tx =>
        {
            var result = await tx.RunAsync(updateQuery, new
            {
                oldCidade = enderecoEscolhido.Cidade,
                oldBairro = enderecoEscolhido.Bairro,
                oldRua = enderecoEscolhido.Rua,
                oldNumero = enderecoEscolhido.Numero,
                oldEstado = enderecoEscolhido.Estado,
                cidade,
                bairro,
                rua,
                numero,
                estado
            });
            return await result.SingleAsync();
        }).Result;

        var updatedNode = updatedEndereco["e"].As<INode>();
        Console.WriteLine($"Endereço atualizado com sucesso: {updatedNode["cidade"].As<string>()}, {updatedNode["bairro"].As<string>()}, {updatedNode["rua"].As<string>()}, {updatedNode["numero"].As<string>()}, {updatedNode["estado"].As<string>()}");
    }


}
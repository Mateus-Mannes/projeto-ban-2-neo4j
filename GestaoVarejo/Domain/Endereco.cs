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


}
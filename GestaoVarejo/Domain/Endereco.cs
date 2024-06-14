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


}
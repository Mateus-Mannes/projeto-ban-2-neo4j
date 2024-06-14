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
        return $"CNPJ: {Cnpj}, Email: {Email}, Telefone: {Telefone}";
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
}

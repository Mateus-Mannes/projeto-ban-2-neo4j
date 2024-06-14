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
        return $"Cliente: {Nome} {UltimoNome}, CPF: {Cpf}, Telefone: {Telefone}, Email: {Email} ";
    }


}
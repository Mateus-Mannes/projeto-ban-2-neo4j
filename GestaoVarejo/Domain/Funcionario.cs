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
        return $"Funcionário: {Nome} {UltimoNome}, CPF: {Cpf}, Salário: {Salario}, Email: {Email}";
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
}

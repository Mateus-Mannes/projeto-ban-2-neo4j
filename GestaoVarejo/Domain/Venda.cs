using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Neo4j.Driver;

namespace GestaoVarejo.Domain;

[Table("venda")]
[Display(Name = "Venda")]
public class Venda : IQueryableEntity<Venda>
{
    [Column("nfe")]
    [Display(Name = "NFE")]
    public string Nfe { get; set; } = string.Empty;
    [Column("data")]
    [Display(Name = "Data")]
    public DateTime Data { get; set; }
    [Column("valor")]
    [Display(Name = "Valor")]
    public decimal Valor { get; set; }

    public Cliente Cliente { get; set; } = null!;
    public Funcionario Funcionario { get; set; } = null!;

    public INode Node { get; set; } = null!;

    public override string ToString()
    {
        return $"NFE: {Nfe}, Data: {Data.ToString("yyyy-MM-dd")}, Valor: {Valor}, Cliente Cpf: {Cliente.Cpf}, Funcionário Cpf: {Funcionario.Cpf}";
    }

    public void FillValues(IReadOnlyDictionary<string, object> values)
    {
        if (values.TryGetValue("nfe", out var nfe))
        {
            Nfe = nfe.ToString()!;
        }
        if (values.TryGetValue("data", out var data))
        {
            Data = DateTime.Parse(data.ToString()!);
        }
        if (values.TryGetValue("valor", out var valor))
        {
            Valor = Convert.ToDecimal(valor);
        }
    }

    public static List<Venda> GetAll(IAsyncSession session)
    {
        var query = @"
         MATCH (v:venda)<-[:FEZ]-(c:cliente), (v)-[:ATENDIDO_POR]->(f:funcionario)
            RETURN v, c, f
        ";

        var vendas = new List<Venda>();
        var result = session.RunAsync(query).Result;

        result.ForEachAsync(record =>
        {
            var vendaNode = record["v"].As<INode>();
            var clienteNode = record["c"].As<INode>();
            var funcionarioNode = record["f"].As<INode>();

            var venda = new Venda() { Node = vendaNode };
            venda.FillValues(vendaNode.Properties);

            var cliente = new Cliente();
            cliente.FillValues(clienteNode.Properties);
            venda.Cliente = cliente;

            var funcionario = new Funcionario();
            funcionario.FillValues(funcionarioNode.Properties);
            venda.Funcionario = funcionario;

            vendas.Add(venda);
        }).Wait();

        return vendas;
    }

    public static void Create(IAsyncSession session)
    {
        throw new NotImplementedException();
    }
}

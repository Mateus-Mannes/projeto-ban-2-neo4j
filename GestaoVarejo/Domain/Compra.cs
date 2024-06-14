using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Neo4j.Driver;

namespace GestaoVarejo.Domain;

[Table("compra")]
[Display(Name = "Compra")]
public class Compra : IQueryableEntity<Compra>
{
    [Column("nfe")]
    [Display(Name = "NFE")]
    public string Nfe { get; set; } = string.Empty;
    [Column("data")]
    [Display(Name = "Data")]
    public DateTime Data { get; set; } = DateTime.Now;

    public Fornecedor Fornecedor { get; set; } = null!;

    public override string ToString()
    {
        return $"NFE: {Nfe}, Data: {Data.ToString("yyyy-MM-dd")}, Fornecedor Cnpj: {Fornecedor.Cnpj}";
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
    }

    public static List<Compra> GetAll(IAsyncSession session)
    {
        var query = @"
            MATCH (c:compra)-[:FORNECE]->(f:fornecedor)
            RETURN c, f";

        var compras = new List<Compra>();
        var result = session.RunAsync(query).Result;

        result.ForEachAsync(record =>
        {
            var compraNode = record["c"].As<INode>();
            var fornecedorNode = record["f"].As<INode>();

            var compra = new Compra();
            compra.FillValues(compraNode.Properties);

            var fornecedor = new Fornecedor();
            fornecedor.FillValues(fornecedorNode.Properties);
            compra.Fornecedor = fornecedor;

            compras.Add(compra);
        }).Wait();

        return compras;
    }

    public static void Create(IAsyncSession session)
    {
        throw new NotImplementedException();
    }
}

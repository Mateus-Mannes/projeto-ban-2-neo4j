using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Neo4j.Driver;

namespace GestaoVarejo.Domain;

[Table("produto")]
[Display(Name = "Produto")]
public class Produto : IQueryableEntity<Produto>
{
    [Column("data_fabricacao")]
    [Display(Name = "Data Fabricação")]
    public DateTime DataFabricacao { get; set; }
    [Column("data_validade")]
    [Display(Name = "Data Validade")]
    public DateTime? DataValidade { get; set; }
    [Column("data_entrega")]
    [Display(Name = "Data Entrega")]
    public DateTime? DataEntrega { get; set; } 
    [Column("valor_compra")]
    [Display(Name = "Valor Compra")]
    public decimal ValorUnitarioCompra { get; set; }

    public Venda Venda { get; set; } = null!;
    public CatalogoProduto CatalogoProduto { get; set; } = null!;
    public Compra Compra { get; set; } = null!;

    public override string ToString()
    {
        return $"Produto: {CatalogoProduto.Nome}, " +
               $"Fabricação: {DataFabricacao.ToString("yyyy-MM-dd")}, " +
               $"Validade: {(DataValidade.HasValue ? DataValidade.Value.ToString("yyyy-MM-dd") : "N/A")}, " +
               $"Entrega: {(DataEntrega.HasValue ? DataEntrega.Value.ToString("yyyy-MM-dd") : "N/A")}, " +
               $"Valor Unitário de Compra: {ValorUnitarioCompra}, " +
               $"Compra: {Compra.Nfe} " +
               $"Venda: {Venda?.Nfe ?? "N/A"}";
    }

    public void FillValues(IReadOnlyDictionary<string, object> values)
    {
        if (values.TryGetValue("data_fabricacao", out var dataFabricacao))
        {
            DataFabricacao = DateTime.Parse(dataFabricacao.ToString()!);
        }
        if (values.TryGetValue("data_validade", out var dataValidade))
        {
            DataValidade = DateTime.Parse(dataValidade.ToString()!);
        }
        if (values.TryGetValue("data_entrega", out var dataEntrega))
        {
            DataEntrega = DateTime.Parse(dataEntrega.ToString()!);
        }
        if (values.TryGetValue("valor_compra", out var valorCompra))
        {
            ValorUnitarioCompra = Convert.ToDecimal(valorCompra);
        }
    }

    public static List<Produto> GetAll(IAsyncSession session)
    {
        var query = @"
            MATCH (p:produto)-[:CATEGORIZADO_COM]->(cp:catalogo_produto),
                  (p)-[:PARTE_DE]->(compra)
            OPTIONAL MATCH (p)-[:PARTE_DE]->(venda:venda)
            RETURN p, cp, compra, venda";

        var produtos = new List<Produto>();
        var result = session.RunAsync(query).Result;

        result.ForEachAsync(record =>
        {
            var produtoNode = record["p"].As<INode>();
            var catalogoProdutoNode = record["cp"].As<INode>();
            var compraNode = record["compra"].As<INode>();
            var vendaNode = record.ContainsKey("venda") ? record["venda"].As<INode>() : null;

            var produto = new Produto();
            produto.FillValues(produtoNode.Properties);

            var catalogoProduto = new CatalogoProduto();
            catalogoProduto.FillValues(catalogoProdutoNode.Properties);
            produto.CatalogoProduto = catalogoProduto;

            var compra = new Compra();
            compra.FillValues(compraNode.Properties);
            produto.Compra = compra;

            if (vendaNode != null)
            {
                var venda = new Venda();
                venda.FillValues(vendaNode.Properties);
                produto.Venda = venda;
            }

            produtos.Add(produto);
        }).Wait();

        return produtos;
    }

    public static void Create(IAsyncSession session)
    {
        throw new NotImplementedException();
    }
}

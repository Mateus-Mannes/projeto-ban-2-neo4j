using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Neo4j.Driver;

namespace GestaoVarejo.Domain;

[Table("catalogo_produto")]
[Display(Name = "Catálogo De Produtos")]
public class CatalogoProduto : IQueryableEntity<CatalogoProduto>
{
    [Column("nome")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;
    [Column("descricao")]
    [Display(Name = "Descrição")]
    public string Descricao { get; set; } = string.Empty;
    [Column("preco")]
    [Display(Name = "Preço")]
    public decimal Preco { get; set; }

    public Categoria Categoria { get; set; } = null!;

    public void FillValues(IReadOnlyDictionary<string, object> values)
    {
        if (values.TryGetValue("nome", out var nome))
        {
            Nome = nome.ToString()!;
        }
        if (values.TryGetValue("descricao", out var descricao))
        {
            Descricao = descricao.ToString()!;
        }
        if (values.TryGetValue("preco", out var preco))
        {
            Preco = decimal.Parse(preco.ToString()!);
        }
    }

    public static List<CatalogoProduto> GetAll(IAsyncSession session)
    {
        var query = @"
            MATCH (p:catalogo_produto)<-[:CONTAINS]-(c:categoria)
            RETURN p, c";

        var catalogoProdutos = new List<CatalogoProduto>();

        var result = session.RunAsync(query).Result;
        result.ForEachAsync(record =>
        {
            var produtoNode = record["p"].As<INode>();
            var categoriaNode = record["c"].As<INode>();

            var catalogoProduto = new CatalogoProduto();
            catalogoProduto.FillValues(produtoNode.Properties);

            var categoria = new Categoria();
            categoria.FillValues(categoriaNode.Properties);
            catalogoProduto.Categoria = categoria;

            catalogoProdutos.Add(catalogoProduto);
        }).Wait();

        return catalogoProdutos;
    }

    public override string ToString()
    {
        return $"Nome: {Nome}, Descrição: {Descricao}, Preço: {Preco}, Categoria: {Categoria?.Nome ?? "N/A"}";
    }
}
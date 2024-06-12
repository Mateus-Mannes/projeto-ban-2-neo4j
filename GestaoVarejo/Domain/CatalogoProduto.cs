using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoVarejo.Domain;

[Table("catalogo_produto")]
[Display(Name = "Catálogo De Produtos")]
public class CatalogoProduto : QueryableEntity
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
}
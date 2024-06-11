using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoVarejo.Domain;

[Table("produto")]
[Display(Name = "Produto")]
public class Produto  : QueryableEntity
{
    [Column("id")]
    [Display(Name = "Id")]
    public override int Id { get; set; }
    [Column("catalogo_produto_id")]
    [Display(Name = "Catalogo Produto Id")]
    [Fk<CatalogoProduto>]
    public int CatalogoProdutoId { get; set; } 
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
    [Column("compra_id")]
    [Display(Name = "Compra Id")]
    [Fk<Compra>]
    public int CompraId { get; set; } 
    [Column("venda_id")]
    [Display(Name = "Venda Id")]
    [Fk<Venda>]
    public int? VendaId { get; set; }
}
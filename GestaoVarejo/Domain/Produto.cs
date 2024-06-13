using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoVarejo.Domain;

[Table("produto")]
[Display(Name = "Produto")]
public class Produto  : QueryableEntity
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
}
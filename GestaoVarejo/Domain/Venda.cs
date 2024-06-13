using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoVarejo.Domain;

[Table("venda")]
[Display(Name = "Venda")]
public class Venda : QueryableEntity
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
}
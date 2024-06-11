using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoVarejo.Domain;

[Table("compra")]
[Display(Name = "Compra")]
public class Compra : QueryableEntity
{ 
    [Column("id")]
    [Display(Name = "Id")]
    public override int Id { get; set; }
    [Column("nfe")]
    [Display(Name = "NFE")]
    public string Nfe { get; set; } = string.Empty;
    [Column("data")]
    [Display(Name = "Data")]
    public DateTime Data { get; set; } = DateTime.Now;
    [Column("fornecedor_id")]
    [Display(Name = "Fornecedor Id")]
    [Fk<Fornecedor>]
    public int FornecedorId { get; set; }
}
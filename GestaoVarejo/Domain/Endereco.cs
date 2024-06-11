using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoVarejo.Domain;

[Table("endereco")]
[Display(Name = "Endereço")]
public class Endereco  : QueryableEntity
{
    [Column("id")]
    [Display(Name = "Id")]
    public override int Id { get; set; }
    [Column("cidade")]
    [Display(Name = "Cidade")]
    public string Cidade { get; set; } = string.Empty;
    [Column("bairro")]
    [Display(Name = "Bairro")]   
    public string Bairro { get; set; } = string.Empty;
    [Column("rua")]
    [Display(Name = "Rua")]
    public string Rua { get; set; } = string.Empty;
    [Column("numero")]
    [Display(Name = "Número")]
    public string Numero { get; set; } = string.Empty;
    [Column("estado")]
    [Display(Name = "Estado")]
    public string Estado { get; set; } = string.Empty;
}
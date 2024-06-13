using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoVarejo.Domain;

[Table("fornecedor")]
[Display(Name = "Fornecedor")]
public class Fornecedor : QueryableEntity
{
    [Column("cnpj")]
    [Display(Name = "Cnpj")]
    public string Cnpj { get; set; } = string.Empty;
    [Column("email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
    [Column("telefone")]
    [Display(Name = "Telefone")]
    public string Telefone { get; set; } = string.Empty;
}
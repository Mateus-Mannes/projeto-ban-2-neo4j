using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoVarejo.Domain;

[Table("cliente")]
[Display(Name = "Cliente")]
public class Cliente : QueryableEntity
{
    [Column("cpf")]
    [Display(Name = "CPF")]
    public string Cpf { get; set; } = string.Empty;
    [Column("nome")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;
    [Column("ultimo_nome")]
    [Display(Name = "Último Nome")]
    public string UltimoNome { get; set; } = string.Empty;
    [Column("telefone")]
    [Display(Name = "Telefone")]
    public string Telefone { get; set; } = string.Empty;
    [Column("email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}
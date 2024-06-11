using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoVarejo.Domain;

[Table("cliente")]
[Display(Name = "Cliente")]
public class Cliente : QueryableEntity
{
    [Column("id")]
    [Display(Name = "Id")]
    public override int Id { get; set; }
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
    [Column("endereco_id")]
    [Display(Name = "Endereço Id")]
    [Fk<Endereco>]
    public int EnderecoId { get; set; }
}
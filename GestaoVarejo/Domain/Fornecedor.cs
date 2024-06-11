using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoVarejo.Domain;

[Table("fornecedor")]
[Display(Name = "Fornecedor")]
public class Fornecedor : QueryableEntity
{
    [Column("id")]
    [Display(Name = "Id")]
    public override int Id { get; set; }
    [Column("nome")]
    [Display(Name = "Nome")]
    public string Cnpj { get; set; } = string.Empty;
    [Column("email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
    [Column("telefone")]
    [Display(Name = "Telefone")]
    public string Telefone { get; set; } = string.Empty;
    [Column("endereco_id")]
    [Display(Name = "Endereço Id")]
    [Fk<Endereco>]
    public int EnderecoId { get; set; }
}
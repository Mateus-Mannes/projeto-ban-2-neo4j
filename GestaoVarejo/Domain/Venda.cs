using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoVarejo.Domain;

[Table("venda")]
[Display(Name = "Venda")]
public class Venda : QueryableEntity
{
    [Column("id")]
    [Display(Name = "Id")]
    public override int Id { get; set; }
    [Column("nfe")]
    [Display(Name = "NFE")]
    public string Nfe { get; set; } = string.Empty;
    [Column("data")]
    [Display(Name = "Data")]
    public DateTime Data { get; set; }
    [Column("valor")]
    [Display(Name = "Valor")]
    public decimal Valor { get; set; }
    [Column("cliente_id")]
    [Fk<Cliente>]
    [Display(Name = "Cliente Id")]
    public int ClienteId { get; set; }
    [Column("funcionario_id")]
    [Display(Name = "Funcionário Id")]
    [Fk<Funcionario>]
    public int FuncionarioId { get; set; }
}
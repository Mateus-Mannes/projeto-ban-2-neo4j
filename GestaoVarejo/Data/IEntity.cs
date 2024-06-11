namespace GestaoVarejo;

public interface IEntity
{
    public int Id { get; set; }
    public void FillValues(string[] values);
}

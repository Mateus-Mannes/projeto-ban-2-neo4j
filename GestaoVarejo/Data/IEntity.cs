namespace GestaoVarejo;

public interface IEntity
{
        public void FillValues(IReadOnlyDictionary<string, object> values);
}

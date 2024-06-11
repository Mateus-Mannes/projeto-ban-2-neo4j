namespace GestaoVarejo;

public class FkAttribute<T> : Attribute where T : QueryableEntity
{
    public Type ReferencedType => typeof(T);
}
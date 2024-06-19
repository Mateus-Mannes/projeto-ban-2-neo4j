using Neo4j.Driver;

namespace GestaoVarejo;

public interface IQueryableEntity<T>
{
    public abstract static List<T> GetAll(IAsyncSession session);
    public abstract static void Create(IAsyncSession session);
    public abstract static void Delete(IAsyncSession session);
    // public abstract void Update(IAsyncSession session);
}
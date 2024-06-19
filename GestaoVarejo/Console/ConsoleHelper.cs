using Neo4j.Driver;

namespace GestaoVarejo;

public static class ConsoleHelper 
{
    public static void PrintEntityData<T>(IAsyncSession session, string nomeEntidade) where T : IQueryableEntity<T>
    {
        var items = T.GetAll(session);
        Console.WriteLine($"{nomeEntidade}'s:");
        foreach (var item in items)
        {
            Console.WriteLine(item.ToString());
        }
    }

    public static void CreateEntity<T>(IAsyncSession session) where T : IQueryableEntity<T>
    {
        T.Create(session);
    }
    
    public static void DeleteEntity<T>(IAsyncSession session) where T : IQueryableEntity<T>
    {
        T.Delete(session);
    }
}
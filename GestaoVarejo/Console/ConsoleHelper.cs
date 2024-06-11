using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Dapper;
using GestaoVarejo.Domain;

namespace GestaoVarejo;

public static class ConsoleHelper 
{
    public static void PrintEntityData<T>(Repository repository, string nomeEntidade, int? id = null) where T : QueryableEntity
    {
        var items = repository.GetAll<T>();
        if(id is null )Console.WriteLine($"{nomeEntidade}'s:");
        else Console.WriteLine($"{nomeEntidade} (id = {id}):");
        foreach (var item in items)
        {
            if(id is null || item.Id == id)
                Console.WriteLine(item.ToDisplayString());
        }
    }

    public static void CreateUpdateEntity<T>(Repository repository, string nomeEntidade, int? id = null, int? idCompra = null) where T :QueryableEntity
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

        var values = new List<string>();

        var idAtualizacao = string.Empty;
        if (id != null) idAtualizacao = $" (id = {id})";
            
        Console.WriteLine($"Preencha os valores para {nomeEntidade}{idAtualizacao}:");
        foreach (var property in properties)
        {
            // Ignora a propriedade Id se for autoincrementável/gerada pelo banco
            if (property.Name == "Id") continue;

            if(idCompra is not null && property.Name == "CompraId") 
            {
                values.Add(idCompra.ToString()!);
                continue;
            }
            else if(property.Name == "VendaId") 
            {
                values.Add(string.Empty);
                continue;
            }

            string aviso = ""; // Inicializa a variável de aviso vazia

            // Define o aviso baseado no tipo da propriedade e se é nullable
            bool isNullable = Nullable.GetUnderlyingType(property.PropertyType) != null;
            if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
            {
                aviso = $"formato yyyy-mm-dd, {(isNullable ? "opcional" : "obrigatório")}";
            }
            else if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?))
            {
                aviso = $"formato 00.00, {(isNullable ? "opcional" : "obrigatório")}";
            }
            else
            {
                aviso = isNullable ? "opcional" : "obrigatório";
            }

            if(property.Name.EndsWith("Id")) 
            {
                var atributoFk = property.GetCustomAttributes()
                    .FirstOrDefault(attr => attr.GetType().IsGenericType &&
                    attr.GetType().GetGenericTypeDefinition() == typeof(FkAttribute<>));
                if (atributoFk != null)
                {
                    // Extrai o tipo genérico associado ao atributo
                    Type referencedType = atributoFk.GetType().GenericTypeArguments[0];
                    var method = typeof(ConsoleHelper).GetMethod(nameof(ConsoleHelper.PrintEntityData))!
                        .MakeGenericMethod(new Type[] { referencedType });
                    method.Invoke(null, new object[] { repository, referencedType.GetCustomAttribute<DisplayAttribute>()!.Name!, null!});
                }
            }

            // Solicita a entrada do usuário com o aviso adequado
            Console.Write($"{property.Name} ({aviso}): ");
            string? value = Console.ReadLine();
            values.Add(value ?? string.Empty);
        }

        int idCriado = 0;
        // Chamada genérica ao método Create<T> usando reflection
        if(id is null) idCriado = repository.Create<T>(values.ToArray());
        else repository.Update<T>(id.Value, values.ToArray());

        // Criação de entidade composta
        if((nomeEntidade == "Venda" || nomeEntidade == "Compra") && id == null)
        {
            CreateEntidadeComposta(repository, nomeEntidade, idCriado);
        }
    }

    private static void CreateEntidadeComposta(Repository repository, string nomeEntidade, int idCriado) 
    {
        if(nomeEntidade == "Venda") 
        {
            AdicionarProdutosAVenda(repository, nomeEntidade, idCriado);
        }
        else if(nomeEntidade == "Compra") 
        {
            CadastrarProdutosCompra(repository, nomeEntidade, idCriado);
        }
    }

    private static void AdicionarProdutosAVenda(Repository repository, string nomeEntidade, int idCriado) 
    {
        Console.WriteLine("Produtos:");
            var query = """
                SELECT 
                    p.id,
                    cat.nome,
                    c.nome,
                    c.descricao,
                    c.preco
                FROM produto p 
                inner join catalogo_produto c
                    on p.catalogo_produto_id = c.id
                inner join categoria cat
                    on cat.id = c.categoria_id
                where p.venda_id IS NULL
                order by p.id
            """;
            var result = repository.ExecuteQuery(query);
            foreach(var row in result)
            {
                Console.WriteLine($"Id: {row[0]}, Categoria: {row[1]}, Nome: {row[2]}, Descricao: {row[3]}, Preço: {row[4]}");
            }
            Console.Write("Escreva os Ids dos produtos que fazem parte da venda (separados por vírgula): ");
            var ids = Console.ReadLine();
            if(ids is null) throw new ArgumentException();
            ids = ids!.Replace(" ", "");
            var idList = ids.Split(',').Select(x => int.Parse(x)).ToArray();
            var parametrosQuery = string.Join(", ", idList.Select((p, index) => $"@param{index}"));
            query = $"""
                UPDATE produto SET venda_id = @venda WHERE id in ({parametrosQuery})
            """;
            var parametros = new DynamicParameters();
            parametros.Add("venda", idCriado);
            for(int i = 0; i < idList.Length; i++) 
            {
                parametros.Add($"param{i}", idList[i]);
            }
            repository.Execute(query, parametros);
    }

    private static void CadastrarProdutosCompra (Repository repository, string nomeEntidade, int idCriado) 
    {
        Console.Write("Escreva a quantidade de produtos a serem cadastros na compra: ");
        var quantidade = int.Parse(Console.ReadLine()!);
        for(int i = 0; i < quantidade; i++) 
        {
            Console.WriteLine($"Produto {i+1}:");
            CreateUpdateEntity<Produto>(repository, "Produto", idCompra: idCriado);
        }
    }
}
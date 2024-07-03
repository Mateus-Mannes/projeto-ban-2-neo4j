using Neo4j.Driver;

namespace GestaoVarejo;

public class ReportService
{
    private readonly IDriver _driver;

    public ReportService(IDriver driver)
    {
        _driver = driver;
    }

    public async Task<List<(string NomeFuncionario, decimal ValorTotalVendas, string Estado)>> GetTopBestSellersInRangeAsync(DateTime dataInicial, DateTime dataFinal)
    {
        List<(string, decimal, string)> result = new();

        var query = @"
            MATCH (e:endereco)<-[:TRABALHA_EM]-(f:funcionario)<-[:ATENDIDO_POR]-(v:venda)
            WHERE date(v.data) >= date($DataInicial) AND date(v.data) <= date($DataFinal)
            WITH f, SUM(v.valor) AS TotalVendido, e.estado AS Estado
            ORDER BY TotalVendido DESC
            RETURN f.nome + ' ' + f.ultimo_nome AS NomeCompleto, TotalVendido, Estado
            LIMIT 3
        ";

        try
        {
            using var session = _driver.AsyncSession();
            result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(query, new { DataInicial = dataInicial, DataFinal = dataFinal });

                while (await cursor.FetchAsync())
                {
                    var nomeCompleto = cursor.Current["NomeCompleto"].As<string>();
                    var totalVendido = cursor.Current["TotalVendido"].As<decimal>();
                    var estado = cursor.Current["Estado"].As<string>();

                    result.Add((nomeCompleto, totalVendido, estado));
                }

                return result;
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        return result;
    }

    public async Task<List<(string NomeCliente, decimal ValorTotalCompras, string Cidade)>> GetTopClientsInRangeAsync(DateTime dataInicial, DateTime dataFinal)
    {
        List<(string, decimal, string)> result = new();

        var query = @"
            MATCH (cl:cliente)-[:FEZ]->(v:venda)
            WHERE date(v.data) >= date($DataInicial) AND date(v.data) <= date($DataFinal)
            WITH cl, SUM(v.valor) AS ValorTotalCompras
            MATCH (cl)-[:RESIDE_EM]->(e:endereco)
            RETURN cl.nome + ' ' + cl.ultimo_nome AS NomeCliente, ValorTotalCompras, e.cidade AS Cidade
            ORDER BY ValorTotalCompras DESC
            LIMIT 3
        ";

        try
        {
            using var session = _driver.AsyncSession();
            result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(query, new { DataInicial = dataInicial, DataFinal = dataFinal });
                
                while (await cursor.FetchAsync())
                {
                    var nomeCliente = cursor.Current["NomeCliente"].As<string>();
                    var valorTotalCompras = cursor.Current["ValorTotalCompras"].As<decimal>();
                    var cidade = cursor.Current["Cidade"].As<string>();

                    result.Add((nomeCliente, valorTotalCompras, cidade));
                }
                
                return result;
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        return result;
    }

    public async Task<List<(string Cidade, string NomeFuncionario, string NomeCliente, DateTime DataVenda, decimal ValorTotalVenda)>> GetSalesByRegionInRangeAsync(DateTime dataInicial, DateTime dataFinal)
    {
        List<(string, string, string, DateTime, decimal)> result = new();

        var query = @"
            MATCH (cl:cliente)-[:FEZ]->(v:venda)-[:ATENDIDO_POR]->(f:funcionario)
            MATCH (cl)-[:RESIDE_EM]->(e:endereco)
            WHERE date(v.data) >= date($DataInicial) AND date(v.data) <= date($DataFinal)
            RETURN e.cidade AS Cidade, f.nome + ' ' + f.ultimo_nome AS NomeFuncionario, cl.nome + ' ' + cl.ultimo_nome AS NomeCliente, v.data AS DataVenda, v.valor AS ValorTotalVenda
            ORDER BY Cidade, DataVenda DESC
        ";

        try
        {
            using var session = _driver.AsyncSession();
            result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(query, new { DataInicial = dataInicial, DataFinal = dataFinal });
                
                while (await cursor.FetchAsync())
                {
                    var cidade = cursor.Current["Cidade"].As<string>();
                    var nomeFuncionario = cursor.Current["NomeFuncionario"].As<string>();
                    var nomeCliente = cursor.Current["NomeCliente"].As<string>();
                    var dataVenda = cursor.Current["DataVenda"].As<DateTime>();
                    var valorTotalVenda = cursor.Current["ValorTotalVenda"].As<decimal>();

                    result.Add((cidade, nomeFuncionario, nomeCliente, dataVenda, valorTotalVenda));
                }
                
                return result;
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        return result;
    }

    public async Task<List<(string NomeProduto, DateTime DataFabricacao, DateTime? DataValidade, DateTime DataCompra, string EmailFornecedor, decimal PrecoProduto, int QuantidadeComprada, decimal ValorTotalCompra, decimal Frete)>> GetProductPurchaseInRangeAsync(DateTime dataInicial, DateTime dataFinal)
    {
        List<(string, DateTime, DateTime?, DateTime, string, decimal, int, decimal, decimal)> result = new();

        var query = @"
            MATCH (p:produto)-[:CATEGORIZADO_COM]->(cp:catalogo_produto), (p)-[:PARTE_DE]->(c:compra)-[:FORNECE]-(f:fornecedor)
            WHERE date(c.data) >= date($DataInicial) AND date(c.data) <= date($DataFinal)
            RETURN cp.nome AS NomeProduto, p.data_fabricacao AS DataFabricacao, p.data_validade AS DataValidade, c.data AS DataCompra, f.email AS EmailFornecedor,
                cp.preco AS PrecoProduto, SUM(p.valor_compra) AS ValorTotalCompra, toInteger(SUM(p.valor_compra / cp.preco)) AS QuantidadeComprada, SUM(p.valor_compra % cp.preco) AS Frete
            ORDER BY DataValidade DESC

        ";

        try
        {
            using var session = _driver.AsyncSession();
            result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(query, new { DataInicial = dataInicial, DataFinal = dataFinal });
                return await cursor.ToListAsync(record =>
                    (record["NomeProduto"].As<string>(),
                    record["DataFabricacao"].As<DateTime>(),
                    record["DataValidade"].As<DateTime?>(),
                    record["DataCompra"].As<DateTime>(),
                    record["EmailFornecedor"].As<string>(),
                    record["PrecoProduto"].As<decimal>(),
                    record["QuantidadeComprada"].As<int>(),
                    record["ValorTotalCompra"].As<decimal>(),
                    record["Frete"].As<decimal>())
                );
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        return result;
    }
}

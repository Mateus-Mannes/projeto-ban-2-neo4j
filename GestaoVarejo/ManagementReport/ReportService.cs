using System.Data;
using Dapper;

namespace GestaoVarejo;

public class ReportService
{
    private readonly IDbConnection _dbContext;

    public ReportService(IDbConnection connection)
    {
        _dbContext = connection;
    }

    public List<(string NomeFuncionario, decimal ValorTotalVendas, string Estado)> GetTopBestSellersInRange(DateTime dataInicial, DateTime dataFinal)
    {
        var query = @"
            SELECT CONCAT(f.nome, ' ', f.ultimo_nome) AS NomeFuncionario,
                SUM(v.valor) AS ValorTotalVendas,
                e.estado AS Estado
            FROM Venda v
            INNER JOIN Funcionario f ON v.funcionario_id = f.Id
            INNER JOIN Endereco e ON f.endereco_id = e.Id
            WHERE v.data >= @DataInicial
                AND v.data < @DataFinal
            GROUP BY NomeFuncionario, Estado
            ORDER BY SUM(v.valor) DESC
            LIMIT 3
            ";

        var results = _dbContext.Query<(string, decimal, string)>(query, new
        {
            DataInicial = dataInicial,
            DataFinal = dataFinal
        }).AsList();

        var bestSellers = new List<(string, decimal, string)>(results);

        return bestSellers;
    }

    public List<(string NomeCliente, decimal ValorTotalCompras, string Cidade, string Estado)> GetTopClientsInRange(DateTime dataInicial, DateTime dataFinal)
    {
        var query = @"
            SELECT CONCAT(c.nome, ' ', c.ultimo_nome) AS NomeCliente,
                SUM(v.valor) AS ValorTotalCompras,
                e.cidade AS Cidade,
                e.estado AS Estado
            FROM Cliente c
            INNER JOIN Venda v ON c.Id = v.cliente_id
            INNER JOIN Endereco e ON c.endereco_id = e.Id
            WHERE v.data >= @DataInicial
                AND v.data < @DataFinal
            GROUP BY NomeCliente, Cidade, Estado
            ORDER BY SUM(v.valor) DESC
            LIMIT 3;
        ";

        var results = _dbContext.Query<(string, decimal, string, string)>(query, new
        {
            DataInicial = dataInicial,
            DataFinal = dataFinal
        }).AsList();

        var topClients = new List<(string, decimal, string, string)>(results);

        return topClients;
    }

    public List<(string Estado, string NomeFuncionario, string NomeCliente, DateTime DataVenda, decimal ValorTotalVenda)> GetSalesByRegionInRange(DateTime dataInicial, DateTime dataFinal)
    {
        var query = @"
            SELECT e.estado AS Estado,
                CONCAT(f.nome, ' ', f.ultimo_nome) AS NomeFuncionario,
                CONCAT(c.nome, ' ', c.ultimo_nome) AS NomeCliente,
                v.data AS DataVenda,
                v.valor AS ValorTotalVenda
            FROM Venda v
            INNER JOIN Funcionario f 
                ON v.funcionario_id = f.id
            INNER JOIN Cliente c 
                ON v.cliente_id = c.id
            INNER JOIN Endereco e 
                ON c.endereco_id = e.id
            WHERE v.data >= @DataInicial
                AND v.data < @DataFinal
            ORDER BY e.estado, v.data DESC;
        ";

        var results = _dbContext.Query<(string Estado, string NomeFuncionario, string NomeCliente, DateTime DataVenda, decimal ValorTotalVenda)>(query, new
        {
            DataInicial = dataInicial,
            DataFinal = dataFinal
        }).AsList();

        return results;
    }

    public List<(string NomeProduto, 
        DateTime DataFabricacao, 
        DateTime? DataValidade, 
        DateTime DataCompra,
        string EmailFornecedor,
        decimal PrecoProduto,
        int QuantidadeComprada,
        decimal ValorTotalCompra,
        decimal Frete)> 
    GetProductPurchaseInRange(DateTime dataInicial, DateTime dataFinal)
    {
        dataFinal = dataFinal.AddDays(1); // Adiciona 1 dia à data final para incluir o último dia no intervalo

        var query = @"
            SELECT cp.nome AS NomeProduto,
                p.data_fabricacao AS DataFabricacao,
                p.data_validade AS DataValidade,
                co.data AS DataCompra,
                f.email AS EmailFornecedor,
                cp.preco AS PrecoProduto,
                FLOOR(SUM(p.valor_compra / cp.preco))::INTEGER AS QuantidadeComprada,
                SUM(p.valor_compra) AS ValorTotalCompra,
                SUM(p.valor_compra % cp.preco) AS Frete
            FROM 
                produto p
            INNER JOIN catalogo_produto cp 
                ON p.catalogo_produto_id = cp.id
            INNER JOIN compra co 
                ON p.compra_id = co.id
            INNER JOIN fornecedor f 
                ON co.fornecedor_id = f.id
            WHERE co.data >= @DataInicial
                AND co.data < @DataFinal
            GROUP BY cp.nome,
                p.data_fabricacao, 
                p.data_validade, 
                co.data,
                cp.preco, 
                f.email
            ORDER BY DataValidade DESC;
        ";

        var results = _dbContext
        .Query<(string, 
        DateTime, 
        DateTime?,
        DateTime,
        string,
        decimal,
        int,
        decimal,
        decimal)>(query,
            new
            {
                DataInicial = dataInicial,
                DataFinal = dataFinal
            }).AsList();

        var purchases = results.Select(r => (
            NomeProduto: r.Item1,
            DataFabricacao: r.Item2,
            DataValidade: r.Item3,
            DataCompra: r.Item4,
            EmailFornecedor: r.Item5,
            PrecoProduto: r.Item6,
            QuantidadeComprada: r.Item7,
            ValorTotalCompra: r.Item8,
            Frete: r.Item9
        )).ToList();

        return purchases;
    }
}
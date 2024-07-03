using System.ComponentModel.DataAnnotations;
using System.Reflection;
using GestaoVarejo;
using Neo4j.Driver;

Console.WriteLine("Iniciando sistema ...");

var driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.None);
var session = driver.AsyncSession();
var reportService = new ReportService(driver);

var entityTypes = Assembly.GetExecutingAssembly().GetTypes()
    .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryableEntity<>)));

var displayNames = new List<string>(); 
foreach (var type in entityTypes)
{
    if(type.GetCustomAttribute<DisplayAttribute>() != null)
        displayNames.Add(type.GetCustomAttribute<DisplayAttribute>()?.Name ?? type.Name);
} 

while (true)
{
    Console.WriteLine("\nEscolha uma opção:");
    Console.WriteLine("1. Consultar registros");
    Console.WriteLine("2. Criar registro");
    Console.WriteLine("3. Editar registro");
    Console.WriteLine("4. Deletar registro");
    Console.WriteLine("5. Relatórios");
    Console.WriteLine("6. Sair");
    Console.Write("Sua escolha: ");
    
    string? escolha = Console.ReadLine();
    if (escolha == "6") break; 

    switch (escolha)
    {
        case "1":
            ConsultarEntidade();
            break;
        case "2":
            CriarRegistroEntidade();
            break;
        case "3":
            EditarEntidade();
            break;
        case "4":
            DeletarEntidade();
            break;
        case "5":
            await ShowRelatoriosSubMenu(reportService);
            break;
        default:
            Console.WriteLine("Opção inválida. Tente novamente.");
            break;
    }
}

async Task ShowRelatoriosSubMenu(ReportService reportService)
{
    while (true)
    {
        Console.WriteLine("\nEscolha um relatório:");
        Console.WriteLine("1. Top 3 Vendedores Por Período");
        Console.WriteLine("2. Top Clientes Por Período");
        Console.WriteLine("3. Vendas por Região Por Período");
        Console.WriteLine("4. Compras por Período");
        Console.WriteLine("5. Voltar");

        Console.Write("Sua escolha: ");
        string? escolha = Console.ReadLine();

        switch (escolha)
        {
            case "1":
                await ConsultarTopVendedoresPeriodoAsync(reportService);
                break;
            case "2":
                await ConsultarTopClientesPorPeriodoAsync(reportService);
                break;
            case "3":
                await ConsultarVendasPorRegiaoPorPeriodoAsync(reportService);
                break;
            case "4":
                await ConsultarComprasProdutosPorPeriodoAsync(reportService);
                break;
            case "5":
                return;
            default:
                Console.WriteLine("Opção inválida. Tente novamente.");
                break;
        }
    }
}

void ConsultarEntidade()
{
    Console.WriteLine("\nSelecione uma entidade para ler:");

    int index = 1;
    foreach (var displayName in displayNames) Console.WriteLine($"{index++}. {displayName}");
    Console.WriteLine($"{index}. Sair");
    Console.Write("Sua escolha: ");

    int.TryParse(Console.ReadLine(), out int consulta);

    if (consulta >= 1 && consulta < index)
    {
        Console.WriteLine();
        var selectedType = entityTypes.ElementAt(consulta - 1);
        var method = typeof(ConsoleHelper).GetMethod(nameof(ConsoleHelper.PrintEntityData))!
            .MakeGenericMethod(new Type[] { selectedType });
        method.Invoke(null, new object[] { session, displayNames[consulta - 1] });
    }
    else
    {
        Console.WriteLine("Opção inválida. Tente novamente.");
    }
}

void CriarRegistroEntidade()
{
    Console.WriteLine("\nSelecione uma entidade para criar:");

    int index = 1;
    foreach (var displayName in displayNames) Console.WriteLine($"{index++}. {displayName}");
    Console.WriteLine($"{index}. Sair");
    Console.Write("Sua escolha: ");

    int.TryParse(Console.ReadLine(), out int consulta);

    if (consulta >= 1 && consulta < index)
    {
        Console.WriteLine();
        var selectedType = entityTypes.ElementAt(consulta - 1);
        var method = typeof(ConsoleHelper).GetMethod(nameof(ConsoleHelper.CreateEntity))!
            .MakeGenericMethod(new Type[] { selectedType });
        try 
        {
            method.Invoke(null, new object[] { session });
        }
        catch
        {
            Console.WriteLine("Não foi possível criar a entidade. Verifique se os dados foram preenchidos corretamente.");
        }
    }
    else
    {
        Console.WriteLine("Opção inválida. Tente novamente.");
    }
}

void DeletarEntidade()
{
    Console.WriteLine("\nSelecione uma entidade para deletar:");

    int index = 1;
    foreach (var displayName in displayNames) Console.WriteLine($"{index++}. {displayName}");
    Console.WriteLine($"{index}. Sair");
    Console.Write("Sua escolha: ");

    int.TryParse(Console.ReadLine(), out int consulta);

    if (consulta >= 1 && consulta < index)
    {
        Console.WriteLine();
        var selectedType = entityTypes.ElementAt(consulta - 1);
        var method = typeof(ConsoleHelper).GetMethod(nameof(ConsoleHelper.DeleteEntity))!
            .MakeGenericMethod(new Type[] { selectedType });
        try 
        {
            method.Invoke(null, new object[] { session });
        }
        catch(Exception e)
        {
            if(e is TargetInvocationException && e.InnerException is AggregateException)
            {
                if(e.InnerException.Message.Contains("because it still has relationships"))
                {
                    Console.WriteLine("Não foi possível deletar a entidade. Existem relacionamentos ativos.");
                    return;
                }
            }
            Console.WriteLine("Não foi possível deletar a entidade. Verifique se os dados foram preenchidos corretamente.");
        }
    }
    else
    {
        Console.WriteLine("Opção inválida. Tente novamente.");
    }
}

void EditarEntidade()
{
    Console.WriteLine("\nSelecione uma editar para criar:");

    int index = 1;
    foreach (var displayName in displayNames) Console.WriteLine($"{index++}. {displayName}");
    Console.WriteLine($"{index}. Sair");
    Console.Write("Sua escolha: ");

    int.TryParse(Console.ReadLine(), out int consulta);

    if (consulta >= 1 && consulta < index)
    {
        Console.WriteLine();
        var selectedType = entityTypes.ElementAt(consulta - 1);
        var method = typeof(ConsoleHelper).GetMethod(nameof(ConsoleHelper.EditEntity))!
            .MakeGenericMethod(new Type[] { selectedType });
        try 
        {
            method.Invoke(null, new object[] { session });
        }
        catch
        {
            Console.WriteLine("Não foi possível atualizar a entidade. Verifique se os dados foram preenchidos corretamente.");
        }
    }
    else
    {
        Console.WriteLine("Opção inválida. Tente novamente.");
    }
}

static async Task ConsultarTopVendedoresPeriodoAsync(ReportService reportService)
{
    Console.WriteLine("Por favor, informe a data inicial (no formato YYYY-MM-DD):");
    string inputDataInicial = Console.ReadLine()!;

    Console.WriteLine("Por favor, informe a data final (no formato YYYY-MM-DD):");
    string inputDataFinal = Console.ReadLine()!;

    Console.WriteLine("Consultar Top Vendedores por Período...\n");

    if (DateTime.TryParse(inputDataInicial, out DateTime dataInicial) && DateTime.TryParse(inputDataFinal, out DateTime dataFinal))
    {
        dataFinal = dataFinal.AddDays(1);

        var topVendedores = await reportService.GetTopBestSellersInRangeAsync(dataInicial, dataFinal);

        if (topVendedores.Any())
        {
            Console.WriteLine($"Top Vendedores do Período {dataInicial:yyyy-MM-dd} a {dataFinal.AddDays(-1):yyyy-MM-dd}:");

            int posicao = 1;
            foreach (var (nomeFuncionario, valorTotalVendas, estado) in topVendedores)
            {
                Console.WriteLine($"Posição {posicao}: {nomeFuncionario} - Total de Vendas: R$ {valorTotalVendas:N2} - Estado: {estado}");
                posicao++;
            }
        }
        else
        {
            Console.WriteLine("Nenhum vendedor encontrado ou não há dados suficientes para determinar o top vendedores no período especificado.");
        }
    }
    else
    {
        Console.WriteLine("Datas fornecidas são inválidas. Certifique-se de informar as datas no formato correto (YYYY-MM-DD).");
    }
}

static async Task ConsultarTopClientesPorPeriodoAsync(ReportService reportService)
{
    Console.WriteLine("Por favor, informe a data inicial (no formato YYYY-MM-DD):");
    string inputDataInicial = Console.ReadLine()!;

    Console.WriteLine("Por favor, informe a data final (no formato YYYY-MM-DD):");
    string inputDataFinal = Console.ReadLine()!;

    Console.WriteLine("Consultando Top Clientes por Período...\n");

    if (DateTime.TryParse(inputDataInicial, out DateTime dataInicial) && DateTime.TryParse(inputDataFinal, out DateTime dataFinal))
    {
        dataFinal = dataFinal.AddDays(1);

        var topClients = await reportService.GetTopClientsInRangeAsync(dataInicial, dataFinal);

        if (topClients.Any())
        {
            Console.WriteLine($"Top Clientes no período {dataInicial:yyyy-MM-dd} a {dataFinal.AddDays(-1):yyyy-MM-dd}:");

            foreach (var (nomeCliente, valorTotalCompras, cidade) in topClients)
            {
                Console.WriteLine($"Nome: {nomeCliente} - Valor Total Compras: R$ {valorTotalCompras:N2} - Cidade: {cidade}");
            }
        }
        else
        {
            Console.WriteLine("Nenhum cliente encontrado no período especificado.");
        }
    }
    else
    {
        Console.WriteLine("Datas fornecidas são inválidas. Certifique-se de informar as datas no formato correto (YYYY-MM-DD).");
    }
}

static async Task ConsultarVendasPorRegiaoPorPeriodoAsync(ReportService reportService)
{
    Console.WriteLine("Por favor, informe a data inicial (no formato YYYY-MM-DD):");
    string inputDataInicial = Console.ReadLine()!;

    Console.WriteLine("Por favor, informe a data final (no formato YYYY-MM-DD):");
    string inputDataFinal = Console.ReadLine()!;

    Console.WriteLine("Consultando Vendas por Região por Período...\n");

    if (DateTime.TryParse(inputDataInicial, out DateTime dataInicial) && DateTime.TryParse(inputDataFinal, out DateTime dataFinal))
    {
        dataFinal = dataFinal.AddDays(1);

        var salesByRegion = await reportService.GetSalesByRegionInRangeAsync(dataInicial, dataFinal);

        if (salesByRegion.Any())
        {
            Console.WriteLine($"Relatório de Vendas por Região no período {dataInicial:yyyy-MM-dd} a {dataFinal.AddDays(-1):yyyy-MM-dd}:");

            string lastState = null!; 

            foreach (var sale in salesByRegion)
            {
                if (sale.Cidade != lastState)
                {
                    Console.WriteLine($"Cidade: {sale.Cidade}");
                    lastState = sale.Cidade;
                }

                Console.WriteLine($"  - Funcionário: {sale.NomeFuncionario}");
                Console.WriteLine($"  - Cliente: {sale.NomeCliente}");
                Console.WriteLine($"  - Data da Venda: {sale.DataVenda}");
                Console.WriteLine($"  - Valor Total da Venda: R$ {sale.ValorTotalVenda:N2}\n");
            }
        }
        else
        {
            Console.WriteLine("Nenhuma venda encontrada por região no período especificado.");
        }
    }
    else
    {
        Console.WriteLine("Datas fornecidas são inválidas. Certifique-se de informar as datas no formato correto (YYYY-MM-DD).");
    }
}

static async Task ConsultarComprasProdutosPorPeriodoAsync(ReportService reportService)
{
    Console.WriteLine("Por favor, informe a data inicial (no formato YYYY-MM-DD):");
    string inputDataInicial = Console.ReadLine()!;

    Console.WriteLine("Por favor, informe a data final (no formato YYYY-MM-DD):");
    string inputDataFinal = Console.ReadLine()!;

    Console.WriteLine("Consulta de Compras de Produtos por Período");
    Console.WriteLine("--------------------------------------------");

    if (DateTime.TryParse(inputDataInicial, out DateTime dataInicial) && DateTime.TryParse(inputDataFinal, out DateTime dataFinal))
    {
        var purchases = await reportService.GetProductPurchaseInRangeAsync(dataInicial, dataFinal);

        if (purchases.Count > 0)
        {
            Console.WriteLine($"\nCompras de Produtos no período de {dataInicial:yyyy-MM-dd} a {dataFinal:yyyy-MM-dd}:\n");

            foreach (var purchase in purchases)
            {
                Console.WriteLine($"Produto: {purchase.NomeProduto}");
                Console.WriteLine($"   - Data de Fabricação: {purchase.DataFabricacao:yyyy-MM-dd}");
                Console.WriteLine($"   - Data de Validade: {(purchase.DataValidade.HasValue ? purchase.DataValidade.Value.ToString("yyyy-MM-dd") : "N/A")}");
                Console.WriteLine($"   - Quantidade Comprada: {purchase.QuantidadeComprada}");
                Console.WriteLine($"   - Preço do Produto: {purchase.PrecoProduto:N2}");
                Console.WriteLine($"   - Valor Total Compra: R$ {purchase.ValorTotalCompra:N2}");
                Console.WriteLine($"   - Fornecedor: {purchase.EmailFornecedor}");
                Console.WriteLine($"   - Frete: {purchase.Frete:N2}");
                Console.WriteLine($"   - Data da Compra: {purchase.DataCompra:yyyy-MM-dd}");
                Console.WriteLine();
            }
        }
        else
        {
            Console.WriteLine("Nenhuma compra de produtos encontrada no período especificado.");
        }
    }
    else
    {
        Console.WriteLine("Datas fornecidas são inválidas. Certifique-se de informar as datas no formato correto (YYYY-MM-DD).");
    }
}

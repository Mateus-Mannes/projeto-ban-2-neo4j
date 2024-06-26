﻿using System.ComponentModel.DataAnnotations;
using System.Reflection;
using GestaoVarejo;
using Neo4j.Driver;

Console.WriteLine("Iniciando sistema ...");

var driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.None);
var session = driver.AsyncSession();

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
        // case "5":
        //     ShowRelatoriosSubMenu(reportService);
        //     break;
        default:
            Console.WriteLine("Opção inválida. Tente novamente.");
            break;
    }
}

// void ShowRelatoriosSubMenu(ReportService reportService)
// {
//     while (true)
//     {
//         Console.WriteLine("\nEscolha um relatório:");
//         Console.WriteLine("1. Top 3 Vendedores Por Período");
//         Console.WriteLine("2. Top Clientes Por Período");
//         Console.WriteLine("3. Vendas por Região Por Período");
//         Console.WriteLine("4. Compras por Período");
//         Console.WriteLine("5. Voltar");

//         Console.Write("Sua escolha: ");
//         string? escolha = Console.ReadLine();

//         switch (escolha)
//         {
//             case "1":
//                 ConsultarTopVendedoresPeriodo(reportService);
//                 break;
//             case "2":
//                 ConsultarTopClientesPorPeriodo(reportService);
//                 break;
//             case "3":
//                 ConsultarVendasPorRegiaoPorPeriodo(reportService);
//                 break;
//             case "4":
//                 ConsultarComprasProdutosPorPeriodo(reportService);
//                 break;
//             case "5":
//                 return;
//             default:
//                 Console.WriteLine("Opção inválida. Tente novamente.");
//                 break;
//         }
//     }
// }

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

// void ConsultarTopVendedoresPeriodo(ReportService reportService)
// {
//     Console.WriteLine("Por favor, informe a data inicial (no formato YYYY-MM-DD):");
//     string inputDataInicial = Console.ReadLine()!;

//     Console.WriteLine("Por favor, informe a data final (no formato YYYY-MM-DD):");
//     string inputDataFinal = Console.ReadLine()!;

//     Console.WriteLine("Consultar Top Vendedores por Período...\n");

//     if (DateTime.TryParse(inputDataInicial, out DateTime dataInicial) && DateTime.TryParse(inputDataFinal, out DateTime dataFinal))
//     {
//         // Adiciona 1 dia à data final para incluir o último dia no intervalo
//         dataFinal = dataFinal.AddDays(1);

//         var topVendedores = reportService.GetTopBestSellersInRange(dataInicial, dataFinal);

//         if (topVendedores != null && topVendedores.Any())
//         {
//             Console.WriteLine($"Top Vendedores do Período {dataInicial.ToString("yyyy-MM-dd")} a {dataFinal.AddDays(-1).ToString("yyyy-MM-dd")}:");

//             int posicao = 1; // Inicializa a posição como 1

//             foreach (var (nomeFuncionario, valorTotalVendas, estado) in topVendedores)
//             {
//                 Console.WriteLine($"Posição {posicao}: {nomeFuncionario} - Total de Vendas: R$ {valorTotalVendas:N2} - Estado: {estado}");
//                 posicao++; // Incrementa a posição a cada iteração
//             }
//         }
//         else
//         {
//             Console.WriteLine("Nenhum vendedor encontrado ou não há dados suficientes para determinar o top vendedores no período especificado.");
//         }
//     }
//     else
//     {
//         Console.WriteLine("Datas fornecidas são inválidas. Certifique-se de informar as datas no formato correto (YYYY-MM-DD).");
//     }
// }


// void ConsultarTopClientesPorPeriodo(ReportService reportService)
// {
//     Console.WriteLine("Por favor, informe a data inicial (no formato YYYY-MM-DD):");
//     string inputDataInicial = Console.ReadLine()!;

//     Console.WriteLine("Por favor, informe a data final (no formato YYYY-MM-DD):");
//     string inputDataFinal = Console.ReadLine()!;

//     Console.WriteLine("Consultando Top Clientes por Período...\n");

//     if (DateTime.TryParse(inputDataInicial, out DateTime dataInicial) && DateTime.TryParse(inputDataFinal, out DateTime dataFinal))
//     {
//         // Adiciona 1 dia à data final para incluir o último dia no intervalo
//         dataFinal = dataFinal.AddDays(1);

//         var topClients = reportService.GetTopClientsInRange(dataInicial, dataFinal);

//         if (topClients != null && topClients.Any())
//         {
//             Console.WriteLine($"Top Clientes no período {dataInicial.ToString("yyyy-MM-dd")} a {dataFinal.AddDays(-1).ToString("yyyy-MM-dd")}:");

//             foreach (var (nomeCliente, valorTotalCompras, cidade, estado) in topClients)
//             {
//                 Console.WriteLine($"Nome: {nomeCliente} - Valor Total Compras: R$ {valorTotalCompras:N2} - Cidade: {cidade}, {estado}");
//             }
//         }
//         else
//         {
//             Console.WriteLine("Nenhum cliente encontrado no período especificado.");
//         }
//     }
//     else
//     {
//         Console.WriteLine("Datas fornecidas são inválidas. Certifique-se de informar as datas no formato correto (YYYY-MM-DD).");
//     }
// }


// void ConsultarVendasPorRegiaoPorPeriodo(ReportService reportService)
// {
//     Console.WriteLine("Por favor, informe a data inicial (no formato YYYY-MM-DD):");
//     string inputDataInicial = Console.ReadLine()!;

//     Console.WriteLine("Por favor, informe a data final (no formato YYYY-MM-DD):");
//     string inputDataFinal = Console.ReadLine()!;

//     Console.WriteLine("Consultando Vendas por Região por Período...\n");

//     if (DateTime.TryParse(inputDataInicial, out DateTime dataInicial) && DateTime.TryParse(inputDataFinal, out DateTime dataFinal))
//     {
//         // Adiciona 1 dia à data final para incluir o último dia no intervalo
//         dataFinal = dataFinal.AddDays(1);

//         var salesByRegion = reportService.GetSalesByRegionInRange(dataInicial, dataFinal);

//         if (salesByRegion != null && salesByRegion.Any())
//         {
//             Console.WriteLine($"Relatório de Vendas por Região no período {dataInicial.ToString("yyyy-MM-dd")} a {dataFinal.AddDays(-1).ToString("yyyy-MM-dd")}:");

//             string lastState = null!;  // Variável para armazenar o último estado exibido

//             foreach (var sale in salesByRegion)
//             {
//                 if (sale.Estado != lastState)
//                 {
//                     Console.WriteLine($"Estado: {sale.Estado}");
//                     lastState = sale.Estado;
//                 }

//                 Console.WriteLine($"  - Funcionário: {sale.NomeFuncionario}");
//                 Console.WriteLine($"  - Cliente: {sale.NomeCliente}");
//                 Console.WriteLine($"  - Data da Venda: {sale.DataVenda}");
//                 Console.WriteLine($"  - Valor Total da Venda: R$ {sale.ValorTotalVenda:N2}\n");
//             }
//         }
//         else
//         {
//             Console.WriteLine("Nenhuma venda encontrada por região no período especificado.");
//         }
//     }
//     else
//     {
//         Console.WriteLine("Datas fornecidas são inválidas. Certifique-se de informar as datas no formato correto (YYYY-MM-DD).");
//     }
// }

// void ConsultarComprasProdutosPorPeriodo(ReportService reportService)
// {
//     Console.WriteLine("Por favor, informe a data inicial (no formato YYYY-MM-DD):");
//     string inputDataInicial = Console.ReadLine()!;

//     Console.WriteLine("Por favor, informe a data final (no formato YYYY-MM-DD):");
//     string inputDataFinal = Console.ReadLine()!;

//     Console.WriteLine("Consulta de Compras de Produtos por Período");
//     Console.WriteLine("--------------------------------------------");

//     if (DateTime.TryParse(inputDataInicial, out DateTime dataInicial) && DateTime.TryParse(inputDataFinal, out DateTime dataFinal))
//     {
//         try
//         {
//             var purchases = reportService.GetProductPurchaseInRange(dataInicial, dataFinal);

//             if (purchases.Count > 0)
//             {
//                 Console.WriteLine($"\nCompras de Produtos no período de {dataInicial:yyyy-MM-dd} a {dataFinal:yyyy-MM-dd}:\n");

//                 foreach (var purchase in purchases)
//                 {
//                     Console.WriteLine($"Produto: {purchase.NomeProduto}");
//                     Console.WriteLine($"   - Data de Fabricação: {purchase.DataFabricacao:yyyy-MM-dd}");
//                     Console.WriteLine($"   - Data de Validade: {(purchase.DataValidade.HasValue ? purchase.DataValidade.Value.ToString("yyyy-MM-dd") : "N/A")}");
//                     Console.WriteLine($"   - Quantidade Comprada: {purchase.QuantidadeComprada}");
//                     Console.WriteLine($"   - Preço do Produto: {purchase.PrecoProduto:N2}");
//                     Console.WriteLine($"   - Valor Total Compra: R$ {purchase.ValorTotalCompra:N2}");
//                     Console.WriteLine($"   - Fornecedor: {purchase.EmailFornecedor}");
//                     Console.WriteLine($"   - Frete: {purchase.Frete}");
//                     Console.WriteLine($"   - Data da Compra: {purchase.DataCompra:yyyy-MM-dd}");
//                     Console.WriteLine();
//                 }
//             }
//             else
//             {
//                 Console.WriteLine("Nenhuma compra de produtos encontrada no período especificado.");
//             }
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"Ocorreu um erro ao consultar as compras de produtos: {ex.Message}");
//         }
//     }
//     else
//     {
//         Console.WriteLine("Datas fornecidas são inválidas. Certifique-se de informar as datas no formato correto (YYYY-MM-DD).");
//     }
// }

# projeto-ban-2

Projeto de banco de dados utilizando o Neo4j para gerenciar uma loja de varejo.

## Instruções para Compilação e Execução

Para iniciar o projeto em um Codespace do GitHub, siga estas instruções:

1.  **Acesse o Repositório:** Navegue até o repositório do projeto no GitHub.
2.  **Abrir o Codespace:** Clique no botão "Code" e selecione "Open with Codespaces".

### Executar o Projeto

1.  **Navegar até a Pasta do Projeto:** Abra o terminal no Codespace e navegue até a pasta do projeto:

    ```bash
    cd GestaoVarejo/
    ```

2.  **Executar o Setup:** Execute o script de setup para criar o banco de dados e popular com dados iniciais:

    ```bash
    ./setup.sh
    ```

3.  **Compilar e Executar o Projeto:**

    ```bash
    dotnet run
    ```

## Modelo de Dados (Neo4j)

O projeto utiliza um banco de dados Neo4j, que é um banco de dados orientado a grafos. Isso significa que os dados são armazenados como nós e relacionamentos, permitindo uma representação flexível e eficiente das informações da loja de varejo.

### Nós

*   **endereco:** Representa os endereços dos clientes, funcionários e fornecedores.
*   **categoria:** Representa as categorias de produtos.
*   **catalogo\_produto:** Representa os produtos do catálogo da loja.
*   **cliente:** Representa os clientes da loja.
*   **funcionario:** Representa os funcionários da loja.
*   **fornecedor:** Representa os fornecedores da loja.
*   **compra:** Representa as compras realizadas junto aos fornecedores.
*   **produto:** Representa os produtos em estoque, incluindo informações sobre sua compra e eventual venda.
*   **venda:** Representa as vendas realizadas aos clientes.

### Relacionamentos

*   **RESIDE\_EM:** Liga um cliente ou funcionário ao seu endereço.
*   **LOCALIZADO\_EM:** Liga um fornecedor ao seu endereço.
*   **CONTAINS:** Liga uma categoria aos seus produtos.
*   **CATEGORIZADO\_COM:** Liga um produto do catálogo à sua categoria.
*   **FORNECE:** Liga um fornecedor às compras realizadas com ele.
*   **PARTE\_DE:** Liga um produto à compra da qual ele faz parte.
*   **FEZ:** Liga um cliente às vendas que ele realizou.
*   **ATENDIDO\_POR:** Liga uma venda ao funcionário que a realizou.
*   **VENDIDO:** Liga um produto à venda da qual ele faz parte (opcional, pois nem todos os produtos são vendidos).

## DataSeed

O script `setup.sh` contém um comando para popular o banco de dados com dados iniciais de endereços, categorias, produtos, clientes, funcionários, fornecedores, compras e vendas.

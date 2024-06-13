CREATE (:endereco {cidade: 'São Paulo', bairro: 'Bela Vista', rua: 'Av. Paulista', numero: '1000', estado: 'SP'});
CREATE (:endereco {cidade: 'Rio de Janeiro', bairro: 'Centro', rua: 'Av. Presidente Vargas', numero: '100', estado: 'RJ'});
CREATE (:endereco {cidade: 'Belo Horizonte', bairro: 'Savassi', rua: 'Rua da Bahia', numero: '1200', estado: 'MG'});
CREATE (:endereco {cidade: 'Porto Alegre', bairro: 'Moinhos de Vento', rua: 'Rua Padre Chagas', numero: '850', estado: 'RS'});
CREATE (:endereco {cidade: 'Curitiba', bairro: 'Batel', rua: 'Av. do Batel', numero: '1320', estado: 'PR'});
CREATE (:endereco {cidade: 'Salvador', bairro: 'Barra', rua: 'Av. Oceânica', numero: '409', estado: 'BA'});
CREATE (:endereco {cidade: 'Recife', bairro: 'Boa Viagem', rua: 'Av. Conselheiro Aguiar', numero: '2332', estado: 'PE'});
CREATE (:endereco {cidade: 'Fortaleza', bairro: 'Meireles', rua: 'Av. Beira Mar', numero: '2020', estado: 'CE'});
CREATE (:endereco {cidade: 'Manaus', bairro: 'Centro', rua: 'Av. Eduardo Ribeiro', numero: '900', estado: 'AM'});
CREATE (:endereco {cidade: 'Brasília', bairro: 'Asa Norte', rua: 'SQN 406', numero: 'Bloco D', estado: 'DF'});

CREATE (:categoria {nome: 'Livros'});
CREATE (:categoria {nome: 'Eletrodomésticos'});
CREATE (:categoria {nome: 'Informática'});
CREATE (:categoria {nome: 'Moda Masculina'});
CREATE (:categoria {nome: 'Moda Feminina'});
CREATE (:categoria {nome: 'Brinquedos'});
CREATE (:categoria {nome: 'Decoração'});
CREATE (:categoria {nome: 'Esportes'});
CREATE (:categoria {nome: 'Automotivo'});
CREATE (:categoria {nome: 'Beleza e Perfumaria'});

MATCH (c:categoria {nome: 'Livros'})
CREATE (c)-[:CONTAINS]->(:catalogo_produto {nome: '1984', descricao: 'Clássico da literatura distópica.', preco: 39.90});

MATCH (c:categoria {nome: 'Eletrodomésticos'})
CREATE (c)-[:CONTAINS]->(:catalogo_produto {nome: 'Geladeira Duplex', descricao: 'Geladeira espaçosa com freezer.', preco: 2100.00});

MATCH (c:categoria {nome: 'Informática'})
CREATE (c)-[:CONTAINS]->(:catalogo_produto {nome: 'Notebook Gamer', descricao: 'Alto desempenho para jogos.', preco: 5800.00});

MATCH (c:categoria {nome: 'Moda Masculina'})
CREATE (c)-[:CONTAINS]->(:catalogo_produto {nome: 'Camiseta Polo', descricao: 'Camiseta masculina casual.', preco: 89.90});

MATCH (c:categoria {nome: 'Moda Feminina'})
CREATE (c)-[:CONTAINS]->(:catalogo_produto {nome: 'Vestido Floral', descricao: 'Vestido feminino para verão.', preco: 120.00});

MATCH (c:categoria {nome: 'Brinquedos'})
CREATE (c)-[:CONTAINS]->(:catalogo_produto {nome: 'Lego Star Wars', descricao: 'Conjunto de montar Star Wars.', preco: 299.90});

MATCH (c:categoria {nome: 'Decoração'})
CREATE (c)-[:CONTAINS]->(:catalogo_produto {nome: 'Luminária de Mesa', descricao: 'Decoração moderna para interiores.', preco: 130.50});

MATCH (c:categoria {nome: 'Esportes'})
CREATE (c)-[:CONTAINS]->(:catalogo_produto {nome: 'Bicicleta de Montanha', descricao: 'Ideal para trilhas e terrenos irregulares.', preco: 890.00});

MATCH (c:categoria {nome: 'Automotivo'})
CREATE (c)-[:CONTAINS]->(:catalogo_produto {nome: 'Pneu Aro 15', descricao: 'Pneus de alta durabilidade.', preco: 400.00});

MATCH (c:categoria {nome: 'Beleza e Perfumaria'})
CREATE (c)-[:CONTAINS]->(:catalogo_produto {nome: 'Perfume Importado', descricao: 'Fragrância feminina, 50ml.', preco: 250.00});


MATCH (e:endereco {cidade: 'Belo Horizonte', rua: 'Rua da Bahia', numero: '1200'})
CREATE (e)<-[:RESIDE_EM]-(c:cliente {cpf: '22233344455', nome: 'Lucas', ultimo_nome: 'Moura', telefone: '21987654321', email: 'lucas.moura@email.com'});

MATCH (e:endereco {cidade: 'Porto Alegre', rua: 'Rua Padre Chagas', numero: '850'})
CREATE (e)<-[:RESIDE_EM]-(c:cliente {cpf: '33344455566', nome: 'Isabela', ultimo_nome: 'Fernandes', telefone: '31987654321', email: 'isabela.fernandes@email.com'});

MATCH (e:endereco {cidade: 'Curitiba', rua: 'Av. do Batel', numero: '1320'})
CREATE (e)<-[:RESIDE_EM]-(c:cliente {cpf: '44455566677', nome: 'Carlos', ultimo_nome: 'Andrade', telefone: '51987654321', email: 'carlos.andrade@email.com'});

MATCH (e:endereco {cidade: 'Salvador', rua: 'Av. Oceânica', numero: '409'})
CREATE (e)<-[:RESIDE_EM]-(c:cliente {cpf: '55566677788', nome: 'Patrícia', ultimo_nome: 'Barbosa', telefone: '61987654321', email: 'patricia.barbosa@email.com'});

MATCH (e:endereco {cidade: 'Recife', rua: 'Av. Conselheiro Aguiar', numero: '2332'})
CREATE (e)<-[:RESIDE_EM]-(c:cliente {cpf: '66677788899', nome: 'Joana', ultimo_nome: 'D’Arc', telefone: '71987654321', email: 'joana.arc@email.com'});

MATCH (e:endereco {cidade: 'Fortaleza', rua: 'Av. Beira Mar', numero: '2020'})
CREATE (e)<-[:RESIDE_EM]-(c:cliente {cpf: '77788899900', nome: 'Roberto', ultimo_nome: 'Silva', telefone: '81987654321', email: 'roberto.silva@email.com'});

MATCH (e:endereco {cidade: 'São Paulo', rua: 'Av. Paulista', numero: '1000'})
CREATE (e)<-[:RESIDE_EM]-(c:cliente {cpf: '88899900011', nome: 'Mariana', ultimo_nome: 'Costa', telefone: '91987654321', email: 'mariana.costa@email.com'});

MATCH (e:endereco {cidade: 'Rio de Janeiro', rua: 'Av. Presidente Vargas', numero: '100'})
CREATE (e)<-[:RESIDE_EM]-(c:cliente {cpf: '99900011122', nome: 'Rafael', ultimo_nome: 'Santos', telefone: '21987654322', email: 'rafael.santos@email.com'});


MATCH (e:endereco {cidade: 'Belo Horizonte', rua: 'Rua da Bahia', numero: '1200'})
CREATE (e)<-[:TRABALHA_EM]-(f:funcionario {cpf: '99911122233', nome: 'Tânia', ultimo_nome: 'Gomes', salario: 4500.00, email: 'tania.gomes@email.com'});

MATCH (e:endereco {cidade: 'Porto Alegre', rua: 'Rua Padre Chagas', numero: '850'})
CREATE (e)<-[:TRABALHA_EM]-(f:funcionario {cpf: '88822233344', nome: 'Marcelo', ultimo_nome: 'Dias', salario: 5500.00, email: 'marcelo.dias@email.com'});

MATCH (e:endereco {cidade: 'Curitiba', rua: 'Av. do Batel', numero: '1320'})
CREATE (e)<-[:TRABALHA_EM]-(f:funcionario {cpf: '77733344455', nome: 'Fernanda', ultimo_nome: 'Lima', salario: 6200.00, email: 'fernanda.lima@email.com'});

MATCH (e:endereco {cidade: 'Salvador', rua: 'Av. Oceânica', numero: '409'})
CREATE (e)<-[:TRABALHA_EM]-(f:funcionario {cpf: '66644455566', nome: 'Antonio', ultimo_nome: 'Carvalho', salario: 4700.00, email: 'antonio.carvalho@email.com'});

MATCH (e:endereco {cidade: 'Recife', rua: 'Av. Conselheiro Aguiar', numero: '2332'})
CREATE (e)<-[:TRABALHA_EM]-(f:funcionario {cpf: '55566677788', nome: 'Beatriz', ultimo_nome: 'Machado', salario: 3900.00, email: 'beatriz.machado@email.com'});

MATCH (e:endereco {cidade: 'Fortaleza', rua: 'Av. Beira Mar', numero: '2020'})
CREATE (e)<-[:TRABALHA_EM]-(f:funcionario {cpf: '44477788899', nome: 'Gabriel', ultimo_nome: 'Alves', salario: 5300.00, email: 'gabriel.alves@email.com'});

MATCH (e:endereco {cidade: 'São Paulo', rua: 'Av. Paulista', numero: '1000'})
CREATE (e)<-[:TRABALHA_EM]-(f:funcionario {cpf: '33388899900', nome: 'Sofia', ultimo_nome: 'Castro', salario: 4100.00, email: 'sofia.castro@email.com'});

MATCH (e:endereco {cidade: 'Rio de Janeiro', rua: 'Av. Presidente Vargas', numero: '100'})
CREATE (e)<-[:TRABALHA_EM]-(f:funcionario {cpf: '22299900011', nome: 'Eduardo', ultimo_nome: 'Pereira', salario: 5600.00, email: 'eduardo.pereira@email.com'});


MATCH (e:endereco {cidade: 'Belo Horizonte', rua: 'Rua da Bahia', numero: '1200'})
CREATE (e)<-[:LOCALIZADO_EM]-(f:fornecedor {cnpj: '11223344000100', email: 'contato@fornecedor3.com', telefone: '1133224455'});

MATCH (e:endereco {cidade: 'Porto Alegre', rua: 'Rua Padre Chagas', numero: '850'})
CREATE (e)<-[:LOCALIZADO_EM]-(f:fornecedor {cnpj: '22334455000111', email: 'vendas@fornecedor4.com', telefone: '2133445566'});

MATCH (e:endereco {cidade: 'Curitiba', rua: 'Av. do Batel', numero: '1320'})
CREATE (e)<-[:LOCALIZADO_EM]-(f:fornecedor {cnpj: '33445566000122', email: 'suporte@fornecedor5.com', telefone: '3134556677'});

MATCH (e:endereco {cidade: 'Salvador', rua: 'Av. Oceânica', numero: '409'})
CREATE (e)<-[:LOCALIZADO_EM]-(f:fornecedor {cnpj: '44556677000133', email: 'info@fornecedor6.com', telefone: '4135667788'});

MATCH (e:endereco {cidade: 'Recife', rua: 'Av. Conselheiro Aguiar', numero: '2332'})
CREATE (e)<-[:LOCALIZADO_EM]-(f:fornecedor {cnpj: '55667788000144', email: 'atendimento@fornecedor7.com', telefone: '5136778899'});

MATCH (e:endereco {cidade: 'Fortaleza', rua: 'Av. Beira Mar', numero: '2020'})
CREATE (e)<-[:LOCALIZADO_EM]-(f:fornecedor {cnpj: '66778899000155', email: 'comercial@fornecedor8.com', telefone: '6137889900'});

MATCH (e:endereco {cidade: 'São Paulo', rua: 'Av. Paulista', numero: '1000'})
CREATE (e)<-[:LOCALIZADO_EM]-(f:fornecedor {cnpj: '77889900011266', email: 'relacionamento@fornecedor9.com', telefone: '7138990011'});

MATCH (e:endereco {cidade: 'Rio de Janeiro', rua: 'Av. Presidente Vargas', numero: '100'})
CREATE (e)<-[:LOCALIZADO_EM]-(f:fornecedor {cnpj: '88990011122377', email: 'pedido@fornecedor10.com', telefone: '8139001122'});


MATCH (f:fornecedor {cnpj: '11223344000100'})
CREATE (f)<-[:FORNECE]-(c:compra {nfe: 'NFE456789', data: '2023-09-01'});

MATCH (f:fornecedor {cnpj: '22334455000111'})
CREATE (f)<-[:FORNECE]-(c:compra {nfe: 'NFE567890', data: '2023-09-02'});

MATCH (f:fornecedor {cnpj: '33445566000122'})
CREATE (f)<-[:FORNECE]-(c:compra {nfe: 'NFE678901', data: '2023-09-03'});

MATCH (f:fornecedor {cnpj: '44556677000133'})
CREATE (f)<-[:FORNECE]-(c:compra {nfe: 'NFE789012', data: '2023-09-04'});

MATCH (f:fornecedor {cnpj: '55667788000144'})
CREATE (f)<-[:FORNECE]-(c:compra {nfe: 'NFE890123', data: '2023-09-05'});


MATCH (cp:catalogo_produto {nome: '1984'}), (c:compra {nfe: 'NFE456789'})
CREATE (cp)<-[:CATEGORIZADO_COM]-(p:produto {data_fabricacao: '2023-08-01', data_validade: '2024-08-01', data_entrega: '2023-07-15', valor_compra: 2150.00})-[:PARTE_DE]->(c);

MATCH (cp:catalogo_produto {nome: 'Geladeira Duplex'}), (c:compra {nfe: 'NFE456789'})
CREATE (cp)<-[:CATEGORIZADO_COM]-(p:produto {data_fabricacao: '2023-08-05', data_entrega: '2023-07-15', valor_compra: 4250.00})-[:PARTE_DE]->(c);

MATCH (cp:catalogo_produto {nome: 'Notebook Gamer'}), (c:compra {nfe: 'NFE567890'})
CREATE (cp)<-[:CATEGORIZADO_COM]-(p:produto {data_fabricacao: '2023-08-10', data_validade: '2024-08-10', data_entrega: '2023-07-15', valor_compra: 11600.00})-[:PARTE_DE]->(c);

MATCH (cp:catalogo_produto {nome: 'Camiseta Polo'}), (c:compra {nfe: 'NFE567890'})
CREATE (cp)<-[:CATEGORIZADO_COM]-(p:produto {data_fabricacao: '2023-07-15', data_entrega: '2023-07-15', valor_compra: 100.00})-[:PARTE_DE]->(c);

MATCH (cp:catalogo_produto {nome: 'Vestido Floral'}), (c:compra {nfe: 'NFE678901'})
CREATE (cp)<-[:CATEGORIZADO_COM]-(p:produto {data_fabricacao: '2023-08-20', data_validade: '2024-08-20', data_entrega: '2023-07-15', valor_compra: 1700.00})-[:PARTE_DE]->(c);

MATCH (cp:catalogo_produto {nome: '1984'}), (c:compra {nfe: 'NFE789012'})
CREATE (cp)<-[:CATEGORIZADO_COM]-(p:produto {data_fabricacao: '2023-08-25', data_validade: '2024-08-25', data_entrega: '2023-07-15', valor_compra: 2150.00})-[:PARTE_DE]->(c);

MATCH (cp:catalogo_produto {nome: 'Geladeira Duplex'}), (c:compra {nfe: 'NFE789012'})
CREATE (cp)<-[:CATEGORIZADO_COM]-(p:produto {data_fabricacao: '2023-08-30', data_entrega: '2023-07-15', valor_compra: 4250.00})-[:PARTE_DE]->(c);

MATCH (cp:catalogo_produto {nome: 'Notebook Gamer'}), (c:compra {nfe: 'NFE890123'})
CREATE (cp)<-[:CATEGORIZADO_COM]-(p:produto {data_fabricacao: '2023-09-01', data_validade: '2024-09-01', data_entrega: '2023-07-15', valor_compra: 11600.00})-[:PARTE_DE]->(c);

MATCH (cp:catalogo_produto {nome: 'Camiseta Polo'}), (c:compra {nfe: 'NFE890123'})
CREATE (cp)<-[:CATEGORIZADO_COM]-(p:produto {data_fabricacao: '2023-07-10', data_entrega: '2023-07-15', valor_compra: 100.00})-[:PARTE_DE]->(c);

MATCH (cp:catalogo_produto {nome: 'Vestido Floral'}), (c:compra {nfe: 'NFE678901'})
CREATE (cp)<-[:CATEGORIZADO_COM]-(p:produto {data_fabricacao: '2023-08-15', data_validade: '2024-08-15', data_entrega: '2023-07-15', valor_compra: 1700.00})-[:PARTE_DE]->(c);


MATCH (cl:cliente {cpf: '22233344455'}), (f:funcionario {cpf: '99911122233'})
CREATE (cl)-[:FEZ]->(:venda {nfe: 'VENDA45678', data: '2023-09-10', valor: 2350.00})-[:ATENDIDO_POR]->(f);

MATCH (cl:cliente {cpf: '33344455566'}), (f:funcionario {cpf: '88822233344'})
CREATE (cl)-[:FEZ]->(:venda {nfe: 'VENDA56789', data: '2023-09-11', valor: 1300.00})-[:ATENDIDO_POR]->(f);

MATCH (cl:cliente {cpf: '44455566677'}), (f:funcionario {cpf: '77733344455'})
CREATE (cl)-[:FEZ]->(:venda {nfe: 'VENDA67890', data: '2023-09-12', valor: 2900.00})-[:ATENDIDO_POR]->(f);

-- Inserir registros em endereco
INSERT INTO endereco (cidade, bairro, rua, numero, estado) VALUES
('São Paulo', 'Bela Vista', 'Av. Paulista', '1000', 'SP'),
('Rio de Janeiro', 'Centro', 'Av. Presidente Vargas', '100', 'RJ'),
('Belo Horizonte', 'Savassi', 'Rua da Bahia', '1200', 'MG'),
('Porto Alegre', 'Moinhos de Vento', 'Rua Padre Chagas', '850', 'RS'),
('Curitiba', 'Batel', 'Av. do Batel', '1320', 'PR'),
('Salvador', 'Barra', 'Av. Oceânica', '409', 'BA'),
('Recife', 'Boa Viagem', 'Av. Conselheiro Aguiar', '2332', 'PE'),
('Fortaleza', 'Meireles', 'Av. Beira Mar', '2020', 'CE'),
('Manaus', 'Centro', 'Av. Eduardo Ribeiro', '900', 'AM'),
('Brasília', 'Asa Norte', 'SQN 406', 'Bloco D', 'DF');

-- Inserir registros em categoria
INSERT INTO categoria (nome) VALUES
('Livros'),
('Eletrodomésticos'),
('Informática'),
('Moda Masculina'),
('Moda Feminina'),
('Brinquedos'),
('Decoração'),
('Esportes'),
('Automotivo'),
('Beleza e Perfumaria');

-- Inserir registros em catalogo_produto
INSERT INTO catalogo_produto (nome, descricao, preco, categoria_id) VALUES
('1984', 'Clássico da literatura distópica.', 39.90, (SELECT id FROM categoria WHERE nome = 'Livros')),
('Geladeira Duplex', 'Geladeira espaçosa com freezer.', 2100.00, (SELECT id FROM categoria WHERE nome = 'Eletrodomésticos')),
('Notebook Gamer', 'Alto desempenho para jogos.', 5800.00, (SELECT id FROM categoria WHERE nome = 'Informática')),
('Camiseta Polo', 'Camiseta masculina casual.', 89.90, (SELECT id FROM categoria WHERE nome = 'Moda Masculina')),
('Vestido Floral', 'Vestido feminino para verão.', 120.00, (SELECT id FROM categoria WHERE nome = 'Moda Feminina')),
('Lego Star Wars', 'Conjunto de montar Star Wars.', 299.90, (SELECT id FROM categoria WHERE nome = 'Brinquedos')),
('Luminária de Mesa', 'Decoração moderna para interiores.', 130.50, (SELECT id FROM categoria WHERE nome = 'Decoração')),
('Bicicleta de Montanha', 'Ideal para trilhas e terrenos irregulares.', 890.00, (SELECT id FROM categoria WHERE nome = 'Esportes')),
('Pneu Aro 15', 'Pneus de alta durabilidade.', 400.00, (SELECT id FROM categoria WHERE nome = 'Automotivo')),
('Perfume Importado', 'Fragrância feminina, 50ml.', 250.00, (SELECT id FROM categoria WHERE nome = 'Beleza e Perfumaria'));

-- Inserir registros em cliente
INSERT INTO cliente (cpf, nome, ultimo_nome, telefone, email, endereco_id) VALUES
('22233344455', 'Lucas', 'Moura', '21987654321', 'lucas.moura@email.com', 3),
('33344455566', 'Isabela', 'Fernandes', '31987654321', 'isabela.fernandes@email.com', 4),
('44455566677', 'Carlos', 'Andrade', '51987654321', 'carlos.andrade@email.com', 5),
('55566677788', 'Patrícia', 'Barbosa', '61987654321', 'patricia.barbosa@email.com', 6),
('66677788899', 'Joana', 'D’Arc', '71987654321', 'joana.arc@email.com', 7),
('77788899900', 'Roberto', 'Silva', '81987654321', 'roberto.silva@email.com', 8),
('88899900011', 'Mariana', 'Costa', '91987654321', 'mariana.costa@email.com', 1),
('99900011122', 'Rafael', 'Santos', '21987654322', 'rafael.santos@email.com', 2);

-- Inserir registros em funcionario
INSERT INTO funcionario (cpf, nome, ultimo_nome, salario, email, endereco_id) VALUES
('99911122233', 'Tânia', 'Gomes', 4500.00, 'tania.gomes@email.com', 3),
('88822233344', 'Marcelo', 'Dias', 5500.00, 'marcelo.dias@email.com', 4),
('77733344455', 'Fernanda', 'Lima', 6200.00, 'fernanda.lima@email.com', 5),
('66644455566', 'Antonio', 'Carvalho', 4700.00, 'antonio.carvalho@email.com', 6),
('55566677788', 'Beatriz', 'Machado', 3900.00, 'beatriz.machado@email.com', 7),
('44477788899', 'Gabriel', 'Alves', 5300.00, 'gabriel.alves@email.com', 8),
('33388899900', 'Sofia', 'Castro', 4100.00, 'sofia.castro@email.com', 1),
('22299900011', 'Eduardo', 'Pereira', 5600.00, 'eduardo.pereira@email.com', 2);

-- Inserir registros em fornecedor
INSERT INTO fornecedor (cnpj, email, telefone, endereco_id) VALUES
('11223344000100', 'contato@fornecedor3.com', '1133224455', 3),
('22334455000111', 'vendas@fornecedor4.com', '2133445566', 4),
('33445566000122', 'suporte@fornecedor5.com', '3134556677', 5),
('44556677000133', 'info@fornecedor6.com', '4135667788', 6),
('55667788000144', 'atendimento@fornecedor7.com', '5136778899', 7),
('66778899000155', 'comercial@fornecedor8.com', '6137889900', 8),
('77889900011266', 'relacionamento@fornecedor9.com', '7138990011', 1),
('88990011122377', 'pedido@fornecedor10.com', '8139001122', 2);

-- Inserir registros em compra
INSERT INTO compra (nfe, data, fornecedor_id) VALUES
('NFE456789', '2023-09-01', 1),
('NFE567890', '2023-09-02', 2),
('NFE678901', '2023-09-03', 3),
('NFE789012', '2023-09-04', 4),
('NFE890123', '2023-09-05', 5);

-- Ajuste os IDs conforme necessário para os próximos INSERTS
-- Inserir registros em produto
INSERT INTO produto (catalogo_produto_id, data_fabricacao, data_validade, valor_compra, compra_id) VALUES
(1, '2023-08-01', '2024-08-01', 2150.00, 1),
(2, '2023-08-05', NULL, 4250.00, 1),
(3, '2023-08-10', '2024-08-10', 11600.00, 2),
(4, '2023-07-15', NULL, 100.00, 2),
(5, '2023-08-20', '2024-08-20', 1700.00, 3),
(1, '2023-08-25', '2024-08-25', 2150.00, 4),
(2, '2023-08-30', NULL, 4250.00, 4),
(3, '2023-09-01', '2024-09-01', 11600.00, 5),
(4, '2023-07-10', NULL, 100.00, 5),
(5, '2023-08-15', '2024-08-15', 1700.00, 3);

-- Inserir registros em venda
-- Certifique-se de que cliente_id e funcionario_id correspondam a registros válidos
INSERT INTO venda (nfe, data, valor, cliente_id, funcionario_id) VALUES
('VENDA45678', '2023-09-10', 2350.00, 1, 1),
('VENDA56789', '2023-09-11', 1300.00, 2, 2),
('VENDA67890', '2023-09-12', 2900.00, 3, 3);

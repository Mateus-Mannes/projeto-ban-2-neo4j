-- endereco
CREATE TABLE endereco (
    id SERIAL PRIMARY KEY,
    cidade VARCHAR(255),
    bairro VARCHAR(255),
    rua VARCHAR(255),
    numero VARCHAR(50),
    estado VARCHAR(2)
);


-- cliente
CREATE TABLE cliente (
    id SERIAL PRIMARY KEY,
    cpf VARCHAR(11) NULL,
    nome VARCHAR(255) NOT NULL,
    ultimo_nome VARCHAR(255) NOT NULL,
    telefone VARCHAR(20) NULL,
    email VARCHAR(255) NULL,
    endereco_id INTEGER REFERENCES endereco(id)
);

-- funcionario
CREATE TABLE funcionario (
    id SERIAL PRIMARY KEY,
    cpf VARCHAR(11) NOT NULL,
    nome VARCHAR(255) NOT NULL,
    ultimo_nome VARCHAR(255) NOT NULL,
    salario NUMERIC(10, 2) NOT NULL,
    email VARCHAR(255) NULL,
    endereco_id INTEGER REFERENCES endereco(id)
);


-- fornecedor
CREATE TABLE fornecedor (
    id SERIAL PRIMARY KEY,
    cnpj VARCHAR(14) NULL,
    email VARCHAR(255) NOT NULL,
    telefone VARCHAR(20) NOT NULL,
    endereco_id INTEGER REFERENCES endereco(id)
);

-- compra
CREATE TABLE compra (
    id SERIAL PRIMARY KEY,
    nfe VARCHAR(255) NOT NULL,
    data DATE NOT NULL,
    fornecedor_id INTEGER REFERENCES fornecedor(id)
);

-- venda
CREATE TABLE venda (
    id SERIAL PRIMARY KEY,
    nfe VARCHAR(255) NOT NULL,
    data DATE NOT NULL,
    valor NUMERIC(10, 2) NOT NULL,
    cliente_id INTEGER REFERENCES cliente(id),
    funcionario_id INTEGER REFERENCES funcionario(id)
);

-- categoria
CREATE TABLE categoria (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(255) NOT NULL
);

CREATE TABLE catalogo_produto (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(255) NOT NULL,
    descricao TEXT NULL,
    preco NUMERIC(10, 2) NOT NULL,
    categoria_id INTEGER REFERENCES categoria(id)
);

-- produto
CREATE TABLE produto (
    id SERIAL PRIMARY KEY,
    catalogo_produto_id INTEGER REFERENCES catalogo_produto(id),
    data_fabricacao DATE NOT NULL,
    data_validade DATE NULL,
    data_entrega DATE NULL,
    valor_compra NUMERIC(10, 2) NOT NULL,
    compra_id INTEGER REFERENCES compra(id),
    venda_id INTEGER REFERENCES venda(id) NULL
);


CREATE DATABASE AFReparosAutomotivos
GO 

USE AFReparosAutomotivos
GO

CREATE TABLE Pessoa
(
	idPessoa		INT				NOT NULL	PRIMARY KEY	IDENTITY,
	nome			VARCHAR(50)		NOT NULL,
	celular			VARCHAR(15)		NOT NULL,
	documento		VARCHAR(18)		NOT NULL	UNIQUE,
	tipo_doc		CHAR			NOT NULL							CHECK	(tipo_doc in ('F', 'J'))
)
GO

CREATE TABLE Endereco
(
    pessoaId		INT				NOT NULL	PRIMARY KEY,
    logradouro		VARCHAR(150)	NOT NULL,
	numero			VARCHAR(5)		NOT NULL,
    cidade			VARCHAR(100)	NOT NULL,
    estado			VARCHAR(2)		NOT NULL,
	CEP				VARCHAR(9)		NOT NULL,

    FOREIGN KEY (pessoaId) REFERENCES Pessoa(idPessoa) ON DELETE CASCADE
)
GO

CREATE TABLE Funcionario
(
	idFuncionario	INT				NOT NULL	PRIMARY KEY	REFERENCES	Pessoa(idPessoa),
	permissao		INT				NOT NULL	DEFAULT		3								CHECK	(permissao	in	(1, 2, 3)), -- Administrador, escrita ou leitura
	usuario			VARCHAR(16)		NOT NULL	UNIQUE,
	senha			VARCHAR(15)		NOT NULL,
	statusFunc			INT			NOT NULL	DEFAULT		1								CHECK	(statusFunc		in	(1, 2)) -- Ativo ou inativo
)
GO

CREATE TABLE Cliente
(
	idCliente		INT				NOT NULL	PRIMARY KEY	REFERENCES	Pessoa(idPessoa),
	telefone		VARCHAR(14)		NULL,
	email			VARCHAR(50)		NOT NULL,
	statusCli		INT				NOT NULL	DEFAULT		1								CHECK	(statusCli		in	(1, 2)), -- Ativo ou inativo
	chaveCli		VARCHAR(19)		NOT NULL	UNIQUE	
)
GO

CREATE TABLE Veiculo
(
	idVeiculo		INT				NOT NULL	PRIMARY KEY IDENTITY,
	clienteId		INT				NOT NULL	REFERENCES Cliente(idCliente),
	marca			VARCHAR(50)		NOT NULL,
	placa			VARCHAR(7)		NOT NULL	UNIQUE,
	modelo			VARCHAR(50)		NOT NULL,
	cor				VARCHAR(20)		NOT NULL,
	ano				INT				NOT NULL	CHECK	(ano >= 1886 AND ano <= YEAR(GETDATE()) + 1) -- O primeiro carro foi inventado em 1886
)
GO

CREATE TABLE Peca
(
	idPeca			INT				NOT NULL	PRIMARY KEY	IDENTITY,
	nome			VARCHAR(20)		NOT NULL,
	valor			MONEY			NOT NULL,
	qtdEsto			INT				NOT NULL
)
GO

CREATE TABLE Compra
(
	idCompra		INT				NOT NULL	PRIMARY KEY	IDENTITY,
	funcionarioId	INT				NOT NULL	REFERENCES	Funcionario(idFuncionario),
	pecaId			INT				NOT NULL	REFERENCES	Peca(idPeca),
	qtd				INT				NOT NULL,
	preco			MONEY			NOT NULL,
	dataComp		DATETIME		NOT NULL
)
GO

CREATE TABLE Orcamento
(
	idOrcamento		INT				NOT NULL	PRIMARY KEY	IDENTITY,
	clienteId		INT				NOT NULL	REFERENCES	Cliente(idCliente),
	funcionarioId	INT				NOT NULL	REFERENCES	Funcionario(idFuncionario),
	veiculoId		INT				NOT NULL	REFERENCES	Veiculo(idVeiculo),
	data_criacao	DATETIME		NOT NULL,
	data_entrega	DATETIME		NULL,
	statusOrc		INT				NOT NULL	DEFAULT		1								CHECK	(statusOrc		in	(1, 2, 3, 4 ,5)), -- Em analise, aprovado, recusado, sendo executado, finalizado
	total			DECIMAL(10, 2)	NOT NULL	CHECK		(total >= 0),
	forma_pgto		VARCHAR(20)		NULL,
	parcelas		INT				NULL


)
GO

CREATE TABLE Servico
(
	idServico		INT				NOT NULL	PRIMARY KEY	IDENTITY,
	descricao		VARCHAR(50)		NOT NULL,
	valorBase		MONEY			NOT NULL
	
)
GO

CREATE TABLE Itens
(
	orcamentoId		INT				NOT NULL	REFERENCES Orcamento(idOrcamento),
	servicoId		INT				NOT NULL	REFERENCES Servico(idServico),
	funcionarioID	INT				NOT NULL	REFERENCES Funcionario(idFuncionario),
	pecaId			INT				NULL		REFERENCES Peca(idPeca),
	preco			MONEY			NOT NULL,
	desconto		MONEY			NULL,
	dataEntrega		DATETIME		NULL,
	PRIMARY KEY(orcamentoId,servicoId)
)
GO

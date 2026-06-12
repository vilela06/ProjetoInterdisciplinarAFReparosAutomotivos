USE AFReparosAutomotivos
GO

/* ============================================================
   FUNCOES ESCALARES
   ============================================================ */

CREATE OR ALTER FUNCTION FN_StatusOrcamentoTexto(@status INT)
RETURNS VARCHAR(30)
AS
BEGIN
    RETURN CASE @status
        WHEN 1 THEN 'Em analise'
        WHEN 2 THEN 'Aprovado'
        WHEN 3 THEN 'Recusado'
        WHEN 4 THEN 'Sendo executado'
        WHEN 5 THEN 'Finalizado'
        ELSE 'Catalogo'
    END
END
GO

-- Teste de execucao:
-- SELECT dbo.FN_StatusOrcamentoTexto(1) AS StatusOrcamento
-- GO

CREATE OR ALTER FUNCTION FN_FormatarEndereco
(
    @logradouro VARCHAR(150),
    @numero VARCHAR(5),
    @cidade VARCHAR(100),
    @estado VARCHAR(2),
    @cep VARCHAR(9)
)
RETURNS VARCHAR(300)
AS
BEGIN
    IF NULLIF(LTRIM(RTRIM(ISNULL(@logradouro, ''))), '') IS NULL
    BEGIN
        RETURN ''
    END

    RETURN CONCAT(
        ISNULL(@logradouro, ''),
        ', ',
        ISNULL(@numero, ''),
        ', ',
        ISNULL(@cidade, ''),
        ' - ',
        ISNULL(@estado, ''),
        ', ',
        ISNULL(@cep, '')
    )
END
GO

-- Teste de execucao:
-- SELECT dbo.FN_FormatarEndereco('Rua Teste', '10', 'Campinas', 'SP', '13000-000') AS Endereco
-- GO

CREATE OR ALTER FUNCTION FN_CalcularSubtotalItem
(
    @preco MONEY,
    @qtd INT,
    @valorPeca MONEY,
    @qtdPeca INT,
    @taxa DECIMAL(10,2),
    @desconto DECIMAL(10,2)
)
RETURNS DECIMAL(10,2)
AS
BEGIN
    DECLARE @subtotal DECIMAL(10,2)

    SET @subtotal =
        (CONVERT(DECIMAL(10,2), ISNULL(@preco, 0)) * IIF(ISNULL(@qtd, 0) <= 0, 1, @qtd)) +
        (CONVERT(DECIMAL(10,2), ISNULL(@valorPeca, 0)) * IIF(ISNULL(@qtdPeca, 0) <= 0, 0, @qtdPeca)) +
        ISNULL(@taxa, 0) -
        ISNULL(@desconto, 0)

    RETURN IIF(@subtotal < 0, 0, @subtotal)
END
GO

-- Teste de execucao:
-- SELECT dbo.FN_CalcularSubtotalItem(100, 2, 30, 1, 10, 5) AS Subtotal
-- GO

/* ============================================================
   VIEWS
   Uma view para cada tabela, exceto Pessoa por ser entidade pai.
   As entidades filhas da heranca usam join com Pessoa.
   ============================================================ */

-- 1. Visualizacao da tabela Cliente com dados da entidade pai Pessoa e Endereco.
CREATE OR ALTER VIEW VW_ClientesCompletos
AS
SELECT
    c.idCliente,
    p.nome,
    p.documento,
    c.telefone,
    p.celular,
    c.email,
    c.statusCli,
    c.chaveCli,
    p.tipo_doc,
    dbo.FN_FormatarEndereco(e.logradouro, e.numero, e.cidade, e.estado, e.CEP) AS endereco,
    ISNULL(e.logradouro, '') AS logradouro,
    ISNULL(e.numero, '') AS numero,
    ISNULL(e.cidade, '') AS cidade,
    ISNULL(e.estado, '') AS estado,
    ISNULL(e.CEP, '') AS CEP
FROM Cliente c
INNER JOIN Pessoa p ON p.idPessoa = c.idCliente
LEFT JOIN Endereco e ON e.pessoaId = p.idPessoa
GO

-- Teste de execucao:
-- SELECT * FROM VW_ClientesCompletos
-- GO

-- 2. Visualizacao da tabela Funcionario com dados da entidade pai Pessoa.
CREATE OR ALTER VIEW VW_FuncionariosCompletos
AS
SELECT
    f.idFuncionario,
    p.nome,
    p.celular,
    p.documento,
    p.tipo_doc,
    f.permissao,
    f.usuario,
    f.statusFunc,
    f.email,
    f.foto
FROM Funcionario f
INNER JOIN Pessoa p ON p.idPessoa = f.idFuncionario
GO

-- Teste de execucao:
-- SELECT * FROM VW_FuncionariosCompletos
-- GO

-- 3. Visualizacao da tabela Endereco com dados da Pessoa.
CREATE OR ALTER VIEW VW_EnderecosCompletos
AS
SELECT
    e.idEndereco,
    e.pessoaId,
    p.nome,
    p.documento,
    e.logradouro,
    e.numero,
    e.cidade,
    e.estado,
    e.CEP,
    dbo.FN_FormatarEndereco(e.logradouro, e.numero, e.cidade, e.estado, e.CEP) AS endereco
FROM Endereco e
INNER JOIN Pessoa p ON p.idPessoa = e.pessoaId
GO

-- Teste de execucao:
-- SELECT * FROM VW_EnderecosCompletos
-- GO

-- 4. Visualizacao da tabela HistoricoEndereco com dados da Pessoa.
CREATE OR ALTER VIEW VW_HistoricoEnderecosCompletos
AS
SELECT
    h.idHistorico,
    h.pessoaId,
    p.nome,
    p.documento,
    h.logradouro,
    h.Numero,
    h.cidade,
    h.estado,
    h.CEP,
    h.DataFim,
    dbo.FN_FormatarEndereco(h.logradouro, h.Numero, h.cidade, h.estado, h.CEP) AS endereco
FROM HistoricoEndereco h
INNER JOIN Pessoa p ON p.idPessoa = h.pessoaId
GO

-- Teste de execucao:
-- SELECT * FROM VW_HistoricoEnderecosCompletos
-- GO

-- 5. Visualizacao da tabela Veiculo com dados do Cliente.
CREATE OR ALTER VIEW VW_VeiculosCompletos
AS
SELECT
    v.idVeiculo,
    v.clienteId,
    p.nome AS nomeCliente,
    p.documento AS documentoCliente,
    v.placa,
    v.marca,
    v.modelo,
    v.cor,
    v.ano
FROM Veiculo v
INNER JOIN Cliente c ON c.idCliente = v.clienteId
INNER JOIN Pessoa p ON p.idPessoa = c.idCliente
GO

-- Teste de execucao:
-- SELECT * FROM VW_VeiculosCompletos
-- GO

-- 6. Visualizacao da tabela Peca com dados do Funcionario responsavel.
CREATE OR ALTER VIEW VW_PecasComFuncionario
AS
SELECT
    p.idPeca,
    p.nome,
    p.valor,
    p.qtdEsto,
    p.funcionarioId,
    pf.nome AS nomeFuncionario
FROM Peca p
INNER JOIN Funcionario f ON f.idFuncionario = p.funcionarioId
INNER JOIN Pessoa pf ON pf.idPessoa = f.idFuncionario
GO

-- Teste de execucao:
-- SELECT * FROM VW_PecasComFuncionario
-- GO

-- 7. Visualizacao da tabela Servico com dados de orcamento e funcionario, quando houver.
CREATE OR ALTER VIEW VW_ServicosCompletos
AS
SELECT
    s.idServico,
    s.descricao,
    s.valorBase,
    COALESCE(p.nome, 'Sem responsavel') AS funcionario,
    MAX(o.idOrcamento) AS idOrcamento,
    dbo.FN_StatusOrcamentoTexto(MAX(o.statusOrc)) AS statusServico
FROM Servico s
LEFT JOIN Itens i ON i.servicoId = s.idServico
LEFT JOIN Funcionario f ON f.idFuncionario = i.funcionarioID
LEFT JOIN Pessoa p ON p.idPessoa = f.idFuncionario
LEFT JOIN Orcamento o ON o.idOrcamento = i.orcamentoId
GROUP BY s.idServico, s.descricao, s.valorBase, p.nome
GO

-- Teste de execucao:
-- SELECT * FROM VW_ServicosCompletos
-- GO

-- 8. Visualizacao da tabela Orcamento com cliente, funcionario, veiculo e endereco.
CREATE OR ALTER VIEW VW_OrcamentosCompletos
AS
SELECT
    o.idOrcamento,
    o.clienteId,
    o.funcionarioId,
    o.veiculoId,
    o.data_criacao,
    o.data_entrega,
    o.statusOrc,
    o.total,
    o.forma_pgto,
    o.parcelas,
    pc.nome AS nomeCliente,
    pf.nome AS nomeFuncionario,
    pc.documento,
    COALESCE(c.telefone, pc.celular) AS telefoneContato,
    dbo.FN_FormatarEndereco(e.logradouro, e.numero, e.cidade, e.estado, e.CEP) AS endereco,
    v.placa,
    v.marca,
    v.modelo,
    v.cor,
    v.ano,
    c.chaveCli
FROM Orcamento o
INNER JOIN Cliente c ON c.idCliente = o.clienteId
INNER JOIN Pessoa pc ON pc.idPessoa = c.idCliente
INNER JOIN Funcionario f ON f.idFuncionario = o.funcionarioId
INNER JOIN Pessoa pf ON pf.idPessoa = f.idFuncionario
INNER JOIN Veiculo v ON v.idVeiculo = o.veiculoId
LEFT JOIN Endereco e ON e.pessoaId = pc.idPessoa
GO

-- Teste de execucao:
-- SELECT * FROM VW_OrcamentosCompletos
-- GO

-- 9. Visualizacao da tabela Itens com servico, orcamento, peca e funcionario.
CREATE OR ALTER VIEW VW_ItensOrcamentoCompletos
AS
SELECT
    i.orcamentoId,
    i.servicoId,
    i.funcionarioID,
    i.pecaId,
    i.qtd,
    i.preco,
    i.desconto,
    i.taxa,
    i.qtdPeca,
    i.dataEntrega,
    s.descricao AS descricaoServico,
    o.veiculoId,
    p.nome AS nomePeca,
    p.valor AS valorPeca,
    pf.nome AS nomeFuncionario
FROM Itens i
INNER JOIN Servico s ON s.idServico = i.servicoId
INNER JOIN Orcamento o ON o.idOrcamento = i.orcamentoId
INNER JOIN Funcionario f ON f.idFuncionario = i.funcionarioID
INNER JOIN Pessoa pf ON pf.idPessoa = f.idFuncionario
LEFT JOIN Peca p ON p.idPeca = i.pecaId
GO

-- Teste de execucao:
-- SELECT * FROM VW_ItensOrcamentoCompletos
-- GO

-- 10. Visualizacao dos dados principais da empresa pelo primeiro funcionario.
CREATE OR ALTER VIEW VW_EmpresaDados
AS
SELECT
    f.idFuncionario,
    p.nome AS nomeEmpresa,
    p.documento AS cnpj,
    p.celular,
    ISNULL(f.email, 'afreparos@gmail.com') AS email,
    ISNULL(f.foto, '/images/logo-af-reparos.png') AS foto,
    ISNULL(e.logradouro, '') AS logradouro,
    ISNULL(e.numero, '') AS numero,
    ISNULL(e.cidade, '') AS cidade,
    ISNULL(e.estado, '') AS estado,
    ISNULL(e.CEP, '') AS CEP
FROM Funcionario f
INNER JOIN Pessoa p ON p.idPessoa = f.idFuncionario
LEFT JOIN Endereco e ON e.pessoaId = p.idPessoa
GO

-- Teste de execucao:
-- SELECT * FROM VW_EmpresaDados
-- GO

/* ============================================================
   FUNCOES DO TIPO TABLE
   ============================================================ */

CREATE OR ALTER FUNCTION FN_BuscarClientes(@termo VARCHAR(80))
RETURNS TABLE
AS
RETURN
(
    SELECT *
    FROM VW_ClientesCompletos
    WHERE NULLIF(LTRIM(RTRIM(@termo)), '') IS NULL
       OR nome LIKE '%' + @termo + '%'
       OR CONVERT(VARCHAR(20), idCliente) LIKE '%' + @termo + '%'
)
GO

-- Teste de execucao:
-- SELECT * FROM FN_BuscarClientes('Roberto')
-- GO

CREATE OR ALTER FUNCTION FN_BuscarFuncionarios(@termo VARCHAR(80))
RETURNS TABLE
AS
RETURN
(
    SELECT *
    FROM VW_FuncionariosCompletos
    WHERE NULLIF(LTRIM(RTRIM(@termo)), '') IS NULL
       OR CONVERT(VARCHAR(20), idFuncionario) LIKE '%' + @termo + '%'
       OR nome LIKE '%' + @termo + '%'
       OR usuario LIKE '%' + @termo + '%'
)
GO

-- Teste de execucao:
-- SELECT * FROM FN_BuscarFuncionarios('admin')
-- GO

CREATE OR ALTER FUNCTION FN_BuscarPecas(@termo VARCHAR(50))
RETURNS TABLE
AS
RETURN
(
    SELECT *
    FROM VW_PecasComFuncionario
    WHERE NULLIF(LTRIM(RTRIM(@termo)), '') IS NULL
       OR nome LIKE '%' + @termo + '%'
       OR CONVERT(VARCHAR(20), idPeca) LIKE '%' + @termo + '%'
)
GO

-- Teste de execucao:
-- SELECT * FROM FN_BuscarPecas('Lampada')
-- GO

CREATE OR ALTER FUNCTION FN_BuscarVeiculosCliente(@clienteId INT, @termo VARCHAR(50))
RETURNS TABLE
AS
RETURN
(
    SELECT idVeiculo, clienteId, placa, marca, modelo, cor, ano
    FROM Veiculo
    WHERE clienteId = @clienteId
      AND (
            NULLIF(LTRIM(RTRIM(@termo)), '') IS NULL
         OR placa LIKE '%' + @termo + '%'
         OR marca LIKE '%' + @termo + '%'
         OR modelo LIKE '%' + @termo + '%'
         OR cor LIKE '%' + @termo + '%'
         OR CONVERT(VARCHAR(10), ano) LIKE '%' + @termo + '%'
      )
)
GO

-- Teste de execucao:
-- SELECT * FROM FN_BuscarVeiculosCliente(9, 'ABC')
-- GO

CREATE OR ALTER FUNCTION FN_FiltrarOrcamentos
(
    @statusId INT,
    @cpf VARCHAR(30),
    @nome VARCHAR(80),
    @busca VARCHAR(80),
    @dataCriacao DATE,
    @dataEntrega DATE,
    @formaPagamento VARCHAR(20),
    @parcelas INT,
    @preco DECIMAL(10,2)
)
RETURNS TABLE
AS
RETURN
(
    SELECT *
    FROM VW_OrcamentosCompletos
    WHERE (@statusId IS NULL OR statusOrc = @statusId)
      AND (NULLIF(LTRIM(RTRIM(@cpf)), '') IS NULL OR documento LIKE '%' + @cpf + '%')
      AND (NULLIF(LTRIM(RTRIM(@nome)), '') IS NULL OR nomeCliente LIKE '%' + @nome + '%')
      AND (
            NULLIF(LTRIM(RTRIM(@busca)), '') IS NULL
         OR nomeCliente LIKE '%' + @busca + '%'
         OR CONVERT(VARCHAR(20), idOrcamento) LIKE '%' + @busca + '%'
         OR CONVERT(VARCHAR(10), data_criacao, 23) LIKE '%' + @busca + '%'
         OR CONVERT(VARCHAR(10), data_criacao, 103) LIKE '%' + @busca + '%'
         OR CONVERT(VARCHAR(10), data_criacao, 105) LIKE '%' + @busca + '%'
      )
      AND (@dataCriacao IS NULL OR CAST(data_criacao AS DATE) = @dataCriacao)
      AND (@dataEntrega IS NULL OR CAST(data_entrega AS DATE) = @dataEntrega)
      AND (NULLIF(LTRIM(RTRIM(@formaPagamento)), '') IS NULL OR forma_pgto LIKE '%' + @formaPagamento + '%')
      AND (@parcelas IS NULL OR parcelas = @parcelas)
      AND (@preco IS NULL OR total = @preco)
)
GO

-- Teste de execucao:
-- SELECT * FROM FN_FiltrarOrcamentos(1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
-- GO

/* ============================================================
   PROCEDURES DE INSERT E UPDATE POR TABELA
   Nao ha procedure exclusiva para Pessoa, pois ela e entidade pai.
   ============================================================ */

-- 1. Procedure para cadastrar um endereco na tabela Endereco.
CREATE OR ALTER PROCEDURE SP_AdicionarEndereco
    @pessoaId INT,
    @logradouro VARCHAR(150),
    @numero VARCHAR(5),
    @cidade VARCHAR(100),
    @estado VARCHAR(2),
    @cep VARCHAR(9)
AS
BEGIN
    INSERT INTO Endereco (pessoaId, logradouro, numero, cidade, estado, CEP)
    VALUES (@pessoaId, @logradouro, @numero, @cidade, @estado, @cep)
END
GO

-- Teste de execucao:
-- EXEC SP_AdicionarEndereco 1, 'Rua Teste', '10', 'Campinas', 'SP', '13000-000'
-- GO

-- 2. Procedure para atualizar um endereco na tabela Endereco.
CREATE OR ALTER PROCEDURE SP_AtualizarEndereco
    @idEndereco INT,
    @pessoaId INT,
    @logradouro VARCHAR(150),
    @numero VARCHAR(5),
    @cidade VARCHAR(100),
    @estado VARCHAR(2),
    @cep VARCHAR(9)
AS
BEGIN
    UPDATE Endereco
       SET pessoaId = @pessoaId,
           logradouro = @logradouro,
           numero = @numero,
           cidade = @cidade,
           estado = @estado,
           CEP = @cep
     WHERE idEndereco = @idEndereco
END
GO

-- Teste de execucao:
-- EXEC SP_AtualizarEndereco 1, 1, 'Rua Alterada', '11', 'Campinas', 'SP', '13000-001'
-- GO

-- 3. Procedure para cadastrar historico na tabela HistoricoEndereco.
CREATE OR ALTER PROCEDURE SP_AdicionarHistoricoEndereco
    @pessoaId INT,
    @logradouro VARCHAR(150),
    @numero VARCHAR(5),
    @cidade VARCHAR(100),
    @estado VARCHAR(2),
    @cep VARCHAR(9),
    @dataFim DATETIME = NULL
AS
BEGIN
    INSERT INTO HistoricoEndereco (pessoaId, logradouro, Numero, cidade, estado, CEP, DataFim)
    VALUES (@pessoaId, @logradouro, @numero, @cidade, @estado, @cep, ISNULL(@dataFim, GETDATE()))
END
GO

-- Teste de execucao:
-- EXEC SP_AdicionarHistoricoEndereco 1, 'Rua Antiga', '9', 'Campinas', 'SP', '13000-000', NULL
-- GO

-- 4. Procedure para atualizar historico na tabela HistoricoEndereco.
CREATE OR ALTER PROCEDURE SP_AtualizarHistoricoEndereco
    @idHistorico INT,
    @pessoaId INT,
    @logradouro VARCHAR(150),
    @numero VARCHAR(5),
    @cidade VARCHAR(100),
    @estado VARCHAR(2),
    @cep VARCHAR(9),
    @dataFim DATETIME
AS
BEGIN
    UPDATE HistoricoEndereco
       SET pessoaId = @pessoaId,
           logradouro = @logradouro,
           Numero = @numero,
           cidade = @cidade,
           estado = @estado,
           CEP = @cep,
           DataFim = @dataFim
     WHERE idHistorico = @idHistorico
END
GO

-- Teste de execucao:
-- EXEC SP_AtualizarHistoricoEndereco 1, 1, 'Rua Historico', '9', 'Campinas', 'SP', '13000-002', '2026-01-01'
-- GO

-- 5. Procedure para cadastrar um funcionario na tabela Funcionario com dados da Pessoa.
CREATE OR ALTER PROCEDURE SP_AdicionarFuncionario
    @nome VARCHAR(50),
    @celular VARCHAR(15),
    @documento VARCHAR(18),
    @tipo_doc CHAR(1),
    @permissao INT,
    @usuario VARCHAR(16),
    @senha VARCHAR(15),
    @statusFunc INT
AS
BEGIN
    DECLARE @id INT

    INSERT INTO Pessoa (nome, celular, documento, tipo_doc)
    VALUES (@nome, @celular, @documento, @tipo_doc)

    SET @id = SCOPE_IDENTITY()

    INSERT INTO Funcionario (idFuncionario, permissao, usuario, senha, statusFunc)
    VALUES (@id, @permissao, @usuario, @senha, @statusFunc)

    SELECT @id AS idFuncionario
END
GO

-- Teste de execucao:
-- EXEC SP_AdicionarFuncionario 'Funcionario Teste', '(19)99999-0000', '000.000.000-00', 'F', 3, 'testeuser', '123', 1
-- GO

-- 6. Procedure para atualizar um funcionario na tabela Funcionario com dados da Pessoa.
CREATE OR ALTER PROCEDURE SP_AtualizarFuncionario
    @idFuncionario INT,
    @nome VARCHAR(50),
    @celular VARCHAR(15),
    @documento VARCHAR(18),
    @tipo_doc CHAR(1),
    @permissao INT,
    @usuario VARCHAR(16),
    @senha VARCHAR(15),
    @statusFunc INT
AS
BEGIN
    UPDATE Pessoa
       SET nome = @nome,
           celular = @celular,
           documento = @documento,
           tipo_doc = @tipo_doc
     WHERE idPessoa = @idFuncionario

    UPDATE Funcionario
       SET permissao = @permissao,
           usuario = @usuario,
           senha = COALESCE(NULLIF(@senha, ''), senha),
           statusFunc = @statusFunc
     WHERE idFuncionario = @idFuncionario
END
GO

-- Teste de execucao:
-- EXEC SP_AtualizarFuncionario 1, 'AF Reparos Automotivos', '(19)98765-4321', '12.345.678/0001-10', 'J', 1, 'SuperAdmin', NULL, 1
-- GO

-- 7. Procedure para cadastrar um cliente na tabela Cliente com dados da Pessoa.
CREATE OR ALTER PROCEDURE SP_AdicionarCliente
    @nome VARCHAR(50),
    @celular VARCHAR(15),
    @documento VARCHAR(18),
    @tipo_doc CHAR(1),
    @telefone VARCHAR(14) = NULL,
    @email VARCHAR(50) = NULL,
    @logradouro VARCHAR(150) = NULL,
    @numero VARCHAR(5) = NULL,
    @cidade VARCHAR(100) = NULL,
    @estado VARCHAR(2) = NULL,
    @cep VARCHAR(9) = NULL
AS
BEGIN
    DECLARE @id INT

    INSERT INTO Pessoa (nome, celular, documento, tipo_doc)
    VALUES (@nome, @celular, @documento, @tipo_doc)

    SET @id = SCOPE_IDENTITY()

    INSERT INTO Cliente (idCliente, telefone, email, statusCli, chaveCli)
    VALUES (@id, NULLIF(@telefone, ''), COALESCE(NULLIF(@email, ''), CONCAT(@id, '@sem-email.local')), 1, LEFT(REPLACE(CONVERT(VARCHAR(36), NEWID()), '-', ''), 19))

    IF NULLIF(LTRIM(RTRIM(ISNULL(@logradouro, ''))), '') IS NOT NULL
    BEGIN
        EXEC SP_UpsertEndereco @id, @logradouro, @numero, @cidade, @estado, @cep
    END

    SELECT @id AS idCliente
END
GO

-- Teste de execucao:
-- EXEC SP_AdicionarCliente 'Cliente Teste', '(19)99999-1111', '111.111.111-11', 'F', NULL, 'cliente@teste.com', 'Rua Cliente', '10', 'Campinas', 'SP', '13000-000'
-- GO

-- 8. Procedure para atualizar um cliente na tabela Cliente com dados da Pessoa.
CREATE OR ALTER PROCEDURE SP_AtualizarCliente
    @id INT,
    @nome VARCHAR(50),
    @celular VARCHAR(15),
    @documento VARCHAR(18),
    @tipo_doc CHAR(1),
    @telefone VARCHAR(14) = NULL,
    @email VARCHAR(50) = NULL,
    @logradouro VARCHAR(150) = NULL,
    @numero VARCHAR(5) = NULL,
    @cidade VARCHAR(100) = NULL,
    @estado VARCHAR(2) = NULL,
    @cep VARCHAR(9) = NULL
AS
BEGIN
    UPDATE Pessoa
       SET nome = @nome,
           celular = @celular,
           documento = @documento,
           tipo_doc = @tipo_doc
     WHERE idPessoa = @id

    UPDATE Cliente
       SET telefone = NULLIF(@telefone, ''),
           email = COALESCE(NULLIF(@email, ''), CONCAT(@id, '@sem-email.local'))
     WHERE idCliente = @id

    IF NULLIF(LTRIM(RTRIM(ISNULL(@logradouro, ''))), '') IS NOT NULL
    BEGIN
        EXEC SP_UpsertEndereco @id, @logradouro, @numero, @cidade, @estado, @cep
    END
END
GO

-- Teste de execucao:
-- EXEC SP_AtualizarCliente 9, 'Roberto Almeida', '(19)92222-1111', '901.234.567-89', 'F', NULL, 'roberto@email.com', 'Rua Cliente Um', '10', 'Campinas', 'SP', '13060-000'
-- GO

-- 9. Procedure para cadastrar um veiculo na tabela Veiculo.
CREATE OR ALTER PROCEDURE SP_AdicionarVeiculo
    @clienteId INT,
    @marca VARCHAR(50),
    @placa VARCHAR(7),
    @modelo VARCHAR(50),
    @cor VARCHAR(20),
    @ano INT
AS
BEGIN
    INSERT INTO Veiculo (clienteId, marca, placa, modelo, cor, ano)
    VALUES (@clienteId, @marca, @placa, @modelo, @cor, @ano)

    SELECT SCOPE_IDENTITY() AS idVeiculo
END
GO

-- Teste de execucao:
-- EXEC SP_AdicionarVeiculo 9, 'Toyota', 'ZZZ9999', 'Corolla', 'Prata', 2022
-- GO

-- 10. Procedure para atualizar um veiculo na tabela Veiculo.
CREATE OR ALTER PROCEDURE SP_AtualizarVeiculo
    @idVeiculo INT,
    @clienteId INT,
    @marca VARCHAR(50),
    @placa VARCHAR(7),
    @modelo VARCHAR(50),
    @cor VARCHAR(20),
    @ano INT
AS
BEGIN
    UPDATE Veiculo
       SET clienteId = @clienteId,
           marca = @marca,
           placa = @placa,
           modelo = @modelo,
           cor = @cor,
           ano = @ano
     WHERE idVeiculo = @idVeiculo
END
GO

-- Teste de execucao:
-- EXEC SP_AtualizarVeiculo 1, 9, 'Toyota', 'ABC1234', 'Corolla', 'Prata', 2022
-- GO

-- 11. Procedure para cadastrar uma peca na tabela Peca.
CREATE OR ALTER PROCEDURE SP_AdicionarPeca
    @funcionarioId INT,
    @nome VARCHAR(20),
    @valor MONEY,
    @qtdEsto INT
AS
BEGIN
    INSERT INTO Peca (funcionarioId, nome, valor, qtdEsto)
    VALUES (@funcionarioId, @nome, @valor, @qtdEsto)

    SELECT SCOPE_IDENTITY() AS idPeca
END
GO

-- Teste de execucao:
-- EXEC SP_AdicionarPeca 1, 'Peca Teste', 50.00, 10
-- GO

-- 12. Procedure para atualizar uma peca na tabela Peca.
CREATE OR ALTER PROCEDURE SP_AtualizarPeca
    @id INT,
    @funcionarioId INT,
    @nome VARCHAR(20),
    @valor MONEY,
    @qtdEsto INT
AS
BEGIN
    UPDATE Peca
       SET funcionarioId = COALESCE(@funcionarioId, funcionarioId),
           nome = @nome,
           valor = @valor,
           qtdEsto = @qtdEsto
     WHERE idPeca = @id
END
GO

-- Teste de execucao:
-- EXEC SP_AtualizarPeca 1, 1, 'Lampada', 30.00, 34
-- GO

-- 13. Procedure para cadastrar um orcamento na tabela Orcamento.
CREATE OR ALTER PROCEDURE SP_AdicionarOrcamento
    @clienteId INT,
    @funcionarioId INT,
    @veiculoId INT,
    @data_criacao DATETIME,
    @data_entrega DATETIME,
    @statusOrc INT,
    @total DECIMAL(10,2),
    @forma_pgto VARCHAR(20) = NULL,
    @parcelas INT = NULL
AS
BEGIN
    INSERT INTO Orcamento (clienteId, funcionarioId, veiculoId, data_criacao, data_entrega, statusOrc, total, forma_pgto, parcelas)
    VALUES (@clienteId, @funcionarioId, @veiculoId, @data_criacao, @data_entrega, IIF(@statusOrc = 0, 1, @statusOrc), @total, NULLIF(@forma_pgto, ''), NULLIF(@parcelas, 0))

    SELECT SCOPE_IDENTITY() AS idOrcamento
END
GO

-- Teste de execucao:
-- EXEC SP_AdicionarOrcamento 9, 3, 1, GETDATE(), NULL, 1, 500.00, NULL, NULL
-- GO

-- 14. Procedure para atualizar um orcamento na tabela Orcamento.
CREATE OR ALTER PROCEDURE SP_AtualizarOrcamento
    @id INT,
    @data_entrega DATETIME,
    @statusOrc INT,
    @total DECIMAL(10,2),
    @forma_pgto VARCHAR(20) = NULL,
    @parcelas INT = NULL
AS
BEGIN
    UPDATE Orcamento
       SET data_entrega = @data_entrega,
           statusOrc = @statusOrc,
           total = @total,
           forma_pgto = NULLIF(@forma_pgto, ''),
           parcelas = NULLIF(@parcelas, 0)
     WHERE idOrcamento = @id
END
GO

-- Teste de execucao:
-- EXEC SP_AtualizarOrcamento 1, '2025-01-15', 2, 850.00, 'Pix', 1
-- GO

-- 15. Procedure para cadastrar um servico na tabela Servico.
CREATE OR ALTER PROCEDURE SP_AdicionarServico
    @descricao VARCHAR(50),
    @valorBase MONEY
AS
BEGIN
    INSERT INTO Servico (descricao, valorBase)
    VALUES (@descricao, @valorBase)

    SELECT SCOPE_IDENTITY() AS idServico
END
GO

-- Teste de execucao:
-- EXEC SP_AdicionarServico 'Servico Teste', 100.00
-- GO

-- 16. Procedure para atualizar um servico na tabela Servico.
CREATE OR ALTER PROCEDURE SP_AtualizarServico
    @idServico INT,
    @descricao VARCHAR(50),
    @valorBase MONEY
AS
BEGIN
    UPDATE Servico
       SET descricao = @descricao,
           valorBase = @valorBase
     WHERE idServico = @idServico
END
GO

-- Teste de execucao:
-- EXEC SP_AtualizarServico 1, 'Funilaria Porta', 850.00
-- GO

-- 17. Procedure para cadastrar um item na tabela Itens.
CREATE OR ALTER PROCEDURE SP_AdicionarItemOrcamento
    @orcamentoId INT,
    @servicoId INT,
    @funcionarioID INT,
    @pecaId INT,
    @qtd INT,
    @qtdPeca INT,
    @preco MONEY,
    @desconto DECIMAL(10,2),
    @taxa DECIMAL(10,2),
    @dataEntrega DATETIME
AS
BEGIN
    INSERT INTO Itens (orcamentoId, servicoId, funcionarioID, pecaId, qtd, qtdPeca, preco, desconto, taxa, dataEntrega)
    VALUES (@orcamentoId, @servicoId, @funcionarioID, @pecaId, IIF(@qtd <= 0, 1, @qtd), @qtdPeca, @preco, @desconto, @taxa, @dataEntrega)
END
GO

-- Teste de execucao:
-- EXEC SP_AdicionarItemOrcamento 1, 1, 3, NULL, 1, NULL, 850.00, 50.00, 0.00, '2025-01-15'
-- GO

-- 18. Procedure para atualizar um item na tabela Itens.
CREATE OR ALTER PROCEDURE SP_AtualizarItemOrcamento
    @orcamentoId INT,
    @servicoId INT,
    @funcionarioID INT,
    @pecaId INT,
    @qtd INT,
    @qtdPeca INT,
    @preco MONEY,
    @desconto DECIMAL(10,2),
    @taxa DECIMAL(10,2),
    @dataEntrega DATETIME
AS
BEGIN
    UPDATE Itens
       SET funcionarioID = @funcionarioID,
           pecaId = @pecaId,
           qtd = IIF(@qtd <= 0, 1, @qtd),
           qtdPeca = @qtdPeca,
           preco = @preco,
           desconto = @desconto,
           taxa = @taxa,
           dataEntrega = @dataEntrega
     WHERE orcamentoId = @orcamentoId
       AND servicoId = @servicoId
END
GO

-- Teste de execucao:
-- EXEC SP_AtualizarItemOrcamento 1, 1, 3, NULL, 1, NULL, 850.00, 50.00, 0.00, '2025-01-15'
-- GO

/* ============================================================
   PROCEDURES AUXILIARES USADAS PELA APLICACAO
   ============================================================ */

CREATE OR ALTER PROCEDURE SP_UpsertEndereco
    @pessoaId INT,
    @logradouro VARCHAR(150),
    @numero VARCHAR(5),
    @cidade VARCHAR(100),
    @estado VARCHAR(2),
    @cep VARCHAR(9)
AS
BEGIN
    MERGE Endereco AS destino
    USING (SELECT @pessoaId AS pessoaId) AS origem
       ON destino.pessoaId = origem.pessoaId
    WHEN MATCHED THEN
        UPDATE SET logradouro = @logradouro, numero = @numero, cidade = @cidade, estado = @estado, CEP = @cep
    WHEN NOT MATCHED THEN
        INSERT (pessoaId, logradouro, numero, cidade, estado, CEP)
        VALUES (@pessoaId, @logradouro, @numero, @cidade, @estado, @cep);
END
GO

CREATE OR ALTER PROCEDURE SP_ListarClientes
AS
BEGIN
    SELECT * FROM VW_ClientesCompletos ORDER BY nome
END
GO

CREATE OR ALTER PROCEDURE SP_BuscarClientes
    @termo VARCHAR(80) = NULL
AS
BEGIN
    SELECT * FROM FN_BuscarClientes(@termo) ORDER BY nome
END
GO

CREATE OR ALTER PROCEDURE SP_ObterClientePorId
    @id INT
AS
BEGIN
    SELECT * FROM VW_ClientesCompletos WHERE idCliente = @id
END
GO

CREATE OR ALTER PROCEDURE SP_ExcluirClienteCriado
    @id INT
AS
BEGIN
    DELETE FROM Endereco WHERE pessoaId = @id
    DELETE FROM Cliente WHERE idCliente = @id
    DELETE FROM Pessoa WHERE idPessoa = @id
END
GO

CREATE OR ALTER PROCEDURE SP_ListarFuncionarios
AS
BEGIN
    SELECT idFuncionario, nome, celular, documento, tipo_doc, permissao, usuario, statusFunc
    FROM VW_FuncionariosCompletos
    ORDER BY nome
END
GO

CREATE OR ALTER PROCEDURE SP_ListarFuncionariosAtivos
AS
BEGIN
    SELECT idFuncionario, nome, celular, documento, tipo_doc, permissao, usuario, statusFunc
    FROM VW_FuncionariosCompletos
    WHERE statusFunc = 1
    ORDER BY nome
END
GO

CREATE OR ALTER PROCEDURE SP_BuscarFuncionarios
    @pesquisa VARCHAR(80) = NULL
AS
BEGIN
    SELECT idFuncionario, nome, celular, documento, tipo_doc, permissao, usuario, statusFunc
    FROM FN_BuscarFuncionarios(@pesquisa)
    ORDER BY nome
END
GO

CREATE OR ALTER PROCEDURE SP_ObterFuncionarioPorId
    @id INT
AS
BEGIN
    SELECT idFuncionario, nome, celular, documento, tipo_doc, permissao, usuario, statusFunc
    FROM VW_FuncionariosCompletos
    WHERE idFuncionario = @id
END
GO

CREATE OR ALTER PROCEDURE SP_AutenticarFuncionario
    @usuario VARCHAR(16),
    @senha VARCHAR(15)
AS
BEGIN
    SELECT idFuncionario, nome, permissao, usuario, statusFunc
    FROM VW_FuncionariosCompletos v
    WHERE usuario = @usuario
      AND statusFunc = 1
      AND EXISTS (
          SELECT 1
          FROM Funcionario f
          WHERE f.idFuncionario = v.idFuncionario
            AND f.senha = @senha
      )
END
GO

CREATE OR ALTER PROCEDURE SP_ObterEmpresaDados
AS
BEGIN
    SELECT TOP 1 *
    FROM VW_EmpresaDados
    ORDER BY idFuncionario
END
GO

CREATE OR ALTER PROCEDURE SP_AtualizarEmpresa
    @idFuncionario INT,
    @nome VARCHAR(50),
    @celular VARCHAR(15),
    @cnpj VARCHAR(18),
    @email VARCHAR(50),
    @foto VARCHAR(150),
    @logradouro VARCHAR(150),
    @numero VARCHAR(5),
    @cidade VARCHAR(100),
    @estado VARCHAR(2),
    @cep VARCHAR(9)
AS
BEGIN
    UPDATE Pessoa
       SET nome = @nome, celular = @celular, documento = @cnpj, tipo_doc = 'J'
     WHERE idPessoa = @idFuncionario

    UPDATE Funcionario
       SET email = @email, foto = COALESCE(NULLIF(@foto, ''), '/images/logo-af-reparos.png')
     WHERE idFuncionario = @idFuncionario

    EXEC SP_UpsertEndereco @idFuncionario, @logradouro, @numero, @cidade, @estado, @cep
END
GO

CREATE OR ALTER PROCEDURE SP_ListarPecas
AS
BEGIN
    SELECT idPeca, nome, valor, qtdEsto, funcionarioId, nomeFuncionario
    FROM VW_PecasComFuncionario
    ORDER BY nome
END
GO

CREATE OR ALTER PROCEDURE SP_ListarPecasDisponiveis
AS
BEGIN
    SELECT idPeca, nome, valor, qtdEsto, funcionarioId, nomeFuncionario
    FROM VW_PecasComFuncionario
    WHERE qtdEsto > 0
    ORDER BY nome
END
GO

CREATE OR ALTER PROCEDURE SP_BuscarPecas
    @termo VARCHAR(50) = NULL
AS
BEGIN
    SELECT idPeca, nome, valor, qtdEsto, funcionarioId, nomeFuncionario
    FROM FN_BuscarPecas(@termo)
    ORDER BY nome
END
GO

CREATE OR ALTER PROCEDURE SP_ObterPecaPorId
    @id INT
AS
BEGIN
    SELECT idPeca, nome, valor, qtdEsto, funcionarioId, nomeFuncionario
    FROM VW_PecasComFuncionario
    WHERE idPeca = @id
END
GO

CREATE OR ALTER PROCEDURE SP_ListarServicos
    @termo VARCHAR(80) = NULL
AS
BEGIN
    SELECT idServico, descricao, valorBase, funcionario, idOrcamento, statusServico
    FROM VW_ServicosCompletos
    WHERE NULLIF(LTRIM(RTRIM(@termo)), '') IS NULL
       OR funcionario LIKE '%' + @termo + '%'
       OR CONVERT(VARCHAR(20), idOrcamento) LIKE '%' + @termo + '%'
    ORDER BY descricao
END
GO

CREATE OR ALTER PROCEDURE SP_ObterServicoPorId
    @id INT
AS
BEGIN
    SELECT idServico, descricao, valorBase, funcionario, idOrcamento, statusServico
    FROM VW_ServicosCompletos
    WHERE idServico = @id
END
GO

CREATE OR ALTER PROCEDURE SP_ExcluirServicoCriado
    @id INT
AS
BEGIN
    DELETE FROM Servico WHERE idServico = @id AND NOT EXISTS (SELECT 1 FROM Itens WHERE servicoId = @id)
END
GO

CREATE OR ALTER PROCEDURE SP_ObterVeiculoPorId
    @id INT
AS
BEGIN
    SELECT idVeiculo, clienteId, placa, marca, modelo, cor, ano
    FROM Veiculo
    WHERE idVeiculo = @id
END
GO

CREATE OR ALTER PROCEDURE SP_BuscarVeiculosCliente
    @clienteId INT,
    @termo VARCHAR(50) = NULL
AS
BEGIN
    SELECT idVeiculo, clienteId, placa, marca, modelo, cor, ano
    FROM FN_BuscarVeiculosCliente(@clienteId, @termo)
    ORDER BY placa
END
GO

CREATE OR ALTER PROCEDURE SP_ExcluirVeiculoCriado
    @id INT
AS
BEGIN
    DELETE FROM Veiculo WHERE idVeiculo = @id
END
GO

CREATE OR ALTER PROCEDURE SP_FiltrarOrcamentos
    @statusId INT = NULL,
    @cpf VARCHAR(30) = NULL,
    @nome VARCHAR(80) = NULL,
    @busca VARCHAR(80) = NULL,
    @dataCriacao DATE = NULL,
    @dataEntrega DATE = NULL,
    @formaPagamento VARCHAR(20) = NULL,
    @parcelas INT = NULL,
    @preco DECIMAL(10,2) = NULL
AS
BEGIN
    SELECT *
    FROM FN_FiltrarOrcamentos(@statusId, @cpf, @nome, @busca, @dataCriacao, @dataEntrega, @formaPagamento, @parcelas, @preco)
    ORDER BY data_criacao DESC
END
GO

CREATE OR ALTER PROCEDURE SP_ListarOrcamentosPorChave
    @chaveAcesso VARCHAR(19)
AS
BEGIN
    SELECT *
    FROM VW_OrcamentosCompletos
    WHERE chaveCli = @chaveAcesso
    ORDER BY data_criacao DESC
END
GO

CREATE OR ALTER PROCEDURE SP_ObterOrcamentoPorId
    @id INT
AS
BEGIN
    SELECT *
    FROM VW_OrcamentosCompletos
    WHERE idOrcamento = @id
END
GO

CREATE OR ALTER PROCEDURE SP_AtualizarStatusOrcamentoCliente
    @id INT,
    @chaveAcesso VARCHAR(19),
    @status INT,
    @forma_pgto VARCHAR(20) = NULL,
    @parcelas INT = NULL
AS
BEGIN
    IF @status = 2 AND (NULLIF(LTRIM(RTRIM(ISNULL(@forma_pgto, ''))), '') IS NULL OR ISNULL(@parcelas, 0) NOT BETWEEN 1 AND 12)
    BEGIN
        THROW 51003, 'Informe forma de pagamento e parcelas para aprovar o orcamento.', 1;
    END

    UPDATE o
       SET statusOrc = @status,
           forma_pgto = CASE WHEN @status = 2 THEN @forma_pgto ELSE forma_pgto END,
           parcelas = CASE WHEN @status = 2 THEN @parcelas ELSE parcelas END
    FROM Orcamento o
    INNER JOIN Cliente c ON c.idCliente = o.clienteId
    WHERE o.idOrcamento = @id
      AND c.chaveCli = @chaveAcesso
      AND o.statusOrc = 1
      AND @status IN (2, 3)

    SELECT @@ROWCOUNT AS LinhasAfetadas
END
GO

CREATE OR ALTER PROCEDURE SP_ExcluirOrcamento
    @id INT
AS
BEGIN
    DELETE FROM Itens WHERE orcamentoId = @id
    DELETE FROM Orcamento WHERE idOrcamento = @id
END
GO

CREATE OR ALTER PROCEDURE SP_ExcluirItensOrcamento
    @orcamentoId INT
AS
BEGIN
    DELETE FROM Itens WHERE orcamentoId = @orcamentoId
END
GO

CREATE OR ALTER PROCEDURE SP_ListarItensOrcamento
    @orcamentoId INT
AS
BEGIN
    SELECT orcamentoId, servicoId, funcionarioID, pecaId, qtd, preco, desconto, taxa, qtdPeca, dataEntrega, descricaoServico, veiculoId
    FROM VW_ItensOrcamentoCompletos
    WHERE orcamentoId = @orcamentoId
END
GO

/* ============================================================
   TRIGGERS
   ============================================================ */

CREATE OR ALTER TRIGGER TR_SalvarHistoricoEndereco
ON Endereco
AFTER UPDATE
AS
BEGIN
    INSERT INTO HistoricoEndereco (pessoaId, logradouro, Numero, cidade, estado, CEP, DataFim)
    SELECT d.pessoaId, d.logradouro, d.numero, d.cidade, d.estado, d.CEP, GETDATE()
    FROM deleted d
    INNER JOIN inserted i ON i.idEndereco = d.idEndereco
    WHERE ISNULL(i.logradouro, '') <> ISNULL(d.logradouro, '')
       OR ISNULL(i.numero, '') <> ISNULL(d.numero, '')
       OR ISNULL(i.cidade, '') <> ISNULL(d.cidade, '')
       OR ISNULL(i.estado, '') <> ISNULL(d.estado, '')
       OR ISNULL(i.CEP, '') <> ISNULL(d.CEP, '')
END
GO

CREATE OR ALTER TRIGGER TR_ValidarDataEntregaOrcamento
ON Orcamento
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE statusOrc IN (4, 5)
          AND data_entrega IS NULL
    )
    BEGIN
        THROW 51000, 'Informe a data de entrega para colocar o orcamento em execucao ou finalizado.', 1;
    END
END
GO

CREATE OR ALTER TRIGGER TR_RecalcularTotalOrcamento
ON Itens
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    ;WITH OrcamentosAfetados AS
    (
        SELECT orcamentoId FROM inserted
        UNION
        SELECT orcamentoId FROM deleted
    )
    UPDATE o
       SET total = COALESCE((
            SELECT SUM(dbo.FN_CalcularSubtotalItem(i.preco, i.qtd, p.valor, i.qtdPeca, i.taxa, i.desconto))
            FROM Itens i
            LEFT JOIN Peca p ON p.idPeca = i.pecaId
            WHERE i.orcamentoId = o.idOrcamento
       ), 0)
    FROM Orcamento o
    INNER JOIN OrcamentosAfetados oa ON oa.orcamentoId = o.idOrcamento
END
GO

CREATE OR ALTER TRIGGER TR_ControlarEstoqueItens
ON Itens
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    ;WITH Devolucoes AS
    (
        SELECT d.pecaId, SUM(ISNULL(d.qtdPeca, 0)) AS quantidade
        FROM deleted d
        INNER JOIN Orcamento o ON o.idOrcamento = d.orcamentoId
        WHERE d.pecaId IS NOT NULL
          AND ISNULL(d.qtdPeca, 0) > 0
          AND o.statusOrc <> 3
        GROUP BY d.pecaId
    )
    UPDATE p
       SET qtdEsto = qtdEsto + d.quantidade
    FROM Peca p
    INNER JOIN Devolucoes d ON d.pecaId = p.idPeca

    IF EXISTS (
        SELECT 1
        FROM (
            SELECT i.pecaId, SUM(ISNULL(i.qtdPeca, 0)) AS quantidade
            FROM inserted i
            INNER JOIN Orcamento o ON o.idOrcamento = i.orcamentoId
            WHERE i.pecaId IS NOT NULL
              AND ISNULL(i.qtdPeca, 0) > 0
              AND o.statusOrc <> 3
            GROUP BY i.pecaId
        ) solicitacao
        INNER JOIN Peca p ON p.idPeca = solicitacao.pecaId
        WHERE p.qtdEsto < solicitacao.quantidade
    )
    BEGIN
        THROW 51001, 'Estoque insuficiente para uma das pecas do orcamento.', 1;
    END

    ;WITH Baixas AS
    (
        SELECT i.pecaId, SUM(ISNULL(i.qtdPeca, 0)) AS quantidade
        FROM inserted i
        INNER JOIN Orcamento o ON o.idOrcamento = i.orcamentoId
        WHERE i.pecaId IS NOT NULL
          AND ISNULL(i.qtdPeca, 0) > 0
          AND o.statusOrc <> 3
        GROUP BY i.pecaId
    )
    UPDATE p
       SET qtdEsto = qtdEsto - b.quantidade
    FROM Peca p
    INNER JOIN Baixas b ON b.pecaId = p.idPeca
END
GO

CREATE OR ALTER TRIGGER TR_ControlarEstoqueStatusOrcamento
ON Orcamento
AFTER UPDATE
AS
BEGIN
    ;WITH Recusados AS
    (
        SELECT i.pecaId, SUM(ISNULL(i.qtdPeca, 0)) AS quantidade
        FROM inserted n
        INNER JOIN deleted a ON a.idOrcamento = n.idOrcamento
        INNER JOIN Itens i ON i.orcamentoId = n.idOrcamento
        WHERE n.statusOrc = 3
          AND a.statusOrc <> 3
          AND i.pecaId IS NOT NULL
          AND ISNULL(i.qtdPeca, 0) > 0
        GROUP BY i.pecaId
    )
    UPDATE p
       SET qtdEsto = qtdEsto + r.quantidade
    FROM Peca p
    INNER JOIN Recusados r ON r.pecaId = p.idPeca

    IF EXISTS (
        SELECT 1
        FROM (
            SELECT i.pecaId, SUM(ISNULL(i.qtdPeca, 0)) AS quantidade
            FROM inserted n
            INNER JOIN deleted a ON a.idOrcamento = n.idOrcamento
            INNER JOIN Itens i ON i.orcamentoId = n.idOrcamento
            WHERE n.statusOrc <> 3
              AND a.statusOrc = 3
              AND i.pecaId IS NOT NULL
              AND ISNULL(i.qtdPeca, 0) > 0
            GROUP BY i.pecaId
        ) solicitacao
        INNER JOIN Peca p ON p.idPeca = solicitacao.pecaId
        WHERE p.qtdEsto < solicitacao.quantidade
    )
    BEGIN
        THROW 51002, 'Estoque insuficiente para reativar o orcamento recusado.', 1;
    END

    ;WITH Reativados AS
    (
        SELECT i.pecaId, SUM(ISNULL(i.qtdPeca, 0)) AS quantidade
        FROM inserted n
        INNER JOIN deleted a ON a.idOrcamento = n.idOrcamento
        INNER JOIN Itens i ON i.orcamentoId = n.idOrcamento
        WHERE n.statusOrc <> 3
          AND a.statusOrc = 3
          AND i.pecaId IS NOT NULL
          AND ISNULL(i.qtdPeca, 0) > 0
        GROUP BY i.pecaId
    )
    UPDATE p
       SET qtdEsto = qtdEsto - r.quantidade
    FROM Peca p
    INNER JOIN Reativados r ON r.pecaId = p.idPeca
END
GO

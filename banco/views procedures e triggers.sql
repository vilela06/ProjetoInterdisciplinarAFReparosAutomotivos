USE AFReparosAutomotivos
GO

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
    c.chaveCli,
    COALESCE(c.telefone, pc.celular) AS telefoneContato,
    COALESCE(e.logradouro + ', ' + e.numero + ', ' + e.cidade + ' - ' + e.estado + ', ' + e.CEP, '') AS endereco,
    v.placa,
    v.marca,
    v.modelo,
    v.cor,
    v.ano
FROM Orcamento o
INNER JOIN Cliente c ON c.idCliente = o.clienteId
INNER JOIN Pessoa pc ON pc.idPessoa = c.idCliente
INNER JOIN Funcionario f ON f.idFuncionario = o.funcionarioId
INNER JOIN Pessoa pf ON pf.idPessoa = f.idFuncionario
INNER JOIN Veiculo v ON v.idVeiculo = o.veiculoId
LEFT JOIN Endereco e ON e.pessoaId = pc.idPessoa
GO

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
    i.dataEntrega,
    s.descricao AS descricaoServico,
    o.veiculoId,
    p.nome AS nomePeca
FROM Itens i
INNER JOIN Servico s ON s.idServico = i.servicoId
INNER JOIN Orcamento o ON o.idOrcamento = i.orcamentoId
LEFT JOIN Peca p ON p.idPeca = i.pecaId
GO

CREATE OR ALTER PROCEDURE SP_BuscarPecas
    @termo VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT idPeca, nome, valor, qtdEsto
    FROM Peca
    WHERE NULLIF(LTRIM(RTRIM(@termo)), '') IS NULL
       OR nome LIKE '%' + @termo + '%'
       OR CONVERT(VARCHAR(20), idPeca) LIKE '%' + @termo + '%'
    ORDER BY nome;
END
GO

CREATE OR ALTER PROCEDURE SP_ListarPecasDisponiveis
AS
BEGIN
    SET NOCOUNT ON;

    SELECT idPeca, nome, valor, qtdEsto
    FROM Peca
    WHERE qtdEsto > 0
    ORDER BY nome;
END
GO

CREATE OR ALTER PROCEDURE SP_AtualizarStatusOrcamentoCliente
    @id INT,
    @chaveAcesso VARCHAR(19),
    @status INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE o
       SET statusOrc = @status
    FROM Orcamento o
    INNER JOIN Cliente c ON c.idCliente = o.clienteId
    WHERE o.idOrcamento = @id
      AND c.chaveCli = @chaveAcesso
      AND o.statusOrc = 1
      AND @status IN (2, 3);

    SELECT @@ROWCOUNT AS LinhasAfetadas;
END
GO

CREATE OR ALTER TRIGGER TR_SalvarHistoricoEndereco
ON Endereco
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO HistoricoEndereco (pessoaId, logradouro, Numero, cidade, estado, CEP, DataFim)
    SELECT
        d.pessoaId,
        d.logradouro,
        d.numero,
        d.cidade,
        d.estado,
        d.CEP,
        GETDATE()
    FROM deleted d
    INNER JOIN inserted i ON i.idEndereco = d.idEndereco
    WHERE ISNULL(i.logradouro, '') <> ISNULL(d.logradouro, '')
       OR ISNULL(i.numero, '') <> ISNULL(d.numero, '')
       OR ISNULL(i.cidade, '') <> ISNULL(d.cidade, '')
       OR ISNULL(i.estado, '') <> ISNULL(d.estado, '')
       OR ISNULL(i.CEP, '') <> ISNULL(d.CEP, '');
END
GO

CREATE OR ALTER TRIGGER TR_ValidarDataEntregaOrcamento
ON Orcamento
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

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
    SET NOCOUNT ON;

    ;WITH OrcamentosAfetados AS
    (
        SELECT orcamentoId FROM inserted
        UNION
        SELECT orcamentoId FROM deleted
    )
    UPDATE o
       SET total = COALESCE((
            SELECT SUM(i.preco)
            FROM Itens i
            WHERE i.orcamentoId = o.idOrcamento
       ), 0)
    FROM Orcamento o
    INNER JOIN OrcamentosAfetados oa ON oa.orcamentoId = o.idOrcamento;
END
GO


select * from Cliente
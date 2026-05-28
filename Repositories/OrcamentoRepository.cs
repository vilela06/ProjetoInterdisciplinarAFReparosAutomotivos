using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AfReparosAutomotivos.Repositories
{
    public class OrcamentoRepository : SqlRepositoryBase, IOrcamentoRepository
    {
        private readonly IItemRepository _itemRepository;

        public OrcamentoRepository(IConfiguration configuration, IItemRepository itemRepository) : base(configuration)
        {
            _itemRepository = itemRepository;
        }

        public async Task<int> Add(Orcamentos orcamento)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                "INSERT INTO Orcamento (clienteId, funcionarioId, veiculoId, data_criacao, data_entrega, statusOrc, total, forma_pgto, parcelas) " +
                "OUTPUT INSERTED.idOrcamento VALUES (@clienteId, @funcionarioId, @veiculoId, @data_criacao, @data_entrega, @statusOrc, @total, @forma_pgto, @parcelas)",
                connection);
            command.Parameters.AddWithValue("@clienteId", orcamento.clienteId);
            command.Parameters.AddWithValue("@funcionarioId", orcamento.funcionarioId);
            command.Parameters.AddWithValue("@veiculoId", orcamento.veiculoId);
            command.Parameters.AddWithValue("@data_criacao", orcamento.dataCriacao);
            command.Parameters.AddWithValue("@data_entrega", (object?)orcamento.dataEntrega ?? DBNull.Value);
            command.Parameters.AddWithValue("@statusOrc", orcamento.statusOrc == 0 ? 1 : orcamento.statusOrc);
            command.Parameters.AddWithValue("@total", orcamento.total);
            command.Parameters.AddWithValue("@forma_pgto", string.IsNullOrWhiteSpace(orcamento.formaPagamento) ? DBNull.Value : orcamento.formaPagamento);
            command.Parameters.AddWithValue("@parcelas", orcamento.parcelas == 0 ? DBNull.Value : orcamento.parcelas);
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        public async Task Delete(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            try
            {
                await using (var itensCommand = new SqlCommand("DELETE FROM Itens WHERE orcamentoId = @id", connection, transaction))
                {
                    itensCommand.Parameters.AddWithValue("@id", id);
                    await itensCommand.ExecuteNonQueryAsync();
                }

                await using (var command = new SqlCommand("DELETE FROM Orcamento WHERE idOrcamento = @id", connection, transaction))
                {
                    command.Parameters.AddWithValue("@id", id);
                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<OrcamentosViewModel>> GetByChaveCliente(string chaveAcesso)
        {
            var orcamentos = new List<OrcamentosViewModel>();
            if (string.IsNullOrWhiteSpace(chaveAcesso))
            {
                return orcamentos;
            }

            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                BaseSelect() + " WHERE chaveCli = @chaveAcesso ORDER BY data_criacao DESC",
                connection);
            command.Parameters.AddWithValue("@chaveAcesso", chaveAcesso.Trim());

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                orcamentos.Add(Map(reader));
            }

            return orcamentos;
        }

        public async Task<IEnumerable<OrcamentosViewModel>> GetFilter(OrcamentosFilterViewModel filtros)
        {
            var query = BaseSelect() + " WHERE 1 = 1";
            var parameters = new List<SqlParameter>();

            if (filtros.statusId.HasValue)
            {
                query += " AND statusOrc = @status";
                parameters.Add(new SqlParameter("@status", filtros.statusId.Value));
            }

            if (!string.IsNullOrWhiteSpace(filtros.cpf))
            {
                query += " AND documento LIKE @cpf";
                parameters.Add(new SqlParameter("@cpf", "%" + filtros.cpf + "%"));
            }

            if (!string.IsNullOrWhiteSpace(filtros.nome))
            {
                query += " AND nomeCliente LIKE @nome";
                parameters.Add(new SqlParameter("@nome", "%" + filtros.nome + "%"));
            }

            if (!string.IsNullOrWhiteSpace(filtros.busca))
            {
                query += " AND (" +
                    "nomeCliente LIKE @busca OR " +
                    "CONVERT(VARCHAR(20), idOrcamento) LIKE @busca OR " +
                    "CONVERT(VARCHAR(10), data_criacao, 23) LIKE @busca OR " +
                    "CONVERT(VARCHAR(10), data_criacao, 103) LIKE @busca OR " +
                    "CONVERT(VARCHAR(10), data_criacao, 105) LIKE @busca" +
                    ")";
                parameters.Add(new SqlParameter("@busca", "%" + filtros.busca.Trim() + "%"));
            }

            if (filtros.dataCriacao.HasValue)
            {
                query += " AND CAST(data_criacao AS date) = @dataCriacao";
                parameters.Add(new SqlParameter("@dataCriacao", filtros.dataCriacao.Value.Date));
            }

            if (filtros.dataEntrega.HasValue)
            {
                query += " AND CAST(data_entrega AS date) = @dataEntrega";
                parameters.Add(new SqlParameter("@dataEntrega", filtros.dataEntrega.Value.Date));
            }

            if (!string.IsNullOrWhiteSpace(filtros.formaPagamento))
            {
                query += " AND forma_pgto LIKE @formaPagamento";
                parameters.Add(new SqlParameter("@formaPagamento", "%" + filtros.formaPagamento + "%"));
            }

            if (filtros.parcelas.HasValue)
            {
                query += " AND parcelas = @parcelas";
                parameters.Add(new SqlParameter("@parcelas", filtros.parcelas.Value));
            }

            if (filtros.preco.HasValue)
            {
                query += " AND total = @preco";
                parameters.Add(new SqlParameter("@preco", filtros.preco.Value));
            }

            query += " ORDER BY data_criacao DESC";

            var orcamentos = new List<OrcamentosViewModel>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddRange(parameters.ToArray());
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                orcamentos.Add(Map(reader));
            }

            return orcamentos;
        }

        public async Task<OrcamentosViewModel?> GetId(int id)
        {
            OrcamentosViewModel? orcamento = null;
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(BaseSelect() + " WHERE idOrcamento = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                orcamento = Map(reader);
            }

            if (orcamento == null)
            {
                return null;
            }

            var itens = (await _itemRepository.GetByOrcamento(id)).ToList();
            orcamento.ServicosAssociados = itens.Select(item => new ItemViewModel
            {
                idItem = item.idItem,
                idServico = item.servicoId,
                funcionarioId = item.funcionarioId,
                pecaId = item.pecaId,
                qtd = item.qtd,
                qtdPeca = item.qtdPeca,
                data_entrega = item.dataEntrega,
                preco = item.preco,
                descricao = item.descricao,
                observacao = item.descricao,
                taxa = item.taxa ?? 0m,
                desconto = item.desconto ?? 0m
            }).ToList();

            return orcamento;
        }

        public async Task<bool> UpdateStatusByChaveCliente(int id, string chaveAcesso, int status)
        {
            if (string.IsNullOrWhiteSpace(chaveAcesso) || status is < 1 or > 5)
            {
                return false;
            }

            await using var connection = CreateConnection();
            await connection.OpenAsync();

            try
            {
                await using var procedureCommand = new SqlCommand("SP_AtualizarStatusOrcamentoCliente", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                procedureCommand.Parameters.AddWithValue("@id", id);
                procedureCommand.Parameters.AddWithValue("@chaveAcesso", chaveAcesso.Trim());
                procedureCommand.Parameters.AddWithValue("@status", status);

                return Convert.ToInt32(await procedureCommand.ExecuteScalarAsync()) > 0;
            }
            catch (SqlException)
            {
                await using var command = new SqlCommand(
                    "UPDATE o SET statusOrc = @status " +
                    "FROM Orcamento o INNER JOIN Cliente c ON c.idCliente = o.clienteId " +
                    "WHERE o.idOrcamento = @id AND c.chaveCli = @chaveAcesso AND o.statusOrc = 1 AND @status IN (2, 3)",
                    connection);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@chaveAcesso", chaveAcesso.Trim());
                command.Parameters.AddWithValue("@status", status);

                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task Update(OrcamentosViewModel orcamento)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                "UPDATE Orcamento SET data_entrega = @data_entrega, statusOrc = @statusOrc, total = @total, forma_pgto = @forma_pgto, parcelas = @parcelas WHERE idOrcamento = @id",
                connection);
            command.Parameters.AddWithValue("@id", orcamento.idOrcamento);
            command.Parameters.AddWithValue("@data_entrega", (object?)orcamento.dataEntrega ?? DBNull.Value);
            command.Parameters.AddWithValue("@statusOrc", orcamento.status);
            command.Parameters.AddWithValue("@total", orcamento.total);
            command.Parameters.AddWithValue("@forma_pgto", string.IsNullOrWhiteSpace(orcamento.formaPagamento) ? DBNull.Value : orcamento.formaPagamento);
            command.Parameters.AddWithValue("@parcelas", orcamento.parcelas == 0 ? DBNull.Value : orcamento.parcelas);
            await command.ExecuteNonQueryAsync();
        }

        private static string BaseSelect() =>
            "SELECT * FROM (" +
            "SELECT o.idOrcamento, o.clienteId, o.funcionarioId, o.veiculoId, o.data_criacao, o.data_entrega, " +
            "o.statusOrc, o.total, o.forma_pgto, o.parcelas, pc.nome AS nomeCliente, pf.nome AS nomeFuncionario, " +
            "pc.documento, COALESCE(c.telefone, pc.celular) AS telefoneContato, COALESCE(e.logradouro + ', ' + e.numero + ', ' + e.cidade + ' - ' + e.estado + ', ' + e.CEP, '') AS endereco, " +
            "v.placa, v.marca, v.modelo, v.cor, v.ano, c.chaveCli " +
            "FROM Orcamento o " +
            "INNER JOIN Cliente c ON c.idCliente = o.clienteId " +
            "INNER JOIN Pessoa pc ON pc.idPessoa = c.idCliente " +
            "INNER JOIN Funcionario f ON f.idFuncionario = o.funcionarioId " +
            "INNER JOIN Pessoa pf ON pf.idPessoa = f.idFuncionario " +
            "INNER JOIN Veiculo v ON v.idVeiculo = o.veiculoId " +
            "LEFT JOIN Endereco e ON e.pessoaId = pc.idPessoa" +
            ") dados";

        private static OrcamentosViewModel Map(SqlDataReader reader) => new()
        {
            idOrcamento = reader.GetInt32(0),
            idCliente = reader.GetInt32(1),
            idFuncionario = reader.GetInt32(2),
            idVeiculo = reader.GetInt32(3),
            dataCriacao = reader.GetDateTime(4),
            dataEntrega = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
            status = reader.GetInt32(6),
            total = reader.GetDecimal(7),
            formaPagamento = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
            parcelas = reader.IsDBNull(9) ? 0 : reader.GetInt32(9),
            nome = reader.GetString(10),
            nomeFunc = reader.GetString(11),
            DocumentoCli = reader.GetString(12),
            TelefoneCli = reader.GetString(13),
            EnderecoCli = reader.GetString(14),
            Placa = reader.GetString(15),
            Marca = reader.GetString(16),
            Modelo = reader.GetString(17),
            Cor = reader.GetString(18),
            Ano = reader.GetInt32(19)
        };
    }
}

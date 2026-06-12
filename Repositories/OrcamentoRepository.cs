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
            await using var command = new SqlCommand("SP_AdicionarOrcamento", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@clienteId", SqlDbType.Int).Value = orcamento.clienteId;
            command.Parameters.Add("@funcionarioId", SqlDbType.Int).Value = orcamento.funcionarioId;
            command.Parameters.Add("@veiculoId", SqlDbType.Int).Value = orcamento.veiculoId;
            command.Parameters.Add("@data_criacao", SqlDbType.DateTime).Value = orcamento.dataCriacao;
            command.Parameters.Add("@data_entrega", SqlDbType.DateTime).Value = (object?)orcamento.dataEntrega ?? DBNull.Value;
            command.Parameters.Add("@statusOrc", SqlDbType.Int).Value = orcamento.statusOrc == 0 ? 1 : orcamento.statusOrc;
            var total = command.Parameters.Add("@total", SqlDbType.Decimal);
            total.Precision = 10;
            total.Scale = 2;
            total.Value = orcamento.total;
            command.Parameters.Add("@forma_pgto", SqlDbType.VarChar, 20).Value =
                string.IsNullOrWhiteSpace(orcamento.formaPagamento) ? DBNull.Value : orcamento.formaPagamento;
            command.Parameters.Add("@parcelas", SqlDbType.Int).Value = orcamento.parcelas == 0 ? DBNull.Value : orcamento.parcelas;
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        public async Task Delete(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            try
            {
                await using var command = new SqlCommand("SP_ExcluirOrcamento", connection, transaction)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                await command.ExecuteNonQueryAsync();
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
            await using var command = new SqlCommand("SP_ListarOrcamentosPorChave", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@chaveAcesso", SqlDbType.VarChar, 19).Value = chaveAcesso.Trim();

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                orcamentos.Add(Map(reader));
            }

            return orcamentos;
        }

        public async Task<IEnumerable<OrcamentosViewModel>> GetFilter(OrcamentosFilterViewModel filtros)
        {
            var orcamentos = new List<OrcamentosViewModel>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SP_FiltrarOrcamentos", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@statusId", SqlDbType.Int).Value = (object?)filtros.statusId ?? DBNull.Value;
            command.Parameters.Add("@cpf", SqlDbType.VarChar, 30).Value =
                string.IsNullOrWhiteSpace(filtros.cpf) ? DBNull.Value : filtros.cpf.Trim();
            command.Parameters.Add("@nome", SqlDbType.VarChar, 80).Value =
                string.IsNullOrWhiteSpace(filtros.nome) ? DBNull.Value : filtros.nome.Trim();
            command.Parameters.Add("@busca", SqlDbType.VarChar, 80).Value =
                string.IsNullOrWhiteSpace(filtros.busca) ? DBNull.Value : filtros.busca.Trim();
            command.Parameters.Add("@dataCriacao", SqlDbType.Date).Value = (object?)filtros.dataCriacao?.Date ?? DBNull.Value;
            command.Parameters.Add("@dataEntrega", SqlDbType.Date).Value = (object?)filtros.dataEntrega?.Date ?? DBNull.Value;
            command.Parameters.Add("@formaPagamento", SqlDbType.VarChar, 20).Value =
                string.IsNullOrWhiteSpace(filtros.formaPagamento) ? DBNull.Value : filtros.formaPagamento.Trim();
            command.Parameters.Add("@parcelas", SqlDbType.Int).Value = (object?)filtros.parcelas ?? DBNull.Value;
            var preco = command.Parameters.Add("@preco", SqlDbType.Decimal);
            preco.Precision = 10;
            preco.Scale = 2;
            preco.Value = (object?)filtros.preco ?? DBNull.Value;

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
            await using var command = new SqlCommand("SP_ObterOrcamentoPorId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@id", SqlDbType.Int).Value = id;
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

        public async Task<bool> UpdateStatusByChaveCliente(int id, string chaveAcesso, int status, string? formaPagamento = null, int? parcelas = null)
        {
            if (string.IsNullOrWhiteSpace(chaveAcesso) || status is < 1 or > 5)
            {
                return false;
            }

            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SP_AtualizarStatusOrcamentoCliente", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@id", SqlDbType.Int).Value = id;
            command.Parameters.Add("@chaveAcesso", SqlDbType.VarChar, 19).Value = chaveAcesso.Trim();
            command.Parameters.Add("@status", SqlDbType.Int).Value = status;
            command.Parameters.Add("@forma_pgto", SqlDbType.VarChar, 20).Value =
                string.IsNullOrWhiteSpace(formaPagamento) ? DBNull.Value : formaPagamento.Trim();
            command.Parameters.Add("@parcelas", SqlDbType.Int).Value = (object?)parcelas ?? DBNull.Value;

            return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
        }

        public async Task Update(OrcamentosViewModel orcamento)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SP_AtualizarOrcamento", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@id", SqlDbType.Int).Value = orcamento.idOrcamento;
            command.Parameters.Add("@data_entrega", SqlDbType.DateTime).Value = (object?)orcamento.dataEntrega ?? DBNull.Value;
            command.Parameters.Add("@statusOrc", SqlDbType.Int).Value = orcamento.status;
            var total = command.Parameters.Add("@total", SqlDbType.Decimal);
            total.Precision = 10;
            total.Scale = 2;
            total.Value = orcamento.total;
            command.Parameters.Add("@forma_pgto", SqlDbType.VarChar, 20).Value =
                string.IsNullOrWhiteSpace(orcamento.formaPagamento) ? DBNull.Value : orcamento.formaPagamento;
            command.Parameters.Add("@parcelas", SqlDbType.Int).Value = orcamento.parcelas == 0 ? DBNull.Value : orcamento.parcelas;
            await command.ExecuteNonQueryAsync();
        }

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

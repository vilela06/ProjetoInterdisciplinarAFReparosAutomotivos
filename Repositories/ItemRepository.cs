using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AfReparosAutomotivos.Repositories
{
    public class ItemRepository : SqlRepositoryBase, IItemRepository
    {
        public ItemRepository(IConfiguration configuration) : base(configuration) { }

        public async Task Add(IEnumerable<Item> itens)
        {
            var lista = itens.ToList();
            if (!lista.Any())
            {
                return;
            }

            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            try
            {
                await AddItens(connection, transaction, lista);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<Item>> GetByOrcamento(int orcamentoId)
        {
            var itens = new List<Item>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SP_ListarItensOrcamento", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@orcamentoId", SqlDbType.Int).Value = orcamentoId;
            await using var reader = await command.ExecuteReaderAsync();

            var index = 1;
            while (await reader.ReadAsync())
            {
                itens.Add(new Item
                {
                    idItem = index++,
                    orcamentoId = reader.GetInt32(0),
                    servicoId = reader.GetInt32(1),
                    funcionarioId = reader.GetInt32(2),
                    pecaId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    qtd = reader.GetInt32(4),
                    preco = reader.GetDecimal(5),
                    desconto = reader.IsDBNull(6) ? 0m : reader.GetDecimal(6),
                    taxa = reader.IsDBNull(7) ? 0m : reader.GetDecimal(7),
                    qtdPeca = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                    dataEntrega = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                    descricao = reader.GetString(10),
                    idVeiculo = reader.GetInt32(11)
                });
            }

            return itens;
        }

        public async Task Update(IEnumerable<Item> itens)
        {
            var lista = itens.ToList();
            var orcamentoId = lista.FirstOrDefault()?.orcamentoId;
            if (orcamentoId is null)
            {
                return;
            }

            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            try
            {
                await DeleteByOrcamento(connection, transaction, orcamentoId.Value);
                await AddItens(connection, transaction, lista);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static async Task DeleteByOrcamento(SqlConnection connection, SqlTransaction transaction, int orcamentoId)
        {
            await using var command = new SqlCommand("SP_ExcluirItensOrcamento", connection, transaction)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@orcamentoId", SqlDbType.Int).Value = orcamentoId;
            await command.ExecuteNonQueryAsync();
        }

        private static async Task AddItens(SqlConnection connection, SqlTransaction transaction, IEnumerable<Item> itens)
        {
            foreach (var item in itens)
            {
                await using var command = new SqlCommand("SP_AdicionarItemOrcamento", connection, transaction)
                {
                    CommandType = CommandType.StoredProcedure
                };
                FillInsertParameters(command, item);
                await command.ExecuteNonQueryAsync();
            }
        }

        private static void FillInsertParameters(SqlCommand command, Item item)
        {
            command.Parameters.Add("@orcamentoId", SqlDbType.Int).Value = item.orcamentoId;
            command.Parameters.Add("@servicoId", SqlDbType.Int).Value = item.servicoId;
            command.Parameters.Add("@funcionarioID", SqlDbType.Int).Value = item.funcionarioId;
            command.Parameters.Add("@pecaId", SqlDbType.Int).Value = (object?)item.pecaId ?? DBNull.Value;
            command.Parameters.Add("@qtd", SqlDbType.Int).Value = item.qtd <= 0 ? 1 : item.qtd;
            command.Parameters.Add("@qtdPeca", SqlDbType.Int).Value = item.pecaId.HasValue ? Math.Max(1, item.qtdPeca) : DBNull.Value;
            command.Parameters.Add("@preco", SqlDbType.Money).Value = item.preco;
            var desconto = command.Parameters.Add("@desconto", SqlDbType.Decimal);
            desconto.Precision = 10;
            desconto.Scale = 2;
            desconto.Value = (object?)item.desconto ?? DBNull.Value;
            var taxa = command.Parameters.Add("@taxa", SqlDbType.Decimal);
            taxa.Precision = 10;
            taxa.Scale = 2;
            taxa.Value = (object?)item.taxa ?? DBNull.Value;
            command.Parameters.Add("@dataEntrega", SqlDbType.DateTime).Value = (object?)item.dataEntrega ?? DBNull.Value;
        }
    }
}

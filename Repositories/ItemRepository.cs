using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;

namespace AfReparosAutomotivos.Repositories
{
    public class ItemRepository : SqlRepositoryBase, IItemRepository
    {
        public ItemRepository(IConfiguration configuration) : base(configuration) { }

        public async Task Add(IEnumerable<Item> itens)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            foreach (var item in itens)
            {
                await using var command = InsertCommand(connection);
                FillInsertParameters(command, item);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteByOrcamento(int orcamentoId)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("DELETE FROM Itens WHERE orcamentoId = @orcamentoId", connection);
            command.Parameters.AddWithValue("@orcamentoId", orcamentoId);
            await command.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<Item>> GetByOrcamento(int orcamentoId)
        {
            var itens = new List<Item>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                "SELECT i.orcamentoId, i.servicoId, i.funcionarioID, i.pecaId, i.preco, i.desconto, i.dataEntrega, " +
                "s.descricao, o.veiculoId " +
                "FROM Itens i INNER JOIN Servico s ON s.idServico = i.servicoId " +
                "INNER JOIN Orcamento o ON o.idOrcamento = i.orcamentoId " +
                "WHERE i.orcamentoId = @orcamentoId",
                connection);
            command.Parameters.AddWithValue("@orcamentoId", orcamentoId);
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
                    preco = reader.GetDecimal(4),
                    desconto = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5),
                    dataEntrega = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                    descricao = reader.GetString(7),
                    idVeiculo = reader.GetInt32(8),
                    qtd = 1,
                    taxa = 0m
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

            await DeleteByOrcamento(orcamentoId.Value);
            await Add(lista);
        }

        private static SqlCommand InsertCommand(SqlConnection connection) => new(
            "INSERT INTO Itens (orcamentoId, servicoId, funcionarioID, pecaId, preco, desconto, dataEntrega) " +
            "VALUES (@orcamentoId, @servicoId, @funcionarioID, @pecaId, @preco, @desconto, @dataEntrega)",
            connection);

        private static void FillInsertParameters(SqlCommand command, Item item)
        {
            command.Parameters.AddWithValue("@orcamentoId", item.orcamentoId);
            command.Parameters.AddWithValue("@servicoId", item.servicoId);
            command.Parameters.AddWithValue("@funcionarioID", item.funcionarioId);
            command.Parameters.AddWithValue("@pecaId", (object?)item.pecaId ?? DBNull.Value);
            command.Parameters.AddWithValue("@preco", item.preco);
            command.Parameters.AddWithValue("@desconto", (object?)item.desconto ?? DBNull.Value);
            command.Parameters.AddWithValue("@dataEntrega", (object?)item.dataEntrega ?? DBNull.Value);
        }
    }
}

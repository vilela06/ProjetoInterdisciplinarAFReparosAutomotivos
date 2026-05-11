using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;

namespace AfReparosAutomotivos.Repositories
{
    public class ServicoRepository : SqlRepositoryBase, IServicoRepository
    {
        public ServicoRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<int> Add(Servicos servico)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                "INSERT INTO Servico (descricao, valorBase) OUTPUT INSERTED.idServico VALUES (@descricao, @valorBase)",
                connection);
            command.Parameters.AddWithValue("@descricao", servico.Descricao);
            command.Parameters.AddWithValue("@valorBase", servico.PrecoBase);
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        public async Task Delete(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("DELETE FROM Servico WHERE idServico = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            await command.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<Servicos>> Get()
        {
            var servicos = new List<Servicos>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SELECT idServico, descricao, valorBase FROM Servico ORDER BY descricao", connection);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                servicos.Add(Map(reader));
            }

            return servicos;
        }

        public async Task<Servicos?> GetId(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SELECT idServico, descricao, valorBase FROM Servico WHERE idServico = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public Task<Servicos?> Update(int id) => GetId(id);

        public async Task Update(Servicos servico)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                "UPDATE Servico SET descricao = @descricao, valorBase = @valorBase WHERE idServico = @id",
                connection);
            command.Parameters.AddWithValue("@id", servico.IdServico);
            command.Parameters.AddWithValue("@descricao", servico.Descricao);
            command.Parameters.AddWithValue("@valorBase", servico.PrecoBase);
            await command.ExecuteNonQueryAsync();
        }

        public async Task<decimal> GetPrecoBaseByIdAsync(int id)
        {
            var servico = await GetId(id);
            return servico?.PrecoBase ?? 0m;
        }

        private static Servicos Map(SqlDataReader reader) => new()
        {
            IdServico = reader.GetInt32(0),
            Descricao = reader.GetString(1),
            PrecoBase = reader.GetDecimal(2)
        };
    }
}

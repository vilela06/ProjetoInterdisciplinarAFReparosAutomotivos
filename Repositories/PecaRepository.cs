using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;

namespace AfReparosAutomotivos.Repositories
{
    public class PecaRepository : SqlRepositoryBase, IPecaRepository
    {
        public PecaRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<int> Add(Pecas peca)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                "INSERT INTO Peca (nome, valor, qtdEsto) OUTPUT INSERTED.idPeca VALUES (@nome, @valor, @qtdEsto)",
                connection);
            command.Parameters.AddWithValue("@nome", peca.nome);
            command.Parameters.AddWithValue("@valor", peca.valor);
            command.Parameters.AddWithValue("@qtdEsto", peca.qtdEsto);
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        public async Task<IEnumerable<Pecas>> GetDisponiveis()
        {
            return await Get("WHERE qtdEsto > 0");
        }

        public async Task<IEnumerable<Pecas>> GetAll()
        {
            return await Get(string.Empty);
        }

        private async Task<IEnumerable<Pecas>> Get(string where)
        {
            var pecas = new List<Pecas>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                $"SELECT idPeca, nome, valor, qtdEsto FROM Peca {where} ORDER BY nome",
                connection);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                pecas.Add(Map(reader));
            }

            return pecas;
        }

        public async Task<Pecas?> GetId(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                "SELECT idPeca, nome, valor, qtdEsto FROM Peca WHERE idPeca = @id",
                connection);
            command.Parameters.AddWithValue("@id", id);
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task Update(Pecas peca)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                "UPDATE Peca SET nome = @nome, valor = @valor, qtdEsto = @qtdEsto WHERE idPeca = @id",
                connection);
            command.Parameters.AddWithValue("@id", peca.idPeca);
            command.Parameters.AddWithValue("@nome", peca.nome);
            command.Parameters.AddWithValue("@valor", peca.valor);
            command.Parameters.AddWithValue("@qtdEsto", peca.qtdEsto);
            await command.ExecuteNonQueryAsync();
        }

        private static Pecas Map(SqlDataReader reader) => new()
        {
            idPeca = reader.GetInt32(0),
            nome = reader.GetString(1),
            valor = reader.GetDecimal(2),
            qtdEsto = reader.GetInt32(3)
        };
    }
}

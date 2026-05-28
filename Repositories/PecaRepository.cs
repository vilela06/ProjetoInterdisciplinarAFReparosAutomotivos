using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;
using System.Data;

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
            try
            {
                return await ExecutePecaProcedure("SP_ListarPecasDisponiveis");
            }
            catch (SqlException)
            {
                return await Get("WHERE qtdEsto > 0");
            }
        }

        public async Task<IEnumerable<Pecas>> GetAll()
        {
            return await Get(string.Empty);
        }

        public async Task<IEnumerable<Pecas>> Search(string termo)
        {
            try
            {
                return await ExecutePecaProcedure("SP_BuscarPecas", termo);
            }
            catch (SqlException)
            {
                if (string.IsNullOrWhiteSpace(termo))
                {
                    return await GetAll();
                }

                var pecas = new List<Pecas>();
                await using var connection = CreateConnection();
                await connection.OpenAsync();
                await using var command = new SqlCommand(
                    "SELECT idPeca, nome, valor, qtdEsto FROM Peca " +
                    "WHERE nome LIKE @termo OR CONVERT(VARCHAR(20), idPeca) LIKE @termo " +
                    "ORDER BY nome",
                    connection);
                command.Parameters.AddWithValue("@termo", $"%{termo.Trim()}%");
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    pecas.Add(Map(reader));
                }

                return pecas;
            }
        }

        private async Task<IEnumerable<Pecas>> ExecutePecaProcedure(string procedure, string? termo = null)
        {
            var pecas = new List<Pecas>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(procedure, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (procedure == "SP_BuscarPecas")
            {
                command.Parameters.Add("@termo", SqlDbType.VarChar, 50).Value =
                    string.IsNullOrWhiteSpace(termo) ? DBNull.Value : termo.Trim();
            }

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                pecas.Add(Map(reader));
            }

            return pecas;
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

        public async Task<bool> BaixarEstoque(int id, int quantidade)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                "UPDATE Peca SET qtdEsto = qtdEsto - @quantidade WHERE idPeca = @id AND qtdEsto >= @quantidade",
                connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@quantidade", quantidade);
            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task ReporEstoque(int id, int quantidade)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                "UPDATE Peca SET qtdEsto = qtdEsto + @quantidade WHERE idPeca = @id",
                connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@quantidade", quantidade);
            await command.ExecuteNonQueryAsync();
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

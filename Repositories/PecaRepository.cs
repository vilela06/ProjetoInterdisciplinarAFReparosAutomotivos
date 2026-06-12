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
            await using var command = new SqlCommand("SP_AdicionarPeca", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@funcionarioId", SqlDbType.Int).Value = peca.funcionarioId <= 0 ? 1 : peca.funcionarioId;
            AddPecaParameters(command, peca);
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        public Task<IEnumerable<Pecas>> GetDisponiveis() => ExecutePecaProcedure("SP_ListarPecasDisponiveis");

        public Task<IEnumerable<Pecas>> GetAll() => ExecutePecaProcedure("SP_ListarPecas");

        public Task<IEnumerable<Pecas>> Search(string termo) => ExecutePecaProcedure("SP_BuscarPecas", termo);

        public async Task<Pecas?> GetId(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SP_ObterPecaPorId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@id", SqlDbType.Int).Value = id;
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task Update(Pecas peca)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SP_AtualizarPeca", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@id", SqlDbType.Int).Value = peca.idPeca;
            command.Parameters.Add("@funcionarioId", SqlDbType.Int).Value = peca.funcionarioId <= 0 ? DBNull.Value : peca.funcionarioId;
            AddPecaParameters(command, peca);
            await command.ExecuteNonQueryAsync();
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

        private static Pecas Map(SqlDataReader reader) => new()
        {
            idPeca = reader.GetInt32(0),
            nome = reader.GetString(1),
            valor = reader.GetDecimal(2),
            qtdEsto = reader.GetInt32(3),
            funcionarioId = reader.FieldCount > 4 && !reader.IsDBNull(4) ? reader.GetInt32(4) : 0,
            nomeFuncionario = reader.FieldCount > 5 && !reader.IsDBNull(5) ? reader.GetString(5) : string.Empty
        };

        private static void AddPecaParameters(SqlCommand command, Pecas peca)
        {
            command.Parameters.Add("@nome", SqlDbType.VarChar, 20).Value = peca.nome;
            command.Parameters.Add("@valor", SqlDbType.Money).Value = peca.valor;
            command.Parameters.Add("@qtdEsto", SqlDbType.Int).Value = peca.qtdEsto;
        }
    }
}

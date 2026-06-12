using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AfReparosAutomotivos.Repositories
{
    public class ServicoRepository : SqlRepositoryBase, IServicoRepository
    {
        public ServicoRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<int> Add(Servicos servico)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SP_AdicionarServico", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@descricao", SqlDbType.VarChar, 50).Value = servico.Descricao;
            command.Parameters.Add("@valorBase", SqlDbType.Money).Value = servico.PrecoBase;
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        public Task<IEnumerable<Servicos>> Get() => GetFiltered(null);

        public Task<IEnumerable<Servicos>> Search(string termo) => GetFiltered(termo);

        public async Task<Servicos?> GetId(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SP_ObterServicoPorId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@id", SqlDbType.Int).Value = id;
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task<decimal> GetPrecoBaseByIdAsync(int id)
        {
            var servico = await GetId(id);
            return servico?.PrecoBase ?? 0m;
        }

        public async Task DeleteCreated(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SP_ExcluirServicoCriado", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@id", SqlDbType.Int).Value = id;
            await command.ExecuteNonQueryAsync();
        }

        private async Task<IEnumerable<Servicos>> GetFiltered(string? termo)
        {
            var servicos = new List<Servicos>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SP_ListarServicos", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@termo", SqlDbType.VarChar, 80).Value =
                string.IsNullOrWhiteSpace(termo) ? DBNull.Value : termo.Trim();

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                servicos.Add(Map(reader));
            }

            return servicos;
        }

        private static Servicos Map(SqlDataReader reader) => new()
        {
            IdServico = reader.GetInt32(0),
            Descricao = reader.GetString(1),
            PrecoBase = reader.GetDecimal(2),
            FuncionarioResponsavel = reader.GetString(3),
            IdOrcamento = reader.IsDBNull(4) ? null : reader.GetInt32(4),
            Status = reader.GetString(5)
        };
    }
}

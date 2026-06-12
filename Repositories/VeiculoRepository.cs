using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AfReparosAutomotivos.Repositories
{
    public class VeiculoRepository : SqlRepositoryBase, IVeiculoRepository
    {
        public VeiculoRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<int> Add(Veiculos veiculo)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SP_AdicionarVeiculo", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            AddVeiculoParameters(command, veiculo);
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        public async Task<Veiculos?> GetId(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SP_ObterVeiculoPorId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@id", SqlDbType.Int).Value = id;
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task<IEnumerable<Veiculos>> SearchByCliente(int clienteId, string termo)
        {
            var veiculos = new List<Veiculos>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SP_BuscarVeiculosCliente", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@clienteId", SqlDbType.Int).Value = clienteId;
            command.Parameters.Add("@termo", SqlDbType.VarChar, 50).Value =
                string.IsNullOrWhiteSpace(termo) ? DBNull.Value : termo.Trim();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                veiculos.Add(Map(reader));
            }

            return veiculos;
        }

        public async Task DeleteCreated(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SP_ExcluirVeiculoCriado", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@id", SqlDbType.Int).Value = id;
            await command.ExecuteNonQueryAsync();
        }

        private static void AddVeiculoParameters(SqlCommand command, Veiculos veiculo)
        {
            command.Parameters.Add("@clienteId", SqlDbType.Int).Value = veiculo.clienteId;
            command.Parameters.Add("@marca", SqlDbType.VarChar, 50).Value = veiculo.marca;
            command.Parameters.Add("@placa", SqlDbType.VarChar, 7).Value = veiculo.placa;
            command.Parameters.Add("@modelo", SqlDbType.VarChar, 50).Value = veiculo.modelo;
            command.Parameters.Add("@cor", SqlDbType.VarChar, 20).Value = veiculo.cor;
            command.Parameters.Add("@ano", SqlDbType.Int).Value = veiculo.ano;
        }

        private static Veiculos Map(SqlDataReader reader) => new()
        {
            id = reader.GetInt32(0),
            clienteId = reader.GetInt32(1),
            placa = reader.GetString(2),
            marca = reader.GetString(3),
            modelo = reader.GetString(4),
            cor = reader.GetString(5),
            ano = reader.GetInt32(6)
        };
    }
}

using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;

namespace AfReparosAutomotivos.Repositories
{
    public class VeiculoRepository : SqlRepositoryBase, IVeiculoRepository
    {
        public VeiculoRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<int> Add(Veiculos veiculo)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                "INSERT INTO Veiculo (clienteId, marca, placa, modelo) OUTPUT INSERTED.idVeiculo VALUES (@clienteId, @marca, @placa, @modelo)",
                connection);
            command.Parameters.AddWithValue("@clienteId", veiculo.clienteId);
            command.Parameters.AddWithValue("@marca", veiculo.marca);
            command.Parameters.AddWithValue("@placa", veiculo.placa);
            command.Parameters.AddWithValue("@modelo", veiculo.modelo);
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        public async Task<Veiculos?> GetId(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SELECT idVeiculo, clienteId, placa, marca, modelo FROM Veiculo WHERE idVeiculo = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task<Veiculos?> GetByPlaca(string placa)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SELECT idVeiculo, clienteId, placa, marca, modelo FROM Veiculo WHERE placa = @placa", connection);
            command.Parameters.AddWithValue("@placa", placa);
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        private static Veiculos Map(SqlDataReader reader) => new()
        {
            id = reader.GetInt32(0),
            clienteId = reader.GetInt32(1),
            placa = reader.GetString(2),
            marca = reader.GetString(3),
            modelo = reader.GetString(4)
        };
    }
}

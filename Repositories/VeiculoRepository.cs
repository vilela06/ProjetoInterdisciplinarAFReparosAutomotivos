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
                "INSERT INTO Veiculo (clienteId, marca, placa, modelo, cor, ano) OUTPUT INSERTED.idVeiculo VALUES (@clienteId, @marca, @placa, @modelo, @cor, @ano)",
                connection);
            command.Parameters.AddWithValue("@clienteId", veiculo.clienteId);
            command.Parameters.AddWithValue("@marca", veiculo.marca);
            command.Parameters.AddWithValue("@placa", veiculo.placa);
            command.Parameters.AddWithValue("@modelo", veiculo.modelo);
            command.Parameters.AddWithValue("@cor", veiculo.cor);
            command.Parameters.AddWithValue("@ano", veiculo.ano);
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        public async Task<Veiculos?> GetId(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SELECT idVeiculo, clienteId, placa, marca, modelo, cor, ano FROM Veiculo WHERE idVeiculo = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task<IEnumerable<Veiculos>> SearchByCliente(int clienteId, string termo)
        {
            var veiculos = new List<Veiculos>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                "SELECT idVeiculo, clienteId, placa, marca, modelo, cor, ano FROM Veiculo " +
                "WHERE clienteId = @clienteId AND (placa LIKE @termo OR marca LIKE @termo OR modelo LIKE @termo OR cor LIKE @termo OR CONVERT(VARCHAR(10), ano) LIKE @termo) " +
                "ORDER BY placa",
                connection);
            command.Parameters.AddWithValue("@clienteId", clienteId);
            command.Parameters.AddWithValue("@termo", $"%{termo?.Trim()}%");
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                veiculos.Add(Map(reader));
            }

            return veiculos;
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

using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;

namespace AfReparosAutomotivos.Repositories
{
    public class ClienteRepository : SqlRepositoryBase, IClienteRepository
    {
        public ClienteRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<int> Add(Clientes cliente)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            try
            {
                var tipoDoc = cliente.tipo_doc ?? (cliente.documento.Length == 14 ? 'J' : 'F');
                await using var pessoaCommand = new SqlCommand(
                    "INSERT INTO Pessoa (nome, telefone, documento, tipo_doc) OUTPUT INSERTED.idPessoa VALUES (@nome, @telefone, @documento, @tipo_doc)",
                    connection, transaction);
                pessoaCommand.Parameters.AddWithValue("@nome", cliente.nome);
                pessoaCommand.Parameters.AddWithValue("@telefone", cliente.telefone);
                pessoaCommand.Parameters.AddWithValue("@documento", cliente.documento);
                pessoaCommand.Parameters.AddWithValue("@tipo_doc", tipoDoc);
                var id = Convert.ToInt32(await pessoaCommand.ExecuteScalarAsync());

                await using var clienteCommand = new SqlCommand(
                    "INSERT INTO Cliente (idCliente, email, statusCli, chaveCli) VALUES (@id, @email, 1, @chave)",
                    connection, transaction);
                clienteCommand.Parameters.AddWithValue("@id", id);
                clienteCommand.Parameters.AddWithValue("@email", string.Empty);
                clienteCommand.Parameters.AddWithValue("@chave", Guid.NewGuid().ToString("N")[..19]);
                await clienteCommand.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
                return id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<Clientes>> GetAllAsync()
        {
            var clientes = new List<Clientes>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(BaseSelect() + " ORDER BY p.nome", connection);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                clientes.Add(Map(reader));
            }

            return clientes;
        }

        public async Task<Clientes?> GetId(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(BaseSelect() + " WHERE c.idCliente = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task<Clientes?> GetByDocumento(string documento)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(BaseSelect() + " WHERE p.documento = @documento", connection);
            command.Parameters.AddWithValue("@documento", documento);
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task Update(Clientes cliente)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                "UPDATE Pessoa SET nome = @nome, telefone = @telefone, documento = @documento, tipo_doc = @tipo_doc WHERE idPessoa = @id",
                connection);
            command.Parameters.AddWithValue("@id", cliente.id);
            command.Parameters.AddWithValue("@nome", cliente.nome);
            command.Parameters.AddWithValue("@telefone", cliente.telefone);
            command.Parameters.AddWithValue("@documento", cliente.documento);
            command.Parameters.AddWithValue("@tipo_doc", cliente.tipo_doc ?? (cliente.documento.Length == 14 ? 'J' : 'F'));
            await command.ExecuteNonQueryAsync();
        }

        private static string BaseSelect() =>
            "SELECT c.idCliente, p.nome, p.documento, p.telefone, p.tipo_doc, " +
            "COALESCE(e.logradouro + ', ' + e.cidade + ' - ' + e.estado + ', ' + e.CEP, '') AS endereco " +
            "FROM Cliente c INNER JOIN Pessoa p ON p.idPessoa = c.idCliente " +
            "LEFT JOIN Endereco e ON e.pessoaId = p.idPessoa";

        private static Clientes Map(SqlDataReader reader) => new()
        {
            id = reader.GetInt32(0),
            nome = reader.GetString(1),
            documento = reader.GetString(2),
            telefone = reader.GetString(3),
            tipo_doc = reader.GetString(4)[0],
            endereco = reader.GetString(5)
        };
    }
}

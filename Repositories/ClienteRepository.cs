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
                    "INSERT INTO Pessoa (nome, celular, documento, tipo_doc) OUTPUT INSERTED.idPessoa VALUES (@nome, @celular, @documento, @tipo_doc)",
                    connection, transaction);
                pessoaCommand.Parameters.AddWithValue("@nome", cliente.nome);
                pessoaCommand.Parameters.AddWithValue("@celular", cliente.celular);
                pessoaCommand.Parameters.AddWithValue("@documento", cliente.documento);
                pessoaCommand.Parameters.AddWithValue("@tipo_doc", tipoDoc);
                var id = Convert.ToInt32(await pessoaCommand.ExecuteScalarAsync());

                await using var clienteCommand = new SqlCommand(
                    "INSERT INTO Cliente (idCliente, telefone, email, statusCli, chaveCli) VALUES (@id, @telefone, @email, 1, @chave)",
                    connection, transaction);
                clienteCommand.Parameters.AddWithValue("@id", id);
                clienteCommand.Parameters.AddWithValue("@telefone", string.IsNullOrWhiteSpace(cliente.telefone) ? DBNull.Value : cliente.telefone);
                clienteCommand.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(cliente.email) ? $"{id}@sem-email.local" : cliente.email);
                clienteCommand.Parameters.AddWithValue("@chave", Guid.NewGuid().ToString("N")[..19]);
                await clienteCommand.ExecuteNonQueryAsync();

                await UpsertEndereco(connection, transaction, id, cliente);

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
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            try
            {
                await using (var pessoaCommand = new SqlCommand(
                    "UPDATE Pessoa SET nome = @nome, celular = @celular, documento = @documento, tipo_doc = @tipo_doc WHERE idPessoa = @id",
                    connection, transaction))
                {
                    pessoaCommand.Parameters.AddWithValue("@id", cliente.id);
                    pessoaCommand.Parameters.AddWithValue("@nome", cliente.nome);
                    pessoaCommand.Parameters.AddWithValue("@celular", cliente.celular);
                    pessoaCommand.Parameters.AddWithValue("@documento", cliente.documento);
                    pessoaCommand.Parameters.AddWithValue("@tipo_doc", cliente.tipo_doc ?? (cliente.documento.Length == 14 ? 'J' : 'F'));
                    await pessoaCommand.ExecuteNonQueryAsync();
                }

                await using (var clienteCommand = new SqlCommand(
                    "UPDATE Cliente SET telefone = @telefone, email = @email WHERE idCliente = @id",
                    connection, transaction))
                {
                    clienteCommand.Parameters.AddWithValue("@id", cliente.id);
                    clienteCommand.Parameters.AddWithValue("@telefone", string.IsNullOrWhiteSpace(cliente.telefone) ? DBNull.Value : cliente.telefone);
                    clienteCommand.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(cliente.email) ? $"{cliente.id}@sem-email.local" : cliente.email);
                    await clienteCommand.ExecuteNonQueryAsync();
                }

                await UpsertEndereco(connection, transaction, cliente.id, cliente);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static string BaseSelect() =>
            "SELECT c.idCliente, p.nome, p.documento, c.telefone, p.celular, c.email, c.statusCli, c.chaveCli, p.tipo_doc, " +
            "COALESCE(e.logradouro + ', ' + e.numero + ', ' + e.cidade + ' - ' + e.estado + ', ' + e.CEP, '') AS endereco, " +
            "COALESCE(e.logradouro, ''), COALESCE(e.numero, ''), COALESCE(e.cidade, ''), COALESCE(e.estado, ''), COALESCE(e.CEP, '') " +
            "FROM Cliente c INNER JOIN Pessoa p ON p.idPessoa = c.idCliente " +
            "LEFT JOIN Endereco e ON e.pessoaId = p.idPessoa";

        private static Clientes Map(SqlDataReader reader) => new()
        {
            id = reader.GetInt32(0),
            nome = reader.GetString(1),
            documento = reader.GetString(2),
            telefone = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            celular = reader.GetString(4),
            email = reader.GetString(5),
            statusCli = reader.GetInt32(6),
            chaveCli = reader.GetString(7),
            tipo_doc = reader.GetString(8)[0],
            endereco = reader.GetString(9),
            logradouro = reader.GetString(10),
            numero = reader.GetString(11),
            cidade = reader.GetString(12),
            estado = reader.GetString(13),
            cep = reader.GetString(14)
        };

        private static async Task UpsertEndereco(SqlConnection connection, SqlTransaction transaction, int pessoaId, Clientes cliente)
        {
            if (string.IsNullOrWhiteSpace(cliente.logradouro) &&
                string.IsNullOrWhiteSpace(cliente.numero) &&
                string.IsNullOrWhiteSpace(cliente.cidade) &&
                string.IsNullOrWhiteSpace(cliente.estado) &&
                string.IsNullOrWhiteSpace(cliente.cep))
            {
                return;
            }

            await using var command = new SqlCommand(
                "MERGE Endereco AS destino " +
                "USING (SELECT @id AS pessoaId) AS origem ON destino.pessoaId = origem.pessoaId " +
                "WHEN MATCHED THEN UPDATE SET logradouro = @logradouro, numero = @numero, cidade = @cidade, estado = @estado, CEP = @cep " +
                "WHEN NOT MATCHED THEN INSERT (pessoaId, logradouro, numero, cidade, estado, CEP) VALUES (@id, @logradouro, @numero, @cidade, @estado, @cep);",
                connection, transaction);
            command.Parameters.AddWithValue("@id", pessoaId);
            command.Parameters.AddWithValue("@logradouro", cliente.logradouro);
            command.Parameters.AddWithValue("@numero", cliente.numero);
            command.Parameters.AddWithValue("@cidade", cliente.cidade);
            command.Parameters.AddWithValue("@estado", cliente.estado);
            command.Parameters.AddWithValue("@cep", cliente.cep);
            await command.ExecuteNonQueryAsync();
        }
    }
}

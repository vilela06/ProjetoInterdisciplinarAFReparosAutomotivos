using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;
using System.Data;

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
                await using var command = new SqlCommand("SP_AdicionarCliente", connection, transaction)
                {
                    CommandType = CommandType.StoredProcedure
                };
                AddClienteParameters(command, cliente, includeId: false);
                var id = Convert.ToInt32(await command.ExecuteScalarAsync());
                await transaction.CommitAsync();
                return id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public Task<IEnumerable<Clientes>> GetAllAsync() => ExecuteClienteProcedure("SP_ListarClientes");

        public Task<IEnumerable<Clientes>> Search(string termo) => ExecuteClienteProcedure("SP_BuscarClientes", command =>
        {
            command.Parameters.Add("@termo", SqlDbType.VarChar, 80).Value =
                string.IsNullOrWhiteSpace(termo) ? DBNull.Value : termo.Trim();
        });

        public async Task<Clientes?> GetId(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SP_ObterClientePorId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@id", SqlDbType.Int).Value = id;
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
                await using var command = new SqlCommand("SP_AtualizarCliente", connection, transaction)
                {
                    CommandType = CommandType.StoredProcedure
                };
                AddClienteParameters(command, cliente, includeId: true);
                await command.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteCreated(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            try
            {
                await using var command = new SqlCommand("SP_ExcluirClienteCriado", connection, transaction)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                await command.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<IEnumerable<Clientes>> ExecuteClienteProcedure(string procedure, Action<SqlCommand>? configure = null)
        {
            var clientes = new List<Clientes>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(procedure, connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            configure?.Invoke(command);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                clientes.Add(Map(reader));
            }

            return clientes;
        }

        private static void AddClienteParameters(SqlCommand command, Clientes cliente, bool includeId)
        {
            if (includeId)
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = cliente.id;
            }

            command.Parameters.Add("@nome", SqlDbType.VarChar, 50).Value = cliente.nome;
            command.Parameters.Add("@celular", SqlDbType.VarChar, 15).Value = cliente.celular;
            command.Parameters.Add("@documento", SqlDbType.VarChar, 18).Value = cliente.documento;
            command.Parameters.Add("@tipo_doc", SqlDbType.Char, 1).Value = cliente.tipo_doc ?? (cliente.documento.Length == 14 ? 'J' : 'F');
            command.Parameters.Add("@telefone", SqlDbType.VarChar, 14).Value =
                string.IsNullOrWhiteSpace(cliente.telefone) ? DBNull.Value : cliente.telefone;
            command.Parameters.Add("@email", SqlDbType.VarChar, 50).Value =
                string.IsNullOrWhiteSpace(cliente.email) ? DBNull.Value : cliente.email;
            command.Parameters.Add("@logradouro", SqlDbType.VarChar, 150).Value =
                string.IsNullOrWhiteSpace(cliente.logradouro) ? DBNull.Value : cliente.logradouro;
            command.Parameters.Add("@numero", SqlDbType.VarChar, 5).Value =
                string.IsNullOrWhiteSpace(cliente.numero) ? DBNull.Value : cliente.numero;
            command.Parameters.Add("@cidade", SqlDbType.VarChar, 100).Value =
                string.IsNullOrWhiteSpace(cliente.cidade) ? DBNull.Value : cliente.cidade;
            command.Parameters.Add("@estado", SqlDbType.VarChar, 2).Value =
                string.IsNullOrWhiteSpace(cliente.estado) ? DBNull.Value : cliente.estado;
            command.Parameters.Add("@cep", SqlDbType.VarChar, 9).Value =
                string.IsNullOrWhiteSpace(cliente.cep) ? DBNull.Value : cliente.cep;
        }

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
    }
}

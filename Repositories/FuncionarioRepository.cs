using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AfReparosAutomotivos.Repositories
{
    public class FuncionarioRepository : SqlRepositoryBase, IFuncionarioRepository
    {
        public FuncionarioRepository(IConfiguration configuration) : base(configuration) { }

        public Task<IEnumerable<Funcionarios>> GetAll() => ExecuteFuncionarioProcedure("SP_ListarFuncionarios");

        public Task<IEnumerable<Funcionarios>> GetAtivos() => ExecuteFuncionarioProcedure("SP_ListarFuncionariosAtivos");

        public Task<IEnumerable<Funcionarios>> Search(string pesquisa) => ExecuteFuncionarioProcedure("SP_BuscarFuncionarios", command =>
        {
            command.Parameters.Add("@pesquisa", SqlDbType.VarChar, 80).Value =
                string.IsNullOrWhiteSpace(pesquisa) ? DBNull.Value : pesquisa.Trim();
        });

        public async Task<Funcionarios?> GetId(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SP_ObterFuncionarioPorId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@id", SqlDbType.Int).Value = id;
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task Add(Funcionarios funcionario)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            try
            {
                await using var command = new SqlCommand("SP_AdicionarFuncionario", connection, transaction)
                {
                    CommandType = CommandType.StoredProcedure
                };
                AddFuncionarioParameters(command, funcionario, includeId: false, includeSenha: true);
                await command.ExecuteScalarAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task Update(Funcionarios funcionario)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            try
            {
                await using var command = new SqlCommand("SP_AtualizarFuncionario", connection, transaction)
                {
                    CommandType = CommandType.StoredProcedure
                };
                AddFuncionarioParameters(command, funcionario, includeId: true, includeSenha: true);
                await command.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<IEnumerable<Funcionarios>> ExecuteFuncionarioProcedure(string procedure, Action<SqlCommand>? configure = null)
        {
            var funcionarios = new List<Funcionarios>();
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
                funcionarios.Add(Map(reader));
            }

            return funcionarios;
        }

        private static void AddFuncionarioParameters(SqlCommand command, Funcionarios funcionario, bool includeId, bool includeSenha)
        {
            if (includeId)
            {
                command.Parameters.Add("@idFuncionario", SqlDbType.Int).Value = funcionario.idFuncionario;
            }

            command.Parameters.Add("@nome", SqlDbType.VarChar, 50).Value = funcionario.Nome;
            command.Parameters.Add("@celular", SqlDbType.VarChar, 15).Value = funcionario.celular;
            command.Parameters.Add("@documento", SqlDbType.VarChar, 18).Value = funcionario.documento;
            command.Parameters.Add("@tipo_doc", SqlDbType.Char, 1).Value = funcionario.tipo_doc;
            command.Parameters.Add("@permissao", SqlDbType.Int).Value = funcionario.permissao;
            command.Parameters.Add("@usuario", SqlDbType.VarChar, 16).Value = funcionario.usuario;
            command.Parameters.Add("@statusFunc", SqlDbType.Int).Value = funcionario.statusFunc;

            if (includeSenha)
            {
                command.Parameters.Add("@senha", SqlDbType.VarChar, 15).Value =
                    string.IsNullOrWhiteSpace(funcionario.senha) ? DBNull.Value : funcionario.senha;
            }
        }

        private static Funcionarios Map(SqlDataReader reader) => new()
        {
            idFuncionario = reader.GetInt32(0),
            Nome = reader.GetString(1),
            celular = reader.GetString(2),
            documento = reader.GetString(3),
            tipo_doc = reader.GetString(4),
            permissao = reader.GetInt32(5),
            usuario = reader.GetString(6),
            statusFunc = reader.GetInt32(7)
        };
    }
}

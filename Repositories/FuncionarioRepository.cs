using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AfReparosAutomotivos.Repositories
{
    public class FuncionarioRepository : SqlRepositoryBase, IFuncionarioRepository
    {
        public FuncionarioRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<Funcionarios>> GetAll()
        {
            var funcionarios = new List<Funcionarios>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(BaseSelect() + " ORDER BY p.nome", connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                funcionarios.Add(Map(reader));
            }

            return funcionarios;
        }

        public async Task<IEnumerable<Funcionarios>> GetAtivos()
        {
            var funcionarios = new List<Funcionarios>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                BaseSelect() + " WHERE f.statusFunc = 1 ORDER BY p.nome",
                connection);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                funcionarios.Add(Map(reader));
            }

            return funcionarios;
        }

        public async Task<IEnumerable<Funcionarios>> Search(string pesquisa)
        {
            var funcionarios = new List<Funcionarios>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                BaseSelect() +
                " WHERE CAST(f.idFuncionario AS VARCHAR(20)) LIKE @pesquisa " +
                "OR p.nome LIKE @pesquisa OR f.usuario LIKE @pesquisa ORDER BY p.nome",
                connection);
            command.Parameters.AddWithValue("@pesquisa", $"%{pesquisa}%");
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                funcionarios.Add(Map(reader));
            }

            return funcionarios;
        }

        public async Task<Funcionarios?> GetId(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(BaseSelect() + " WHERE f.idFuncionario = @id", connection);
            command.Parameters.AddWithValue("@id", id);
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
                await using var pessoaCommand = new SqlCommand(
                    "INSERT INTO Pessoa (nome, celular, documento, tipo_doc) " +
                    "OUTPUT INSERTED.idPessoa VALUES (@nome, @celular, @documento, @tipo_doc)",
                    connection,
                    transaction);
                AddPessoaParameters(pessoaCommand, funcionario);
                var idPessoa = (int)(await pessoaCommand.ExecuteScalarAsync() ?? 0);

                await using var funcionarioCommand = new SqlCommand(
                    "INSERT INTO Funcionario (idFuncionario, permissao, usuario, senha, statusFunc) " +
                    "VALUES (@idFuncionario, @permissao, @usuario, @senha, @statusFunc)",
                    connection,
                    transaction);
                funcionarioCommand.Parameters.AddWithValue("@idFuncionario", idPessoa);
                AddFuncionarioParameters(funcionarioCommand, funcionario, includeSenha: true);
                await funcionarioCommand.ExecuteNonQueryAsync();

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
                await using var pessoaCommand = new SqlCommand(
                    "UPDATE Pessoa SET nome = @nome, celular = @celular, documento = @documento, tipo_doc = @tipo_doc " +
                    "WHERE idPessoa = @idFuncionario",
                    connection,
                    transaction);
                pessoaCommand.Parameters.AddWithValue("@idFuncionario", funcionario.idFuncionario);
                AddPessoaParameters(pessoaCommand, funcionario);
                await pessoaCommand.ExecuteNonQueryAsync();

                var sql =
                    "UPDATE Funcionario SET permissao = @permissao, usuario = @usuario, statusFunc = @statusFunc";
                if (!string.IsNullOrWhiteSpace(funcionario.senha))
                {
                    sql += ", senha = @senha";
                }
                sql += " WHERE idFuncionario = @idFuncionario";

                await using var funcionarioCommand = new SqlCommand(sql, connection, transaction);
                funcionarioCommand.Parameters.AddWithValue("@idFuncionario", funcionario.idFuncionario);
                AddFuncionarioParameters(funcionarioCommand, funcionario, includeSenha: !string.IsNullOrWhiteSpace(funcionario.senha));
                await funcionarioCommand.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static string BaseSelect()
        {
            return "SELECT f.idFuncionario, p.nome, p.celular, p.documento, p.tipo_doc, " +
                   "f.permissao, f.usuario, f.statusFunc " +
                   "FROM Funcionario f INNER JOIN Pessoa p ON p.idPessoa = f.idFuncionario";
        }

        private static Funcionarios Map(SqlDataReader reader)
        {
            return new Funcionarios
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

        private static void AddPessoaParameters(SqlCommand command, Funcionarios funcionario)
        {
            command.Parameters.Add("@nome", SqlDbType.VarChar, 50).Value = funcionario.Nome;
            command.Parameters.Add("@celular", SqlDbType.VarChar, 15).Value = funcionario.celular;
            command.Parameters.Add("@documento", SqlDbType.VarChar, 18).Value = funcionario.documento;
            command.Parameters.Add("@tipo_doc", SqlDbType.Char, 1).Value = funcionario.tipo_doc;
        }

        private static void AddFuncionarioParameters(SqlCommand command, Funcionarios funcionario, bool includeSenha)
        {
            command.Parameters.Add("@permissao", SqlDbType.Int).Value = funcionario.permissao;
            command.Parameters.Add("@usuario", SqlDbType.VarChar, 16).Value = funcionario.usuario;
            command.Parameters.Add("@statusFunc", SqlDbType.Int).Value = funcionario.statusFunc;

            if (includeSenha)
            {
                command.Parameters.Add("@senha", SqlDbType.VarChar, 15).Value = funcionario.senha;
            }
        }
    }
}

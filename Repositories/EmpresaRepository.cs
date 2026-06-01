using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AfReparosAutomotivos.Repositories
{
    public class EmpresaRepository : SqlRepositoryBase, IEmpresaRepository
    {
        private const string FotoPadrao = "/images/logo-af-reparos.png";

        public EmpresaRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<EmpresaViewModel?> GetDadosAsync()
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await EnsureEmpresaColumnsAsync(connection);

            await using var command = new SqlCommand(
                "SELECT TOP 1 f.idFuncionario, p.nome, p.documento, p.celular, " +
                "ISNULL(f.email, 'afreparos@gmail.com'), ISNULL(f.foto, @fotoPadrao), " +
                "ISNULL(e.logradouro, ''), ISNULL(e.numero, ''), ISNULL(e.cidade, ''), " +
                "ISNULL(e.estado, ''), ISNULL(e.CEP, '') " +
                "FROM Funcionario f " +
                "INNER JOIN Pessoa p ON p.idPessoa = f.idFuncionario " +
                "LEFT JOIN Endereco e ON e.pessoaId = p.idPessoa " +
                "ORDER BY f.idFuncionario",
                connection);
            command.Parameters.AddWithValue("@fotoPadrao", FotoPadrao);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new EmpresaViewModel
            {
                idFuncionario = reader.GetInt32(0),
                nomeEmpresa = reader.GetString(1),
                cnpj = reader.GetString(2),
                celular = reader.GetString(3),
                email = reader.GetString(4),
                fotoUrl = reader.GetString(5),
                logradouro = reader.GetString(6),
                numero = reader.GetString(7),
                cidade = reader.GetString(8),
                estado = reader.GetString(9),
                cep = reader.GetString(10)
            };
        }

        public async Task UpdateAsync(EmpresaViewModel empresa)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

            try
            {
                await EnsureEmpresaColumnsAsync(connection, transaction);

                await using var pessoaCommand = new SqlCommand(
                    "UPDATE Pessoa SET nome = @nome, celular = @celular, documento = @cnpj, tipo_doc = 'J' " +
                    "WHERE idPessoa = @idFuncionario",
                    connection,
                    transaction);
                pessoaCommand.Parameters.Add("@idFuncionario", SqlDbType.Int).Value = empresa.idFuncionario;
                pessoaCommand.Parameters.Add("@nome", SqlDbType.VarChar, 50).Value = empresa.nomeEmpresa;
                pessoaCommand.Parameters.Add("@celular", SqlDbType.VarChar, 15).Value = empresa.celular;
                pessoaCommand.Parameters.Add("@cnpj", SqlDbType.VarChar, 18).Value = empresa.cnpj;
                await pessoaCommand.ExecuteNonQueryAsync();

                await using var funcionarioCommand = new SqlCommand(
                    "UPDATE Funcionario SET email = @email, foto = @foto WHERE idFuncionario = @idFuncionario",
                    connection,
                    transaction);
                funcionarioCommand.Parameters.Add("@idFuncionario", SqlDbType.Int).Value = empresa.idFuncionario;
                funcionarioCommand.Parameters.Add("@email", SqlDbType.VarChar, 50).Value = empresa.email;
                funcionarioCommand.Parameters.Add("@foto", SqlDbType.VarChar, 150).Value =
                    string.IsNullOrWhiteSpace(empresa.fotoUrl) ? FotoPadrao : empresa.fotoUrl;
                await funcionarioCommand.ExecuteNonQueryAsync();

                await using var enderecoCommand = new SqlCommand(
                    "IF EXISTS (SELECT 1 FROM Endereco WHERE pessoaId = @idFuncionario) " +
                    "BEGIN " +
                    "UPDATE Endereco SET logradouro = @logradouro, numero = @numero, cidade = @cidade, estado = @estado, CEP = @cep " +
                    "WHERE pessoaId = @idFuncionario " +
                    "END ELSE BEGIN " +
                    "INSERT INTO Endereco (pessoaId, logradouro, numero, cidade, estado, CEP) " +
                    "VALUES (@idFuncionario, @logradouro, @numero, @cidade, @estado, @cep) " +
                    "END",
                    connection,
                    transaction);
                enderecoCommand.Parameters.Add("@idFuncionario", SqlDbType.Int).Value = empresa.idFuncionario;
                enderecoCommand.Parameters.Add("@logradouro", SqlDbType.VarChar, 150).Value = empresa.logradouro;
                enderecoCommand.Parameters.Add("@numero", SqlDbType.VarChar, 5).Value = empresa.numero;
                enderecoCommand.Parameters.Add("@cidade", SqlDbType.VarChar, 100).Value = empresa.cidade;
                enderecoCommand.Parameters.Add("@estado", SqlDbType.VarChar, 2).Value = empresa.estado.ToUpperInvariant();
                enderecoCommand.Parameters.Add("@cep", SqlDbType.VarChar, 9).Value = empresa.cep;
                await enderecoCommand.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static async Task EnsureEmpresaColumnsAsync(SqlConnection connection, SqlTransaction? transaction = null)
        {
            await using var command = new SqlCommand(
                "IF COL_LENGTH('Funcionario', 'email') IS NULL ALTER TABLE Funcionario ADD email VARCHAR(50) NULL; " +
                "IF COL_LENGTH('Funcionario', 'foto') IS NULL ALTER TABLE Funcionario ADD foto VARCHAR(150) NULL;",
                connection,
                transaction);
            await command.ExecuteNonQueryAsync();
        }
    }
}

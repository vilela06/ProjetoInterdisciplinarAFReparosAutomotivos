using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;

namespace AfReparosAutomotivos.Repositories
{
    public class LoginRepository : SqlRepositoryBase, ILoginRepository
    {
        public LoginRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<Funcionarios?> GetFuncionarioByCredentialsAsync(string usuario, string senha)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                "SELECT f.idFuncionario, p.nome, f.permissao, f.usuario, f.statusFunc " +
                "FROM Funcionario f INNER JOIN Pessoa p ON p.idPessoa = f.idFuncionario " +
                "WHERE f.usuario = @usuario AND f.senha = @senha AND f.statusFunc = 1",
                connection);
            command.Parameters.AddWithValue("@usuario", usuario);
            command.Parameters.AddWithValue("@senha", senha);
            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new Funcionarios
            {
                idFuncionario = reader.GetInt32(0),
                Nome = reader.GetString(1),
                permissao = reader.GetInt32(2),
                usuario = reader.GetString(3),
                statusFunc = reader.GetInt32(4)
            };
        }
    }
}

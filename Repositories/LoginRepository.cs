using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AfReparosAutomotivos.Repositories
{
    public class LoginRepository : SqlRepositoryBase, ILoginRepository
    {
        public LoginRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<Funcionarios?> GetFuncionarioByCredentialsAsync(string usuario, string senha)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("SP_AutenticarFuncionario", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@usuario", SqlDbType.VarChar, 16).Value = usuario;
            command.Parameters.Add("@senha", SqlDbType.VarChar, 15).Value = senha;
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

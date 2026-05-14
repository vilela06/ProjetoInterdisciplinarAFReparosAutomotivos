using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;

namespace AfReparosAutomotivos.Repositories
{
    public class FuncionarioRepository : SqlRepositoryBase, IFuncionarioRepository
    {
        public FuncionarioRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<Funcionarios>> GetAtivos()
        {
            var funcionarios = new List<Funcionarios>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                "SELECT f.idFuncionario, p.nome, f.permissao, f.usuario, f.statusFunc " +
                "FROM Funcionario f INNER JOIN Pessoa p ON p.idPessoa = f.idFuncionario " +
                "WHERE f.statusFunc = 1 ORDER BY p.nome",
                connection);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                funcionarios.Add(new Funcionarios
                {
                    idFuncionario = reader.GetInt32(0),
                    Nome = reader.GetString(1),
                    permissao = reader.GetInt32(2),
                    usuario = reader.GetString(3),
                    statusFunc = reader.GetInt32(4)
                });
            }

            return funcionarios;
        }
    }
}

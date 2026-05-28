using AfReparosAutomotivos.Interfaces;
using AfReparosAutomotivos.Models;
using Microsoft.Data.SqlClient;

namespace AfReparosAutomotivos.Repositories
{
    public class ServicoRepository : SqlRepositoryBase, IServicoRepository
    {
        public ServicoRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<int> Add(Servicos servico)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                "INSERT INTO Servico (descricao, valorBase) OUTPUT INSERTED.idServico VALUES (@descricao, @valorBase)",
                connection);
            command.Parameters.AddWithValue("@descricao", servico.Descricao);
            command.Parameters.AddWithValue("@valorBase", servico.PrecoBase);
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        public async Task<IEnumerable<Servicos>> Get()
        {
            return await GetFiltered(null);
        }

        public async Task<IEnumerable<Servicos>> Search(string termo)
        {
            return await GetFiltered(termo);
        }

        private async Task<IEnumerable<Servicos>> GetFiltered(string? termo)
        {
            var servicos = new List<Servicos>();
            await using var connection = CreateConnection();
            await connection.OpenAsync();

            var query =
                "SELECT s.idServico, s.descricao, s.valorBase, COALESCE(p.nome, 'Sem responsavel') AS funcionario, " +
                "MAX(o.idOrcamento) AS idOrcamento, " +
                "CASE MAX(o.statusOrc) WHEN 1 THEN 'Em analise' WHEN 2 THEN 'Aprovado' WHEN 3 THEN 'Recusado' WHEN 4 THEN 'Sendo executado' WHEN 5 THEN 'Finalizado' ELSE 'Catalogo' END AS statusServico " +
                "FROM Servico s " +
                "LEFT JOIN Itens i ON i.servicoId = s.idServico " +
                "LEFT JOIN Funcionario f ON f.idFuncionario = i.funcionarioID " +
                "LEFT JOIN Pessoa p ON p.idPessoa = f.idFuncionario " +
                "LEFT JOIN Orcamento o ON o.idOrcamento = i.orcamentoId ";

            if (!string.IsNullOrWhiteSpace(termo))
            {
                query += "WHERE p.nome LIKE @termo OR CONVERT(VARCHAR(20), o.idOrcamento) LIKE @termo ";
            }

            query += "GROUP BY s.idServico, s.descricao, s.valorBase, p.nome ORDER BY s.descricao";

            await using var command = new SqlCommand(query, connection);
            if (!string.IsNullOrWhiteSpace(termo))
            {
                command.Parameters.AddWithValue("@termo", $"%{termo.Trim()}%");
            }

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                servicos.Add(Map(reader));
            }

            return servicos;
        }

        public async Task<Servicos?> GetId(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                "SELECT idServico, descricao, valorBase, 'Sem responsavel', NULL, 'Catalogo' FROM Servico WHERE idServico = @id",
                connection);
            command.Parameters.AddWithValue("@id", id);
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task<decimal> GetPrecoBaseByIdAsync(int id)
        {
            var servico = await GetId(id);
            return servico?.PrecoBase ?? 0m;
        }

        public async Task DeleteCreated(int id)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                "DELETE FROM Servico WHERE idServico = @id AND NOT EXISTS (SELECT 1 FROM Itens WHERE servicoId = @id)",
                connection);
            command.Parameters.AddWithValue("@id", id);
            await command.ExecuteNonQueryAsync();
        }

        private static Servicos Map(SqlDataReader reader) => new()
        {
            IdServico = reader.GetInt32(0),
            Descricao = reader.GetString(1),
            PrecoBase = reader.GetDecimal(2),
            FuncionarioResponsavel = reader.GetString(3),
            IdOrcamento = reader.IsDBNull(4) ? null : reader.GetInt32(4),
            Status = reader.GetString(5)
        };
    }
}

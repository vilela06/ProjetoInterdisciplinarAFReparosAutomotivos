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
            await using var command = new SqlCommand("SP_ObterEmpresaDados", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

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
                await using var command = new SqlCommand("SP_AtualizarEmpresa", connection, transaction)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add("@idFuncionario", SqlDbType.Int).Value = empresa.idFuncionario;
                command.Parameters.Add("@nome", SqlDbType.VarChar, 50).Value = empresa.nomeEmpresa;
                command.Parameters.Add("@celular", SqlDbType.VarChar, 15).Value = empresa.celular;
                command.Parameters.Add("@cnpj", SqlDbType.VarChar, 18).Value = empresa.cnpj;
                command.Parameters.Add("@email", SqlDbType.VarChar, 50).Value = empresa.email;
                command.Parameters.Add("@foto", SqlDbType.VarChar, 150).Value =
                    string.IsNullOrWhiteSpace(empresa.fotoUrl) ? FotoPadrao : empresa.fotoUrl;
                command.Parameters.Add("@logradouro", SqlDbType.VarChar, 150).Value = empresa.logradouro;
                command.Parameters.Add("@numero", SqlDbType.VarChar, 5).Value = empresa.numero;
                command.Parameters.Add("@cidade", SqlDbType.VarChar, 100).Value = empresa.cidade;
                command.Parameters.Add("@estado", SqlDbType.VarChar, 2).Value = empresa.estado.ToUpperInvariant();
                command.Parameters.Add("@cep", SqlDbType.VarChar, 9).Value = empresa.cep;
                await command.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}

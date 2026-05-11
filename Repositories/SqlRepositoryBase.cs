using Microsoft.Data.SqlClient;

namespace AfReparosAutomotivos.Repositories
{
    public abstract class SqlRepositoryBase
    {
        private readonly IConfiguration _configuration;

        protected SqlRepositoryBase(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected SqlConnection CreateConnection()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection nao foi configurada.");
            }

            return new SqlConnection(connectionString);
        }
    }
}

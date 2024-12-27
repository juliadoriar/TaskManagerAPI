using Microsoft.Data.SqlClient;

namespace TaskManagerAPI.Data
{
    public class Database
    {
        private readonly string _connectionString;

        public Database(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
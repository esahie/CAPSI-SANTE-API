
using Npgsql;
using Microsoft.Extensions.Configuration;
using System.Data;
using CAPSI.Sante.Application.Data;

namespace CAPSI.Sante.Infrastructure.PostgreSQL.Data
{
    public class PostgresConnection : IDatabaseConnection
    {
        private readonly string _connectionString;

        public PostgresConnection(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            var connection = new NpgsqlConnection(_connectionString);
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            return connection;
        }

    }
}

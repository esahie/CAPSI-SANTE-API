using CAPSI.Sante.Application.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Infrastructure.SqlServer.Data
{
    public class DatabaseConnection : IDatabaseConnection
    {
        private readonly string _sqlServerConnectionString;
        private readonly string _postgresConnectionString;

        public DatabaseConnection(IConfiguration configuration)
        {
            _sqlServerConnectionString = configuration.GetConnectionString("SqlServerConnection");
            _postgresConnectionString = configuration.GetConnectionString("PostgresConnection");
        }

        // Méthode requise par l'interface IDatabaseConnection
        public IDbConnection CreateConnection()
        {
            // Par défaut, retourne une connexion SQL Server
            return CreateSqlServerConnection();
        }

        // Méthodes supplémentaires spécifiques à cette implémentation
        public IDbConnection CreateSqlServerConnection() => new SqlConnection(_sqlServerConnectionString);

        public IDbConnection CreatePostgresConnection() => new NpgsqlConnection(_postgresConnectionString);

    }
}

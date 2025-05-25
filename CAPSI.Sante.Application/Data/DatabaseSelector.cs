
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Data
{
    public class DatabaseSelector : IDatabaseSelector
    {
        private readonly IDatabaseConnection _sqlServerConnection;
        private readonly IDatabaseConnection _postgresConnection;

        public DatabaseSelector(
            IDatabaseConnection sqlServerConnection,
            IDatabaseConnection postgresConnection)
        {
            _sqlServerConnection = sqlServerConnection;
            _postgresConnection = postgresConnection;
        }

        public IDatabaseConnection GetConnection(string dbType)
        {
            return dbType.ToLower() switch
            {
                "sqlserver" => _sqlServerConnection,
                "postgres" => _postgresConnection,
                _ => throw new ArgumentException($"Type de base de données non pris en charge: {dbType}")
            };
        }
    }
}

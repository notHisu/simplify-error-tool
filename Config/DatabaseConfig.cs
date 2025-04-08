using DotNetEnv;
using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ErrorTool.Config
{
    public class DatabaseConfig
    {
        private const string DEFAULT_ENV_PATH = ".env";
        private static bool _initialized = false;

        public string ConnectionString { get; private set; }
        public bool IsConfigured => !string.IsNullOrEmpty(ConnectionString);

        public DatabaseConfig(string envPath = DEFAULT_ENV_PATH)
        {
            // Load environment variables if not already loaded
            if (!_initialized)
            {
                try
                {
                    Env.Load(envPath);
                    _initialized = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load .env file: {ex.Message}");
                }
            }

            // Get connection string from environment
            ConnectionString = Env.GetString("SQL_CONNECTION_STRING");
        }

        public SqlConnection CreateConnection()
        {
            if (!IsConfigured)
                throw new InvalidOperationException("Database is not configured. Check your .env file.");

            var connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        public async Task<DataTable> ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            using (var connection = CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                using (var adapter = new SqlDataAdapter(command))
                {
                    var table = new DataTable();
                    await Task.Run(() => adapter.Fill(table));
                    return table;
                }
            }
        }

        public async Task<int> ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (var connection = CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                return await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<object?> ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (var connection = CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                return await command.ExecuteScalarAsync();
            }
        }
    }
}
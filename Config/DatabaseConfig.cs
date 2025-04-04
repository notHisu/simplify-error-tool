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

        /// <summary>
        /// Creates a new SQL connection using the configured connection string
        /// </summary>
        /// <returns>An open SqlConnection</returns>
        public SqlConnection CreateConnection()
        {
            if (!IsConfigured)
                throw new InvalidOperationException("Database is not configured. Check your .env file.");

            var connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Executes a SQL query and returns the results as a DataTable
        /// </summary>
        public DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
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
                    adapter.Fill(table);
                    return table;
                }
            }
        }

        /// <summary>
        /// Executes a non-query SQL command (INSERT, UPDATE, DELETE) and returns the number of rows affected
        /// </summary>
        public int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (var connection = CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes a SQL query and returns the first column of the first row
        /// </summary>
        public object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (var connection = CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                return command.ExecuteScalar();
            }
        }
    }
}
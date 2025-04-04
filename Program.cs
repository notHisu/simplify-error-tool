using DotNetEnv;
using ErrorTool.Config;
using ErrorTool.Interfaces;
using ErrorTool.Services;

namespace ErrorTool
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                // Load environment variables
                string envName = Environment.GetEnvironmentVariable("APP_ENVIRONMENT") ?? "Development";
                string envFile = $".env.{envName}";

                if (System.IO.File.Exists(envFile))
                    Env.TraversePath().Load(envFile);
                else
                    Env.TraversePath().Load(".env");

                // Create configurations
                var elasticConfig = new ElasticConfig();
                var dbConfig = new DatabaseConfig();

                // Create services 
                IElasticSearchService elasticService = new ElasticSearchService(elasticConfig);
                IParcelService parcelService = new ParcelService(dbConfig);

                // Initialize application with services
                ApplicationConfiguration.Initialize();
                Application.Run(new MainForm(elasticService, parcelService));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fatal error starting application: {ex.Message}", 
                    "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
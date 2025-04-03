using DotNetEnv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorTool.Config
{
    public class ElasticConfig
    {
        private const string DEFAULT_ENV_PATH = ".env";
        private static bool _initialized = false;

        public string CloudId { get; private set; }
        public string ApiKey { get; private set; }
        public bool IsConfigured => !string.IsNullOrEmpty(CloudId) && !string.IsNullOrEmpty(ApiKey);

        public ElasticConfig(string envPath = DEFAULT_ENV_PATH)
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

            // Get credentials from environment
            CloudId = Env.GetString("ELASTIC_CLOUD_ID");
            ApiKey = Env.GetString("ELASTIC_API_KEY");
        }
    }
}

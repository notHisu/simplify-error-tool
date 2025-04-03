using DotNetEnv;

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
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            string envName = Environment.GetEnvironmentVariable("APP_ENVIRONMENT") ?? "Development";
            string envFile = $".env.{envName}";

            if (System.IO.File.Exists(envFile))
                Env.TraversePath().Load(envFile);
            else
                Env.TraversePath().Load(".env");

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
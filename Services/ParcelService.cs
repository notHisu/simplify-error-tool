using DotNetEnv;
using ErrorTool.Config;
using ErrorTool.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorTool.Services
{
    public class ParcelService : IParcelService
    {
        private readonly DatabaseConfig _dbConfig;

        public ParcelService(DatabaseConfig dbConfig)
        {
            _dbConfig = dbConfig ?? throw new ArgumentNullException(nameof(dbConfig));
        }

        public async Task<string> ConfirmParcel(long parcelId, string sessionId)
        {
            try
            {
                // You may want to load this from config
                string apiEndpoint = Env.GetString("PARCEL_DOWNLOAD_CONFIRM_URL");
                string url = $"{apiEndpoint}?SessionId={Uri.EscapeDataString(sessionId)}&ParcelId={parcelId}";

                Debug.WriteLine($"Making API call to: {url}");

                using (var client = new HttpClient())
                {
                    // Set appropriate timeouts
                    client.Timeout = TimeSpan.FromSeconds(30);

                    // Make the API call
                    return await client.GetStringAsync(url);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to confirm parcel: {ex.Message}", ex);
            }
        }

        public async Task<string> GetParcelContent(long parcelId, string userName, string connectionName)
        {
            try
            {
                string userId = await GetUserIdAsync(userName);
                if (string.IsNullOrEmpty(userId))
                {
                    throw new InvalidOperationException($"User not found: {userName}");
                }
                
                string encodedApiKey = await GetApiKeyAsync(userId, connectionName);
                if (string.IsNullOrEmpty(encodedApiKey))
                {
                    throw new InvalidOperationException($"API key not found for {userName} with connection {connectionName}");
                }
                
                //string sessionId = DecodeApiKey(encodedApiKey);

                string sessionId = Env.GetString("GLOBE_ELECTRIC_API_KEY");

                string xmlResponse = await GetParcelXmlAsync(sessionId, parcelId);

                string base64Content = ExtractContentFromXml(xmlResponse);

                string decodedContent = DecodeBase64(base64Content);

                return decodedContent;

            }
            catch (Exception ex)
            {
                // Log the exception details
                System.Diagnostics.Debug.WriteLine($"Error retrieving parcel content: {ex.Message}");
                throw;
            }
        }

        private async Task<string> GetUserIdAsync(string displayName)
        {
            // Use _dbConfig to execute SQL query
            string query = "SELECT Id FROM [User] WHERE DisplayName = @DisplayName";
            var parameter = new Microsoft.Data.SqlClient.SqlParameter("@DisplayName", displayName);
            
            var result = await ExecuteScalarQueryAsync(query, parameter);
            return result?.ToString();
        }

        private async Task<string> GetApiKeyAsync(string userId, string connectionName)
        {
            string query = "SELECT ApiKey FROM IntegrationConnection WHERE CreatedBy = @CreatedBy AND Name = @Name";
            var parameters = new[] {
                new Microsoft.Data.SqlClient.SqlParameter("@CreatedBy", userId),
                new Microsoft.Data.SqlClient.SqlParameter("@Name", connectionName)
            };
            
            var result = await ExecuteScalarQueryAsync(query, parameters);
            return result?.ToString();
        }

        private async Task<object> ExecuteScalarQueryAsync(string sql, params Microsoft.Data.SqlClient.SqlParameter[] parameters)
        {
            using (var connection = new Microsoft.Data.SqlClient.SqlConnection(_dbConfig.ConnectionString))
            {
                await connection.OpenAsync();
                
                using (var command = new Microsoft.Data.SqlClient.SqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    
                    return await command.ExecuteScalarAsync();
                }
            }
        }

        private string DecodeApiKey(string encodedApiKey)
        {
            try
            {
                // Assuming the ApiKey is Base64 encoded
                byte[] bytes = Convert.FromBase64String(encodedApiKey);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to decode API key: {ex.Message}", ex);
            }
        }

        private async Task<string> GetParcelXmlAsync(string sessionId, long parcelId)
        {
            try
            {
                // You may want to load this from config
                string apiEndpoint = Env.GetString("PARCEL_DOWNLOAD_NO_UPDATE_URL");
                string url = $"{apiEndpoint}?SessionId={Uri.EscapeDataString(sessionId)}&ParcelId={parcelId}";

                Debug.WriteLine($"Making API call to: {url}");

                using (var client = new HttpClient())
                {
                    // Set appropriate timeouts
                    client.Timeout = TimeSpan.FromSeconds(30);
                    
                    // Make the API call
                    return await client.GetStringAsync(url);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve parcel data: {ex.Message}", ex);
            }
        }
        
        private string ExtractContentFromXml(string xml)
        {
            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(xml);
                
                // Get the root element
                var root = doc.DocumentElement;
                if (root == null)
                {
                    throw new InvalidOperationException("XML document has no root element");
                }
                
                // Extract the default namespace from the root element
                string defaultNamespace = root.GetAttribute("xmlns");
                
                // Create namespace manager and add the namespace if it exists
                var nsManager = new System.Xml.XmlNamespaceManager(doc.NameTable);
                
                if (!string.IsNullOrEmpty(defaultNamespace))
                {
                    nsManager.AddNamespace("ns", defaultNamespace);
                    
                    // Try to select the Content node with the extracted namespace
                    var contentNode = doc.SelectSingleNode("//ns:Content", nsManager);
                    if (contentNode != null)
                    {
                        return contentNode.InnerText;
                    }
                }
                
                // Fallback options if namespace approach doesn't work
                
                // Try getting the default namespace from the root element's namespace URI
                if (!string.IsNullOrEmpty(root.NamespaceURI))
                {
                    nsManager = new System.Xml.XmlNamespaceManager(doc.NameTable);
                    nsManager.AddNamespace("ns", root.NamespaceURI);
                    
                    var contentNodeWithNsUri = doc.SelectSingleNode("//ns:Content", nsManager);
                    if (contentNodeWithNsUri != null)
                    {
                        return contentNodeWithNsUri.InnerText;
                    }
                }
                
                // Try without any namespace as a last resort
                var contentNodeNoNs = doc.SelectSingleNode("//Content");
                if (contentNodeNoNs != null)
                {
                    return contentNodeNoNs.InnerText;
                }
                
                throw new InvalidOperationException("Content node not found in XML response");
            }
            catch (System.Xml.XmlException ex)
            {
                throw new InvalidOperationException($"Failed to parse XML: {ex.Message}", ex);
            }
        }

        private string DecodeBase64(string base64Content)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(base64Content);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to decode base64 content: {ex.Message}", ex);
            }
        }

        public Task<bool> ProcessParcel(long parcelId)
        {
            throw new NotImplementedException();
        }
    }
}

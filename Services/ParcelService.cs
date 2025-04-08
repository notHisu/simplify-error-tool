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

        public async Task<string?> ConfirmParcel(long parcelId, string userName, string connectionName)
        {
            try
            {
                string? userId = await GetUserIdAsync(userName);
                if (string.IsNullOrEmpty(userId))
                {
                    throw new InvalidOperationException($"User not found: {userName}");
                }

                string? encodedApiKey = await GetApiKeyAsync(userId, connectionName);
                if (string.IsNullOrEmpty(encodedApiKey))
                {
                    throw new InvalidOperationException($"API key not found for {userName} with connection {connectionName}");
                }

                string sessionId = Env.GetString("GLOBE_ELECTRIC_API_KEY");
                if (string.IsNullOrEmpty(sessionId))
                {
                    sessionId = DecodeApiKey(encodedApiKey);
                }

                string apiEndpoint = Env.GetString("PARCEL_DOWNLOAD_CONFIRM_URL");
                if (string.IsNullOrEmpty(apiEndpoint))
                {
                    throw new InvalidOperationException("Missing PARCEL_DOWNLOAD_CONFIRM_URL in configuration");
                }

                string url = $"{apiEndpoint}?SessionId={Uri.EscapeDataString(sessionId)}&ParcelId={parcelId}";
                Debug.WriteLine($"Confirming parcel {parcelId} with URL: {url}");

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    var response = await client.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"Error response: {response.StatusCode}, Content: {errorContent}");
                        throw new InvalidOperationException($"API returned error: {response.StatusCode} - {errorContent}");
                    }

                    var result = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Confirm parcel successful: {result}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in ConfirmParcel: {ex.Message}");
                throw new InvalidOperationException($"Failed to confirm parcel: {ex.Message}", ex);
            }
        }

        public async Task<string?> GetParcelContent(long parcelId, string userName, string connectionName)
        {
            try
            {
                string? userId = await GetUserIdAsync(userName);
                if (string.IsNullOrEmpty(userId))
                {
                    throw new InvalidOperationException($"User not found: {userName}");
                }
                
                string? encodedApiKey = await GetApiKeyAsync(userId, connectionName);
                if (string.IsNullOrEmpty(encodedApiKey))
                {
                    throw new InvalidOperationException($"API key not found for {userName} with connection {connectionName}");
                }
                
                //string sessionId = DecodeApiKey(encodedApiKey);

                string sessionId = Env.GetString("GLOBE_ELECTRIC_API_KEY");

                string xmlResponse = await GetParcelXmlAsync(sessionId, parcelId);

                string base64Content = ExtractContentFromXml(xmlResponse);

                string decodedContent = DecodeBase64(base64Content);

                string parcelInfo = await GetParcelInfo(parcelId, sessionId);

                return decodedContent + "\n\n\n\n\n" + parcelInfo;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving parcel content: {ex.Message}");
                throw;
            }
        }

        public async Task<string?> ProcessParcel(long parcelId, string mailBoxId)
        {
            try
            {
                string apiEndpoint = Env.GetString("PARCEL_PROCESS_URL");
                if (string.IsNullOrEmpty(apiEndpoint))
                {
                    throw new InvalidOperationException("Missing PARCEL_PROCESS_URL in configuration");
                }

                Debug.WriteLine($"Processing parcel {parcelId} for mailbox {mailBoxId}");

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    var formContent = new FormUrlEncodedContent(new[]
                    {
                new KeyValuePair<string, string>("ParcelID", parcelId.ToString()),
                new KeyValuePair<string, string>("MailBoxID", mailBoxId)
            });

                    Debug.WriteLine($"Sending POST request to {apiEndpoint}");
                    var response = await client.PostAsync(apiEndpoint, formContent);


                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"Error response: {response.StatusCode}, Content: {errorContent}");
                        throw new InvalidOperationException($"API returned error: {response.StatusCode} - {errorContent}");
                    }

                    var result = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Process parcel successful: {result}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in ProcessParcel: {ex.Message}");
                throw new InvalidOperationException($"Failed to process parcel: {ex.Message}", ex);
            }
        }

        private async Task<string?> GetUserIdAsync(string displayName)
        {
            // Use _dbConfig to execute SQL query
            string query = "SELECT Id FROM [User] WHERE DisplayName = @DisplayName";
            var parameter = new Microsoft.Data.SqlClient.SqlParameter("@DisplayName", displayName);
            
            var result = await _dbConfig.ExecuteScalar(query, parameter);
            return result?.ToString();
        }

        private async Task<string?> GetApiKeyAsync(string userId, string connectionName)
        {
            string query = "SELECT ApiKey FROM IntegrationConnection WHERE CreatedBy = @CreatedBy AND Name = @Name";
            var parameters = new[] {
                new Microsoft.Data.SqlClient.SqlParameter("@CreatedBy", userId),
                new Microsoft.Data.SqlClient.SqlParameter("@Name", connectionName)
            };
            
            var result = await _dbConfig.ExecuteScalar(query, parameters);
            return result?.ToString();
        }

        private string DecodeApiKey(string encodedApiKey)
        {
            try
            {
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
                string apiEndpoint = Env.GetString("PARCEL_DOWNLOAD_NO_UPDATE_URL");
                string url = $"{apiEndpoint}?SessionId={Uri.EscapeDataString(sessionId)}&ParcelId={parcelId}";

                Debug.WriteLine($"Making API call to: {url}");

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    
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
                
                var root = doc.DocumentElement;
                if (root == null)
                {
                    throw new InvalidOperationException("XML document has no root element");
                }
                
                string defaultNamespace = root.GetAttribute("xmlns");
                
                var nsManager = new System.Xml.XmlNamespaceManager(doc.NameTable);
                
                if (!string.IsNullOrEmpty(defaultNamespace))
                {
                    nsManager.AddNamespace("ns", defaultNamespace);
                    
                    var contentNode = doc.SelectSingleNode("//ns:Content", nsManager);
                    if (contentNode != null)
                    {
                        return contentNode.InnerText;
                    }
                }
                
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

        private async Task<string> GetParcelInfo(long parcelId, string sessionId)
        {
            try
            {
                string apiEndpoint = Env.GetString("PARCEL_INFO_URL");
                string url = $"{apiEndpoint}?SessionId={Uri.EscapeDataString(sessionId)}&ParcelId={parcelId}";
                Debug.WriteLine($"Making API call to: {url}");
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    return await client.GetStringAsync(url);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve parcel data: {ex.Message}", ex);
            }
        }
    }
}

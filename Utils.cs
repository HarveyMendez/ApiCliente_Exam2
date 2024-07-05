using System.Data.SqlClient;

namespace ApiCliente
{
    public class Utils
    {
        private readonly string _connectionString;
        private readonly string _apiHost;
        private readonly string _emailAPI;

        public Utils(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ModuloCitasExpediente");
            _apiHost = configuration.GetValue<string>("APIHost");
            _emailAPI = configuration.GetValue<string>("EmailAPI");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(DecryptConnectionString(_connectionString));
        }

        public static string DecryptConnectionString(string encryptedConnectionString)
        {


            byte[] decodedBytes = Convert.FromBase64String(encryptedConnectionString);
            string decryptedConnectionString = System.Text.Encoding.UTF8.GetString(decodedBytes);

            return decryptedConnectionString;
        }

        public string GetEmailAPI()
        {
            return _emailAPI;
        }

        public HttpClient GetAPIHost()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(_apiHost);
            return client;
        }
    }
}

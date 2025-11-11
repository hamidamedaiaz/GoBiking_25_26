using LetsGoBiking.ServiceReference1;
using System;
using System.IO;
using System.ServiceModel.Web;  // ✅ Ajouter cette ligne
using System.Text;

namespace RoutingService
{
    public class RoutingService : IRoutingService
    {
        public Stream GetItinerary(string o, string d)
        {
            try
            {
                // 1️⃣ Créer le client SOAP vers ton Proxy
                ProxyServiceClient proxy = new ProxyServiceClient("BasicHttpBinding_IProxyService");

                // 2️⃣ Préparer l'URL de l'API JCDecaux à demander au Proxy
                string apiUrl = $"https://api.jcdecaux.com/vls/v1/contracts?apiKey=4ffffd09ca3de7e586d3f46bebbd9a8a7f98191f";

                // 3️⃣ Appeler le Proxy
                string response = proxy.Get(apiUrl);

                Console.WriteLine($"✅ Proxy SOAP contacté avec succès !");
                
                // 4️⃣ Retourner le JSON brut comme Stream
                byte[] resultBytes = Encoding.UTF8.GetBytes(response);
                WebOperationContext.Current.OutgoingResponse.ContentType = "application/json";
                return new MemoryStream(resultBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur dans RoutingService : {ex.Message}");
                string errorJson = $"{{\"error\":\"{ex.Message}\"}}";
                byte[] errorBytes = Encoding.UTF8.GetBytes(errorJson);
                WebOperationContext.Current.OutgoingResponse.ContentType = "application/json";
                return new MemoryStream(errorBytes);
            }
        }
    }
}

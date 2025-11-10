using LetsGoBiking.ServiceReference1;
using System;

namespace RoutingService
{
    public class RoutingService : IRoutingService
    {
        public string GetItinerary(string o, string d)
        {
            try
            {
                // 1️⃣ Créer le client SOAP vers ton Proxy
                ProxyServiceClient proxy = new ProxyServiceClient("BasicHttpBinding_IProxyService");

                // 2️⃣ Préparer l’URL de l’API JCDecaux à demander au Proxy
                string apiUrl = $"https://api.jcdecaux.com/vls/v1/contracts?apiKey=4ffffd09ca3de7e586d3f46bebbd9a8a7f98191f";

                // 3️⃣ Appeler le Proxy
                string response = proxy.Get(apiUrl);

                Console.WriteLine($"✅ Proxy SOAP contacté avec succès !");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur dans RoutingService : {ex.Message}");
                return $"{{\"error\":\"{ex.Message}\"}}";
            }
        }
    }
}

using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace RoutingService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("?? Démarrage du RoutingService...");

            using (ServiceHost host = new ServiceHost(typeof(RoutingService)))
            {
                try
                {
                    host.Open();
                    Console.WriteLine("? RoutingService démarré sur http://localhost:8081/RoutingServer");
                    Console.WriteLine();
                    Console.WriteLine("?? Endpoints disponibles:");
                    foreach (ServiceEndpoint endpoint in host.Description.Endpoints)
                    {
                        Console.WriteLine($"   - {endpoint.Address} ({endpoint.Binding.Name})");
                    }
                    Console.WriteLine();
                    Console.WriteLine("?? Pour tester, ouvrez votre navigateur et allez sur:");
                    Console.WriteLine("   http://localhost:8081/RoutingServer/itinerary?origin=Nice,France&destination=Monaco");
                    Console.WriteLine();
                    Console.WriteLine("Appuyez sur [Entrée] pour arrêter le service...");
                    Console.ReadLine();
                    host.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"? Erreur: {ex.Message}");
                    Console.WriteLine($"   {ex.StackTrace}");
                    Console.ReadLine();
                }
            }
        }
    }
}

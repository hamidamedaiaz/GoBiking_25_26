using LetsGoBiking.ServiceReference1;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;

namespace RoutingService
{
    public class RoutingService : IRoutingService
    {
        private const string OPENROUTE_API_KEY = "eyJvcmciOiI1YjNjZTM1OTc4NTExMTAwMDFjZjYyNDgiLCJpZCI6IjRkOWQ3ZjljNTRhYTRkYTU4NzMzYmQzYjhiZTJmYTEwIiwiaCI6Im11cm11cjY0In0=";
        private const string JCDECAUX_URL = "https://api.jcdecaux.com/vls/v1/stations?contract=Nice&apiKey=4ffffd09ca3de7e586d3f46bebbd9a8a7f98191f";

        public Stream GetItinerary(string o, string d)
        {
            try
            {
                Console.WriteLine($"🚀 Requête reçue : {o} → {d}");

                // 1️⃣ Géocodage des adresses avec OpenStreetMap
                var originCoords = GetCoordinates(o);
                var destCoords = GetCoordinates(d);

                if (originCoords == null || destCoords == null)
                {
                    return CreateErrorResponse("Impossible de géocoder les adresses");
                }

                Console.WriteLine($"📍 Origine: {originCoords.Lat}, {originCoords.Lng}");
                Console.WriteLine($"📍 Destination: {destCoords.Lat}, {destCoords.Lng}");

                // 2️⃣ Récupérer les stations via le proxy
                ProxyServiceClient proxy = new ProxyServiceClient("BasicHttpBinding_IProxyService");
                string stationsJson = proxy.Get(JCDECAUX_URL);
                var stations = JsonConvert.DeserializeObject<List<Station>>(stationsJson);

                Console.WriteLine($"🚲 Nombre de stations: {stations.Count}");

                // 3️⃣ Calculer l'itinéraire optimal
                var itinerary = CalculateOptimalItinerary(originCoords, destCoords, stations);

                // 4️⃣ Retourner la réponse JSON
                WebOperationContext.Current.OutgoingResponse.ContentType = "application/json";
                string json = JsonConvert.SerializeObject(itinerary, Newtonsoft.Json.Formatting.Indented);
                return new MemoryStream(Encoding.UTF8.GetBytes(json));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur: {ex.Message}");
                return CreateErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Géocode une adresse avec OpenStreetMap Nominatim
        /// </summary>
        private Position GetCoordinates(string address)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "LetsGoBiking/1.0");
                    string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";
                    string json = client.GetStringAsync(url).Result;

                    var results = JsonConvert.DeserializeObject<JArray>(json);
                    if (results.Count == 0) return null;

                    return new Position
                    {
                        Lat = results[0]["lat"].Value<double>(),
                        Lng = results[0]["lon"].Value<double>()
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur géocodage: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Logique métier: calcule l'itinéraire optimal avec ou sans vélo
        /// </summary>
        private Itinerary CalculateOptimalItinerary(Position origin, Position dest, List<Station> stations)
        {
            // Calculer l'itinéraire à pied direct
            var walkDuration = GetRouteDuration(origin, dest, "foot-walking");
            Console.WriteLine($"🚶 Durée à pied direct: {walkDuration / 60:F1} min");

            // Trouver les meilleures stations
            var departureStation = FindNearestStationWithBikes(origin, stations);
            var arrivalStation = FindNearestStationWithStands(dest, stations);

            // Si pas de stations disponibles, itinéraire à pied
            if (departureStation == null || arrivalStation == null)
            {
                Console.WriteLine("⚠️ Pas de stations disponibles, itinéraire à pied uniquement");
                return CreateWalkOnlyItinerary(origin, dest, walkDuration);
            }

            // Calculer l'itinéraire avec vélo
            var walkToStation = GetRouteDuration(origin, departureStation.Position, "foot-walking");
            var bikeRoute = GetRouteDuration(departureStation.Position, arrivalStation.Position, "cycling-regular");
            var walkFromStation = GetRouteDuration(arrivalStation.Position, dest, "foot-walking");
            var bikeTotal = walkToStation + bikeRoute + walkFromStation;

            Console.WriteLine($"🚲 Durée avec vélo: {bikeTotal / 60:F1} min (marche {walkToStation / 60:F1}min + vélo {bikeRoute / 60:F1}min + marche {walkFromStation / 60:F1}min)");

            // Choisir la meilleure option
            if (walkDuration < bikeTotal)
            {
                Console.WriteLine("✅ Itinéraire à pied plus rapide");
                return CreateWalkOnlyItinerary(origin, dest, walkDuration);
            }
            else
            {
                Console.WriteLine("✅ Itinéraire avec vélo plus rapide");
                return CreateBikeItinerary(origin, dest, departureStation, arrivalStation, walkToStation, bikeRoute, walkFromStation);
            }
        }

        /// <summary>
        /// Trouve la station la plus proche avec des vélos disponibles
        /// </summary>
        private Station FindNearestStationWithBikes(Position position, List<Station> stations)
        {
            return stations
                .Where(s => s.AvailableBikes > 0)
                .OrderBy(s => CalculateDistance(position, s.Position))
                .FirstOrDefault();
        }

        /// <summary>
        /// Trouve la station la plus proche avec des places disponibles
        /// </summary>
        private Station FindNearestStationWithStands(Position position, List<Station> stations)
        {
            return stations
                .Where(s => s.AvailableBikeStands > 0)
                .OrderBy(s => CalculateDistance(position, s.Position))
                .FirstOrDefault();
        }

        /// <summary>
        /// Calcule la distance à vol d'oiseau (formule de Haversine)
        /// </summary>
        private double CalculateDistance(Position p1, Position p2)
        {
            const double R = 6371; // Rayon de la Terre en km
            var lat1 = p1.Lat * Math.PI / 180;
            var lat2 = p2.Lat * Math.PI / 180;
            var dLat = (p2.Lat - p1.Lat) * Math.PI / 180;
            var dLon = (p2.Lng - p1.Lng) * Math.PI / 180;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1) * Math.Cos(lat2) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        /// <summary>
        /// Appelle OpenRouteService pour calculer la durée d'un trajet
        /// </summary>
        private double GetRouteDuration(Position start, Position end, string profile)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"https://api.openrouteservice.org/v2/directions/{profile}?api_key={OPENROUTE_API_KEY}&start={start.Lng},{start.Lat}&end={end.Lng},{end.Lat}";
                    string json = client.GetStringAsync(url).Result;

                    var result = JsonConvert.DeserializeObject<JObject>(json);
                    var duration = result["features"][0]["properties"]["segments"][0]["duration"].Value<double>();
                    return duration;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erreur OpenRouteService: {ex.Message}, utilisation de l'estimation");
                // Fallback: estimation basée sur la distance
                var distance = CalculateDistance(start, end);
                return profile == "cycling-regular" ? distance * 300 : distance * 720; // 12km/h vélo, 5km/h marche
            }
        }

        /// <summary>
        /// Crée un itinéraire à pied uniquement
        /// </summary>
        private Itinerary CreateWalkOnlyItinerary(Position origin, Position dest, double duration)
        {
            return new Itinerary
            {
                UseBike = false,
                DepartureStation = null,
                ArrivalStation = null,
                TotalDuration = duration,
                Steps = new List<Step>
                {
                    new Step
                    {
                        Instruction = "Marcher jusqu'à la destination",
                        Duration = duration,
                        Mode = "walk"
                    }
                }
            };
        }

        /// <summary>
        /// Crée un itinéraire avec vélo
        /// </summary>
        private Itinerary CreateBikeItinerary(Position origin, Position dest, 
            Station departureStation, Station arrivalStation,
            double walkToStation, double bikeRoute, double walkFromStation)
        {
            return new Itinerary
            {
                UseBike = true,
                DepartureStation = departureStation,
                ArrivalStation = arrivalStation,
                TotalDuration = walkToStation + bikeRoute + walkFromStation,
                Steps = new List<Step>
                {
                    new Step
                    {
                        Instruction = $"Marcher jusqu'à la station '{departureStation.Name}' ({departureStation.AvailableBikes} vélos disponibles)",
                        Duration = walkToStation,
                        Mode = "walk"
                    },
                    new Step
                    {
                        Instruction = $"Prendre un vélo et rouler jusqu'à la station '{arrivalStation.Name}'",
                        Duration = bikeRoute,
                        Mode = "bike"
                    },
                    new Step
                    {
                        Instruction = $"Déposer le vélo ({arrivalStation.AvailableBikeStands} places disponibles) et marcher jusqu'à la destination",
                        Duration = walkFromStation,
                        Mode = "walk"
                    }
                }
            };
        }

        /// <summary>
        /// Crée une réponse d'erreur JSON
        /// </summary>
        private Stream CreateErrorResponse(string message)
        {
            string err = JsonConvert.SerializeObject(new { error = message });
            return new MemoryStream(Encoding.UTF8.GetBytes(err));
        }
    }
}
